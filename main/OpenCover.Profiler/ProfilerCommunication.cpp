#include "StdAfx.h"
#include "ProfilerCommunication.h"

#define ONERROR_GOEXIT(hr) if (FAILED(hr)) goto Exit

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

    m_memoryCommunication.OpenFileMapping((_T("Local\\OpenCover_Profiler_Communication_MemoryMapFile_") + m_key).c_str());

    m_pMSG = (MSG_Union*)m_memoryCommunication.MapViewOfFile(0, 0, 4096);

}

BOOL ProfilerCommunication::TrackAssembly(WCHAR* pModuleName, WCHAR* pAssemblyName)
{
    CScopedLock<CMutex> lock(m_mutexCommunication);

    ::ZeroMemory(m_pMSG, 4096);    
    m_pMSG->trackRequest.type = MSG_TrackAssembly; 
    wcscpy(m_pMSG->trackRequest.szModuleName, pModuleName);
    wcscpy(m_pMSG->trackRequest.szAssemblyName, pAssemblyName);

    m_eventSendData.Set();
    m_eventReceiveData.Wait();

    return m_pMSG->trackResponse.bResponse;
}

BOOL ProfilerCommunication::GetSequencePoints(mdToken functionToken, WCHAR* pModuleName, std::vector<SequencePoint> &points)
{
    CScopedLock<CMutex> lock(m_mutexCommunication);

    ::ZeroMemory(m_pMSG, 4096); 
    m_pMSG->getSequencePointsRequest.type = MSG_GetSequencePoints;
    m_pMSG->getSequencePointsRequest.functionToken = functionToken;
    wcscpy(m_pMSG->getSequencePointsRequest.szModuleName, pModuleName);

    m_eventSendData.Set();
    m_eventReceiveData.Wait();

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
            m_eventSendData.Set();
            m_eventReceiveData.Wait();
        }
    }while (hasMore);
    
    return (points.size() != 0);
}

void ProfilerCommunication::SendVisitPoints(unsigned int numPoints, VisitPoint **ppPoints)
{
    CScopedLock<CMutex> lock(m_mutexResults);
    return;
}


