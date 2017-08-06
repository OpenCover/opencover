#include "stdafx.h"
#include "CodeCoverage.h"

#include "dllmain.h"

HRESULT STDMETHODCALLTYPE CCodeCoverage::ThreadDestroyed(
    /* [in] */ ThreadID threadId)
{
    ATLTRACE(_T("::ThreadDestroyed(%d)"), threadId);

	return ChainCall([&]() { return CProfilerBase::ThreadDestroyed(threadId); },
		[&]() {
		if (!safe_mode_) {
			_host->ThreadDestroyed(threadId);
		}
		return S_OK;
	});
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::ThreadAssignedToOSThread(
    /* [in] */ ThreadID managedThreadId,
    /* [in] */ DWORD osThreadId)
{
    ATLTRACE(_T("::ThreadAssignedToOSThread(%d, %d)"), managedThreadId, osThreadId);
	return ChainCall([&]() { return CProfilerBase::ThreadAssignedToOSThread(managedThreadId, osThreadId); },
		[&]() {
		if (!safe_mode_) {
			_host->ThreadCreated(managedThreadId, osThreadId);
		}
		return S_OK;
	});
}