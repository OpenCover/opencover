//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#include "StdAfx.h"
#include "ProfilerCommunication.h"

#define ONERROR_GOEXIT(hr) if (FAILED(hr)) goto Exit
#define MAX_MSG_SIZE 65536

ProfilerCommunication::ProfilerCommunication() 
{
}

ProfilerCommunication::~ProfilerCommunication()
{
}

void ProfilerCommunication::Initialise(TCHAR *key)
{
    m_key = key;

    m_mutexCommunication.Initialise((_T("Local\\OpenCover_Profiler_Communication_Mutex_") + m_key).c_str());
    m_mutexResults.Initialise((_T("Local\\OpenCover_Profiler_Results_Mutex_") + m_key).c_str());

    m_eventSendData.Initialise((_T("Local\\OpenCover_Profiler_Communication_SendData_Event_") + m_key).c_str());
    m_eventReceiveData.Initialise((_T("Local\\OpenCover_Profiler_Communication_ReceiveData_Event_") + m_key).c_str());

    m_eventSendResults.Initialise((_T("Local\\OpenCover_Profiler_Communication_SendResults_Event_") + m_key).c_str());
    m_eventReceiveResults.Initialise((_T("Local\\OpenCover_Profiler_Communication_ReceiveResults_Event_") + m_key).c_str());

    m_memoryCommunication.OpenFileMapping((_T("Local\\OpenCover_Profiler_Communication_MemoryMapFile_") + m_key).c_str());
    m_memoryResults.OpenFileMapping((_T("Local\\OpenCover_Profiler_Results_MemoryMapFile_") + m_key).c_str());

    m_pMSG = (MSG_Union*)m_memoryCommunication.MapViewOfFile(0, 0, MAX_MSG_SIZE);
    m_pVisitPoints = (MSG_SendVisitPoints_Request*)m_memoryResults.MapViewOfFile(0, 0, MAX_MSG_SIZE);
}

BOOL ProfilerCommunication::TrackAssembly(WCHAR* pModulePath, WCHAR* pAssemblyName)
{
    CScopedLock<CMutex> lock(m_mutexCommunication);
    m_eventReceiveData.Reset();

    m_pMSG->trackRequest.type = MSG_TrackAssembly; 
    wcscpy_s(m_pMSG->trackRequest.szModulePath, pModulePath);
    wcscpy_s(m_pMSG->trackRequest.szAssemblyName, pAssemblyName);

    m_eventSendData.SignalAndWait(m_eventReceiveData);
    bool response =  m_pMSG->trackResponse.bResponse;
    m_eventReceiveData.Reset();
    return response;
}

BOOL ProfilerCommunication::GetSequencePoints(mdToken functionToken, WCHAR* pModulePath,  WCHAR* pAssemblyName, std::vector<SequencePoint> &points)
{
    CScopedLock<CMutex> lock(m_mutexCommunication);
    m_eventReceiveData.Reset();

    m_pMSG->getSequencePointsRequest.type = MSG_GetSequencePoints;
    m_pMSG->getSequencePointsRequest.functionToken = functionToken;
    wcscpy_s(m_pMSG->getSequencePointsRequest.szModulePath, pModulePath);
    wcscpy_s(m_pMSG->getSequencePointsRequest.szAssemblyName, pAssemblyName);

    m_eventSendData.SignalAndWait(m_eventReceiveData);
    m_eventReceiveData.Reset();

    BOOL hasMore = FALSE;
    do
    {
        for (int i=0; i < m_pMSG->getSequencePointsResponse.count;i++)
        {
            points.push_back(m_pMSG->getSequencePointsResponse.points[i]); 
        }

        hasMore = m_pMSG->getSequencePointsResponse.hasMore;
        if (hasMore)
        {
            m_eventSendData.SignalAndWait(m_eventReceiveData);
            m_eventReceiveData.Reset();
        }
    }while (hasMore);
    
    return (points.size() != 0);
}

void ProfilerCommunication::AddVisitPoint(ULONG uniqueId)
{
    CScopedLock<CMutex> lock(m_mutexResults);
    m_pVisitPoints->points[m_pVisitPoints->count].UniqueId = uniqueId;
    if (++m_pVisitPoints->count == VP_BUFFER_SIZE)
    {
        SendVisitPoints();
        m_pVisitPoints->count=0;
    }
}

void ProfilerCommunication::SendVisitPoints()
{
    m_eventReceiveResults.Reset();
    m_eventSendResults.SignalAndWait(m_eventReceiveResults, 5000);
    m_eventReceiveResults.Reset();
    return;
}


