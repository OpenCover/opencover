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
    m_pMSG->trackRequest.nModuleName = wcslen(pModuleName);
    wcscpy(m_pMSG->trackRequest.szModuleName, pModuleName);
    m_pMSG->trackRequest.nAssemblyName = wcslen(pAssemblyName);
    wcscpy(m_pMSG->trackRequest.szAssemblyName, pAssemblyName);


    m_eventSendData.Set();
    m_eventReceiveData.Wait();

    return m_pMSG->trackResponse.bResponse;
}

BOOL ProfilerCommunication::GetSequencePoints(mdToken functionToken, WCHAR* pModuleName, unsigned int* pNumPoints, SequencePoint*** pppInstrumentPoints)
{
    CScopedLock<CMutex> lock(m_mutexCommunication);
    return false;
}

void ProfilerCommunication::SendVisitPoints(unsigned int numPoints, VisitPoint **ppPoints)
{
    CScopedLock<CMutex> lock(m_mutexResults);
    return;
}


