// OpenCover.Test.Profiler.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

class ProfilerInstantiationTest : public ::testing::Test {
	virtual void SetUp() {
		::CoInitialize(NULL);
	}

	virtual void TearDown() {
		::CoUninitialize();
	}
};

const CLSID CLSID_CodeCoverage = {0x1542C21D,0x80C3,0x45E6,0xA5,0x6C,0xA9,0xC1,0xE4,0xBE,0xB7,0xB8}; 

TEST_F(ProfilerInstantiationTest, ProfilerSuppportsICorProfilerCallack) {

	ICorProfilerCallback3 *pRequest = NULL;

	HRESULT hr = ::CoCreateInstance(CLSID_CodeCoverage,
                           NULL,
                           CLSCTX_INPROC_SERVER,
                           _ATL_IIDOF(ICorProfilerCallback),
                           (void **)&pRequest);

	ASSERT_HRESULT_SUCCEEDED(hr);
	ASSERT_TRUE(pRequest != NULL);

	pRequest->Release();
}

TEST_F(ProfilerInstantiationTest, ProfilerSuppportsICorProfilerCallack2) {

	ICorProfilerCallback3 *pRequest = NULL;

	HRESULT hr = ::CoCreateInstance(CLSID_CodeCoverage,
                           NULL,
                           CLSCTX_INPROC_SERVER,
                           _ATL_IIDOF(ICorProfilerCallback2),
                           (void **)&pRequest);

	ASSERT_HRESULT_SUCCEEDED(hr);
	ASSERT_TRUE(pRequest != NULL);

	pRequest->Release();
}

TEST_F(ProfilerInstantiationTest, ProfilerSuppportsICorProfilerCallack3) {

	ICorProfilerCallback3 *pRequest = NULL;

	HRESULT hr = ::CoCreateInstance(CLSID_CodeCoverage,
                           NULL,
                           CLSCTX_INPROC_SERVER,
                           _ATL_IIDOF(ICorProfilerCallback3),
                           (void **)&pRequest);

	ASSERT_HRESULT_SUCCEEDED(hr);
	ASSERT_TRUE(pRequest != NULL);

	pRequest->Release();
}
