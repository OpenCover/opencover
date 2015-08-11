//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#pragma once

#include "Synchronization.h"
#include "SharedMemory.h"
#include "Messages.h"

#include <exception>
#include <concurrent_vector.h>

#include <unordered_map>

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
	inline void AddTestEnterPoint(ULONG uniqueId) { AddVisitPointToBuffer(uniqueId, IT_MethodEnter); }
	inline void AddTestLeavePoint(ULONG uniqueId) { AddVisitPointToBuffer(uniqueId, IT_MethodLeave); }
	inline void AddTestTailcallPoint(ULONG uniqueId) { AddVisitPointToBuffer(uniqueId, IT_MethodTailcall); }
	inline void AddVisitPoint(ULONG uniqueId) { AddVisitPointToBuffer(uniqueId, IT_VisitPoint); }
    void AddVisitPointToThreadBuffer(ULONG uniqueId, MSG_IdType msgType);
    void CloseChannel(bool sendSingleBuffer);

public: 
    void ThreadCreated(ThreadID threadID, DWORD osThreadID);
    void ThreadDestroyed(ThreadID threadID);

private:
    void AddVisitPointToBuffer(ULONG uniqueId, MSG_IdType msgType);
    void SendVisitPoints();
    void SendThreadVisitPoints(MSG_SendVisitPoints_Request* pVisitPoints);
    bool GetSequencePoints(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, std::vector<SequencePoint> &points);
    bool GetBranchPoints(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, std::vector<BranchPoint> &points);
    void SendRemainingThreadBuffers();
    MSG_SendVisitPoints_Request* AllocateVisitMap(DWORD osThreadID);

private:
    tstring m_key;
    tstring m_namespace;

    template<class BR, class PR>
    void RequestInformation(BR buildRequest, PR processResults, DWORD dwTimeout, tstring message);

    ULONG m_bufferId;

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
    ATL::CComAutoCriticalSection m_critComms;
    bool hostCommunicationActive;

private:
    ATL::CComAutoCriticalSection m_critThreads;
    std::unordered_map<ThreadID, ULONG> m_threadmap;
    std::unordered_map<ULONG, MSG_SendVisitPoints_Request*> m_visitmap;

    MSG_SendVisitPoints_Request* GetVisitMapForOSThread(ULONG osThread);

private:
  
    class CommunicationException : std::exception
    {
        DWORD dwReason;
        DWORD dwTimeout;
    public:
		CommunicationException(DWORD reason, DWORD timeout) {dwReason = reason; dwTimeout = timeout;}

        DWORD getReason() {return dwReason;}
        DWORD getTimeout() {return dwTimeout;}
    };

};

