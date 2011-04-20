// dllmain.cpp : Implementation of DllMain.

#include "stdafx.h"
#include "resource.h"
#ifdef _WIN64
#include "OpenCoverProfiler64_i.h"
#else
#include "OpenCoverProfiler_i.h"
#endif
#include "dllmain.h"
#include "xdlldata.h"

COpenCoverProfilerModule _AtlModule;

// DLL Entry Point
extern "C" BOOL WINAPI DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved)
{
#ifdef _MERGE_PROXYSTUB
	if (!PrxDllMain(hInstance, dwReason, lpReserved))
		return FALSE;
#endif
	hInstance;
	return _AtlModule.DllMain(dwReason, lpReserved); 
}
