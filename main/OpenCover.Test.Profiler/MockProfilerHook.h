#pragma once
#include "../OpenCover.Profiler/ProfilerInfo.h"


class TestProfilerHook :
	public CProfilerHook
{
public:
	virtual DWORD AppendProfilerEventMask(DWORD dwEvents) { return dwEvents; }
};

class MockProfilerHook :
	public TestProfilerHook
{

public:
	MockProfilerHook(){}
	~MockProfilerHook() override {}
	MOCK_METHOD1(AppendProfilerEventMask, DWORD(
		/* [out] */ DWORD dwEvents));

};
