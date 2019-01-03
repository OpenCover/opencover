//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#include "StdAfx.h"
#include "ProfilerCommunication.h"

//#include <concrt.h>
//#include <TlHelp32.h>

#include <sstream>

#define ONERROR_GOEXIT(hr) if (FAILED(hr)) goto Exit
#define COM_WAIT_LONG 60000
#define COM_WAIT_VSHORT 3000

#define MSG_UNION_SIZE sizeof(MSG_Union)
namespace Communication
{
	ProfilerCommunication::ProfilerCommunication(DWORD short_wait, DWORD version_high, DWORD version_low)
	{
		_bufferId = 0;
		_pMSG = nullptr;
		_pVisitPoints = nullptr;
		_hostCommunicationActive = false;
		_short_wait = short_wait;
		_version_high = version_high;
		_version_low = version_low;

		ATLASSERT(MAX_MSG_SIZE >= sizeof(MSG_Union));
		ATLASSERT(MAX_MSG_SIZE >= sizeof(MSG_SendVisitPoints_Request));
		ATLTRACE(_T("Buffer %d, Union %ld, Visit %ld"), MAX_MSG_SIZE, sizeof(MSG_Union), sizeof(MSG_SendVisitPoints_Request));
	}

	bool ProfilerCommunication::InitializePrimarySynchronization(std::wstring sharedKey, std::basic_string<wchar_t>& resource_name)
	{
		_mutexCommunication.Initialise((_namespace + _T("\\OpenCover_Profiler_Communication_Mutex_") + _key).c_str());
		if (!_mutexCommunication.IsValid())
			return false;

		USES_CONVERSION;
		ATLTRACE(_T("ProfilerCommunication::Initialise(...) => Initialised mutexes => %s"), W2CT(sharedKey.c_str()));

		resource_name = (_namespace + _T("\\OpenCover_Profiler_Communication_SendData_Event_") + sharedKey);
		_eventProfilerRequestsInformation.Initialise(resource_name.c_str());
		if (!_eventProfilerRequestsInformation.IsValid()) {
			RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
			return false;
		}

		resource_name = (_namespace + _T("\\OpenCover_Profiler_Communication_ChunkData_Event_") + sharedKey);
		_eventInformationReadByProfiler.Initialise(resource_name.c_str());
		if (!_eventInformationReadByProfiler.IsValid()) {
			RELTRACE(_T("ProfilerCommunication::Initialise(...) = >Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
			return false;
		}

		resource_name = (_namespace + _T("\\OpenCover_Profiler_Communication_ReceiveData_Event_") + sharedKey);
		_eventInformationReadyForProfiler.Initialise(resource_name.c_str());
		if (!_eventInformationReadyForProfiler.IsValid()) {
			RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
			return false;
		}

		resource_name = (_namespace + _T("\\OpenCover_Profiler_Communication_MemoryMapFile_") + sharedKey);
		_memoryCommunication.OpenFileMapping(resource_name.c_str());
		if (!_memoryCommunication.IsValid()) {
			RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
			return false;
		}

		resource_name = (_namespace + _T("\\OpenCover_Profiler_Communication_Semaphore_") + sharedKey);
		_semapore_communication.Initialise(resource_name.c_str());
		if (!_semapore_communication.IsValid()) {
			RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
			return false;
		}

		_pMSG = static_cast<MSG_Union*>(_memoryCommunication.MapViewOfFile(0, 0, MAX_MSG_SIZE));

		return true;
	}

	bool ProfilerCommunication::InitializeBufferSynchronization(std::basic_string<wchar_t>& resource_name)
	{
		ULONG bufferId = 0;
		if (AllocateBuffer(MAX_MSG_SIZE, bufferId))
		{
			std::wstring memoryKey;
			std::wstringstream stream;
			stream << bufferId;
			stream >> memoryKey;

			_bufferId = bufferId;

			memoryKey = _key + memoryKey;

			ATLTRACE(_T("ProfilerCommunication::Initialise(...) => Re-initialising communication interface => %s"), W2CT(memoryKey.c_str()));

			resource_name = (_namespace + _T("\\OpenCover_Profiler_Communication_SendData_Event_") + memoryKey);
			_eventProfilerRequestsInformation.Initialise(resource_name.c_str());
			if (!_eventProfilerRequestsInformation.IsValid()) {
				RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
				_hostCommunicationActive = false;
				return false;
			}

			resource_name = (_namespace + _T("\\OpenCover_Profiler_Communication_ChunkData_Event_") + memoryKey);
			_eventInformationReadByProfiler.Initialise(resource_name.c_str());
			if (!_eventInformationReadByProfiler.IsValid()) {
				RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
				_hostCommunicationActive = false;
				return false;
			}

			resource_name = (_namespace + _T("\\OpenCover_Profiler_Communication_ReceiveData_Event_") + memoryKey);
			_eventInformationReadyForProfiler.Initialise(resource_name.c_str());
			if (!_eventInformationReadyForProfiler.IsValid()) {
				RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
				_hostCommunicationActive = false;
				return false;
			}

			resource_name = (_namespace + _T("\\OpenCover_Profiler_Communication_MemoryMapFile_") + memoryKey);
			_memoryCommunication.OpenFileMapping(resource_name.c_str());
			if (!_memoryCommunication.IsValid()) {
				RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
				_hostCommunicationActive = false;
				return false;
			}

			_pMSG = static_cast<MSG_Union*>(_memoryCommunication.MapViewOfFile(0, 0, MAX_MSG_SIZE));

			resource_name = (_namespace + _T("\\OpenCover_Profiler_Communication_Semaphore_") + memoryKey);
			_semapore_communication.Initialise(resource_name.c_str());
			if (!_semapore_communication.IsValid()) {
				RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
				_hostCommunicationActive = false;
				return false;
			}

			ATLTRACE(_T("ProfilerCommunication::Initialise(...) => Re-initialised communication interface => %s"), W2CT(memoryKey.c_str()));

			resource_name = (_namespace + _T("\\OpenCover_Profiler_Results_SendResults_Event_") + memoryKey);
			_eventProfilerHasResults.Initialise(resource_name.c_str());
			if (!_eventProfilerHasResults.IsValid()) {
				RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
				_hostCommunicationActive = false;
				return false;
			}

			resource_name = (_namespace + _T("\\OpenCover_Profiler_Results_ReceiveResults_Event_") + memoryKey);
			_eventResultsHaveBeenReceived.Initialise(resource_name.c_str());
			if (!_eventResultsHaveBeenReceived.IsValid()) {
				RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
				_hostCommunicationActive = false;
				return false;
			}

			resource_name = (_namespace + _T("\\OpenCover_Profiler_Results_MemoryMapFile_") + memoryKey);
			_memoryResults.OpenFileMapping(resource_name.c_str());
			if (!_memoryResults.IsValid()) {
				RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
				_hostCommunicationActive = false;
				return false;
			}

			_pVisitPoints = static_cast<MSG_SendVisitPoints_Request*>(_memoryResults.MapViewOfFile(0, 0, MAX_MSG_SIZE));

			_pVisitPoints->count = 0;

			resource_name = (_namespace + _T("\\OpenCover_Profiler_Results_Semaphore_") + memoryKey);
			_semapore_results.Initialise(resource_name.c_str());
			if (!_semapore_results.IsValid()) {
				RELTRACE(_T("ProfilerCommunication::Initialise(...) => Failed to initialise resource %s => ::GetLastError() = %d"), W2CT(resource_name.c_str()), ::GetLastError());
				_hostCommunicationActive = false;
				return false;
			}

			RELTRACE(_T("ProfilerCommunication::Initialise(...) => Initialised results interface => %s"), W2CT(memoryKey.c_str()));
		}
		else {
			_hostCommunicationActive = false;
		}

		return true;
	}

	bool ProfilerCommunication::Initialise(
		TCHAR *key, TCHAR *ns, 
		TCHAR *processName, 
		bool safe_mode, int sendVisitPointsTimerInterval)
	{
		_key = key;
		_processName = processName;

		std::wstring sharedKey = key;
		sharedKey.append(_T("-1"));

		_namespace = ns;

		std::basic_string<wchar_t> resource_name;
		if (!InitializePrimarySynchronization(sharedKey, resource_name))
			return false;

		_hostCommunicationActive = true;

		ATLTRACE(_T("ProfilerCommunication::Initialise(...) => Initialised communication interface => %s"), W2CT(sharedKey.c_str()));

		if (!TrackProcess()) {
			RELTRACE(_T("ProfilerCommunication::Initialise(...) => ProfilerCommunication => process is not be tracked"));
			return false;
		}

		if (!InitializeBufferSynchronization(resource_name)) 
			return false;
		
		_sendTimer.Start([=]()
		{
			SendRemainingVisitPoints(safe_mode);
		}, sendVisitPointsTimerInterval);

		return _hostCommunicationActive;
	}

	void ProfilerCommunication::ThreadCreated(ThreadID threadID, DWORD osThreadID) {
		_threadmap[threadID] = osThreadID;
		AllocateVisitMap(osThreadID);
	}

	MSG_SendVisitPoints_Request* ProfilerCommunication::AllocateVisitMap(DWORD osThreadID) {
		ATL::CComCritSecLock<ATL::CComAutoCriticalSection> lock(_critThreads);
		auto it = _visitmap.find(osThreadID);
		if (it == _visitmap.end() || it->second == nullptr)
		{
			auto p = new MSG_SendVisitPoints_Request();
			p->count = 0;
			_visitmap[osThreadID] = p;
			return p;
		}
		return it->second;
	}

	MSG_SendVisitPoints_Request* ProfilerCommunication::GetVisitMapForOSThread(ULONG osThreadID) {
		auto it = _visitmap.find(osThreadID);
		if (it == _visitmap.end() || it->second == nullptr) {
			return AllocateVisitMap(osThreadID);
		}
		return it->second;
	}

	void ProfilerCommunication::ThreadDestroyed(ThreadID threadID) {
		ATL::CComCritSecLock<ATL::CComAutoCriticalSection> lock(_critThreads);
		auto osThreadId = _threadmap[threadID];
		auto points = _visitmap[osThreadId];
		SendThreadVisitPoints(points);
		delete _visitmap[osThreadId];
		_visitmap[osThreadId] = nullptr;
	}

	void ProfilerCommunication::SendRemainingThreadBuffers() {
		for (auto it = _visitmap.begin(); it != _visitmap.end(); ++it) {
			if (it->second != nullptr) {
				SendThreadVisitPoints(it->second);
			}
		}
	}

	void ProfilerCommunication::SendRemainingVisitPoints(bool safemode)
	{
		if (safemode)
			SendVisitPoints();
		else
			SendRemainingThreadBuffers();
	}

	void ProfilerCommunication::AddVisitPointToThreadBuffer(ULONG uniqueId, MSG_IdType msgType)
	{
		auto osThreadId = ::GetCurrentThreadId();
		auto pVisitPoints = GetVisitMapForOSThread(osThreadId);
		pVisitPoints->points[pVisitPoints->count].UniqueId = (uniqueId | msgType);
		if (++pVisitPoints->count == VP_BUFFER_SIZE)
		{
			SendThreadVisitPoints(pVisitPoints);
		}
	}

	void ProfilerCommunication::SendThreadVisitPoints(MSG_SendVisitPoints_Request* pVisitPoints) {
		ATL::CComCritSecLock<ATL::CComAutoCriticalSection> lock(_critResults);

		SendThreadVisitPointsInternal(pVisitPoints);

		pVisitPoints->count = 0;
	}

	void ProfilerCommunication::SendThreadVisitPointsInternal(MSG_SendVisitPoints_Request* pVisitPoints) {
		ATL::CComCritSecLock<ATL::CComAutoCriticalSection> lock(_critResults);

		if (!_hostCommunicationActive)
			return;

		if (!TestSemaphore(_semapore_results))
			return;

		handle_exception([=]() {
			memcpy(_pVisitPoints, pVisitPoints, sizeof(MSG_SendVisitPoints_Request));
		}, _T("SendThreadVisitPoints"));

		SendVisitPoints();
	}

	void ProfilerCommunication::AddVisitPointToBuffer(ULONG uniqueId, MSG_IdType msgType)
	{
		ATL::CComCritSecLock<ATL::CComAutoCriticalSection> lock(_critResults);

		if (!_hostCommunicationActive)
			return;

		if (!TestSemaphore(_semapore_results))
			return;

		handle_exception([=]() {
			_pVisitPoints->points[_pVisitPoints->count].UniqueId = (uniqueId | msgType);
		}, _T("AddVisitPointToBuffer"));

		if (++_pVisitPoints->count == VP_BUFFER_SIZE)
		{
			SendVisitPoints();
		}
	}

	void ProfilerCommunication::SendVisitPoints()
	{
		SendVisitPointsInternal();
		handle_exception([=]() {
			_pVisitPoints->count = 0;
		}, _T("SendVisitPoints"));
	}

	void ProfilerCommunication::SendVisitPointsInternal() {
		if (!_hostCommunicationActive)
			return;
		try {
			ATLTRACE("ProfilerCommunication : Flushing visit points to host\n");

			_memoryResults.FlushViewOfFile();

			DWORD dwSignal = _eventProfilerHasResults.SignalAndWait(_eventResultsHaveBeenReceived, _short_wait);
			if (WAIT_OBJECT_0 != dwSignal)
				throw CommunicationException(dwSignal, _short_wait);
			_eventResultsHaveBeenReceived.Reset();
		}
		catch (const CommunicationException& ex) {
			RELTRACE(_T("ProfilerCommunication::SendVisitPoints() => Communication (Results channel) with host has failed (0x%x, %d)"),
				ex.getReason(), ex.getTimeout());
			_hostCommunicationActive = false;
		}
		return;
	}

	bool ProfilerCommunication::GetPoints(mdToken functionToken, WCHAR* pModulePath,
		WCHAR* pAssemblyName, std::vector<SequencePoint> &seqPoints, std::vector<BranchPoint> &brPoints)
	{
		seqPoints.clear();
		brPoints.clear();
		bool ret = GetSequencePoints(functionToken, pModulePath, pAssemblyName, seqPoints);

		if (ret) {
			GetBranchPoints(functionToken, pModulePath, pAssemblyName, brPoints);
		}

		return ret;
	}

	bool ProfilerCommunication::GetSequencePoints(mdToken functionToken, WCHAR* pModulePath,
		WCHAR* pAssemblyName, std::vector<SequencePoint> &points)
	{
		if (!_hostCommunicationActive)
			return false;

		RequestInformation(
			[=]
		{
			_pMSG->getSequencePointsRequest.type = MSG_GetSequencePoints;
			_pMSG->getSequencePointsRequest.functionToken = functionToken;
			USES_CONVERSION;
			wcscpy_s(_pMSG->getSequencePointsRequest.szProcessName, T2CW(_processName.c_str()));
			wcscpy_s(_pMSG->getSequencePointsRequest.szModulePath, pModulePath);
			wcscpy_s(_pMSG->getSequencePointsRequest.szAssemblyName, pAssemblyName);
		},
			[=, &points]()->BOOL
		{
			if (_pMSG->getSequencePointsResponse.count > SEQ_BUFFER_SIZE) {
				RELTRACE(_T("Received an abnormal count for sequence points (%d) for token 0x%X"),
					_pMSG->getSequencePointsResponse.count, functionToken);
				points.clear();
				return false;
			}

			for (int i = 0; i < _pMSG->getSequencePointsResponse.count; i++)
				points.push_back(_pMSG->getSequencePointsResponse.points[i]);
			BOOL hasMore = _pMSG->getSequencePointsResponse.hasMore;
			::ZeroMemory(_pMSG, MSG_UNION_SIZE);
			return hasMore;
		}
			, _short_wait
			, _T("GetSequencePoints"));

		return (points.size() != 0);
	}

	bool ProfilerCommunication::GetBranchPoints(mdToken functionToken, WCHAR* pModulePath,
		WCHAR* pAssemblyName, std::vector<BranchPoint> &points)
	{
		if (!_hostCommunicationActive)
			return false;

		RequestInformation(
			[=]
		{
			_pMSG->getBranchPointsRequest.type = MSG_GetBranchPoints;
			_pMSG->getBranchPointsRequest.functionToken = functionToken;
			USES_CONVERSION;
			wcscpy_s(_pMSG->getBranchPointsRequest.szProcessName, T2CW(_processName.c_str()));
			wcscpy_s(_pMSG->getBranchPointsRequest.szModulePath, pModulePath);
			wcscpy_s(_pMSG->getBranchPointsRequest.szAssemblyName, pAssemblyName);
		},
			[=, &points]()->BOOL
		{
			if (_pMSG->getBranchPointsResponse.count > BRANCH_BUFFER_SIZE) {
				RELTRACE(_T("Received an abnormal count for branch points (%d) for token 0x%X"),
					_pMSG->getBranchPointsResponse.count, functionToken);
				points.clear();
				return false;
			}

			for (int i = 0; i < _pMSG->getBranchPointsResponse.count; i++)
				points.push_back(_pMSG->getBranchPointsResponse.points[i]);
			BOOL hasMore = _pMSG->getBranchPointsResponse.hasMore;
			::ZeroMemory(_pMSG, MSG_UNION_SIZE);
			return hasMore;
		}
			, _short_wait
			, _T("GetBranchPoints"));

		return (points.size() != 0);
	}

	bool ProfilerCommunication::TrackAssembly(WCHAR* pModulePath, WCHAR* pAssemblyName)
	{
		if (!_hostCommunicationActive)
			return false;

		bool response = false;
		RequestInformation(
			[=]()
		{
			_pMSG->trackAssemblyRequest.type = MSG_TrackAssembly;
			USES_CONVERSION;
			wcscpy_s(_pMSG->trackAssemblyRequest.szProcessName, T2CW(_processName.c_str()));
			wcscpy_s(_pMSG->trackAssemblyRequest.szModulePath, pModulePath);
			wcscpy_s(_pMSG->trackAssemblyRequest.szAssemblyName, pAssemblyName);
		},
			[=, &response]()->BOOL
		{
			response = _pMSG->trackAssemblyResponse.bResponse == TRUE;
			::ZeroMemory(_pMSG, MSG_UNION_SIZE);
			return FALSE;
		}
			, COM_WAIT_LONG
			, _T("TrackAssembly"));

		return response;
	}

	bool ProfilerCommunication::TrackMethod(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, ULONG &uniqueId)
	{
		if (!_hostCommunicationActive)
			return false;

		bool response = false;
		RequestInformation(
			[=]()
		{
			_pMSG->trackMethodRequest.type = MSG_TrackMethod;
			_pMSG->trackMethodRequest.functionToken = functionToken;
			wcscpy_s(_pMSG->trackMethodRequest.szModulePath, pModulePath);
			wcscpy_s(_pMSG->trackMethodRequest.szAssemblyName, pAssemblyName);
		},
			[=, &response, &uniqueId]()->BOOL
		{
			response = _pMSG->trackMethodResponse.bResponse == TRUE;
			uniqueId = _pMSG->trackMethodResponse.ulUniqueId;
			::ZeroMemory(_pMSG, MSG_UNION_SIZE);
			return FALSE;
		}
			, _short_wait
			, _T("TrackMethod"));

		return response;
	}

	bool ProfilerCommunication::AllocateBuffer(LONG bufferSize, ULONG &bufferId)
	{
		Synchronization::CScopedLock<Synchronization::CMutex> lock(_mutexCommunication);

		if (!_hostCommunicationActive)
			return false;

		bool response = false;
		int repeat = 0;
		while (!response && (repeat <= 3)) {
			++repeat;
			_hostCommunicationActive = true;
			RequestInformation(
				[=]()
			{
				_pMSG->allocateBufferRequest.type = MSG_AllocateMemoryBuffer;
				_pMSG->allocateBufferRequest.lBufferSize = bufferSize;
				_pMSG->allocateBufferRequest.dwVersionHigh = _version_high;
				_pMSG->allocateBufferRequest.dwVersionLow = _version_low;

			},
				[=, &response, &bufferId]()->BOOL
			{
				response = _pMSG->allocateBufferResponse.allocated == TRUE;
				bufferId = _pMSG->allocateBufferResponse.ulBufferId;
				::ZeroMemory(_pMSG, MSG_UNION_SIZE);
				return FALSE;
			}
				, COM_WAIT_VSHORT
				, _T("AllocateBuffer"));
		}

		return response;
	}

	void ProfilerCommunication::CloseChannel(bool sendSingleBuffer) {

		_sendTimer.Stop();

		if (_bufferId == 0)
			return;

		if (!_hostCommunicationActive)
			return;

		if (!TestSemaphore(_semapore_results))
			return;

		SendRemainingVisitPoints(sendSingleBuffer);

		if (!_hostCommunicationActive)
			return;

		bool response = false;

		RequestInformation(
			[=]()
		{
			_pMSG->closeChannelRequest.type = MSG_CloseChannel;
			_pMSG->closeChannelRequest.ulBufferId = _bufferId;
		},
			[=, &response]()->BOOL
		{
			response = _pMSG->closeChannelResponse.bResponse == TRUE;
			return FALSE;
		}
			, _short_wait
			, _T("CloseChannel"));

		return;
	}

	bool ProfilerCommunication::TrackProcess() {
		Synchronization::CScopedLock<Synchronization::CMutex> lock(_mutexCommunication);

		if (!_hostCommunicationActive)
			return false;

		bool response = false;

		RequestInformation(
			[=]()
		{
			_pMSG->trackProcessRequest.type = MSG_TrackProcess;
			USES_CONVERSION;
			wcscpy_s(_pMSG->trackProcessRequest.szProcessName, T2CW(_processName.c_str()));
		},
			[=, &response]()->BOOL
		{
			response = _pMSG->trackProcessResponse.bResponse == TRUE;
			return FALSE;
		}
			, _short_wait
			, _T("TrackProcess"));

		return response;
	}

	void ProfilerCommunication::report_runtime(const std::runtime_error& re, const tstring &msg) const {
		USES_CONVERSION;
#pragma warning (suppress : 6255) // can't fix ATL macro
		RELTRACE(_T("Runtime error: %s - %s"), msg.c_str(), A2T(re.what()));
	}

	void ProfilerCommunication::report_exception(const std::exception& re, const tstring &msg) const {
		USES_CONVERSION;
#pragma warning (suppress : 6255) // can't fix ATL macro
		RELTRACE(_T("Error occurred: %s - %s"), msg.c_str(), A2T(re.what()));
	}

	template<class Action>
	void ProfilerCommunication::handle_sehexception(Action action, const tstring& message) {
		__try {
			action();
		}
		__except (GetExceptionCode() == EXCEPTION_IN_PAGE_ERROR ? EXCEPTION_EXECUTE_HANDLER : EXCEPTION_CONTINUE_SEARCH)
		{
			RELTRACE(_T("SEH exception failure occurred: %s - %d"),
				message.c_str(), GetExceptionCode());
		}
	}

	template<class Action>
	void ProfilerCommunication::handle_exception(Action action, const tstring& message) {
		try
		{
			handle_sehexception([&]() {
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
			RELTRACE(_T("Unknown failure occurred. Possible memory corruption - %s"), message.c_str());
			throw;
		}
	}

	template<class BR, class PR>
	void ProfilerCommunication::RequestInformation(BR buildRequest, PR processResults, DWORD dwTimeout, tstring message)
	{
		ATL::CComCritSecLock<ATL::CComAutoCriticalSection> lock(_critComms);
		if (!_hostCommunicationActive)
			return;

		if (!TestSemaphore(_semapore_communication))
			return;

		try {

			handle_exception([&]() { buildRequest(); }, message);

			_memoryCommunication.FlushViewOfFile();

			DWORD dwSignal = _eventProfilerRequestsInformation.SignalAndWait(_eventInformationReadyForProfiler, dwTimeout);
			if (WAIT_OBJECT_0 != dwSignal) throw CommunicationException(dwSignal, dwTimeout);

			_eventInformationReadyForProfiler.Reset();

			BOOL hasMore = FALSE;
			do
			{
				handle_exception([&]() { hasMore = processResults(); }, message);

				if (hasMore)
				{
					dwSignal = _eventInformationReadByProfiler.SignalAndWait(_eventInformationReadyForProfiler, _short_wait);
					if (WAIT_OBJECT_0 != dwSignal)
						throw CommunicationException(dwSignal, _short_wait);

					_eventInformationReadyForProfiler.Reset();
				}
			} while (hasMore);

			_eventInformationReadByProfiler.Set();
		}
		catch (const CommunicationException& ex) {
			RELTRACE(_T("ProfilerCommunication::RequestInformation(...) => Communication (Chat channel - %s) with host has failed (0x%x, %d)"),
				message.c_str(), ex.getReason(), ex.getTimeout());
			_hostCommunicationActive = false;
		}
		catch (...)
		{
			_hostCommunicationActive = false;
		}
	}
}