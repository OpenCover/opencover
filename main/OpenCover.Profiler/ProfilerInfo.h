// ProfilerInfo.h : Declaration of the CProfilerInfo

#pragma once
#include "resource.h"       // main symbols



#include "OpenCoverProfiler_i.h"

#include "ProfilerInfoBase.h"

class CCodeCoverage;

using namespace ATL;

// CProfilerInfoBase

class ATL_NO_VTABLE CProfilerInfo :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CProfilerInfoBase
{
public:
	CProfilerInfo()
	{
	}

	BEGIN_COM_MAP(CProfilerInfo)
		COM_INTERFACE_ENTRY(ICorProfilerInfo)
		COM_INTERFACE_ENTRY(ICorProfilerInfo2)
		COM_INTERFACE_ENTRY(ICorProfilerInfo3)
		COM_INTERFACE_ENTRY(ICorProfilerInfo4)
	END_COM_MAP()


	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

private:
	CCodeCoverage *m_pProfilerHook;

public:
	virtual HRESULT STDMETHODCALLTYPE SetEventMask(
		/* [in] */ DWORD dwEvents);

	friend CCodeCoverage;
};
