// ProfilerInfo.h : Declaration of the CProfilerInfo

#pragma once
#include "resource.h"       // main symbols



#include "OpenCoverProfiler_i.h"

#include "ProfilerInfoBase.h"

class CCodeCoverage;

using namespace ATL;

class CProfilerHook{
public:
	virtual ~CProfilerHook(){}
	virtual DWORD AppendProfilerEventMask(DWORD dwEvents) = 0;
};

class ATL_NO_VTABLE CProfilerInfo :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CProfilerInfoBase
{
public:
	CProfilerInfo() : profilerHook_(nullptr)
	{
	}

	void SetProfilerHook(CProfilerHook *profilerHook)
	{
		profilerHook_ = profilerHook;
	}

	BEGIN_COM_MAP(CProfilerInfo)
		COM_INTERFACE_ENTRY(ICorProfilerInfo)
		COM_INTERFACE_ENTRY(ICorProfilerInfo2)
		COM_INTERFACE_ENTRY(ICorProfilerInfo3)
		COM_INTERFACE_ENTRY(ICorProfilerInfo4)
		COM_INTERFACE_ENTRY(ICorProfilerInfo5)
		COM_INTERFACE_ENTRY(ICorProfilerInfo6)
		COM_INTERFACE_ENTRY(ICorProfilerInfo7)
		COM_INTERFACE_ENTRY(ICorProfilerInfo8)
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
	CProfilerHook *profilerHook_;

public:
	virtual HRESULT STDMETHODCALLTYPE SetEventMask(
		/* [in] */ DWORD dwEvents) override;
};
