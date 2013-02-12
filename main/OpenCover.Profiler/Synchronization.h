//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#pragma once

class CMutex
{
public:
    CMutex() : m_hMutex(NULL) {}
    ~CMutex() { CloseHandle(); }
    bool IsValid() {return m_hMutex!=NULL; }

public:
    void Initialise(const TCHAR * pName) { CloseHandle(); m_hMutex = ::CreateMutex(NULL, false, pName); }
    void Enter(){ if (m_hMutex!=NULL) { ::WaitForSingleObject(m_hMutex, INFINITE);} }
    void Leave(){ if (m_hMutex!=NULL) { ::ReleaseMutex(m_hMutex);} }

private:
    HANDLE m_hMutex;
	void CloseHandle() {if (m_hMutex!=NULL) { ::CloseHandle(m_hMutex); m_hMutex=NULL; }}
};

template<class T>
class CScopedLock
{
public:
    CScopedLock<T>(T&entity) : m_entity(entity) { m_entity.Enter(); }
    ~CScopedLock(void) { m_entity.Leave(); }
private:
    T &m_entity;
};

class CEvent
{
public:
    CEvent () : m_hEvent(NULL) { }
    ~CEvent() { CloseHandle(); }
    bool IsValid() {return m_hEvent!=NULL; }

public:
    void Initialise(const TCHAR * pName, BOOL bManualReset = TRUE) { CloseHandle(); m_hEvent = ::CreateEvent(NULL, bManualReset, FALSE, pName); }
    void Set() { ::SetEvent(m_hEvent); }
    void Wait() { ::WaitForSingleObject(m_hEvent, INFINITE); }

    void Reset() { ::ResetEvent(m_hEvent); }
	DWORD SignalAndWait(CEvent &waitEvent, DWORD dwMilliSeconds = INFINITE) {return ::SignalObjectAndWait(m_hEvent, waitEvent.m_hEvent, dwMilliSeconds, FALSE);}

private:
    HANDLE m_hEvent;
	void CloseHandle() {if (m_hEvent!= NULL) { ::CloseHandle(m_hEvent); m_hEvent = NULL; }}
};

