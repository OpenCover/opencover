//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#include "StdAfx.h"
#include "ProfilerCommunication.h"
#include "ReleaseTrace.h"

#include <concrt.h>

#include <TlHelp32.h>

#define ONERROR_GOEXIT(hr) if (FAILED(hr)) goto Exit
#define MAX_MSG_SIZE 65536

ProfilerCommunication::ProfilerCommunication() 
{
}

ProfilerCommunication::~ProfilerCommunication()
{
}

DWORD GetMainThreadId () {
    const std::tr1::shared_ptr<void> hThreadSnapshot(
        CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, 0), CloseHandle);
    if (hThreadSnapshot.get() == INVALID_HANDLE_VALUE) {
        return 0;
    }
    THREADENTRY32 tEntry;
    tEntry.dwSize = sizeof(THREADENTRY32);
    DWORD result = 0;
    DWORD currentPID = GetCurrentProcessId();
    for (BOOL success = Thread32First(hThreadSnapshot.get(), &tEntry);
        !result && success && GetLastError() != ERROR_NO_MORE_FILES;
        success = Thread32Next(hThreadSnapshot.get(), &tEntry))
    {
        if (tEntry.th32OwnerProcessID == currentPID) {
            result = tEntry.th32ThreadID;
        }
    }
    return result;
}

bool ProfilerCommunication::Initialise(TCHAR *key, TCHAR *ns)
{
    m_key = key;
    m_namespace = ns;

    m_mutexCommunication.Initialise((m_namespace + _T("\\OpenCover_Profiler_Communication_Mutex_") + m_key).c_str());
    if (!m_mutexCommunication.IsValid()) return false;
    m_mutexResults.Initialise((m_namespace + _T("\\OpenCover_Profiler_Results_Mutex_") + m_key).c_str());
    if (!m_mutexResults.IsValid()) return false;
    
    ATLTRACE(_T("Initialised mutexes"));

    m_eventProfilerRequestsInformation.Initialise((m_namespace + _T("\\OpenCover_Profiler_Communication_SendData_Event_") + m_key).c_str());
    if (!m_eventProfilerRequestsInformation.IsValid()) return false;
    m_eventInformationReadyForProfiler.Initialise((m_namespace + _T("\\OpenCover_Profiler_Communication_ReceiveData_Event_") + m_key).c_str());
    if (!m_eventInformationReadyForProfiler.IsValid()) return false;

    m_eventInformationReadByProfiler.Initialise((m_namespace + _T("\\OpenCover_Profiler_Communication_ChunkData_Event_") + m_key).c_str());
    if (!m_eventInformationReadByProfiler.IsValid()) return false;

    m_eventProfilerHasResults.Initialise((m_namespace + _T("\\OpenCover_Profiler_Communication_SendResults_Event_") + m_key).c_str());
    if (!m_eventProfilerHasResults.IsValid()) return false;
    m_eventResultsHaveBeenReceived.Initialise((m_namespace + _T("\\OpenCover_Profiler_Communication_ReceiveResults_Event_") + m_key).c_str());
    if (!m_eventResultsHaveBeenReceived.IsValid()) return false;

    ATLTRACE(_T("Initialised events"));

    m_memoryCommunication.OpenFileMapping((m_namespace + _T("\\OpenCover_Profiler_Communication_MemoryMapFile_") + m_key).c_str());
    if (!m_memoryCommunication.IsValid()) return false;
    m_memoryResults.OpenFileMapping((m_namespace + _T("\\OpenCover_Profiler_Results_MemoryMapFile_") + m_key).c_str());
    if (!m_memoryResults.IsValid()) return false;

    ATLTRACE(_T("Initialised memory maps"));

    m_pMSG = (MSG_Union*)m_memoryCommunication.MapViewOfFile(0, 0, MAX_MSG_SIZE);
    m_pVisitPoints = (MSG_SendVisitPoints_Request*)m_memoryResults.MapViewOfFile(0, 0, MAX_MSG_SIZE);

    DWORD mainThreadId = GetMainThreadId();
    m_mainThread = OpenThread(THREAD_QUERY_INFORMATION , FALSE, mainThreadId);

    ::CreateThread( NULL, 0, QueueProcessingThread, this, 0, NULL);

    return true;
}

DWORD WINAPI ProfilerCommunication::QueueProcessingThread(LPVOID lpParam ) 
{
    ProfilerCommunication * pComm = (ProfilerCommunication*)lpParam; 
    pComm->ProcessResults();
    return 0;
}

void ProfilerCommunication::ProcessResults()
{
    m_bProcessResults = true;
    int mainThreadTestCounter = 0;
    while(m_bProcessResults && ProcessQueue())
    {
        Concurrency::Context::Yield();
        if (mainThreadTestCounter++ == 10000) 
        {
            DWORD exitCode = 0;
            if (GetExitCodeThread(m_mainThread, &exitCode)) 
            {
                mainThreadTestCounter = 0;
                if (exitCode != STILL_ACTIVE)
                {
                    RELTRACE(_T("Main thread has already exited - time to go bye bye"));
                    m_bProcessResults = false;
                    ProcessQueue();
                }
            }
        }
    }
}

bool ProfilerCommunication::ProcessQueue()
{
    CScopedLock<CMutex> lock(m_mutexResults);  
    ULONG id;
    if (m_queue.try_pop(id))
    {
        do
        {
            if (id == 0) return false;
            m_pVisitPoints->points[m_pVisitPoints->count].UniqueId = id;
            if (++m_pVisitPoints->count == VP_BUFFER_SIZE)
            {
                SendVisitPoints();
                m_pVisitPoints->count = 0;
            }
        } while (m_queue.try_pop(id));
    }
    return true;
}

void ProfilerCommunication::Stop()
{
    ATL::CComCritSecLock<ATL::CComAutoCriticalSection> lock(m_critResults);
    if (!m_bProcessResults) return;
    m_bProcessResults = false;
    ProcessQueue();
}

void ProfilerCommunication::SendVisitPoints()
{
    if (m_eventProfilerHasResults.SignalAndWait(m_eventResultsHaveBeenReceived, 5000) == WAIT_TIMEOUT) {ATLTRACE(_T("**** timeout ****"));};
    m_eventResultsHaveBeenReceived.Reset();
    return;
}

bool ProfilerCommunication::GetPoints(mdToken functionToken, WCHAR* pModulePath, 
    WCHAR* pAssemblyName, std::vector<SequencePoint> &seqPoints, std::vector<BranchPoint> &brPoints)
{
    CScopedLock<CMutex> lock(m_mutexCommunication);

    bool ret = GetSequencePoints(functionToken, pModulePath, pAssemblyName, seqPoints);
     
    GetBranchPoints(functionToken, pModulePath, pAssemblyName, brPoints);

    return ret;
}

bool ProfilerCommunication::GetSequencePoints(mdToken functionToken, WCHAR* pModulePath,  
    WCHAR* pAssemblyName, std::vector<SequencePoint> &points)
{
    RequestInformation(
        [=]
        {
            m_pMSG->getSequencePointsRequest.type = MSG_GetSequencePoints;
            m_pMSG->getSequencePointsRequest.functionToken = functionToken;
            wcscpy_s(m_pMSG->getSequencePointsRequest.szModulePath, pModulePath);
            wcscpy_s(m_pMSG->getSequencePointsRequest.szAssemblyName, pAssemblyName);
        }, 
        [=, &points]()->BOOL
        {
            for (int i=0; i < m_pMSG->getSequencePointsResponse.count;i++)
                points.push_back(m_pMSG->getSequencePointsResponse.points[i]); 
            return m_pMSG->getSequencePointsResponse.hasMore;
        }
    );

    return (points.size() != 0);
}

bool ProfilerCommunication::GetBranchPoints(mdToken functionToken, WCHAR* pModulePath, 
    WCHAR* pAssemblyName, std::vector<BranchPoint> &points)
{
    RequestInformation(
        [=]
        {
            m_pMSG->getBranchPointsRequest.type = MSG_GetBranchPoints;
            m_pMSG->getBranchPointsRequest.functionToken = functionToken;
            wcscpy_s(m_pMSG->getBranchPointsRequest.szModulePath, pModulePath);
            wcscpy_s(m_pMSG->getBranchPointsRequest.szAssemblyName, pAssemblyName);
        }, 
        [=, &points]()->BOOL
        {
            for (int i=0; i < m_pMSG->getBranchPointsResponse.count;i++)
                points.push_back(m_pMSG->getBranchPointsResponse.points[i]); 
            return m_pMSG->getBranchPointsResponse.hasMore;
        }
    );

    return (points.size() != 0);
}

bool ProfilerCommunication::TrackAssembly(WCHAR* pModulePath, WCHAR* pAssemblyName)
{
    CScopedLock<CMutex> lock(m_mutexCommunication);

    bool response = false;
    RequestInformation(
        [=]()
        {
            m_pMSG->trackAssemblyRequest.type = MSG_TrackAssembly; 
            wcscpy_s(m_pMSG->trackAssemblyRequest.szModulePath, pModulePath);
            wcscpy_s(m_pMSG->trackAssemblyRequest.szAssemblyName, pAssemblyName);
        }, 
        [=, &response]()->BOOL
        {
            response =  m_pMSG->trackAssemblyResponse.bResponse == TRUE;
            return FALSE;
        }
    );

    return response;
}

bool ProfilerCommunication::TrackMethod(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, ULONG &uniqueId)
{
    CScopedLock<CMutex> lock(m_mutexCommunication);

    bool response = false;
    RequestInformation(
        [=]()
        {
            m_pMSG->trackMethodRequest.type = MSG_TrackMethod; 
            m_pMSG->trackMethodRequest.functionToken = functionToken;
            wcscpy_s(m_pMSG->trackMethodRequest.szModulePath, pModulePath);
            wcscpy_s(m_pMSG->trackMethodRequest.szAssemblyName, pAssemblyName);
        }, 
        [=, &response, &uniqueId]()->BOOL
        {
            response =  m_pMSG->trackMethodResponse.bResponse == TRUE;
            uniqueId = m_pMSG->trackMethodResponse.UniqueId;
            return FALSE;
        }
    );

    return response;
}

template<class BR, class PR>
void ProfilerCommunication::RequestInformation(BR buildRequest, PR processResults)
{
    buildRequest();

    m_eventProfilerRequestsInformation.SignalAndWait(m_eventInformationReadyForProfiler);
    m_eventInformationReadyForProfiler.Reset();

    BOOL hasMore = FALSE;
    do
    {
        hasMore = processResults();

        if (hasMore)
        {
            m_eventInformationReadByProfiler.SignalAndWait(m_eventInformationReadyForProfiler);
            m_eventInformationReadyForProfiler.Reset();
        }
    }while (hasMore);

    m_eventInformationReadByProfiler.Set();
}
