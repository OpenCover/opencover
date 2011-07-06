//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#pragma once

#include "Synchronization.h"
#include "SharedMemory.h"
#include "Messages.h"

#define SEQ_BUFFER_SIZE 8000

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
    BOOL TrackAssembly(WCHAR* pModulePath, WCHAR* pAssemblyName);
    BOOL GetSequencePoints(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, std::vector<SequencePoint> &points);
    void AddVisitPoint(ULONG uniqueId);

private:
    void SendVisitPoints();

private:
    tstring m_key;

private:
    CMutex m_mutexCommunication;
    CSharedMemory m_memoryCommunication;
    CEvent m_eventSendData;
    CEvent m_eventReceiveData;
    MSG_Union *m_pMSG;

private:
    CMutex m_mutexResults;
    CSharedMemory m_memoryResults;
    CEvent m_eventSendResults;
    CEvent m_eventReceiveResults;
    MSG_SendVisitPoints_Request *m_pVisitPoints;

};

