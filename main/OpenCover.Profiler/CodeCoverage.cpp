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

#define CUCKOO_SAFE_METHOD_NAME L"SafeVisited"
#define CUCKOO_CRITICAL_METHOD_NAME L"VisitedCritical"
#define CUCKOO_NEST_TYPE_NAME L"System.CannotUnloadAppDomainException"
#define MSCORLIB_NAME L"mscorlib"

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

    TCHAR ns[1024];
    ::GetEnvironmentVariable(_T("OpenCover_Profiler_Namespace"), ns, 1024);
    ATLTRACE(_T("ns = %s"), ns);

    if (!m_host.Initialise(key, ns))
    {
        ATLTRACE(_T("    ::Initialize Failed to initialise the profiler communications -> GetLastError() => %d"), ::GetLastError());
        return E_FAIL;
    }

    DWORD dwMask = 0;
    dwMask |= COR_PRF_MONITOR_MODULE_LOADS;			// Controls the ModuleLoad, ModuleUnload, and ModuleAttachedToAssembly callbacks.
    dwMask |= COR_PRF_MONITOR_JIT_COMPILATION;	    // Controls the JITCompilation, JITFunctionPitched, and JITInlining callbacks.
    dwMask |= COR_PRF_DISABLE_INLINING;				// Disables all inlining.
    dwMask |= COR_PRF_DISABLE_OPTIMIZATIONS;		// Disables all code optimizations.
    dwMask |= COR_PRF_USE_PROFILE_IMAGES;           // Don't use NGen images
    dwMask |= COR_PRF_MONITOR_ENTERLEAVE;           // Controls the FunctionEnter, FunctionLeave, and FunctionTailcall callbacks.

    m_profilerInfo2->SetEventMask(dwMask);

    if(m_profilerInfo3 != NULL)
        m_profilerInfo3->SetFunctionIDMapper2(FunctionMapper2, this);
    else
        m_profilerInfo2->SetFunctionIDMapper(FunctionMapper);

    g_pProfiler = this;

    m_profilerInfo2->SetEnterLeaveFunctionHooks2(
        _FunctionEnter2, 
        _FunctionLeave2, 
        _FunctionTailcall2);

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

HRESULT STDMETHODCALLTYPE CCodeCoverage::ModuleLoadFinished( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ HRESULT hrStatus) 
{
    CComPtr<IMetaDataEmit> metaDataEmit;
    COM_FAIL_RETURN(m_profilerInfo->GetModuleMetaData(moduleId, 
        ofRead | ofWrite, IID_IMetaDataEmit, (IUnknown**)&metaDataEmit), S_OK);
    if (metaDataEmit==NULL) return S_OK;

    CComPtr<IMetaDataImport> metaDataImport;
    COM_FAIL_RETURN(m_profilerInfo->GetModuleMetaData(moduleId, 
        ofRead | ofWrite, IID_IMetaDataImport, (IUnknown**)&metaDataImport), S_OK);
    if (metaDataImport==NULL) return S_OK;

    mdTypeDef systemObject = mdTokenNil;
    if (S_OK == metaDataImport->FindTypeDefByName(L"System.Object", mdTokenNil, &systemObject))
    {
        mdMethodDef systemObjectCtor;
        COM_FAIL_RETURN(metaDataImport->FindMethod(systemObject, L".ctor", 
            ctorCallSignature, sizeof(ctorCallSignature), &systemObjectCtor), S_OK);

        ULONG ulCodeRVA = 0;
        COM_FAIL_RETURN(metaDataImport->GetMethodProps(systemObjectCtor, NULL, NULL, 
            0, NULL, NULL, NULL, NULL, &ulCodeRVA, NULL), S_OK);

        mdCustomAttribute customAttr;
        mdToken attributeCtor;
        mdTypeDef attributeTypeDef;
        mdTypeDef nestToken;

        COM_FAIL_RETURN(metaDataImport->FindTypeDefByName(CUCKOO_NEST_TYPE_NAME, mdTokenNil, &nestToken), S_OK);

        // create a method that we will mark up with the SecurityCriticalAttribute
        COM_FAIL_RETURN(metaDataEmit->DefineMethod(nestToken, CUCKOO_CRITICAL_METHOD_NAME,
            mdPublic | mdStatic | mdHideBySig, visitedMethodCallSignature, sizeof(visitedMethodCallSignature), 
            ulCodeRVA, miIL | miManaged | miPreserveSig | miNoInlining, &m_cuckooCriticalToken), S_OK);

        COM_FAIL_RETURN(metaDataImport->FindTypeDefByName(L"System.Security.SecurityCriticalAttribute",
            NULL, &attributeTypeDef), S_OK); 

        if (m_runtimeType == COR_PRF_DESKTOP_CLR)
        {
            // for desktop we use the .ctor that takes a SecurityCriticalScope argument as the 
            // default (no arguments) constructor fails with "0x801311C2 - known custom attribute value is bad" 
            // when we try to attach it in .NET2 - .NET4 version doesn't care which one we use
            mdTypeDef scopeToken;
            COM_FAIL_RETURN(metaDataImport->FindTypeDefByName(L"System.Security.SecurityCriticalScope", mdTokenNil, &scopeToken), S_OK);

            ULONG sigLength=4;
            COR_SIGNATURE ctorCallSignatureEnum[] = 
            {
                IMAGE_CEE_CS_CALLCONV_DEFAULT | IMAGE_CEE_CS_CALLCONV_HASTHIS,   
                0x01,                                   
                ELEMENT_TYPE_VOID,
                ELEMENT_TYPE_VALUETYPE,
                0x00,0x00, 0x00, 0x00 // make room for our compressed token - should always be 2 but...
            };

            sigLength += CorSigCompressToken(scopeToken, &ctorCallSignatureEnum[4]);

            COM_FAIL_RETURN(metaDataImport->FindMember(attributeTypeDef, 
                L".ctor", ctorCallSignatureEnum, sigLength, &attributeCtor), S_OK);
        
            unsigned char blob[] = {0x01, 0x00, 0x01, 0x00, 0x00, 0x00}; // prolog U2 plus an enum of I4 (little-endian)
            COM_FAIL_RETURN(metaDataEmit->DefineCustomAttribute(m_cuckooCriticalToken, attributeCtor, blob, sizeof(blob), &customAttr), S_OK);
        }
        else
        {
            // silverlight only has one .ctor for this type
            COM_FAIL_RETURN(metaDataImport->FindMember(attributeTypeDef, 
                L".ctor", ctorCallSignature, sizeof(ctorCallSignature), &attributeCtor), S_OK);
        
            COM_FAIL_RETURN(metaDataEmit->DefineCustomAttribute(m_cuckooCriticalToken, attributeCtor, NULL, 0, &customAttr), S_OK);
        }

        // create a method that we will mark up with the SecuritySafeCriticalAttribute
        COM_FAIL_RETURN(metaDataEmit->DefineMethod(nestToken, CUCKOO_SAFE_METHOD_NAME,
            mdPublic | mdStatic | mdHideBySig, visitedMethodCallSignature, sizeof(visitedMethodCallSignature), 
            ulCodeRVA, miIL | miManaged | miPreserveSig | miNoInlining, &m_cuckooSafeToken), S_OK);

        COM_FAIL_RETURN(metaDataImport->FindTypeDefByName(L"System.Security.SecuritySafeCriticalAttribute",
            NULL, &attributeTypeDef), S_OK); 

        COM_FAIL_RETURN(metaDataImport->FindMember(attributeTypeDef, 
            L".ctor", ctorCallSignature, sizeof(ctorCallSignature), &attributeCtor), S_OK);

        COM_FAIL_RETURN(metaDataEmit->DefineCustomAttribute(m_cuckooSafeToken, attributeCtor, NULL, 0, &customAttr), S_OK);
        
        ATLTRACE(_T("Added methods to mscorlib"));
    }

    return S_OK;
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
        // for modules we are going to instrument add our reference to the method marked 
        // with the SecuritySafeCriticalAttribute
        CComPtr<IMetaDataEmit> metaDataEmit;
        COM_FAIL_RETURN(m_profilerInfo->GetModuleMetaData(moduleId, 
            ofRead | ofWrite, IID_IMetaDataEmit, (IUnknown**)&metaDataEmit), S_OK);
        if (metaDataEmit == NULL) return S_OK;

        mdModuleRef mscorlibRef;
        COM_FAIL_RETURN(GetModuleRef(moduleId, MSCORLIB_NAME, mscorlibRef), S_OK); 

        mdTypeDef nestToken;
        COM_FAIL_RETURN(metaDataEmit->DefineTypeRefByName(mscorlibRef, CUCKOO_NEST_TYPE_NAME, &nestToken), S_OK);

        mdMemberRef cuckooSafeToken;
        COM_FAIL_RETURN(metaDataEmit->DefineMemberRef(nestToken, CUCKOO_SAFE_METHOD_NAME,  
            visitedMethodCallSignature, sizeof(visitedMethodCallSignature), &cuckooSafeToken) , S_OK);

        m_injectedVisitedMethodDefs[modulePath] = cuckooSafeToken;
    }

    return S_OK; 
}

/// <summary>This is the method marked with the SecurityCriticalAttribute</summary>
/// <remarks>This method makes the call into the profiler</remarks>
HRESULT CCodeCoverage::AddCriticalCuckooBody(ModuleID moduleId)
{
    // our profiler hook
    mdSignature pvsig = GetMethodSignatureToken_I4(moduleId);
    void (__fastcall *pt)(ULONG) = &InstrumentPointVisit ;

    BYTE data[] = {(0x01 << 2) | CorILMethod_TinyFormat, CEE_RET};
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
    COM_FAIL_RETURNHR(m_profilerInfo->GetILFunctionBodyAllocator(moduleId, &methodMalloc));

    void* pMethodBody = methodMalloc->Alloc(criticalMethod.GetMethodSize());
    criticalMethod.WriteMethod((IMAGE_COR_ILMETHOD*)pMethodBody);

    COM_FAIL_RETURN(m_profilerInfo->SetILFunctionBody(moduleId, 
        m_cuckooCriticalToken, (LPCBYTE)pMethodBody), S_OK);

    m_addedCriticalCuckoo = true;

    return S_OK;
}

/// <summary>This is the body of our method marked with the SecuritySafeCriticalAttribute</summary>
/// <remarks>Calls the method that is marked with the SecurityCriticalAttribute</remarks>
HRESULT CCodeCoverage::AddSafeCuckooBody(ModuleID moduleId)
{
    BYTE data[] = {(0x01 << 2) | CorILMethod_TinyFormat, CEE_RET};
    Method criticalMethod((IMAGE_COR_ILMETHOD*)data);
    InstructionList instructions;
    instructions.push_back(new Instruction(CEE_LDARG_0));
    instructions.push_back(new Instruction(CEE_CALL, m_cuckooCriticalToken));

    criticalMethod.InsertInstructionsAtOffset(0, instructions);
    criticalMethod.DumpIL();

    CComPtr<IMethodMalloc> methodMalloc;
    COM_FAIL_RETURN(m_profilerInfo->GetILFunctionBodyAllocator(moduleId, &methodMalloc), S_OK);

    void* pMethodBody = methodMalloc->Alloc(criticalMethod.GetMethodSize());
    criticalMethod.WriteMethod((IMAGE_COR_ILMETHOD*)pMethodBody);

    COM_FAIL_RETURN(m_profilerInfo->SetILFunctionBody(moduleId, 
        m_cuckooSafeToken, (LPCBYTE)pMethodBody), S_OK);

    m_addedSafeCuckoo = true;

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
    AssemblyID assemblyId;

    if (GetTokenAndModule(functionId, functionToken, moduleId, modulePath, &assemblyId))
    {
        // add the bodies for our cuckoo methods when required
        if (!(m_addedCriticalCuckoo && m_addedSafeCuckoo) && m_watchForCuckoos)
        {
            if (MSCORLIB_NAME == GetAssemblyName(assemblyId))
            {
                if (m_cuckooCriticalToken == functionToken)
                {
                    COM_FAIL_RETURN(AddCriticalCuckooBody(moduleId), S_OK);
                }

                if (m_cuckooSafeToken == functionToken)
                {
                    COM_FAIL_RETURN(AddSafeCuckooBody(moduleId), S_OK);
                }
            }
        }

        if (!m_allowModules[modulePath]) return S_OK;

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
            m_watchForCuckoos = true;

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
        