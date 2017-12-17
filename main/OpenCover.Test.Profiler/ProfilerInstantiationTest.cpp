#include "stdafx.h"

class ProfilerInstantiationTest : public ::testing::Test {

private:
	void SetUp() override
    {
		auto cwd = executable_path();
		system(("regsvr32.exe /i:user /n /s " + cwd + "\\OpenCover.Profiler.dll").c_str());
		::CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
	}

	void TearDown() override
    {
		auto cwd = executable_path();
		::CoUninitialize();
		system(("regsvr32.exe /i:user /u /n /s " + cwd + "\\OpenCover.Profiler.dll").c_str());
	}

	static std::string executable_path()
	{
		char buf[MAX_PATH] = { 0 };
		DWORD length = GetModuleFileNameA(NULL, buf, MAX_PATH);
		PathRemoveFileSpecA(buf);
		return buf;
	}

	const CLSID CLSID_CodeCoverage = { 0x1542C21D,0x80C3,0x45E6,0xA5,0x6C,0xA9,0xC1,0xE4,0xBE,0xB7,0xB8 };

protected:
	template<class T>
	void InstantiateProfiler() const
	{
		T *pRequest = nullptr;

		HRESULT hr = ::CoCreateInstance(CLSID_CodeCoverage,
			NULL,
			CLSCTX_INPROC_SERVER,
			_ATL_IIDOF(T),
			(void **)&pRequest);

		ASSERT_HRESULT_SUCCEEDED(hr);
		ASSERT_TRUE(pRequest != NULL);

		pRequest->Release();
	}
};


TEST_F(ProfilerInstantiationTest, ProfilerSuppportsICorProfilerCallback) {

	InstantiateProfiler<ICorProfilerCallback>();
}

TEST_F(ProfilerInstantiationTest, ProfilerSuppportsICorProfilerCallback2) {

	InstantiateProfiler<ICorProfilerCallback2>();
}

TEST_F(ProfilerInstantiationTest, ProfilerSuppportsICorProfilerCallback3) {

	InstantiateProfiler<ICorProfilerCallback3>();
}

TEST_F(ProfilerInstantiationTest, ProfilerSuppportsICorProfilerCallback4) {

	InstantiateProfiler<ICorProfilerCallback4>();
}

TEST_F(ProfilerInstantiationTest, ProfilerSuppportsICorProfilerCallback5) {

	InstantiateProfiler<ICorProfilerCallback5>();
}

TEST_F(ProfilerInstantiationTest, ProfilerSuppportsICorProfilerCallback6) {

	InstantiateProfiler<ICorProfilerCallback6>();
}

TEST_F(ProfilerInstantiationTest, ProfilerSuppportsICorProfilerCallback7) {

	InstantiateProfiler<ICorProfilerCallback7>();
}

TEST_F(ProfilerInstantiationTest, ProfilerSuppportsICorProfilerCallback8) {

	InstantiateProfiler<ICorProfilerCallback8>();
}