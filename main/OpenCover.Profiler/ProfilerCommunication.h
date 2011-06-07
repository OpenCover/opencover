#pragma once

#include "Synchronization.h"
#include "SharedMemory.h"

typedef struct SequencePoint
{
    ULONG UniqueId;
    long Offset;
};

typedef struct VisitPoint
{
    ULONG UniqueId;
};

enum MSG_Type : int
{
    MSG_Unknown = 0,
    MSG_TrackAssembly = 1,
};

#pragma pack(push)
#pragma pack(1)

typedef struct _MSG_TrackAssembly_Request
{
    int type;
    short nModuleName;
    WCHAR szModuleName[512];
    short nAssemblyName;
    WCHAR szAssemblyName[512];
} MSG_TrackAssembly_Request;

typedef struct _MSG_TrackAssembly_Response
{
    BOOL bResponse;
} MSG_TrackAssembly_Response;

#pragma pack(pop)

typedef union _MSG_Union
{
    int type;
    MSG_TrackAssembly_Request trackRequest;
    MSG_TrackAssembly_Response trackResponse;
} MSG_Union;

/// <summary>Handles communication back to the profiler host</summary>
/// <remarks>Currently this is handled by using the WebServices API</remarks>
class ProfilerCommunication
{
private:

public:
    ProfilerCommunication();
    ~ProfilerCommunication(void);
    void Initialise(TCHAR* key);

public:
    BOOL TrackAssembly(WCHAR* pModuleName, WCHAR* pAssemblyName);
    BOOL GetSequencePoints(mdToken functionToken, WCHAR* pModuleName, unsigned int* pNumPoints, SequencePoint*** pppInstrumentPoints);
    void SendVisitPoints(unsigned int numPoints, VisitPoint **ppPoints);

private:
    CMutex m_mutexCommunication;
    CSharedMemory m_memoryCommunication;
    CEvent m_eventSendData;
    CEvent m_eventReceiveData;

    CMutex m_mutexResults;
    tstring m_key;
    MSG_Union *m_pMSG;
};

