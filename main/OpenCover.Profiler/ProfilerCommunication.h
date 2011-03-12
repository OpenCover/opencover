#pragma once
class ProfilerCommunication
{
private:
    int _port;

    void Initialise();
    void Cleanup();
    void PrintError(HRESULT errorCode, WS_ERROR* error);
    WS_ERROR* error;
    WS_HEAP* heap;
    WS_SERVICE_PROXY* proxy;


public:
    ProfilerCommunication(int port);
    ~ProfilerCommunication(void);

public:
    void Start();
    void Stop();
    BOOL ShouldTrackAssembly(WCHAR* assemblyName);
};

