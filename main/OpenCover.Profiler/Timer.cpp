#include "StdAfx.h"
#include "Timer.h"

using namespace std;

namespace Communication
{
	Timer::Timer() :
		_isRunning(false)
	{
	}

	Timer::~Timer()
	{
		Stop();	
	}

	void Timer::Start(
		function<void()> timerMethod,
		int timerIntervalMsec)
	{
		_timerMethod = timerMethod;
		_isRunning = true;
		_thread = thread([=]()
		{
			StartTimerMethod(timerIntervalMsec);
		});
	}

	void Timer::Stop()
	{
		StopTimerMethod();
		if (_thread.native_handle() != nullptr)
		{
			_thread.join();
		}
	}

	void Timer::StopTimerMethod()
	{
		unique_lock<mutex> lock(_mutex);
		_isRunning = false;
		_isRunningCondition.notify_one();
	}

	void Timer::StartTimerMethod(int timerIntervalMsec)
	{
		if (timerIntervalMsec == 0)
			return;

	    ATLTRACE(_T("Timer : Started thread with interval %d msec"), timerIntervalMsec);

		unique_lock<mutex> lock(_mutex);
		
		while (_isRunning)
		{
			_isRunningCondition.wait_for(
				lock,
				chrono::milliseconds(timerIntervalMsec),
				[&]() {return !_isRunning; });

			if (_isRunning)
			{
				_timerMethod();
			}
		}
		
		ATLTRACE(_T("Timer : Exited thread"));
	}
}