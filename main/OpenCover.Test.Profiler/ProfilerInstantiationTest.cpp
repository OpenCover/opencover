#include "stdafx.h"

class ProfilerInstantiationTest : public ::testing::Test {
	void SetUp() override
    {
        ::CoInitialize(NULL);
    }

	void TearDown() override
    {
        ::CoUninitialize();
    }
};

const CLSID CLSID_CodeCoverage = {0x1542C21D,0x80C3,0x45E6,0xA5,0x6C,0xA9,0xC1,0xE4,0xBE,0xB7,0xB8}; 
const CLSID CLSID_CodeCoverage64 = {0xA7A1EDD8,0xD9A9,0x4D51,0x85,0xEA,0x51,0x4A,0x8C,0x4A,0x91,0x00}; 

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

TEST_F(ProfilerInstantiationTest, Profiler64SuppportsICorProfilerCallack) {

    ICorProfilerCallback3 *pRequest = NULL;

    HRESULT hr = ::CoCreateInstance(CLSID_CodeCoverage64,
                           NULL,
                           CLSCTX_INPROC_SERVER,
                           _ATL_IIDOF(ICorProfilerCallback),
                           (void **)&pRequest);

    ASSERT_HRESULT_SUCCEEDED(hr);
    ASSERT_TRUE(pRequest != NULL);

    pRequest->Release();
}

TEST_F(ProfilerInstantiationTest, Profiler64SuppportsICorProfilerCallack2) {

    ICorProfilerCallback3 *pRequest = NULL;

    HRESULT hr = ::CoCreateInstance(CLSID_CodeCoverage64,
                           NULL,
                           CLSCTX_INPROC_SERVER,
                           _ATL_IIDOF(ICorProfilerCallback2),
                           (void **)&pRequest);

    ASSERT_HRESULT_SUCCEEDED(hr);
    ASSERT_TRUE(pRequest != NULL);

    pRequest->Release();
}

TEST_F(ProfilerInstantiationTest, Profiler64SuppportsICorProfilerCallack3) {

    ICorProfilerCallback3 *pRequest = NULL;

    HRESULT hr = ::CoCreateInstance(CLSID_CodeCoverage64,
                           NULL,
                           CLSCTX_INPROC_SERVER,
                           _ATL_IIDOF(ICorProfilerCallback3),
                           (void **)&pRequest);

    ASSERT_HRESULT_SUCCEEDED(hr);
    ASSERT_TRUE(pRequest != NULL);

    pRequest->Release();
}
