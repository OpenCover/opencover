#pragma once

#include "ILogger.h"
#include <mutex>

class TraceLogger : public ILogger
{
public:

	void Log(const char*, ...) override;

private:

	std::mutex _mutex;
};
