#pragma once

#include "ILogger.h"
#include <thread>
#include <condition_variable>
#include <functional>

namespace Communication
{
	class Timer
	{
	public:

		Timer();

		void Start(
			std::function<void()> timerMethod,
			int timerIntervalMsec);

		~Timer();
		
		void Stop();

	private:

		void TimerMethod(int timerIntervalMsec);

		ILogger& _logger;
		std::function<void()> _timerMethod;
		std::mutex _mutex;
		std::condition_variable _isRunningCondition;
		bool _isRunning;
		std::thread _thread;
	};
}
