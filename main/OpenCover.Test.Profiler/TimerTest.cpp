#include "stdafx.h"
#include "../OpenCover.Profiler/Timer.h"

class TimerTest : public ::testing::Test {
	void SetUp() override
	{

	}

	void TearDown() override
	{

	}
};


TEST_F(TimerTest, CanInstantiateAndDelete)
{
	auto timer = new Communication::Timer();
	ASSERT_NE(nullptr, timer);
	delete timer;
}

TEST_F(TimerTest, CanStartTimerAndDeleteWithoutFiring)
{
	auto timer = new Communication::Timer();

	timer->Start([]()
	{
		ASSERT_TRUE(false); // this should never happen
	}, 100000);

	Sleep(100);

	delete timer;
}

TEST_F(TimerTest, CanStartTimerAndExecuteExpectedNumberOfTimes)
{
	auto timer = new Communication::Timer();

	int counter = 0;
	timer->Start([&]()
	{
		++counter;
	}, 20);

	Sleep(110);
	delete timer;

	ASSERT_EQ(5, counter);
}

TEST_F(TimerTest, CanStartStopRestartTimerAndExecuteExpectedNumberOfTimes)
{
	auto timer = new Communication::Timer();

	int counter = 0;
	timer->Start([&]()
	{
		++counter;
	}, 20);

	Sleep(110);

	timer->Stop();

	timer->Start([&]()
	{
		++counter;
	}, 20);

	Sleep(110);

	timer->Stop();

	delete timer;

	ASSERT_EQ(10, counter);
}

TEST_F(TimerTest, CanStartNewTimerWithoutStoppingOld)
{
	auto timer = new Communication::Timer();

	int counter = 0;
	timer->Start([&]()
	{
		++counter;
	}, 20);

	Sleep(110);

	ASSERT_EQ(5, counter);

	timer->Start([&]()
	{
		++counter;
	}, 50);

	Sleep(110);

	timer->Stop();

	delete timer;

	ASSERT_EQ(7, counter);

}

