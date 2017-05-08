#include "StdAfx.h"
#include "Timer.h"

using namespace std;

namespace Communication
{
	Timer::Timer() :
		_logger(GetTraceLogger()),
		_isRunning(false)
	{
	}

	Timer::~Timer()
	{
		Stop();
		_thread.join();
	}

	void Timer::Start(
		function<void()> timerMethod,
		int timerIntervalMsec)
	{
		_timerMethod = timerMethod;
		_isRunning = true;
		_thread = thread([=]()
		{
			TimerMethod(timerIntervalMsec);
		});
	}

	void Timer::Stop()
	{
		unique_lock<mutex> lock(_mutex);
		_isRunning = false;
		_isRunningCondition.notify_one();
	}

	void Timer::TimerMethod(int timerIntervalMsec)
	{
		_logger.Log("Started timer thread with interval %d msec\n", timerIntervalMsec);

		if (timerIntervalMsec == 0)
			return;

		unique_lock<mutex> lock(_mutex);
		
		while (_isRunning)
		{
			_logger.Log("Calling Timer method ...\n");

			_timerMethod();

			_isRunningCondition.wait_for(
				lock,
				chrono::milliseconds(timerIntervalMsec),
				[&]() {return !_isRunning; });
		}
		
		_logger.Log("Exited Timer thread\n");
	}
}