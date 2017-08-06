//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#pragma once

#include "Synchronization.h"
#include "SharedMemory.h"
#include "Messages.h"
#include "Timer.h"

#include <exception>

#include <concurrent_unordered_map.h>

namespace Communication
{
	/// <summary>Handles communication back to the profiler host</summary>
	/// <remarks>Currently this is handled by using the WebServices API</remarks>
	class ProfilerCommunication
	{
	private:

	public:
		ProfilerCommunication(DWORD short_wait, DWORD version_high, DWORD version_low);
		
		
		bool Initialise(
			TCHAR* key, TCHAR *ns, TCHAR *processName, 
			bool safe_mode, int sendVisitPointsTimerInterval);

		bool Initialise(TCHAR* key, TCHAR *ns, TCHAR *processName);

	public:
		bool TrackAssembly(WCHAR* pModulePath, WCHAR* pAssemblyName);
		bool GetPoints(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, std::vector<SequencePoint> &seqPoints, std::vector<BranchPoint> &brPoints);
		bool TrackMethod(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, ULONG &uniqueId);
		inline void AddTestEnterPoint(ULONG uniqueId) { AddVisitPointToBuffer(uniqueId, IT_MethodEnter); }
		inline void AddTestLeavePoint(ULONG uniqueId) { AddVisitPointToBuffer(uniqueId, IT_MethodLeave); }
		inline void AddTestTailcallPoint(ULONG uniqueId) { AddVisitPointToBuffer(uniqueId, IT_MethodTailcall); }
		inline void AddVisitPoint(ULONG uniqueId) { AddVisitPointToBuffer(uniqueId, IT_VisitPoint); }
		void AddVisitPointToThreadBuffer(ULONG uniqueId, MSG_IdType msgType);
		void CloseChannel(bool sendSingleBuffer);

	private:
		bool InitializePrimarySynchronization(std::wstring sharedKey, std::basic_string<wchar_t>& resource_name);
		bool InitializeBufferSynchronization(std::basic_string<wchar_t>& resource_name);
		bool AllocateBuffer(LONG bufferSize, ULONG &bufferId);
		bool TrackProcess();

	public:
		void ThreadCreated(ThreadID threadID, DWORD osThreadID);
		void ThreadDestroyed(ThreadID threadID);

	private:
		void AddVisitPointToBuffer(ULONG uniqueId, MSG_IdType msgType);
		void SendVisitPoints();
		void SendVisitPointsInternal();
		void SendThreadVisitPoints(MSG_SendVisitPoints_Request* pVisitPoints);
		void SendThreadVisitPointsInternal(MSG_SendVisitPoints_Request* pVisitPoints);
		bool GetSequencePoints(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, std::vector<SequencePoint> &points);
		bool GetBranchPoints(mdToken functionToken, WCHAR* pModulePath, WCHAR* pAssemblyName, std::vector<BranchPoint> &points);
		void SendRemainingVisitPoints(bool safemode);
		void SendRemainingThreadBuffers();
		MSG_SendVisitPoints_Request* AllocateVisitMap(DWORD osThreadID);

	private:
		tstring _key;
		tstring _namespace;
		tstring _processName;
		DWORD _short_wait;

		template<class BR, class PR>
		void RequestInformation(BR buildRequest, PR processResults, DWORD dwTimeout, tstring message);

		ULONG _bufferId;

		bool TestSemaphore(Synchronization::CSemaphoreEx &semaphore) {
			// the previous value should always be zero unless the host process has released 
			// and that means we have disposed of the shared memory
			if (_hostCommunicationActive && semaphore.ReleaseAndWait() != 0) {
				_hostCommunicationActive = false;
			}
			return _hostCommunicationActive;
		}

	private:
		Synchronization::CMutex _mutexCommunication;
		CSharedMemory _memoryCommunication;
		Synchronization::CEvent _eventProfilerRequestsInformation;
		Synchronization::CEvent _eventInformationReadyForProfiler;
		Synchronization::CEvent _eventInformationReadByProfiler;
		MSG_Union *_pMSG;
		Synchronization::CSemaphoreEx _semapore_communication;

	private:
		CSharedMemory _memoryResults;
		Synchronization::CEvent _eventProfilerHasResults;
		Synchronization::CEvent _eventResultsHaveBeenReceived;
		MSG_SendVisitPoints_Request *_pVisitPoints;
		Synchronization::CSemaphoreEx _semapore_results;

	private:
		ATL::CComAutoCriticalSection _critResults;
		ATL::CComAutoCriticalSection _critComms;
		bool _hostCommunicationActive;

	private:
		ATL::CComAutoCriticalSection _critThreads;
		//std::unordered_map<ThreadID, ULONG> _threadmap;
		//std::unordered_map<ULONG, MSG_SendVisitPoints_Request*> _visitmap;

		Concurrency::concurrent_unordered_map<ThreadID, ULONG> _threadmap;
		Concurrency::concurrent_unordered_map<ULONG, MSG_SendVisitPoints_Request*> _visitmap;

		MSG_SendVisitPoints_Request* GetVisitMapForOSThread(ULONG osThread);

	private:
		void report_runtime(const std::runtime_error& re, const tstring &msg) const;
		void report_exception(const std::exception& re, const tstring &msg) const;

		template<class Action>
		void handle_exception(Action action, const tstring& message);

		template<class Action>
		void static handle_sehexception(Action action, const tstring& message);

	private:
		DWORD _version_high;
		DWORD _version_low;
		Timer _sendTimer;

	private:

		class CommunicationException : std::exception
		{
			DWORD dwReason;
			DWORD dwTimeout;
		public:
			CommunicationException(DWORD reason, DWORD timeout) :
				dwReason(reason), 
				dwTimeout(timeout)
			{ }

			DWORD getReason() const { return dwReason; }
			DWORD getTimeout() const { return dwTimeout; }
		};

	};
}
