// CodeCoverage.cpp : Implementation of CCodeCoverage

#include "stdafx.h"
#include "CodeCoverage.h"

// Print out rich error info
void PrintError(HRESULT errorCode, WS_ERROR* error)
{
    ATLTRACE(L"Failure: errorCode=0x%lx\n", errorCode);

    if (errorCode == E_INVALIDARG || errorCode == WS_E_INVALID_OPERATION)
    {
        // Correct use of the APIs should never generate these errors
        ATLTRACE(L"The error was due to an invalid use of an API.  This is likely due to a bug in the program.\n");
        DebugBreak();
    }

    HRESULT hr = NOERROR;
    if (error != NULL)
    {
        ULONG errorCount;
        hr = WsGetErrorProperty(error, WS_ERROR_PROPERTY_STRING_COUNT, &errorCount, sizeof(errorCount));
        if (FAILED(hr))
        {
            goto Exit;
        }
        for (ULONG i = 0; i < errorCount; i++)
        {
            WS_STRING string;
            hr = WsGetErrorString(error, i, &string);
            if (FAILED(hr))
            {
                goto Exit;
            }
            ATLTRACE(L"%.*s\n", string.length, string.chars);
        }
    }
Exit:
    if (FAILED(hr))
    {
        ATLTRACE(L"Could not get error string (errorCode=0x%lx)\n", hr);
    }
}
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

	WS_ERROR* error = NULL;
	WS_HEAP* heap = NULL;
	WS_SERVICE_PROXY* proxy = NULL;

	
	WCHAR pszPortNumber[10];

	::GetEnvironmentVariableW(L"OpenCover_Port", pszPortNumber, 10);

	int portNumber = _wtoi(pszPortNumber);
	
	HRESULT hr = ERROR_SUCCESS;

	WS_ENDPOINT_ADDRESS address = {};
	WS_STRING url= WS_STRING_VALUE(L"http://localhost:8000/OpenCover.Profiler/");
	address.url = url;
	
	ATLTRACE(_T("STARTING"));

	hr = WsCreateError(NULL,  0,  &error);
    if (FAILED(hr))
    {
		goto Exit;
    }

	ATLTRACE(_T("WsCreateError"));

	hr = WsCreateHeap(2048, 512, NULL, 0, &heap, error); 
    if (FAILED(hr))
    {
		goto Exit;
    }

	ATLTRACE(_T("WsCreateHeap"));

	// Create the proxy
    hr = WsCreateServiceProxy(
            WS_CHANNEL_TYPE_REQUEST, 
            WS_HTTP_CHANNEL_BINDING, 
            NULL, 
            NULL, 
            0, 
            NULL,
            0,
            &proxy, 
            error);

	ATLTRACE(_T("WsCreateServiceProxy"));

    if (FAILED(hr))
    {
		goto Exit;
    }
        
    hr = WsOpenServiceProxy(
        proxy, 
        &address, 
        NULL, 
        error);
    if (FAILED(hr))
    {
		goto Exit;
    }
	ATLTRACE(_T("WsOpenServiceProxy"));
	
	BOOL result;
	hr = DefaultBinding_IProfilerCommunication_Start(proxy, 
		&result, heap, 
		NULL, 
        0, 
        NULL, 
        error);
    if (FAILED(hr))
    {
		ATLTRACE(_T("DefaultBinding_IProfilerCommunication_Start (0x%x)"), hr);
		goto Exit;
    }
	ATLTRACE(_T("DefaultBinding_IProfilerCommunication_Start"));

Exit:
	 if (FAILED(hr))
	 {
		 PrintError(hr, error);
	 }

	if (proxy != NULL)
    {
        WsCloseServiceProxy(
            proxy, 
            NULL, 
            NULL);
    
        WsFreeServiceProxy(
            proxy);
    }
    
    if (heap != NULL)
    {
        WsFreeHeap(heap);
    }

	if (error != NULL)
    {
        WsFreeError(error);
    }

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
