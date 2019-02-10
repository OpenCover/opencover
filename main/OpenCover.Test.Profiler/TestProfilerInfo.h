#pragma once
#include "../OpenCover.Profiler/ProfilerInfoBase.h"

class ATL_NO_VTABLE CTestProfilerInfo :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CProfilerInfoBase
{
public:
	CTestProfilerInfo()
	{
		//m_pProfilerHook = nullptr;
	}

	BEGIN_COM_MAP(CTestProfilerInfo)
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

	virtual ~CTestProfilerInfo() {}
};
