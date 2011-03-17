// CodeCoverage.cpp : Implementation of CCodeCoverage

#include "stdafx.h"
#include "CodeCoverage.h"
#include "NativeCallback.h"

CCodeCoverage* CCodeCoverage::g_pProfiler = NULL;
// CCodeCoverage

HRESULT STDMETHODCALLTYPE CCodeCoverage::Initialize( 
    /* [in] */ IUnknown *pICorProfilerInfoUnk) 
{
    ATLTRACE(_T("::Initialize"));

    m_profilerInfo = pICorProfilerInfoUnk;
    if (m_profilerInfo != NULL) ATLTRACE(_T("    ::Initialize (m_profilerInfo OK)"));
    if (m_profilerInfo == NULL) return E_FAIL;
    m_profilerInfo2 = pICorProfilerInfoUnk;
    if (m_profilerInfo2 != NULL) ATLTRACE(_T("    ::Initialize (m_profilerInfo2 OK)"));
    if (m_profilerInfo2 == NULL) return E_FAIL;
    m_profilerInfo3 = pICorProfilerInfoUnk;
    if (m_profilerInfo3 != NULL) ATLTRACE(_T("    ::Initialize (m_profilerInfo3 OK)"));

    WCHAR pszPortNumber[10];
    ::GetEnvironmentVariableW(L"OpenCover_Port", pszPortNumber, 10);
    int portNumber = _wtoi(pszPortNumber);
    ATLTRACE(_T("->Port Number %d"), portNumber);

    m_host = new ProfilerCommunication(portNumber);

    m_host->Start();

    DWORD dwMask = 0;
    dwMask |= COR_PRF_MONITOR_ASSEMBLY_LOADS;		// Controls the AssemblyLoad and AssemblyUnload callbacks.
    dwMask |= COR_PRF_MONITOR_MODULE_LOADS;			// Controls the ModuleLoad, ModuleUnload, and ModuleAttachedToAssembly callbacks.
    dwMask |= COR_PRF_MONITOR_APPDOMAIN_LOADS;		// Controls the AppDomainCreation and AppDomainShutdown callbacks.
    dwMask |= COR_PRF_MONITOR_CLASS_LOADS;			// Controls the ClassLoad and ClassUnload callbacks.
    //dwMask |= COR_PRF_MONITOR_JIT_COMPILATION;	// Controls the JITCompilation, JITFunctionPitched, and JITInlining callbacks.
    dwMask |= COR_PRF_DISABLE_INLINING;				// Disables all inlining.
    dwMask |= COR_PRF_DISABLE_OPTIMIZATIONS;		// Disables all code optimizations.
    //dwMask |= COR_PRF_MONITOR_ENTERLEAVE;           // Controls the FunctionEnter, FunctionLeave, and FunctionTailcall callbacks.

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

UINT_PTR CCodeCoverage::FunctionMapper2(FunctionID functionId, void* clientData, BOOL* pbHookFunction)
{
    *pbHookFunction = FALSE;
    UINT_PTR retVal = functionId;
    CCodeCoverage* profiler = static_cast<CCodeCoverage*>(clientData);
    if(profiler == NULL)
        return functionId;

    std::wstring fullMethodName = profiler->GetFullMethodName(functionId);
    ATLTRACE(_T("::FunctionMapper2(%x => %s)"), functionId, W2CT(fullMethodName.c_str()));
    
    // we need to make these as filters
    if (fullMethodName.find(L"System.")==0) return functionId;
    if (fullMethodName.find(L"Microsoft.")==0) return functionId;
    
    *pbHookFunction = TRUE;
    return retVal;
}

UINT_PTR CCodeCoverage::FunctionMapper(FunctionID functionId, BOOL* pbHookFunction)
{
    return FunctionMapper2(functionId, g_pProfiler, pbHookFunction);
}


HRESULT STDMETHODCALLTYPE CCodeCoverage::Shutdown( void) 
{ 
    ATLTRACE(_T("::Shutdown"));
    m_host->Stop();
    delete m_host;
    return S_OK; 
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::AppDomainCreationStarted( 
    /* [in] */ AppDomainID appDomainId) 
{
    ATLTRACE(_T("::AppDomainCreationStarted(%X)"), appDomainId);
    return S_OK; 
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::AppDomainCreationFinished( 
    /* [in] */ AppDomainID appDomainId,
    /* [in] */ HRESULT hrStatus) 
{
    ATLTRACE(_T("::AppDomainCreationFinished(%X, 0x%X)"), appDomainId, hrStatus);
    ATLASSERT(SUCCEEDED(hrStatus));
    return S_OK; 
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::AppDomainShutdownStarted( 
    /* [in] */ AppDomainID appDomainId)
{
    ATLTRACE(_T("::AppDomainShutdownStarted(%X)"), appDomainId);
    return S_OK; 
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::AppDomainShutdownFinished( 
    /* [in] */ AppDomainID appDomainId,
    /* [in] */ HRESULT hrStatus)
{
    ATLTRACE(_T("::AppDomainShutdownFinished(%X, 0x%X)"), appDomainId, hrStatus);
    ATLASSERT(SUCCEEDED(hrStatus));
    return S_OK; 
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::AssemblyLoadStarted( 
    /* [in] */ AssemblyID assemblyId)
{
    ATLTRACE(_T("::AssemblyLoadStarted(%X)"), assemblyId);
    return S_OK; 
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::AssemblyLoadFinished( 
    /* [in] */ AssemblyID assemblyId,
    /* [in] */ HRESULT hrStatus)
{
    ATLTRACE(_T("::AssemblyLoadFinished(%X, 0x%X)"), assemblyId, hrStatus);
    ATLASSERT(SUCCEEDED(hrStatus));
    return S_OK; 
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::AssemblyUnloadStarted( 
    /* [in] */ AssemblyID assemblyId)
{
    ATLTRACE(_T("::AssemblyUnloadStarted(%X)"), assemblyId);
    return S_OK; 
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::AssemblyUnloadFinished( 
    /* [in] */ AssemblyID assemblyId,
    /* [in] */ HRESULT hrStatus)
{
    ATLTRACE(_T("::AssemblyUnloadFinished(%X, 0x%X)"), assemblyId, hrStatus);
    ATLASSERT(SUCCEEDED(hrStatus));
    return S_OK; 
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::ModuleLoadStarted( 
    /* [in] */ ModuleID moduleId)
{ 
    ATLTRACE(_T("::ModuleLoadStarted(%X)"), moduleId);
    return S_OK; 
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::ModuleLoadFinished( 
    /* [in] */ ModuleID moduleId,
    /* [in] */ HRESULT hrStatus)
{ 
    ATLTRACE(_T("::ModuleLoadFinished(%X, 0x%X)"), moduleId, hrStatus);
    ATLASSERT(SUCCEEDED(hrStatus));
    return S_OK; 
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::ModuleUnloadStarted( 
    /* [in] */ ModuleID moduleId)
{ 
    ATLTRACE(_T("::ModuleUnloadStarted(%X)"), moduleId);
    return S_OK; 
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::ModuleUnloadFinished( 
    /* [in] */ ModuleID moduleId,
    /* [in] */ HRESULT hrStatus)
{ 
    ATLTRACE(_T("::ModuleUnloadFinished(%X, 0x%X)"), moduleId, hrStatus);
    ATLASSERT(SUCCEEDED(hrStatus));
    return S_OK; 
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::ModuleAttachedToAssembly( 
    /* [in] */ ModuleID moduleId,
    /* [in] */ AssemblyID assemblyId)
{
    ATLTRACE(_T("::ModuleAttachedToAssembly(%X, %X)"), moduleId, assemblyId);

    std::wstring moduleName = GetModuleName(moduleId);
    std::wstring assemblyName = GetAssemblyName(assemblyId);
    ATLTRACE(_T("    ::ModuleAttachedToAssembly(%X => %s, %X => %s)"), 
        moduleId, W2CT(moduleName.c_str()), 
        assemblyId, W2CT(assemblyName.c_str()));
    m_host->ShouldTrackAssembly((LPWSTR)moduleName.c_str(), (LPWSTR)assemblyName.c_str());
    return S_OK; 
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::ClassLoadStarted( 
    /* [in] */ ClassID classId)
{
    ATLTRACE(_T("::ClassLoadStarted(%X)"), classId);
    return S_OK; 
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::ClassLoadFinished( 
    /* [in] */ ClassID classId,
    /* [in] */ HRESULT hrStatus)
{
    ATLTRACE(_T("::ClassLoadFinished(%X, 0x%X)"), classId, hrStatus);
    ATLASSERT(SUCCEEDED(hrStatus));

    ModuleID moduleId;
    mdTypeDef tokenTypeDef;
    m_profilerInfo->GetClassIDInfo( classId, &moduleId, &tokenTypeDef );

    std::wstring moduleName = GetModuleName(moduleId);
    ATLTRACE(_T("    ::ClassLoadFinished(%X => %s)"), moduleId, W2CT(moduleName.c_str()));

    std::wstring className = GetClassName(moduleId, tokenTypeDef);
    ATLTRACE(_T("        ::ClassLoadFinished( => %s)"), W2CT(className.c_str()));

    return S_OK; 
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::ClassUnloadStarted( 
    /* [in] */ ClassID classId)
{
    ATLTRACE(_T("::ClassUnloadStarted(%X)"), classId);
    return S_OK; 
}  

HRESULT STDMETHODCALLTYPE CCodeCoverage::ClassUnloadFinished( 
    /* [in] */ ClassID classId,
    /* [in] */ HRESULT hrStatus)
{
    ATLTRACE(_T("::ClassUnloadFinished(%X, 0x%X)"), classId, hrStatus);
    ATLASSERT(SUCCEEDED(hrStatus));
    return S_OK; 
}
