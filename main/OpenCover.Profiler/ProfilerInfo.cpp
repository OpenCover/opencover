// ProfilerInfo.cpp : Implementation of CProfilerInfo

#include "stdafx.h"
#include "ProfilerInfo.h"

// CProfilerInfo
HRESULT STDMETHODCALLTYPE CProfilerInfo::SetEventMask(
	/* [in] */ DWORD dwEvents){

	DWORD expected = COR_PRF_DISABLE_ALL_NGEN_IMAGES;
	expected |= COR_PRF_DISABLE_TRANSPARENCY_CHECKS_UNDER_FULL_TRUST;
	expected |= COR_PRF_DISABLE_INLINING;
	expected |= COR_PRF_MONITOR_THREADS;
	expected |= COR_PRF_MONITOR_JIT_COMPILATION;
	expected |= COR_PRF_MONITOR_APPDOMAIN_LOADS;
	expected |= COR_PRF_MONITOR_ASSEMBLY_LOADS;
	expected |= COR_PRF_MONITOR_MODULE_LOADS;
    expected |= COR_PRF_ENABLE_REJIT; // VS2012 only

	ATLTRACE(_T("CProfilerInfo::SetEventMask => received => 0x%X, expected 0x%X"), dwEvents, expected);
    ATLASSERT(expected == (dwEvents | expected)); // assert that nothing new has been added that we haven't already tested against

	if (profilerHook_ !=nullptr)
		dwEvents = profilerHook_->AppendProfilerEventMask(dwEvents);

	return CProfilerInfoBase::SetEventMask(dwEvents);
}
