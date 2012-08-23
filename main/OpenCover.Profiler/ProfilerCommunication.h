//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#pragma once

#include "Synchronization.h"
#include "SharedMemory.h"
#include "Messages.h"

#include <ppl.h>
#include <concurrent_queue.h>
#include <exception>

/// <summary>Handles communication back to the profiler host</summary>
/// <remarks>Currently this is handled by using the WebServices API</remarks>
class ProfilerCommunication
{
private:

public:
    ProfilerCommunication();
    ~ProfilerCommunication(void);
    bool Initialise(TCHAR* key, TCHAR *ns);

public:
    bool TrackAssembly(WCHAR* pModulePath, WCHAR* pAssemblyName);
    bool GetPoints(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, std::vector<SequencePoint> &seqPoints, std::vector<BranchPoint> &brPoints);
    bool TrackMethod(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, ULONG &uniqueId);
    bool AllocateBuffer(LONG bufferSize, ULONG &bufferId);
    inline void AddVisitPoint(ULONG uniqueId) { if (uniqueId!=0) AddVisitPointToBuffer(uniqueId | IT_VisitPoint); }
    inline void AddTestEnterPoint(ULONG uniqueId) { if (uniqueId!=0) AddVisitPointToBuffer(uniqueId | IT_MethodEnter); }
    inline void AddTestLeavePoint(ULONG uniqueId) { if (uniqueId!=0) AddVisitPointToBuffer(uniqueId | IT_MethodLeave); }
    inline void AddTestTailcallPoint(ULONG uniqueId) { if (uniqueId!=0) AddVisitPointToBuffer(uniqueId | IT_MethodTailcall); }
    void AddVisitPointToBuffer(ULONG uniqueId);

private:
    void SendVisitPoints();
    bool GetSequencePoints(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, std::vector<SequencePoint> &points);
    bool GetBranchPoints(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, std::vector<BranchPoint> &points);

private:
    tstring m_key;
    tstring m_namespace;

    template<class BR, class PR>
    void RequestInformation(BR buildRequest, PR processResults, DWORD dwTimeout, tstring message);

private:
    CMutex m_mutexCommunication;
    CSharedMemory m_memoryCommunication;
    CEvent m_eventProfilerRequestsInformation;
    CEvent m_eventInformationReadyForProfiler;
    CEvent m_eventInformationReadByProfiler;
    MSG_Union *m_pMSG;

private:
    CSharedMemory m_memoryResults;
    CEvent m_eventProfilerHasResults;
    CEvent m_eventResultsHaveBeenReceived;
    MSG_SendVisitPoints_Request *m_pVisitPoints;

private:
    ATL::CComAutoCriticalSection m_critResults;
    bool hostCommunicationActive;

private:
  
    class CommunicationException : std::exception
    {
        DWORD dwReason;
    public:
        CommunicationException(DWORD reason) {dwReason = reason;}

        DWORD getReason() {return dwReason;}
    };

};

