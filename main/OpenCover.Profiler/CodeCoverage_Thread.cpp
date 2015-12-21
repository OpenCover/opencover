#include "stdafx.h"
#include "CodeCoverage.h"

#include "dllmain.h"

// COR_PRF_MONITOR_THREADS
HRESULT STDMETHODCALLTYPE CCodeCoverage::ThreadCreated(
    /* [in] */ ThreadID threadId)
{
    ATLTRACE(_T("::ThreadCreated(%d)\n"), threadId);
    if (m_chainedProfiler != nullptr)
        m_chainedProfiler->ThreadCreated(threadId);
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::ThreadDestroyed(
    /* [in] */ ThreadID threadId)
{
    ATLTRACE(_T("::ThreadDestroyed(%d)\n"), threadId);
    if (m_chainedProfiler != nullptr)
        m_chainedProfiler->ThreadDestroyed(threadId);

    if (!m_tracingEnabled){
        _host->ThreadDestroyed(threadId);
    }

    return S_OK;
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::ThreadAssignedToOSThread(
    /* [in] */ ThreadID managedThreadId,
    /* [in] */ DWORD osThreadId)
{
    ATLTRACE(_T("::ThreadAssignedToOSThread(%d, %d)\n"), managedThreadId, osThreadId);
    if (m_chainedProfiler != nullptr)
        m_chainedProfiler->ThreadAssignedToOSThread(managedThreadId, osThreadId);

    if (!m_tracingEnabled){
        _host->ThreadCreated(managedThreadId, osThreadId);
    }

    return S_OK;
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::ThreadNameChanged(
    /* [in] */ ThreadID threadId,
    /* [in] */ ULONG cchName,
    /* [in] */
    __in_ecount_opt(cchName)  WCHAR name[])
{
    ATLTRACE(_T("::ThreadNameChanged(%d, %s)\n"), threadId, W2T(name));
    if (m_chainedProfiler != nullptr)
        m_chainedProfiler->ThreadNameChanged(threadId, cchName, name);
    return S_OK;
}

