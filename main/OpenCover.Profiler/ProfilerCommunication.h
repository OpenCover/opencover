//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
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
    bool TrackAssembly(WCHAR* pModulePath, WCHAR* pAssemblyName);
    bool GetPoints(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, std::vector<SequencePoint> &seqPoints, std::vector<BranchPoint> &brPoints);
    void AddVisitPoint(ULONG uniqueId);

private:
    void SendVisitPoints();
    bool GetSequencePoints(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, std::vector<SequencePoint> &points);
    bool GetBranchPoints(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, std::vector<BranchPoint> &points);


private:
    tstring m_key;

private:
    CMutex m_mutexCommunication;
    CSharedMemory m_memoryCommunication;
    CEvent m_eventProfilerRequestsInformation;
    CEvent m_eventInformationReadyForProfiler;
    MSG_Union *m_pMSG;
    CEvent m_eventInformationReadByProfiler;

private:
    CMutex m_mutexResults;
    CSharedMemory m_memoryResults;
    CEvent m_eventProfilerHasResults;
    CEvent m_eventResultsHaveBeenReceived;
    MSG_SendVisitPoints_Request *m_pVisitPoints;

};

