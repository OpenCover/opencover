// CodeCoverage.cpp : Implementation of CCodeCoverage

#include "stdafx.h"
#include "CodeCoverage.h"

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
	

    DWORD dwMask = 0;
	dwMask |= COR_PRF_MONITOR_ASSEMBLY_LOADS;		// Controls the AssemblyLoad and AssemblyUnload callbacks.
	dwMask |= COR_PRF_MONITOR_MODULE_LOADS;			// Controls the ModuleLoad, ModuleUnload, and ModuleAttachedToAssembly callbacks.
	dwMask |= COR_PRF_MONITOR_APPDOMAIN_LOADS;		// Controls the AppDomainCreation and AppDomainShutdown callbacks.
	dwMask |= COR_PRF_MONITOR_CLASS_LOADS;			// Controls the ClassLoad and ClassUnload callbacks.
	//dwMask |= COR_PRF_MONITOR_JIT_COMPILATION;	// Controls the JITCompilation, JITFunctionPitched, and JITInlining callbacks.
	dwMask |= COR_PRF_DISABLE_INLINING;				// Disables all inlining.
	dwMask |= COR_PRF_DISABLE_OPTIMIZATIONS;		// Disables all code optimizations.

	m_profilerInfo->SetEventMask(dwMask);

	return S_OK; 
}
        
HRESULT STDMETHODCALLTYPE CCodeCoverage::Shutdown( void) 
{ 
	ATLTRACE(_T("::Shutdown"));
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
