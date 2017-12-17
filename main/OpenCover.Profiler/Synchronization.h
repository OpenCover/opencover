//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#pragma once
#include "ReleaseTrace.h"

namespace Synchronization
{
	class CMutex
	{
	public:
		CMutex() : m_hMutex(nullptr) {}
		~CMutex() { CloseHandle(); }
		bool IsValid() const { return m_hMutex != nullptr; }

	public:
		void Initialise(const TCHAR * pName) { CloseHandle(); m_hMutex = ::CreateMutex(nullptr, false, pName); }
		void Enter() const { if (m_hMutex != nullptr) { ::WaitForSingleObject(m_hMutex, INFINITE); } }
		void Leave() const { if (m_hMutex != nullptr) { ::ReleaseMutex(m_hMutex); } }

	private:
		HANDLE m_hMutex;
		void CloseHandle() { if (m_hMutex != nullptr) { ::CloseHandle(m_hMutex); m_hMutex = nullptr; } }
	};

	class CSemaphore
	{
	public:
		CSemaphore() : m_hSemaphore(nullptr) {}
		~CSemaphore() { CloseHandle(); }
		bool IsValid() const { return m_hSemaphore != nullptr; }

	public:
		void Initialise(const TCHAR * pName) { CloseHandle(); m_hSemaphore = ::CreateSemaphore(nullptr, 0, 2, pName); _handleName = pName; }
		void Enter() const { if (IsValid()) { ::WaitForSingleObject(m_hSemaphore, INFINITE); } }
		void Leave() const { if (IsValid()) { ::ReleaseSemaphore(m_hSemaphore, 1, nullptr); } }

	protected:
		HANDLE getSemaphore() const { return m_hSemaphore; }
		tstring& getHandleName() { return _handleName; }

	private:
		HANDLE m_hSemaphore;
		tstring _handleName;

	private:
		void CloseHandle() { if (IsValid()) { ::CloseHandle(m_hSemaphore); m_hSemaphore = nullptr; } }
	};

	class CSemaphoreEx : public CSemaphore {
	public:
		LONG ReleaseAndWait() {
			if (IsValid()) {
				LONG prevCount = -1;
				if (::ReleaseSemaphore(getSemaphore(), 1, &prevCount) && prevCount == 0) { // +1
					if (::WaitForSingleObject(getSemaphore(), 1000) == WAIT_TIMEOUT) {     // -1
						RELTRACE(_T("Semaphore wait timed out => %s"), getHandleName().c_str());
						return -1;
					}
				}
				else {
					RELTRACE(_T("Semaphore count failed => %s, %d"), getHandleName().c_str(), prevCount);
				}
				return prevCount;
			}
			return -1;
		}

	};

	template<class T>
	class CScopedLock
	{
	public:
		explicit CScopedLock<T>(T&entity) : m_entity(entity) { m_entity.Enter(); }
		~CScopedLock() { m_entity.Leave(); }
	private:
		T &m_entity;
	};

	class CEvent
	{
	public:
		CEvent() : m_hEvent(nullptr) { }
		~CEvent() { CloseHandle(); }
		bool IsValid() const { return m_hEvent != nullptr; }

	public:
		void Initialise(const TCHAR * pName, BOOL bManualReset = TRUE) { CloseHandle(); m_hEvent = ::CreateEvent(nullptr, bManualReset, FALSE, pName); }
		void Set() const { ::SetEvent(m_hEvent); }
		void Wait() const { ::WaitForSingleObject(m_hEvent, INFINITE); }

		void Reset() const { ::ResetEvent(m_hEvent); }
		DWORD SignalAndWait(CEvent &waitEvent, DWORD dwMilliSeconds = INFINITE) const { return ::SignalObjectAndWait(m_hEvent, waitEvent.m_hEvent, dwMilliSeconds, FALSE); }

	private:
		HANDLE m_hEvent;
		void CloseHandle() { if (m_hEvent != nullptr) { ::CloseHandle(m_hEvent); m_hEvent = nullptr; } }
	};
}