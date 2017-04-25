#include "stdafx.h"
#include "CodeCoverage.h"

#include "dllmain.h"

// COR_PRF_MONITOR_THREADS
HRESULT STDMETHODCALLTYPE CCodeCoverage::ThreadCreated(
    /* [in] */ ThreadID threadId)
{
    ATLTRACE(_T("::ThreadCreated(%d)"), threadId);
    if (chainedProfiler_ != nullptr)
		chainedProfiler_->ThreadCreated(threadId);
    return S_OK;
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::ThreadDestroyed(
    /* [in] */ ThreadID threadId)
{
    ATLTRACE(_T("::ThreadDestroyed(%d)"), threadId);
    if (chainedProfiler_ != nullptr)
		chainedProfiler_->ThreadDestroyed(threadId);

    if (!safe_mode_) {
        _host->ThreadDestroyed(threadId);
    }

    return S_OK;
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::ThreadAssignedToOSThread(
    /* [in] */ ThreadID managedThreadId,
    /* [in] */ DWORD osThreadId)
{
    ATLTRACE(_T("::ThreadAssignedToOSThread(%d, %d)"), managedThreadId, osThreadId);
    if (chainedProfiler_ != nullptr)
		chainedProfiler_->ThreadAssignedToOSThread(managedThreadId, osThreadId);

    if (!safe_mode_) {
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
    ATLTRACE(_T("::ThreadNameChanged(%d, %s)"), threadId, W2T(name));
    if (chainedProfiler_ != nullptr)
		chainedProfiler2_->ThreadNameChanged(threadId, cchName, name);
    return S_OK;
}

