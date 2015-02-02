// ProfilerInfo.cpp : Implementation of CProfilerInfo

#include "stdafx.h"
#include "ProfilerInfo.h"
#include "CodeCoverage.h"

// CProfilerInfo
HRESULT STDMETHODCALLTYPE CProfilerInfo::SetEventMask(
	/* [in] */ DWORD dwEvents){
	ATLTRACE(_T("CProfilerInfo::SetEventMask(0x%X)"), dwEvents);

	DWORD expected = COR_PRF_DISABLE_ALL_NGEN_IMAGES;
	expected |= COR_PRF_DISABLE_TRANSPARENCY_CHECKS_UNDER_FULL_TRUST;
	expected |= COR_PRF_DISABLE_INLINING;
	expected |= COR_PRF_MONITOR_THREADS;
	expected |= COR_PRF_MONITOR_JIT_COMPILATION;
	expected |= COR_PRF_MONITOR_APPDOMAIN_LOADS;
	expected |= COR_PRF_MONITOR_ASSEMBLY_LOADS;
	expected |= COR_PRF_MONITOR_MODULE_LOADS;

	//ATLTRACE(_T("::SetEventMask => expected 0x%X"), expected);
	ATLASSERT(expected == dwEvents);

	if (m_pProfilerHook!=NULL)
		dwEvents = m_pProfilerHook->AppendProfilerEventMask(dwEvents);

	return CProfilerInfoBase::SetEventMask(dwEvents);
}
