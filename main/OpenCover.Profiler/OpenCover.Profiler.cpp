//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
// OpenCover.Profiler.cpp : Implementation of DLL Exports.


#include "stdafx.h"
#include "resource.h"
#include "OpenCoverProfiler_i.h"
#include "dllmain.h"
#include "xdlldata.h"


// Used to determine whether the DLL can be unloaded by OLE.
STDAPI DllCanUnloadNow(void)
{
	#ifdef _MERGE_PROXYSTUB
	HRESULT hr = PrxDllCanUnloadNow();
	if (hr != S_OK)
		return hr;
#endif
			return _AtlModule.DllCanUnloadNow();
	}

// Returns a class factory to create an object of the requested type.
_Check_return_
STDAPI DllGetClassObject(_In_ REFCLSID rclsid, _In_ REFIID riid, _Outptr_ LPVOID* ppv)
{
	#ifdef _MERGE_PROXYSTUB
	if (PrxDllGetClassObject(rclsid, riid, ppv) == S_OK)
		return S_OK;
#endif
		return _AtlModule.DllGetClassObject(rclsid, riid, ppv);
}

// DllRegisterServer - Adds entries to the system registry.
STDAPI DllRegisterServer(void)
{
	// registers object, typelib and all interfaces in typelib
	HRESULT hr = _AtlModule.DllRegisterServer();
	#ifdef _MERGE_PROXYSTUB
	if (FAILED(hr))
		return hr;
	hr = PrxDllRegisterServer();
#endif
		return hr;
}

// DllUnregisterServer - Removes entries from the system registry.
STDAPI DllUnregisterServer(void)
{
	HRESULT hr = _AtlModule.DllUnregisterServer();
	#ifdef _MERGE_PROXYSTUB
	if (FAILED(hr))
		return hr;
	hr = PrxDllRegisterServer();
	if (FAILED(hr))
		return hr;
	hr = PrxDllUnregisterServer();
#endif
		return hr;
}

STDAPI SetPerUserRegistration()
{
	OaEnablePerUserTLibRegistration();

	HKEY key; 
	ATLTRACE(_T("::SetPerUserRegistration - Enter"));
	if ( ERROR_SUCCESS != ::RegOpenKeyW(HKEY_CURRENT_USER, L"Software\\Classes", &key) )
	{ 
		ATLTRACE(_T("::SetPerUserRegistration"));
		return E_FAIL; 
	} 
	if ( ERROR_SUCCESS != ::RegOverridePredefKey(HKEY_CLASSES_ROOT, key) )
	{ 
		ATLTRACE(_T("::SetPerUserRegistration"));
		::RegCloseKey(key); 
		return E_FAIL; 
	}
	ATLTRACE(_T("::SetPerUserRegistration - Exit"));
	::RegCloseKey(key); 
	return S_OK; 
}

// DllInstall - Adds/Removes entries to the system registry per user per machine.
STDAPI DllInstall(BOOL bInstall, _In_opt_ LPCWSTR pszCmdLine)
{
	HRESULT hr = E_FAIL;
	static const wchar_t szUserSwitch[] = L"user";
	if (pszCmdLine != NULL)
	{
		if (_wcsnicmp(pszCmdLine, szUserSwitch, _countof(szUserSwitch)) == 0)
		{
			ATL::AtlSetPerUserRegistration(true);
			//SetPerUserRegistration();
		}
	}

	if (bInstall)
	{	
		hr = DllRegisterServer();
		if (FAILED(hr))
		{
			DllUnregisterServer();
		}
	}
	else
	{
		hr = DllUnregisterServer();
	}

	return hr;
}


