#include "StdAfx.h"
#include "ProfilerCommunication.h"

#include "..\schema\opencover.profiler.wsdl.h"
#include "..\schema\tempuri.org.wsdl.h"
#include "..\schema\schemas.microsoft.com.2003.10.Serialization.xsd.h"


#define ONERROR_GOEXIT(hr) if (FAILED(hr)) goto Exit

ProfilerCommunication::ProfilerCommunication(int port)
{
    error = NULL;
    heap = NULL;
    proxy = NULL;

    _port = port;
    Initialise();
}

ProfilerCommunication::~ProfilerCommunication(void)
{
    Cleanup();
}

// Print out rich error info
void ProfilerCommunication::PrintError(HRESULT errorCode, WS_ERROR* error)
{
    ATLTRACE(L"Failure: errorCode=0x%lx\n", errorCode);

    if (errorCode == E_INVALIDARG || errorCode == WS_E_INVALID_OPERATION)
    {
        // Correct use of the APIs should never generate these errors
        ATLTRACE(_T("The error was due to an invalid use of an API.  This is likely due to a bug in the program.\n"));
        DebugBreak();
    }

    HRESULT hr = NOERROR;
    if (error != NULL)
    {
        ULONG errorCount;
        hr = WsGetErrorProperty(error, WS_ERROR_PROPERTY_STRING_COUNT, &errorCount, sizeof(errorCount));
        ONERROR_GOEXIT(hr);

        for (ULONG i = 0; i < errorCount; i++)
        {
            WS_STRING string;
            hr = WsGetErrorString(error, i, &string);
            ONERROR_GOEXIT(hr);
            ATLTRACE(_T("%s"), W2CT(string.chars));
        }
    }
Exit:
    WsResetError(error);
    if (FAILED(hr))
    {
        ATLTRACE(L"Could not get error string (errorCode=0x%lx)\n", hr);
    }
}

void ProfilerCommunication::Initialise()
{
    HRESULT hr = ERROR_SUCCESS;
    WS_ENDPOINT_ADDRESS address = {};
    WCHAR szUrl[255] = {0};

    ATLTRACE(_T("STARTING"));

    hr = WsCreateError(NULL,  0,  &error);
    ONERROR_GOEXIT(hr);

    ATLTRACE(_T("WsCreateError"));

    hr = NetTcpBinding_IProfilerCommunication_CreateServiceProxy
        (
        NULL,
        NULL,
        0,
        &proxy,
        error
        );
    ONERROR_GOEXIT(hr);
    
    ATLTRACE(_T("NetTcpBinding_IProfilerCommunication_CreateServiceProxy"));

    wsprintf(szUrl, L"net.tcp://localhost:%d/OpenCover.Profiler.Host", _port);
    address.url.chars = szUrl;
    address.url.length = (ULONG)wcslen(address.url.chars);

    hr = WsOpenServiceProxy(
        proxy, 
        &address, 
        NULL, 
        error);
    ONERROR_GOEXIT(hr);

    ATLTRACE(_T("WsOpenServiceProxy"));

    hr = WsCreateHeap(2*1024*1024, 64*1024, NULL, 0, &heap, error); 
    ONERROR_GOEXIT(hr);

    ATLTRACE(_T("WsCreateHeap"));

Exit:
    if (FAILED(hr))
    {
        PrintError(hr, error);
        Cleanup();
    }
}

void ProfilerCommunication::Cleanup()
{
    if (proxy != NULL)
    {
        WsCloseServiceProxy(
            proxy,
            NULL,
            NULL);

        WsFreeServiceProxy(
            proxy);

        proxy = NULL;
    }

    if (heap != NULL)
    {
        WsFreeHeap(heap);
        heap = NULL;
    }

    if (error != NULL)
    {
        WsFreeError(error);
        error = NULL;
    }
}

void ProfilerCommunication::Start()
{
    if (proxy==NULL) return;
    WsResetHeap(heap, error);
    HRESULT hr = NetTcpBinding_IProfilerCommunication_Started(proxy, 
        heap, 
        NULL, 
        0, 
        NULL, 
        error);

    if (FAILED(hr)) PrintError(hr, error);
    ATLTRACE(_T("NetTcpBinding_IProfilerCommunication_Started"));
}

void ProfilerCommunication::Stop()
{
    if (proxy==NULL) return;
    WsResetHeap(heap, error);
    HRESULT hr = NetTcpBinding_IProfilerCommunication_Stopping(proxy, 
        heap, 
        NULL, 
        0, 
        NULL, 
        error);

    if (FAILED(hr)) PrintError(hr, error);
    ATLTRACE(_T("NetTcpBinding_IProfilerCommunication_Stopping"));
}

BOOL ProfilerCommunication::TrackAssembly(WCHAR* pModuleName, WCHAR* pAssemblyName)
{
    BOOL result;
    if (proxy==NULL) return FALSE;
    WsResetHeap(heap, error);
    HRESULT hr = NetTcpBinding_IProfilerCommunication_TrackAssembly(proxy,
        pModuleName,
        pAssemblyName,
        &result,
        heap, 
        NULL, 
        0, 
        NULL, 
        error);

    if (FAILED(hr)) PrintError(hr, error);
    ATLTRACE(_T("NetTcpBinding_IProfilerCommunication_ShouldTrackAssembly %s => %s"), pAssemblyName, result ? _T("Yes") : _T("No"));
    return result;
}

BOOL ProfilerCommunication::GetSequencePoints(mdToken functionToken, WCHAR* pModuleName, unsigned int* pNumPoints, SequencePoint*** pppInstrumentPoints)
{
    BOOL result;
    if (proxy==NULL) return FALSE;
    WsResetHeap(heap, error);
    HRESULT hr = NetTcpBinding_IProfilerCommunication_GetSequencePoints(proxy,
        pModuleName,
        functionToken,
        &result,
        pNumPoints,
        pppInstrumentPoints,
        heap, 
        NULL,
        0,
        NULL,
        error);

    if (FAILED(hr)) PrintError(hr, error);
    ATLTRACE(_T("NetTcpBinding_IProfilerCommunication_GetSequencePoints => %d"), *pNumPoints);
    return result;
}


void ProfilerCommunication::SendVisitPoints(unsigned int numPoints, VisitPoint **ppPoints)
{
    if (proxy==NULL) return;
    WsResetHeap(heap, error);
    HRESULT hr = NetTcpBinding_IProfilerCommunication_Visited(proxy,
        numPoints,
        ppPoints,
        heap,
        NULL,
        0, 
        NULL,
        error);

    if (FAILED(hr)) PrintError(hr, error);
    ATLTRACE(_T("NetTcpBinding_IProfilerCommunication_Visited => %d"), numPoints);
}


