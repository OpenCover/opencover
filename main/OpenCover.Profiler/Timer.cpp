#include "StdAfx.h"
#include "Timer.h"

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
		std::function<void()> timerMethod,
		int timerIntervalMsec)
	{
		Stop();
		_timerMethod = timerMethod;
		_isRunning = true;
		_thread = std::thread([=]()
		{
			StartTimerMethod(timerIntervalMsec);
		});
	}

	void Timer::Stop()
	{
		if (_thread.native_handle() != nullptr)
		{
			StopTimerMethod();
			_thread.join();
		}
	}

	void Timer::StopTimerMethod()
	{
		std::unique_lock<std::mutex> lock(_mutex);
		_isRunning = false;
		_isRunningCondition.notify_one();
	}

	void Timer::StartTimerMethod(int timerIntervalMsec)
	{
		if (timerIntervalMsec == 0)
			return;

	    ATLTRACE(_T("Timer : Started thread with interval %d msec"), timerIntervalMsec);

		std::unique_lock<std::mutex> lock(_mutex);
		
		while (_isRunning)
		{
			_isRunningCondition.wait_for(
				lock,
				std::chrono::milliseconds(timerIntervalMsec),
				[&]() {return !_isRunning; });

			if (_isRunning)
			{
				_timerMethod();
			}
		}
		
		ATLTRACE(_T("Timer : Exited thread"));
	}
}