#pragma once
#include "../OpenCover.Profiler/ProfileBase.h"

class ATL_NO_VTABLE CTestProfiler :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CProfilerBase
{
public:
	CTestProfiler()
	{
	}

	BEGIN_COM_MAP(CTestProfiler)
		COM_INTERFACE_ENTRY(ICorProfilerCallback)
		COM_INTERFACE_ENTRY(ICorProfilerCallback2)
		COM_INTERFACE_ENTRY(ICorProfilerCallback3)
		COM_INTERFACE_ENTRY(ICorProfilerCallback4)
		COM_INTERFACE_ENTRY(ICorProfilerCallback5)
		COM_INTERFACE_ENTRY(ICorProfilerCallback6)
		COM_INTERFACE_ENTRY(ICorProfilerCallback7)
		COM_INTERFACE_ENTRY(ICorProfilerCallback8)
	END_COM_MAP()

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
		ReleaseChainedProfiler();
	}

	virtual ~CTestProfiler() {}
};