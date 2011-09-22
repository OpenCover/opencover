//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
// CodeCoverage.cpp : Implementation of CCodeCoverage

#include "stdafx.h"
#include "CodeCoverage.h"
#include "NativeCallback.h"
#include "CoverageInstrumentation.h"
#include "dllmain.h"

CCodeCoverage* CCodeCoverage::g_pProfiler = NULL;
// CCodeCoverage

/// <summary>Handle <c>ICorProfilerCallback::Initialize</c></summary>
/// <remarks>Initialize the profiling environment and establish connection to the host</remarks>
HRESULT STDMETHODCALLTYPE CCodeCoverage::Initialize( 
    /* [in] */ IUnknown *pICorProfilerInfoUnk) 
{
    ATLTRACE(_T("::Initialize"));
    
    OLECHAR szGuid[40]={0};
    int nCount = ::StringFromGUID2(CLSID_CodeCoverage, szGuid, 40);
    ::OutputDebugStringW(szGuid);

    WCHAR szModuleName[MAX_PATH];
    GetModuleFileNameW(_AtlModule.m_hModule, szModuleName, MAX_PATH);
    ::OutputDebugStringW(szModuleName);

    if (g_pProfiler!=NULL) ATLTRACE(_T("Another instance of the profiler is running under this process..."));

    m_profilerInfo = pICorProfilerInfoUnk;
    if (m_profilerInfo != NULL) ATLTRACE(_T("    ::Initialize (m_profilerInfo OK)"));
    if (m_profilerInfo == NULL) return E_FAIL;
    m_profilerInfo2 = pICorProfilerInfoUnk;
    if (m_profilerInfo2 != NULL) ATLTRACE(_T("    ::Initialize (m_profilerInfo2 OK)"));
    if (m_profilerInfo2 == NULL) return E_FAIL;
    m_profilerInfo3 = pICorProfilerInfoUnk;
    ZeroMemory(&m_runtimeVersion, sizeof(m_runtimeVersion));
    if (m_profilerInfo3 != NULL) 
    {
        ATLTRACE(_T("    ::Initialize (m_profilerInfo3 OK)"));
        
        ZeroMemory(&m_runtimeVersion, sizeof(m_runtimeVersion));
        m_profilerInfo3->GetRuntimeInformation(NULL, &m_runtimeType, 
            &m_runtimeVersion.usMajorVersion, 
            &m_runtimeVersion.usMinorVersion, 
            &m_runtimeVersion.usBuildNumber, 
            &m_runtimeVersion.usRevisionNumber, 0, NULL, NULL); 

        ATLTRACE(_T("Runtime %d"), m_runtimeType);
    }

    TCHAR key[1024];
    ::GetEnvironmentVariable(_T("OpenCover_Profiler_Key"), key, 1024);
    ATLTRACE(_T("key = %s"), key);
    if (!m_host.Initialise(key))
    {
        ATLTRACE(_T("    ::Initialize Failed to initialise the profiler communications -> GetLastError() => %d"), ::GetLastError());
        return E_FAIL;
    }

    DWORD dwMask = 0;
    dwMask |= COR_PRF_MONITOR_MODULE_LOADS;			// Controls the ModuleLoad, ModuleUnload, and ModuleAttachedToAssembly callbacks.
    dwMask |= COR_PRF_MONITOR_JIT_COMPILATION;	    // Controls the JITCompilation, JITFunctionPitched, and JITInlining callbacks.
    dwMask |= COR_PRF_DISABLE_INLINING;				// Disables all inlining.
    dwMask |= COR_PRF_DISABLE_OPTIMIZATIONS;		// Disables all code optimizations.
    //if (m_runtimeVersion.usMajorVersion != 0 && m_runtimeType == COR_PRF_DESKTOP_CLR) 
    //    dwMask |= COR_PRF_DISABLE_TRANSPARENCY_CHECKS_UNDER_FULL_TRUST; 

    m_profilerInfo2->SetEventMask(dwMask);

    g_pProfiler = this;

    return S_OK; 
}

/// <summary>Handle <c>ICorProfilerCallback::Shutdown</c></summary>
HRESULT STDMETHODCALLTYPE CCodeCoverage::Shutdown( void) 
{ 
    ATLTRACE(_T("::Shutdown"));
    try {m_host.Stop();} catch(...){}
    g_pProfiler = NULL;
    return S_OK; 
}

/// <summary>An unmanaged callback that can be called from .NET that has a single I4 parameter</summary>
/// <remarks>
/// void (__fastcall *pt)(long) = &SequencePointVisit ;
/// mdSignature pmsig = GetUnmanagedMethodSignatureToken_I4(moduleId);
/// </remarks>
static void __fastcall InstrumentPointVisit(ULONG seq)
{
    CCodeCoverage::g_pProfiler->m_host.AddVisitPoint(seq);
}

static COR_SIGNATURE visitedMethodCallSignature[] = 
{
    IMAGE_CEE_CS_CALLCONV_DEFAULT,   
    0x01,                                   
    ELEMENT_TYPE_VOID,
    ELEMENT_TYPE_I4
};

static COR_SIGNATURE ctorCallSignature[] = 
{
    IMAGE_CEE_CS_CALLCONV_DEFAULT | IMAGE_CEE_CS_CALLCONV_HASTHIS,   
    0x00,                                   
    ELEMENT_TYPE_VOID
};

static COR_SIGNATURE cctorCallSignature[] = 
{
    IMAGE_CEE_CS_CALLCONV_DEFAULT,   
    0x00,                                   
    ELEMENT_TYPE_VOID
};

HRESULT CCodeCoverage::CreateCriticalMethod(IMetaDataEmit *metaDataEmit, ModuleID moduleId, mdModuleRef mscorlibRef, mdTypeDef typeDef, mdMethodDef& methodDef)
{
    COM_FAIL_RETURNHR(metaDataEmit->DefineMethod(typeDef, L"VisitedCritical",
        mdPublic | mdStatic | mdHideBySig, visitedMethodCallSignature, sizeof(visitedMethodCallSignature), 
        0, miIL | miManaged | miPreserveSig, &methodDef));

    // our profiler hook
    BYTE data[] = {(0x01 << 2) | CorILMethod_TinyFormat, CEE_RET};
    mdSignature pvsig = GetMethodSignatureToken_I4(moduleId);
    void (__fastcall *pt)(ULONG) = &InstrumentPointVisit ;
    //ATLTRACE(_T("====> %X"), pt);

    Method criticalMethod((IMAGE_COR_ILMETHOD*)data);
    InstructionList instructions;
    instructions.push_back(new Instruction(CEE_LDARG_0));
    #if _WIN64
    instructions.push_back(new Instruction(CEE_LDC_I8, (ULONGLONG)pt));
    #else
    instructions.push_back(new Instruction(CEE_LDC_I4, (ULONG)pt));
    #endif
    instructions.push_back(new Instruction(CEE_CALLI, pvsig));

    criticalMethod.InsertInstructionsAtOffset(0, instructions);
    criticalMethod.DumpIL();

    CComPtr<IMethodMalloc> methodMalloc;
    COM_FAIL_RETURNHR(m_profilerInfo3->GetILFunctionBodyAllocator(moduleId, &methodMalloc));

    void* pMethodBody = methodMalloc->Alloc(criticalMethod.GetMethodSize());
    criticalMethod.WriteMethod((IMAGE_COR_ILMETHOD*)pMethodBody);

    COM_FAIL_RETURNHR(m_profilerInfo3->SetILFunctionBody(moduleId, 
        methodDef, (LPCBYTE)pMethodBody), S_OK);

    if (m_profilerInfo3 != NULL) 
    {
        mdTypeDef criticalAttributeTypeDef;
        COM_FAIL_RETURNHR(metaDataEmit->DefineTypeRefByName(mscorlibRef, 
            L"System.Security.SecurityCriticalAttribute", &criticalAttributeTypeDef)); 

        mdToken criticalAttributeCtor;
        COM_FAIL_RETURNHR(metaDataEmit->DefineMemberRef(criticalAttributeTypeDef, 
            L".ctor", ctorCallSignature, sizeof(ctorCallSignature), &criticalAttributeCtor));

        COM_FAIL_RETURNHR(metaDataEmit->DefineCustomAttribute(methodDef, criticalAttributeCtor, NULL, 0, NULL));
    }
    return S_OK;
}

HRESULT CCodeCoverage::CreateSafeCriticalMethod(IMetaDataEmit *metaDataEmit, ModuleID moduleId, 
    mdModuleRef mscorlibRef, mdTypeDef typeDef, mdMethodDef criticalMethodDef, mdMethodDef& methodDef)
{
    COM_FAIL_RETURNHR(metaDataEmit->DefineMethod(typeDef, L"Visited",
        mdPublic | mdStatic | mdHideBySig, visitedMethodCallSignature, sizeof(visitedMethodCallSignature), 
        0, miIL | miManaged | miPreserveSig, &methodDef));

    BYTE data[] = {(0x01 << 2) | CorILMethod_TinyFormat, CEE_RET};
    Method visitedMethod((IMAGE_COR_ILMETHOD*)data);
    InstructionList instructions;
    instructions.push_back(new Instruction(CEE_LDARG_0));
    instructions.push_back(new Instruction(CEE_CALL, criticalMethodDef));
    visitedMethod.InsertInstructionsAtOffset(0, instructions);
    visitedMethod.DumpIL();

    CComPtr<IMethodMalloc> methodMalloc;
    COM_FAIL_RETURNHR(m_profilerInfo3->GetILFunctionBodyAllocator(moduleId, &methodMalloc));

    void* pMethodBody = methodMalloc->Alloc(visitedMethod.GetMethodSize());
    visitedMethod.WriteMethod((IMAGE_COR_ILMETHOD*)pMethodBody);

    COM_FAIL_RETURNHR(m_profilerInfo3->SetILFunctionBody(moduleId, methodDef, (LPCBYTE)pMethodBody));

    if (m_profilerInfo3 != NULL) 
    {
        // get attributes
        mdTypeDef safeAttributeTypeDef;
        COM_FAIL_RETURNHR(metaDataEmit->DefineTypeRefByName(mscorlibRef,
            L"System.Security.SecuritySafeCriticalAttribute", &safeAttributeTypeDef)); 
        
        mdToken safeAttributeCtor;
        COM_FAIL_RETURNHR(metaDataEmit->DefineMemberRef(safeAttributeTypeDef, 
            L".ctor", ctorCallSignature, sizeof(ctorCallSignature), &safeAttributeCtor));

        COM_FAIL_RETURNHR(metaDataEmit->DefineCustomAttribute(methodDef, safeAttributeCtor, NULL, 0, NULL));
    }
}

/// <summary>Handle <c>ICorProfilerCallback::ModuleAttachedToAssembly</c></summary>
/// <remarks>Inform the host that we have a new module attached and that it may be 
/// of interest</remarks>
HRESULT STDMETHODCALLTYPE CCodeCoverage::ModuleAttachedToAssembly( 
    /* [in] */ ModuleID moduleId,
    /* [in] */ AssemblyID assemblyId)
{
    std::wstring modulePath = GetModulePath(moduleId);
    std::wstring assemblyName = GetAssemblyName(assemblyId);
    ATLTRACE(_T("::ModuleAttachedToAssembly(%X => %s, %X => %s)"), 
        moduleId, W2CT(modulePath.c_str()), 
        assemblyId, W2CT(assemblyName.c_str()));
    m_allowModules[modulePath] = m_host.TrackAssembly((LPWSTR)modulePath.c_str(), (LPWSTR)assemblyName.c_str());
    m_allowModulesAssemblyMap[modulePath] = assemblyName;

    if (m_allowModules[modulePath])
    {
        CComPtr<IMetaDataEmit> metaDataEmit;
        COM_FAIL_RETURN(m_profilerInfo->GetModuleMetaData(moduleId, 
            ofRead | ofWrite, IID_IMetaDataEmit, (IUnknown**)&metaDataEmit), S_OK);
            
        mdModuleRef mscorlibRef;
        COM_FAIL_RETURN(GetModuleRef(moduleId, L"mscorlib", mscorlibRef), S_OK); 

        // define type
        mdTypeDef systemObject;
        COM_FAIL_RETURN(metaDataEmit->DefineTypeRefByName(mscorlibRef, 
            L"System.Object", &systemObject), S_OK);

        mdToken implementsNone[] = { mdTokenNil };
        mdTypeDef injectedType;
        COM_FAIL_RETURN(metaDataEmit->DefineTypeDef(L"Injected", 
            tdPublic | tdAutoClass | tdAnsiClass | tdAbstract | tdSealed | tdBeforeFieldInit, systemObject, NULL, &injectedType), S_OK);

        mdMethodDef injectedCriticalMethod;
        COM_FAIL_RETURN(CreateCriticalMethod(metaDataEmit, moduleId, mscorlibRef, injectedType, injectedCriticalMethod), S_OK); 

        mdMethodDef injectedVisitedMethod;
        COM_FAIL_RETURN(CreateSafeCriticalMethod(metaDataEmit, moduleId, mscorlibRef, injectedType, 
            injectedCriticalMethod, injectedVisitedMethod), S_OK); 

        m_injectedVisitedMethodDefs[modulePath] = injectedVisitedMethod;
    }

    return S_OK; 
}

/// <summary>Handle <c>ICorProfilerCallback::JITCompilationStarted</c></summary>
/// <remarks>The 'workhorse' </remarks>
HRESULT STDMETHODCALLTYPE CCodeCoverage::JITCompilationStarted( 
        /* [in] */ FunctionID functionId,
        /* [in] */ BOOL fIsSafeToBlock)
{
    std::wstring modulePath;
    mdToken functionToken;
    ModuleID moduleId;

    if (GetTokenAndModule(functionId, functionToken, moduleId, modulePath))
    {
        if (!m_allowModules[modulePath]) return S_OK;

        //void (__fastcall *pt)(ULONG) = &InstrumentPointVisit ;
        //ATLTRACE(_T("====> %X"), pt);

        std::pair<std::wstring, ULONG32> key(modulePath, functionToken);
        if (m_jitdMethods[key]) return S_OK;
        m_jitdMethods[key] = true;

        ATLTRACE(_T("::JITCompilationStarted(%X, %d, (%X)%s)"), functionId, functionToken, moduleId, W2CT(modulePath.c_str()));
        
        std::vector<SequencePoint> seqPoints;
        std::vector<BranchPoint> brPoints;

        mdMethodDef injectedVisitedMethod = m_injectedVisitedMethodDefs[modulePath];
        
        if (m_host.GetPoints(functionToken, (LPWSTR)modulePath.c_str(), 
            (LPWSTR)m_allowModulesAssemblyMap[modulePath].c_str(), seqPoints, brPoints))
        {
            if (seqPoints.size()==0) return S_OK;
            LPCBYTE pMethodHeader = NULL;
            ULONG iMethodSize = 0;
            m_profilerInfo2->GetILFunctionBody(moduleId, functionToken, &pMethodHeader, &iMethodSize);

            IMAGE_COR_ILMETHOD* pMethod = (IMAGE_COR_ILMETHOD*)pMethodHeader;
            
            CoverageInstrumentation instumentedMethod(pMethod);
            instumentedMethod.IncrementStackSize(2);

            ATLTRACE(_T("Instrumenting..."));
            //seqPoints.clear();
            //brPoints.clear();

            instumentedMethod.AddSequenceCoverage(injectedVisitedMethod, seqPoints);
            instumentedMethod.AddBranchCoverage(injectedVisitedMethod, brPoints);
            
            instumentedMethod.DumpIL();

            CComPtr<IMethodMalloc> methodMalloc;
            m_profilerInfo2->GetILFunctionBodyAllocator(moduleId, &methodMalloc);
            IMAGE_COR_ILMETHOD* pNewMethod = (IMAGE_COR_ILMETHOD*)methodMalloc->Alloc(instumentedMethod.GetMethodSize());
            instumentedMethod.WriteMethod(pNewMethod);
            m_profilerInfo2->SetILFunctionBody(moduleId, functionToken, (LPCBYTE) pNewMethod);

            ULONG mapSize = instumentedMethod.GetILMapSize();
            COR_IL_MAP * pMap = (COR_IL_MAP *)CoTaskMemAlloc(mapSize * sizeof(COR_IL_MAP));
            instumentedMethod.PopulateILMap(mapSize, pMap);
            m_profilerInfo2->SetILInstrumentedCodeMap(functionId, TRUE, mapSize, pMap);

            // only do this for .NET4 and above as there are issues with earlier runtimes (Access Violations)
            if (m_runtimeVersion.usMajorVersion >= 4)
                CoTaskMemFree(pMap);
        }
    }
    
    return S_OK; 
}
        