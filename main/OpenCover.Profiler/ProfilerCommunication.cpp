//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#include "StdAfx.h"
#include "ProfilerCommunication.h"
#include "ReleaseTrace.h"

//#include <concrt.h>
//#include <TlHelp32.h>

#include <sstream>

#define ONERROR_GOEXIT(hr) if (FAILED(hr)) goto Exit
#define COMM_WAIT_SHORT 10000
#define COMM_WAIT_LONG 60000
#define COMM_WAIT_VSHORT 3000

ProfilerCommunication::ProfilerCommunication() 
{
    m_bufferId = 0;
    m_pMSG = nullptr;
    m_pVisitPoints = nullptr;
    hostCommunicationActive = false;
}

ProfilerCommunication::~ProfilerCommunication()
{
}

bool ProfilerCommunication::Initialise(TCHAR *key, TCHAR *ns, TCHAR *processName)
{
	m_key = key;
    m_processName = processName;

	std::wstring sharedKey = key;
	sharedKey.append(_T("-1"));

    m_namespace = ns;

    m_mutexCommunication.Initialise((m_namespace + _T("\\OpenCover_Profiler_Communication_Mutex_") + m_key).c_str());
    if (!m_mutexCommunication.IsValid()) return false;
    
    USES_CONVERSION;
    ATLTRACE(_T("ProfilerCommunication::Initialise(...) => Initialised mutexes => %s"), W2CT(sharedKey.c_str()));

    auto resource_name = (m_namespace + _T("\\OpenCover_Profiler_Communication_SendData_Event_") + sharedKey);
    m_eventProfilerRequestsInformation.Initialise(resource_name.c_str());
    if (!m_eventProfilerRequestsInformation.IsValid()) {
        RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
        return false;
    }

    resource_name = (m_namespace + _T("\\OpenCover_Profiler_Communication_ChunkData_Event_") + sharedKey);
    m_eventInformationReadByProfiler.Initialise(resource_name.c_str());
    if (!m_eventInformationReadByProfiler.IsValid()) {
        RELTRACE(_T("ProfilerCommunication::Initialise(...) = >Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
        return false;
    }

    resource_name = (m_namespace + _T("\\OpenCover_Profiler_Communication_ReceiveData_Event_") + sharedKey);
    m_eventInformationReadyForProfiler.Initialise(resource_name.c_str());
    if (!m_eventInformationReadyForProfiler.IsValid()) {
        RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
        return false;
    }

    resource_name = (m_namespace + _T("\\OpenCover_Profiler_Communication_MemoryMapFile_") + sharedKey);
    m_memoryCommunication.OpenFileMapping(resource_name.c_str());
    if (!m_memoryCommunication.IsValid()) {
        RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
        return false;
    }

    resource_name = (m_namespace + _T("\\OpenCover_Profiler_Communication_Semaphore_") + sharedKey);
    _semapore_communication.Initialise(resource_name.c_str());
    if (!_semapore_communication.IsValid()) {
        RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
        return false;
    }
    m_pMSG = (MSG_Union*)m_memoryCommunication.MapViewOfFile(0, 0, MAX_MSG_SIZE);

    hostCommunicationActive = true;

    ATLTRACE(_T("ProfilerCommunication::Initialise(...) => Initialised communication interface => %s"), W2CT(sharedKey.c_str()));

    if (!TrackProcess()){
        RELTRACE(_T("ProfilerCommunication::Initialise(...) => ProfilerCommunication => process is not be tracked"));
        return false;
    }

    ULONG bufferId =  0;
    if (AllocateBuffer(MAX_MSG_SIZE, bufferId))
    {
        std::wstring memoryKey;
        std::wstringstream stream ;
        stream << bufferId;
        stream >> memoryKey;

        m_bufferId = bufferId;

        memoryKey = m_key + memoryKey;

        ATLTRACE(_T("ProfilerCommunication::Initialise(...) => Re-initialising communication interface => %s"), W2CT(memoryKey.c_str()));

        resource_name = (m_namespace + _T("\\OpenCover_Profiler_Communication_SendData_Event_") + memoryKey);
        m_eventProfilerRequestsInformation.Initialise(resource_name.c_str());
        if (!m_eventProfilerRequestsInformation.IsValid()) {
            RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
            hostCommunicationActive = false;
            return false;
        }

        resource_name = (m_namespace + _T("\\OpenCover_Profiler_Communication_ChunkData_Event_") + memoryKey);
        m_eventInformationReadByProfiler.Initialise(resource_name.c_str());
        if (!m_eventInformationReadByProfiler.IsValid()) {
            RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
            hostCommunicationActive = false;
            return false;
        }

        resource_name = (m_namespace + _T("\\OpenCover_Profiler_Communication_ReceiveData_Event_") + memoryKey);
        m_eventInformationReadyForProfiler.Initialise(resource_name.c_str());
        if (!m_eventInformationReadyForProfiler.IsValid()) {
            RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
            hostCommunicationActive = false;
            return false;
        }

        resource_name = (m_namespace + _T("\\OpenCover_Profiler_Communication_MemoryMapFile_") + memoryKey);
        m_memoryCommunication.OpenFileMapping(resource_name.c_str());
        if (!m_memoryCommunication.IsValid()) {
            RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
            hostCommunicationActive = false;
            return false;
        }

        m_pMSG = (MSG_Union*)m_memoryCommunication.MapViewOfFile(0, 0, MAX_MSG_SIZE);

        resource_name = (m_namespace + _T("\\OpenCover_Profiler_Communication_Semaphore_") + memoryKey);
        _semapore_communication.Initialise(resource_name.c_str());
        if (!_semapore_communication.IsValid()) {
            RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
            hostCommunicationActive = false;
            return false;
        }

        ATLTRACE(_T("ProfilerCommunication::Initialise(...) => Re-initialised communication interface => %s"), W2CT(memoryKey.c_str()));

        resource_name = (m_namespace + _T("\\OpenCover_Profiler_Results_SendResults_Event_") + memoryKey);
        m_eventProfilerHasResults.Initialise(resource_name.c_str());
        if (!m_eventProfilerHasResults.IsValid()) {
            RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
            hostCommunicationActive = false;
            return false;
        }

        resource_name = (m_namespace + _T("\\OpenCover_Profiler_Results_ReceiveResults_Event_") + memoryKey);
        m_eventResultsHaveBeenReceived.Initialise(resource_name.c_str());
        if (!m_eventResultsHaveBeenReceived.IsValid()) {
            RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
            hostCommunicationActive = false;
            return false;
        }

        resource_name = (m_namespace + _T("\\OpenCover_Profiler_Results_MemoryMapFile_") + memoryKey);
        m_memoryResults.OpenFileMapping(resource_name.c_str());
        if (!m_memoryResults.IsValid()) {
            RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
            hostCommunicationActive = false;
            return false;
        }

        m_pVisitPoints = (MSG_SendVisitPoints_Request*)m_memoryResults.MapViewOfFile(0, 0, MAX_MSG_SIZE);

        m_pVisitPoints->count = 0;

        resource_name = (m_namespace + _T("\\OpenCover_Profiler_Results_Semaphore_") + memoryKey);
        _semapore_results.Initialise(resource_name.c_str());
        if (!_semapore_results.IsValid()) {
            RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
            hostCommunicationActive = false;
            return false;
        }

        RELTRACE(_T("ProfilerCommunication::Initialise(...) => Initialised results interface => %s"), W2CT(memoryKey.c_str()));
    }
    else {
        hostCommunicationActive = false;
    }

    return hostCommunicationActive;
}

void ProfilerCommunication::ThreadCreated(ThreadID threadID, DWORD osThreadID){
    ATL::CComCritSecLock<ATL::CComAutoCriticalSection> lock(m_critThreads);
    m_threadmap[threadID] = osThreadID;
    AllocateVisitMap(osThreadID);
}

MSG_SendVisitPoints_Request* ProfilerCommunication::AllocateVisitMap(DWORD osThreadID){
    auto p = new MSG_SendVisitPoints_Request();
    p->count = 0;
    //::ZeroMemory(p, sizeof(MSG_SendVisitPoints_Request));
    m_visitmap[osThreadID] = p;
    return p;
}

MSG_SendVisitPoints_Request* ProfilerCommunication::GetVisitMapForOSThread(ULONG osThreadID){
    MSG_SendVisitPoints_Request * p;
    try {
        p = m_visitmap[osThreadID];
        if (p == nullptr)
            p = AllocateVisitMap(osThreadID);
    }
    catch (...){
        p = AllocateVisitMap(osThreadID);
    }
    return p;
}

void ProfilerCommunication::ThreadDestroyed(ThreadID threadID){
    ATL::CComCritSecLock<ATL::CComAutoCriticalSection> lock(m_critThreads);
    ULONG osThreadId = m_threadmap[threadID];
    auto points = m_visitmap[osThreadId];
    SendThreadVisitPoints(points);
    delete m_visitmap[osThreadId];
    m_visitmap[osThreadId] = NULL;
}

void ProfilerCommunication::SendRemainingThreadBuffers(){
    ATL::CComCritSecLock<ATL::CComAutoCriticalSection> lock(m_critThreads);
    for (auto it = m_visitmap.begin(); it != m_visitmap.end(); ++it){
        if (it->second != NULL){
            SendThreadVisitPoints(it->second);
            //::ZeroMemory(pVisitPoints, sizeof(MSG_SendVisitPoints_Request));        
        }
    }
}

void ProfilerCommunication::AddVisitPointToThreadBuffer(ULONG uniqueId, MSG_IdType msgType)
{
    DWORD osThreadId = ::GetCurrentThreadId();
    auto pVisitPoints = GetVisitMapForOSThread(osThreadId);
    pVisitPoints->points[pVisitPoints->count].UniqueId = (uniqueId | msgType);
    if (++pVisitPoints->count == VP_BUFFER_SIZE)
    {
        SendThreadVisitPoints(pVisitPoints);
        //::ZeroMemory(pVisitPoints, sizeof(MSG_SendVisitPoints_Request));        
    }
}

void ProfilerCommunication::SendThreadVisitPoints(MSG_SendVisitPoints_Request* pVisitPoints){
    ATL::CComCritSecLock<ATL::CComAutoCriticalSection> lock(m_critResults);

    if (!hostCommunicationActive)
        return;

    if (!TestSemaphore(_semapore_results))
        return;

    handle_exception([=](){
        memcpy(m_pVisitPoints, pVisitPoints, sizeof(MSG_SendVisitPoints_Request));
    }, _T("SendThreadVisitPoints"));

    pVisitPoints->count = 0;
    SendVisitPoints();
    //::ZeroMemory(m_pVisitPoints, sizeof(MSG_SendVisitPoints_Request));
    m_pVisitPoints->count = 0;
}

void ProfilerCommunication::AddVisitPointToBuffer(ULONG uniqueId, MSG_IdType msgType)
{
	ATL::CComCritSecLock<ATL::CComAutoCriticalSection> lock(m_critResults);
    
    if (!hostCommunicationActive) 
        return;
    
    if (!TestSemaphore(_semapore_results))
        return;

    handle_exception([=](){
        m_pVisitPoints->points[m_pVisitPoints->count].UniqueId = (uniqueId | msgType);
    }, _T("AddVisitPointToBuffer"));

    if (++m_pVisitPoints->count == VP_BUFFER_SIZE)
    {
        SendVisitPoints();
        //::ZeroMemory(m_pVisitPoints, sizeof(MSG_SendVisitPoints_Request));
        handle_exception([=](){
            m_pVisitPoints->count = 0;
        }, _T("AddVisitPointToBuffer"));
    }
}

void ProfilerCommunication::SendVisitPoints()
{
    if (!hostCommunicationActive) 
        return;
    try {
        m_memoryResults.FlushViewOfFile();

        DWORD dwSignal = m_eventProfilerHasResults.SignalAndWait(m_eventResultsHaveBeenReceived, COMM_WAIT_SHORT);
        if (WAIT_OBJECT_0 != dwSignal) throw CommunicationException(dwSignal, COMM_WAIT_SHORT);
        m_eventResultsHaveBeenReceived.Reset();
    } catch (CommunicationException ex) {
        RELTRACE(_T("ProfilerCommunication::SendVisitPoints() => Communication (Results channel) with host has failed (0x%x, %d)"), 
			ex.getReason(), ex.getTimeout());
        hostCommunicationActive = false;
    }
    return;
}

bool ProfilerCommunication::GetPoints(mdToken functionToken, WCHAR* pModulePath, 
    WCHAR* pAssemblyName, std::vector<SequencePoint> &seqPoints, std::vector<BranchPoint> &brPoints)
{
    seqPoints.clear();
    brPoints.clear();
    bool ret = GetSequencePoints(functionToken, pModulePath, pAssemblyName, seqPoints);
    
    if (ret){
        GetBranchPoints(functionToken, pModulePath, pAssemblyName, brPoints);
    }

    return ret;
}

bool ProfilerCommunication::GetSequencePoints(mdToken functionToken, WCHAR* pModulePath,  
    WCHAR* pAssemblyName, std::vector<SequencePoint> &points)
{
    if (!hostCommunicationActive) 
        return false;

    RequestInformation(
        [=]
        {
            m_pMSG->getSequencePointsRequest.type = MSG_GetSequencePoints;
            m_pMSG->getSequencePointsRequest.functionToken = functionToken;
            USES_CONVERSION;
            wcscpy_s(m_pMSG->getSequencePointsRequest.szProcessName, T2CW(m_processName.c_str()));
            wcscpy_s(m_pMSG->getSequencePointsRequest.szModulePath, pModulePath);
            wcscpy_s(m_pMSG->getSequencePointsRequest.szAssemblyName, pAssemblyName);
        }, 
        [=, &points]()->BOOL
        {
            if (m_pMSG->getSequencePointsResponse.count > SEQ_BUFFER_SIZE){
                RELTRACE(_T("Received an abnormal count for sequence points (%d) for token 0x%X"),
                    m_pMSG->getSequencePointsResponse.count, functionToken);
                points.clear();
                return false;
            }

            for (int i = 0; i < m_pMSG->getSequencePointsResponse.count; i++)
                points.push_back(m_pMSG->getSequencePointsResponse.points[i]);
            BOOL hasMore = m_pMSG->getSequencePointsResponse.hasMore;
            ::ZeroMemory(m_pMSG, MAX_MSG_SIZE);
            return hasMore;
        }
        , COMM_WAIT_SHORT
        , _T("GetSequencePoints"));

    return (points.size() != 0);
}

bool ProfilerCommunication::GetBranchPoints(mdToken functionToken, WCHAR* pModulePath, 
    WCHAR* pAssemblyName, std::vector<BranchPoint> &points)
{
    if (!hostCommunicationActive) 
        return false;
 
    RequestInformation(
        [=]
        {
            m_pMSG->getBranchPointsRequest.type = MSG_GetBranchPoints;
            m_pMSG->getBranchPointsRequest.functionToken = functionToken;
            USES_CONVERSION;
            wcscpy_s(m_pMSG->getBranchPointsRequest.szProcessName, T2CW(m_processName.c_str()));
            wcscpy_s(m_pMSG->getBranchPointsRequest.szModulePath, pModulePath);
            wcscpy_s(m_pMSG->getBranchPointsRequest.szAssemblyName, pAssemblyName);
        }, 
        [=, &points]()->BOOL
        {
            if (m_pMSG->getBranchPointsResponse.count > BRANCH_BUFFER_SIZE){
                RELTRACE(_T("Received an abnormal count for branch points (%d) for token 0x%X"),
                    m_pMSG->getBranchPointsResponse.count, functionToken);
                points.clear();
                return false;
            }

            for (int i=0; i < m_pMSG->getBranchPointsResponse.count;i++)
                points.push_back(m_pMSG->getBranchPointsResponse.points[i]); 
            BOOL hasMore = m_pMSG->getBranchPointsResponse.hasMore;
 		    ::ZeroMemory(m_pMSG, MAX_MSG_SIZE);
			return hasMore;
        }
        , COMM_WAIT_SHORT
        , _T("GetBranchPoints"));

    return (points.size() != 0);
}

bool ProfilerCommunication::TrackAssembly(WCHAR* pModulePath, WCHAR* pAssemblyName)
{
    if (!hostCommunicationActive) 
        return false;

    bool response = false;
    RequestInformation(
        [=]()
        {
            m_pMSG->trackAssemblyRequest.type = MSG_TrackAssembly; 
            USES_CONVERSION;
            wcscpy_s(m_pMSG->trackAssemblyRequest.szProcessName, T2CW(m_processName.c_str()));
            wcscpy_s(m_pMSG->trackAssemblyRequest.szModulePath, pModulePath);
            wcscpy_s(m_pMSG->trackAssemblyRequest.szAssemblyName, pAssemblyName);
        }, 
        [=, &response]()->BOOL
        {
            response =  m_pMSG->trackAssemblyResponse.bResponse == TRUE;
			::ZeroMemory(m_pMSG, MAX_MSG_SIZE);
            return FALSE;
        }
        , COMM_WAIT_LONG
        , _T("TrackAssembly"));

    return response;
}

bool ProfilerCommunication::TrackMethod(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, ULONG &uniqueId)
{
    if (!hostCommunicationActive) 
        return false;

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
            uniqueId = m_pMSG->trackMethodResponse.ulUniqueId;
			::ZeroMemory(m_pMSG, MAX_MSG_SIZE);
            return FALSE;
        }
        , COMM_WAIT_SHORT
        , _T("TrackMethod"));

    return response;
}

bool ProfilerCommunication::AllocateBuffer(LONG bufferSize, ULONG &bufferId)
{
    CScopedLock<CMutex> lock(m_mutexCommunication);
    
    if (!hostCommunicationActive) 
        return false;

    bool response = false;
    int repeat = 0;
    while (!response && (++repeat <= 3)){
        hostCommunicationActive = true;
        RequestInformation(
            [=]()
            {
                m_pMSG->allocateBufferRequest.type = MSG_AllocateMemoryBuffer; 
                m_pMSG->allocateBufferRequest.lBufferSize = bufferSize;
            }, 
            [=, &response, &bufferId]()->BOOL
            {
                response =  m_pMSG->allocateBufferResponse.bResponse == TRUE;
                bufferId = m_pMSG->allocateBufferResponse.ulBufferId;
			    ::ZeroMemory(m_pMSG, MAX_MSG_SIZE);
                return FALSE;
            }
            , COMM_WAIT_VSHORT
            , _T("AllocateBuffer"));
    }

    return response;
}

void ProfilerCommunication::CloseChannel(bool sendSingleBuffer){
    if (m_bufferId == 0) 
        return;

    if (!hostCommunicationActive)
        return;

    if (!TestSemaphore(_semapore_results))
        return;

    if (sendSingleBuffer)
        SendVisitPoints();
    else
        SendRemainingThreadBuffers();

    if (!hostCommunicationActive)
        return;

    bool response = false;

    RequestInformation(
        [=]()
        {
            m_pMSG->closeChannelRequest.type = MSG_CloseChannel;
            m_pMSG->closeChannelRequest.ulBufferId = m_bufferId;
        },
        [=, &response]()->BOOL
        {
            response = m_pMSG->allocateBufferResponse.bResponse == TRUE;
            return FALSE;
        }
        , COMM_WAIT_SHORT
        , _T("CloseChannel"));

    return;
}

bool ProfilerCommunication::TrackProcess(){
    CScopedLock<CMutex> lock(m_mutexCommunication);

    if (!hostCommunicationActive)
        return false;

    bool response = false;

    RequestInformation(
        [=]()
        {
            m_pMSG->trackProcessRequest.type = MSG_TrackProcess;
            USES_CONVERSION;
            wcscpy_s(m_pMSG->trackProcessRequest.szProcessName, T2CW(m_processName.c_str()));
        },
        [=, &response]()->BOOL
        {
            response = m_pMSG->trackProcessResponse.bResponse == TRUE;
            return FALSE;
        }
        , COMM_WAIT_SHORT
        , _T("TrackProcess"));

    return response;
}

void ProfilerCommunication::report_runtime(const std::runtime_error& re, const tstring &msg){
    USES_CONVERSION;
    RELTRACE(_T("Runtime error: %s - %s"), msg.c_str(), A2T(re.what()));
}

void ProfilerCommunication::report_exception(const std::exception& re, const tstring &msg){
    USES_CONVERSION;
    RELTRACE(_T("Error occurred: %s - %s"), msg.c_str(), A2T(re.what()));
}

template<class Action>
void ProfilerCommunication::handle_sehexception(Action action, const tstring& message) {
    __try{
        action();
    }
    __except (GetExceptionCode() == EXCEPTION_IN_PAGE_ERROR ? EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH)
    {
        RELTRACE(_T("SEH exception failure occured: %s - %d"),
            message.c_str(), GetExceptionCode());
    }
}

template<class Action>
void ProfilerCommunication::handle_exception(Action action, const tstring& message) {
    try
    {
        handle_sehexception([&](){
            action();
        }, message);
    }
    catch (const std::runtime_error& re)
    {
        // specific handling for runtime_error
        report_runtime(re, message);
        throw;
    }
    catch (const std::exception& ex)
    {
        // specific handling for all exceptions extending std::exception, except
        // std::runtime_error which is handled explicitly
        report_exception(ex, message);
        throw;
    }
    catch (...)
    {
        // catch any other errors (that we have no information about)
        RELTRACE(_T("Unknown failure occured. Possible memory corruption - %s"), message.c_str());
        throw;
    }
}

template<class BR, class PR>
void ProfilerCommunication::RequestInformation(BR buildRequest, PR processResults, DWORD dwTimeout, tstring message)
{
	ATL::CComCritSecLock<ATL::CComAutoCriticalSection> lock(m_critComms);
    if (!hostCommunicationActive) 
        return;

    if (!TestSemaphore(_semapore_communication))
        return;

	try {

        handle_exception([&](){ buildRequest(); }, message);
        
        m_memoryCommunication.FlushViewOfFile();

        DWORD dwSignal = m_eventProfilerRequestsInformation.SignalAndWait(m_eventInformationReadyForProfiler, dwTimeout);
		if (WAIT_OBJECT_0 != dwSignal) throw CommunicationException(dwSignal, dwTimeout);
    
        m_eventInformationReadyForProfiler.Reset();

        BOOL hasMore = FALSE;
        do
        {
            handle_exception([&](){ hasMore = processResults(); }, message);

            if (hasMore)
            {
                dwSignal = m_eventInformationReadByProfiler.SignalAndWait(m_eventInformationReadyForProfiler, COMM_WAIT_SHORT);
                if (WAIT_OBJECT_0 != dwSignal) throw CommunicationException(dwSignal, COMM_WAIT_SHORT);
            
                m_eventInformationReadyForProfiler.Reset();
            }
        }while (hasMore);

        m_eventInformationReadByProfiler.Set();
    } catch (CommunicationException ex) {
        RELTRACE(_T("ProfilerCommunication::RequestInformation(...) => Communication (Chat channel - %s) with host has failed (0x%x, %d)"),  
			message.c_str(), ex.getReason(), ex.getTimeout());
        hostCommunicationActive = false;
    } 
    catch (...)
    {
        hostCommunicationActive = false;
    }
}
