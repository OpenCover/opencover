#pragma once

typedef struct SequencePoint
{
    ULONG UniqueId;
    long Offset;
};

typedef struct VisitPoint
{
    ULONG UniqueId;
};

/// <summary>Handles communication back to the profiler host</summary>
/// <remarks>Currently this is handled by using the WebServices API</remarks>
class ProfilerCommunication
{
private:

public:
    ProfilerCommunication(int port);
    ~ProfilerCommunication(void);

public:
    void Start();
    void Stop();
    BOOL TrackAssembly(WCHAR* pModuleName, WCHAR* pAssemblyName);
    BOOL GetSequencePoints(mdToken functionToken, WCHAR* pModuleName, unsigned int* pNumPoints, SequencePoint*** pppInstrumentPoints);
    void SendVisitPoints(unsigned int numPoints, VisitPoint **ppPoints);

};

