#pragma once

#include "Synchronization.h"
#include "SharedMemory.h"
#include "Messages.h"

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
    BOOL GetSequencePoints(mdToken functionToken, WCHAR* pModuleName, std::vector<SequencePoint> &points);
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

