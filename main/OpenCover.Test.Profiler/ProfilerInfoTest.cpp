#include "stdafx.h"
#include "MockProfilerInfo.h"
#include "ComBaseTest.h"
#include "../OpenCover.Profiler/ProfilerInfo.h"
#include "MockProfilerHook.h"

using ::testing::_;
using ::testing::Return;
using ::testing::Invoke;

class ProfilerInfoTest : public ComBaseTest {
public:
	ProfilerInfoTest() : mockProfilerInfo_(nullptr),
		testProfilerInfo_(nullptr)
	{
	}

private:
	void SetUp() override
	{
		CreateComObject(&testProfilerInfo_);
		CreateComObject(&mockProfilerInfo_);
		testProfilerInfo_->ChainProfilerInfo(mockProfilerInfo_);
	}

	void TearDown() override
	{
		ASSERT_EQ(0, testProfilerInfo_->Release());
		ASSERT_EQ(0, mockProfilerInfo_->Release());
	}

protected:
	CComObject<MockProfilerInfo> *mockProfilerInfo_;
	CComObject<CProfilerInfo> *testProfilerInfo_;
};

TEST_F(ProfilerInfoTest, ChainedProfilerInfo_HasHookedAllAvailableInterfaces)
{
	ASSERT_EQ(8, mockProfilerInfo_->Release());
	ASSERT_EQ(9, mockProfilerInfo_->AddRef());
}

TEST_F(ProfilerInfoTest, ChainedProfilerInfo_WhenHooked_WillForwardModifiedEvents_SetEventMask_AndReturnSuccess)
{
	testing::NiceMock<MockProfilerHook> hook;
	ON_CALL(hook, AppendProfilerEventMask(_))
		.WillByDefault(Invoke([](DWORD dwEvents) {
		dwEvents |= COR_PRF_DISABLE_ALL_NGEN_IMAGES;
		return dwEvents;
	}));

	DWORD events;
	ON_CALL(*mockProfilerInfo_, SetEventMask(_))
		.WillByDefault(Invoke([this, &events](/* [in] */ DWORD dwEvents) {
		events = dwEvents;
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, SetEventMask(_)).Times(1);

	testProfilerInfo_->SetProfilerHook(&hook);
	ASSERT_EQ(S_OK, testProfilerInfo_->SetEventMask(COR_PRF_DISABLE_ALL_NGEN_IMAGES));
	ASSERT_EQ(COR_PRF_DISABLE_ALL_NGEN_IMAGES, events);
}

