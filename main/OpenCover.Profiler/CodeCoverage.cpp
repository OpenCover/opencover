// CodeCoverage.cpp : Implementation of CCodeCoverage

#include "stdafx.h"
#include "CodeCoverage.h"
#include "NativeCallback.h"
#include "Method.h"

CCodeCoverage* CCodeCoverage::g_pProfiler = NULL;
// CCodeCoverage

/// <summary>Handle <c>ICorProfilerCallback::Initialize</c></summary>
/// <remarks>Initialize the profiling environment and establish connection to the host</remarks>
HRESULT STDMETHODCALLTYPE CCodeCoverage::Initialize( 
    /* [in] */ IUnknown *pICorProfilerInfoUnk) 
{
    m_isV4 = FALSE;
    OLECHAR szGuid[40]={0};
    int nCount = ::StringFromGUID2(CLSID_CodeCoverage, szGuid, 40);

    ATLTRACE(_T("::Initialize - %s"), W2CT(szGuid));

    if (g_pProfiler!=NULL) ATLTRACE(_T("Another instance of the profiler is running under this process..."));

    m_profilerInfo = pICorProfilerInfoUnk;
    if (m_profilerInfo != NULL) ATLTRACE(_T("    ::Initialize (m_profilerInfo OK)"));
    if (m_profilerInfo == NULL) return E_FAIL;
    m_profilerInfo2 = pICorProfilerInfoUnk;
    if (m_profilerInfo2 != NULL) ATLTRACE(_T("    ::Initialize (m_profilerInfo2 OK)"));
    if (m_profilerInfo2 == NULL) return E_FAIL;
    m_profilerInfo3 = pICorProfilerInfoUnk;
    if (m_profilerInfo3 != NULL) 
    {
        m_isV4 = TRUE;
        ATLTRACE(_T("    ::Initialize (m_profilerInfo3 OK)"));
    }

    TCHAR key[1024];
    ::GetEnvironmentVariable(_T("OpenCover_Profiler_Key"), key, 1024);
    m_host.Initialise(key);

    DWORD dwMask = 0;
    dwMask |= COR_PRF_MONITOR_MODULE_LOADS;			// Controls the ModuleLoad, ModuleUnload, and ModuleAttachedToAssembly callbacks.
    dwMask |= COR_PRF_MONITOR_JIT_COMPILATION;	    // Controls the JITCompilation, JITFunctionPitched, and JITInlining callbacks.
    dwMask |= COR_PRF_DISABLE_INLINING;				// Disables all inlining.
    dwMask |= COR_PRF_DISABLE_OPTIMIZATIONS;		// Disables all code optimizations.

    m_profilerInfo2->SetEventMask(dwMask);

    g_pProfiler = this;

    return S_OK; 
}

/// <summary>Handle <c>ICorProfilerCallback::Shutdown</c></summary>
HRESULT STDMETHODCALLTYPE CCodeCoverage::Shutdown( void) 
{ 
    ATLTRACE(_T("::Shutdown"));
    g_pProfiler = NULL;
    return S_OK; 
}

/// <summary>Handle <c>ICorProfilerCallback::ModuleAttachedToAssembly</c></summary>
/// <remarks>Inform the host that we have a new module attached and that it may be 
/// of interest</remarks>
HRESULT STDMETHODCALLTYPE CCodeCoverage::ModuleAttachedToAssembly( 
    /* [in] */ ModuleID moduleId,
    /* [in] */ AssemblyID assemblyId)
{
    std::wstring moduleName = GetModuleName(moduleId);
    std::wstring assemblyName = GetAssemblyName(assemblyId);
    ATLTRACE(_T("::ModuleAttachedToAssembly(%X => %s, %X => %s)"), 
        moduleId, W2CT(moduleName.c_str()), 
        assemblyId, W2CT(assemblyName.c_str()));
    m_allowModules[moduleName] = m_host.TrackAssembly((LPWSTR)moduleName.c_str(), (LPWSTR)assemblyName.c_str());
    return S_OK; 
}

/// <summary>An unmanaged callback that can be called from .NET that has a single I4 parameter</summary>
/// <remarks>
/// void (__fastcall *pt)(long) = &SequencePointVisit ;
/// mdSignature pmsig = GetUnmanagedMethodSignatureToken_I4(moduleId);
/// </remarks>
static void __fastcall SequencePointVisit(ULONG seq)
{
    CCodeCoverage::g_pProfiler->m_host.AddVisitPoint(seq);
}


/// <summary>Handle <c>ICorProfilerCallback::JITCompilationStarted</c></summary>
/// <remarks>The 'workhorse' </remarks>
HRESULT STDMETHODCALLTYPE CCodeCoverage::JITCompilationStarted( 
        /* [in] */ FunctionID functionId,
        /* [in] */ BOOL fIsSafeToBlock)
{
    std::wstring moduleName;
    mdToken functionToken;
    ModuleID moduleId;

    if (GetTokenAndModule(functionId, functionToken, moduleId, moduleName))
    {
        if (!m_allowModules[moduleName]) return S_OK;

        ATLTRACE(_T("::JITCompilationStarted(%X, %d, %s)"), functionId, functionToken, W2CT(moduleName.c_str()));
        
        std::vector<SequencePoint> points;

        if (m_host.GetSequencePoints(functionToken, (LPWSTR)moduleName.c_str(), points))
        {
            if (points.size()==0) return S_OK;
            LPCBYTE pMethodHeader = NULL;
            ULONG iMethodSize = 0;
            m_profilerInfo2->GetILFunctionBody(moduleId, functionToken, &pMethodHeader, &iMethodSize);

            IMAGE_COR_ILMETHOD* pMethod = (IMAGE_COR_ILMETHOD*)pMethodHeader;
            
            void (__fastcall *pt)(ULONG) = &SequencePointVisit ;
            mdSignature pmsig = GetUnmanagedMethodSignatureToken_I4(moduleId);

            Method instumentedMethod(pMethod);
            instumentedMethod.SetMinimumStackSize(2);

            ATLTRACE(_T("Instrumenting..."));
            //points.clear();
            for ( std::vector<SequencePoint>::iterator it = points.begin(); it != points.end(); it++)
            {    
                //ATLTRACE(_T("SEQPT %02d IL_%04X"), i, ppPoints[i]->Offset);
                InstructionList instructions;
                instructions.push_back(new Instruction(CEE_LDC_I4, (*it).UniqueId));
#if _WIN64
                instructions.push_back(new Instruction(CEE_LDC_I8, (ULONGLONG)pt));
#else
                instructions.push_back(new Instruction(CEE_LDC_I4, (ULONG)pt));
#endif
                instructions.push_back(new Instruction(CEE_CALLI, pmsig));

                instumentedMethod.InsertSequenceInstructionsAtOriginalOffset((*it).Offset, instructions);
            }

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

            // only do this for .NET4 as there are issues with earlier runtimes (Access Violations)
            if (m_isV4) CoTaskMemFree(pMap);
        }
    }
    
    return S_OK; 
}
        