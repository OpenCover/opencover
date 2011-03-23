// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)

#include <stdio.h>
#include <mscoree.h>
#include <metahost.h>
#include <windows.h>
#include <process.h>
#include <crtdbg.h>
#include <corerror.h>

#import <mscorlib.tlb>                           \
    rename("_Module", "_MscorlibModule")         \
    rename("ReportEvent", "MscorlibReportEvent") \
    raw_interfaces_only

#define IfFailGo(expression) \
    hr = expression;         \
    if (FAILED(hr))          \
    {                        \
        goto lExit;          \
    }

struct RuntimeAndApp
{
    ICLRRuntimeInfo *m_pRuntime;
    LPCWSTR          m_pwzAppPath;
    LPWSTR           m_pwzAppArgs;
    int              m_dwRuntimeNo;
    LPCWSTR          m_pwzRuntimeVer;
    LONG             m_dwRetCode;

};

enum SyncPoint
{
    SyncPointRuntimeStart,
    SyncPointApplicationStart,
    SyncPointAppDomainUnload
};

enum AppDomainUnloadOrder
{
    NoOrder = 0,
    SameOrder = 1 ,
    ReverseOrder = 2
};

struct CommandLineOptions
{
    bool                    m_bRunMultithreaded;
    bool                    m_bUseDefaultAppDomain;
    bool                    m_bUseLoadByName;
    AppDomainUnloadOrder    m_dwADUnloadOrder;
    SyncPoint               m_dwSyncPoint;
    int                     m_dwDelay;
}
g_options;

HANDLE g_rhndEvents[2];

//-------------------------------------------------------------
// PrintHelp
//
// Prints the help screen for the tool
//-------------------------------------------------------------
int PrintHelp()
{
    printf("Runs two EXEs by different runtimes in-proc side-by-side\n\n");

    printf("Usage: RunSxS [<Options>] <Version1> <Path1> <Args1> <Version2> <Path2> <Args2>\n\n");

    printf("  - Versions are in the usual v-prefixed, dotted form (e.g. v2.0.50727).\n");
    printf("  - Args must be a single argument - if the app takes multiple arguments\n");
    printf("    then enclose them in quotes. Embedded quotes not currently supported.\n\n");

    printf("Supported options:\n");
    printf("  - /mt - runs the EXEs in two separate threads in parallel (default)\n");
    printf("  - /st - runs the EXEs sequentially in one thread, honoring command line order\n");
    printf("  - /ndad - runs the EXEs in non-default AppDomain\n");
    printf("  - /dau - unloads the non-default ADs in the same order of command line\n");
    printf("  - /rau - unloads the non-default ADs in the reverse order of command line\n");
    printf("  - /sync CLR|APP|UNLOAD - stress parallel CLR startup (default) or parallel APP\n");
    printf("                    execution (i.e. rendez-vous the threads before\n");
    printf("                    host->Start() or before appdomain->ExecuteAssembly() \n");
    printf("                    or before Host->UnloadDomain()).\n");
    printf("  - /delay <time> - the number of millisecond the second thread will wait\n");
    printf("                    after leaving the sync point (default is 0)\n");
    printf("  - /byname - uses AppDomain.Load instead of ExecuteAssembly which allows\n");
    printf("                    NGEN images to be loaded, <PathN> are display names\n\n");

    printf("Example:\n");
    printf("  runsxs /mt /sync APP /delay 1000 v2.0.50727 hello.exe \"\" v4.0.30319\n");
    printf("         hello.v4.exe \"arg1 arg2\"\n\n");

    printf("  The tool relays the return value of the first EXE entry point as the exit\n");
    printf("  code. Environment.ExitCode is NOT relayed.\n");

    printf("  Please make sure that the EXEs and their dependencies weren't built by\n");
    printf("  a compiler that is newer than the runtime you try to run them on.\n");

    return 0;
}// PrintHelp


bool ParseOptions(int *pArgc, WCHAR ***pArgv)
{
    int argc = *pArgc - 1;
    WCHAR **argv = (*pArgv) + 1;

    while (argc > 0 && (**argv == L'/' || **argv == L'-'))
    {
        WCHAR *opt = *argv + 1;

        if (_wcsicmp(opt, L"mt") == 0) g_options.m_bRunMultithreaded = true;
        else if (_wcsicmp(opt, L"st") == 0) g_options.m_bRunMultithreaded = false;
        else if (_wcsicmp(opt, L"ndad") == 0) g_options.m_bUseDefaultAppDomain = false;
        else if (_wcsicmp(opt, L"dau") == 0) g_options.m_dwADUnloadOrder = SameOrder;
        else if (_wcsicmp(opt, L"rau") == 0) g_options.m_dwADUnloadOrder = ReverseOrder;
        else if (_wcsicmp(opt, L"byname") == 0) g_options.m_bUseLoadByName = true;
        else if (_wcsicmp(opt, L"sync") == 0)
        {
            argv++; argc--;
            if (_wcsicmp(*argv, L"clr") == 0) g_options.m_dwSyncPoint = SyncPointRuntimeStart;
            else if (_wcsicmp(*argv, L"app") == 0) g_options.m_dwSyncPoint = SyncPointApplicationStart;
            else if (_wcsicmp(*argv, L"unload") == 0) g_options.m_dwSyncPoint = SyncPointAppDomainUnload;
            else return false;
        }
        else if (_wcsicmp(opt, L"delay") == 0)
        {
            argv++; argc--;
            g_options.m_dwDelay = _wtoi(*argv);
        }
        else
        {
            // unrecognized option
            return false;
        }

        argv++; argc--;
    }

    *pArgc = argc;
    *pArgv = argv;

    return true;
}

bool GetNextArg(LPWSTR wszArgs, LPWSTR & wszStart, LPWSTR & wszEnd)
{
    // Eat leading whitespace
    while (*wszArgs == L' ') wszArgs++;
    wszStart = wszArgs;
    while (*wszArgs != L'\0' && *wszArgs != L' ') wszArgs++;
    wszEnd = wszArgs;
    return (wszEnd - wszStart) > 0;
}

SAFEARRAY *CreateArgList(LPWSTR wszArgs)
{
    // Count number of args
    unsigned cArgs = 0;
    LPWSTR wszArgStart;
    LPWSTR wszArgEnd = wszArgs;
    while (GetNextArg(wszArgEnd, wszArgStart, wszArgEnd)) cArgs++;

    // Allocate safearray
    SAFEARRAY * psa;
    SAFEARRAYBOUND rgsabound[1];
    rgsabound[0].lLbound = 0;
    rgsabound[0].cElements = cArgs;
    psa = SafeArrayCreate(VT_BSTR, 1, &rgsabound[0]);

    // Create each string
    wszArgEnd = wszArgs;
    LONG iArg = 0;
    while (GetNextArg(wszArgEnd, wszArgStart, wszArgEnd))
    {
        WCHAR origChar = *wszArgEnd;
        *wszArgEnd = L'\0';
        BSTR bstrArg = SysAllocString(wszArgStart);
        if (bstrArg != NULL)
        {
            HRESULT hr = SafeArrayPutElement(psa, &iArg, bstrArg);
            _ASSERTE(SUCCEEDED(hr));
        }
        *wszArgEnd = origChar;
        iArg++;
    }

    _ASSERT(iArg == cArgs);

    return psa;
}

void ReportSyncPoint(SyncPoint dwSyncPoint, int dwRuntimeNo)
{
    if (dwSyncPoint == g_options.m_dwSyncPoint)
    {
        if (g_options.m_bRunMultithreaded)
        {
            // rendez-vous!
            SetEvent(g_rhndEvents[dwRuntimeNo - 1]);
            WaitForSingleObject(g_rhndEvents[2 - dwRuntimeNo], INFINITE);
        }
        // implement the 'delay' feature
        if (dwRuntimeNo == 2 && g_options.m_dwDelay != 0)
        {
            Sleep(g_options.m_dwDelay);
        }
    }
    else if(g_options.m_bRunMultithreaded && dwSyncPoint == SyncPointAppDomainUnload && g_options.m_dwADUnloadOrder != NoOrder)
    {
        if(dwRuntimeNo != g_options.m_dwADUnloadOrder)
            WaitForSingleObject(g_rhndEvents[2 - dwRuntimeNo], INFINITE);
    }

}

HRESULT ExecuteAssemblyByName(mscorlib::_AppDomain *pAD, BSTR assemblyFile, SAFEARRAY *rgArgs, LONG *pdwRetCode)
{
    HRESULT hr = S_OK;

    mscorlib::_Assembly *pAssembly = NULL;
    mscorlib::_MethodInfo *pEntryPoint = NULL;
    SAFEARRAY *pParams = NULL;
    SAFEARRAY *pArgs = NULL;

    // get the entry point of the assembly
    IfFailGo(pAD->Load_2(assemblyFile, &pAssembly));
    IfFailGo(pAssembly->get_EntryPoint(&pEntryPoint));

    // see if the entry point takes >0 parameters
    IfFailGo(pEntryPoint->GetParameters(&pParams));
    
    LONG dwUBound;
    IfFailGo(SafeArrayGetUBound(pParams, 1, &dwUBound));

    // wrap the BSTR array in a VARIANT array if so (the one parameter is string[])
    if (dwUBound >= 0)
    {
        SAFEARRAYBOUND rgsabound[1];
        rgsabound[0].lLbound = 0;
        rgsabound[0].cElements = 1;
        pArgs = SafeArrayCreate(VT_VARIANT, 1, &rgsabound[0]);

        // rgArgs is a SAFEARRAY of BSTRs (command line passed arguments) corresponding to the
        // string[] parameter of the entry point method. We just need to wrap it in a VARIANT
        // as the object[] array passed to Invoke is represented as SAFEARRAY of VARIANTs here.
        VARIANT variantArg;
        VariantInit(&variantArg);

        V_VT(&variantArg) = VT_ARRAY | VT_BSTR;
        V_ARRAY(&variantArg) = rgArgs;

        IfFailGo(SafeArrayPutElement(pArgs, &rgsabound[0].lLbound, &variantArg));
    }

    // call the entry point
    VARIANT variantRetVal, variantNull;
    VariantInit(&variantRetVal);
    VariantInit(&variantNull);
    V_VT(&variantNull) = VT_NULL;

    IfFailGo(pEntryPoint->Invoke_3(variantNull, pArgs, &variantRetVal));

    // propagate back the return value
    if (V_VT(&variantRetVal) == VT_I4)
    {
        *pdwRetCode = V_I4(&variantRetVal);
    }
    else
    {
        *pdwRetCode = 0;
    }

lExit:
    if (pArgs != NULL)
        SafeArrayDestroy(pArgs);

    if (pParams != NULL)
        SafeArrayDestroy(pParams);

    if (pEntryPoint != NULL)
        pEntryPoint->Release();

    if (pAssembly != NULL)
        pAssembly->Release();

    return hr;
}

unsigned __stdcall ThreadProc(void *pArg)
{
    RuntimeAndApp *pContext = (RuntimeAndApp *)pArg;

    HRESULT hr;

#define IfFailGoMsg(MSG)                                                        \
    if (FAILED(hr))                                                             \
    {                                                                           \
        printf("Runtime %d (%S): " MSG " (%x)\n",                               \
            pContext->m_dwRuntimeNo,                                            \
            pContext->m_pwzRuntimeVer,                                          \
            hr);                                                                \
        goto lExit;                                                             \
    }

    ICLRRuntimeHost  *pHost_V2 = NULL;
    ICorRuntimeHost     *pHost = NULL;
    IUnknown             *pUnk = NULL;
    mscorlib::_AppDomain  *pAD = NULL;

    // Get ICorRuntimeHost to execute the app.
    hr = pContext->m_pRuntime->GetInterface(
        CLSID_CorRuntimeHost,
        IID_ICorRuntimeHost,
        (LPVOID *)&pHost);
    IfFailGoMsg("Failed to load the runtime");

	// Also try to get ICLRRuntimeHost which has a useful Stop method. (ICorRuntimeHost::Stop is empty.)
    if (SUCCEEDED(pContext->m_pRuntime->GetInterface(
        CLSID_CLRRuntimeHost,
        IID_ICLRRuntimeHost,
        (LPVOID *)&pHost_V2)))
    {
        hr = pHost_V2->Start();
        IfFailGoMsg("Failed to start ICLRRuntimeHost");
    }

    ReportSyncPoint(SyncPointRuntimeStart, pContext->m_dwRuntimeNo);
    hr = pHost->Start();
    IfFailGoMsg("Failed to start ICorRuntimeHost");

    if(g_options.m_bUseDefaultAppDomain)
    {
        hr = pHost->GetDefaultDomain(&pUnk);
        IfFailGoMsg("Failed to get default domain");
    }
    else
    {
        hr = pHost->CreateDomain(L"AD2",NULL,&pUnk);
        IfFailGoMsg("Failed to create AppDomain");
    }

    hr = pUnk->QueryInterface(__uuidof(mscorlib::_AppDomain), (LPVOID *)&pAD);
    IfFailGoMsg("Failed to QI for _AppDomain");

    BSTR bstrPath = SysAllocString(pContext->m_pwzAppPath);
    SAFEARRAY *rgArgs = CreateArgList(pContext->m_pwzAppArgs);

    ReportSyncPoint(SyncPointApplicationStart, pContext->m_dwRuntimeNo);

    if (g_options.m_bUseLoadByName)
    {
        hr = ExecuteAssemblyByName(pAD, bstrPath, rgArgs, &pContext->m_dwRetCode);
    }
    else
    {
        hr = pAD->ExecuteAssembly_3(bstrPath, NULL, rgArgs, &pContext->m_dwRetCode);
    }

    if (hr == COR_E_NEWER_RUNTIME)
    {
        IfFailGoMsg("Failed to execute assembly\nWas it built by a compiler that is newer than this runtime?");
    }
    else
    {
        // we don't know whether the error comes from the runtime (failed to execute assembly) or
        // the assembly actually ran and threw an unhandled exception that was converted to the HR
        IfFailGoMsg("ExecuteAssembly returned an error code");
    }

    SysFreeString(bstrPath);
    SafeArrayDestroy(rgArgs);


    if(!g_options.m_bUseDefaultAppDomain)
    {	
        ReportSyncPoint(SyncPointAppDomainUnload, pContext->m_dwRuntimeNo);
        hr = pHost->UnloadDomain(pAD);
        IfFailGoMsg("Failed to unload AppDomain");
    }
    pAD->Release();  pAD = NULL;
    pUnk->Release(); pUnk = NULL;

    hr = pHost->Stop();
    IfFailGoMsg("Failed to stop ICorRuntimeHost");

    if (pHost_V2 != NULL)
    {
        hr = pHost_V2->Stop();
        IfFailGoMsg("Failed to stop ICLRRuntimeHost");

        pHost_V2 = NULL;
    }

lExit:
    if (pHost_V2 != NULL)
        pHost_V2->Release();

    if (pAD != NULL)
        pAD->Release();

    if (pUnk != NULL)
        pUnk->Release();

    if (pHost != NULL)
        pHost->Release();

    if (g_options.m_bRunMultithreaded)
    {
        // make sure we don't deadlock the other thread
        // this is also needed if the value of g_options.m_dwADUnloadOrder is SameOrder or ReverseOrder
        // since one of the threads will be waiting for the other one to Unload the AppDomain.
        SetEvent(g_rhndEvents[pContext->m_dwRuntimeNo - 1]);

        _endthreadex(0);
    }
    return 0;

#undef IfFailGoMsg
}

//-------------------------------------------------------------
// main
//
// Entrypoint - Determines what the user wants to do and does it.
//-------------------------------------------------------------
int _cdecl wmain(int argc, __in_ecount(argc) WCHAR **argv)
{
    enum
    {
        VERSION_1_IDX = 0,
        PATH_1_IDX = VERSION_1_IDX + 1,
        ARGS_1_IDX = PATH_1_IDX + 1,

        VERSION_2_IDX = ARGS_1_IDX + 1,
        PATH_2_IDX = VERSION_2_IDX + 1,
        ARGS_2_IDX = PATH_2_IDX + 1,
    };

    g_options.m_bRunMultithreaded = true;
    g_options.m_bUseDefaultAppDomain = true;
    g_options.m_bUseLoadByName = false;
    g_options.m_dwADUnloadOrder = NoOrder;
    g_options.m_dwSyncPoint = SyncPointRuntimeStart;
    g_options.m_dwDelay = 0;

    if (!ParseOptions(&argc, &argv) ||
        argc != ARGS_2_IDX+1)
    {
        printf("Error parsing args.\n");
        PrintHelp();
        return -1;
    }

    HRESULT hr;

    ICLRMetaHost *pMH;
    hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID *)&pMH);

    if (FAILED(hr))
    {
        printf("Error getting the metahosting interface (%x).\n", hr);
        return -1;
    }

    if (_wcsicmp(argv[VERSION_1_IDX], argv[VERSION_2_IDX]) == 0)
    {
        printf("Warning: Version1 is the same as Version2. RunSxS may attempt to stop and\n");
        printf("         then re-start the runtime, which will fail.\n\n");
    }

    ICLRRuntimeInfo *pRuntime1;
    ICLRRuntimeInfo *pRuntime2;

    hr = pMH->GetRuntime(argv[VERSION_1_IDX], IID_ICLRRuntimeInfo, (LPVOID *)&pRuntime1);
    if (FAILED(hr))
    {
        printf("Error getting runtime 1 \"%S\" (%x).\n", argv[VERSION_1_IDX], hr);
        return -1;
    }

    hr = pMH->GetRuntime(argv[VERSION_2_IDX], IID_ICLRRuntimeInfo, (LPVOID *)&pRuntime2);
    if (FAILED(hr))
    {
        printf("Error getting runtime 2 \"%S\" (%x).\n", argv[VERSION_2_IDX], hr);
        return -1;
    }

    RuntimeAndApp Context1 = { pRuntime1, argv[PATH_1_IDX], argv[ARGS_1_IDX], 1, argv[VERSION_1_IDX] };
    RuntimeAndApp Context2 = { pRuntime2, argv[PATH_2_IDX], argv[ARGS_2_IDX], 2, argv[VERSION_2_IDX] };

    if (g_options.m_bRunMultithreaded)
    {
        g_rhndEvents[0] = CreateEvent(NULL, FALSE, FALSE, NULL);
        g_rhndEvents[1] = CreateEvent(NULL, FALSE, FALSE, NULL);

        if (g_rhndEvents[0] == NULL || g_rhndEvents[1] == NULL)
        {
            printf("Error creating events (%d).\n", GetLastError());
            return -1;
        }

        HANDLE rhndThreads[2];
        rhndThreads[0] = (HANDLE)_beginthreadex(NULL, 0, ThreadProc, &Context1, 0, NULL);
        rhndThreads[1] = (HANDLE)_beginthreadex(NULL, 0, ThreadProc, &Context2, 0, NULL);

        if (rhndThreads[0] == NULL || rhndThreads[1] == NULL)
        {
            printf("Error creating threads (%d).\n", errno);
            return -1;
        }

        WaitForMultipleObjects(2, rhndThreads, TRUE, INFINITE);

        CloseHandle(rhndThreads[0]);
        CloseHandle(rhndThreads[1]);

        CloseHandle(g_rhndEvents[0]);
        CloseHandle(g_rhndEvents[1]);
    }
    else
    {
        ThreadProc(&Context1);
        ThreadProc(&Context2);
    }

    pRuntime1->Release();
    pRuntime2->Release();

    pMH->Release();

    return Context1.m_dwRetCode;
}// main
