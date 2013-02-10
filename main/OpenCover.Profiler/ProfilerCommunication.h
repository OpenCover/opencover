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
	inline void AddVisitPointWithThreshold(ULONG uniqueId, ULONG threshold) { AddVisitPointToBuffer(uniqueId, IT_VisitPoint, threshold); }
	inline void Resize(ULONG minSize) { m_thresholds.grow_to_at_least(minSize); }

private:
    void AddVisitPointToBuffer(ULONG uniqueId, ULONG msgType, ULONG threshold = 0);
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
	Concurrency::concurrent_vector<ULONG> m_thresholds;

private:
    ATL::CComAutoCriticalSection m_critResults;
    bool hostCommunicationActive;

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

