#pragma once

#include "ILogger.h"

class NullLogger : public ILogger
{
public:

	void Log(const char*, ...) override final {}
};
