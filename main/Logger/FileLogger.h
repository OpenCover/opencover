#pragma once

#include "ILogger.h"
#include <string>

class FileLogger : public ILogger
{
public:

	FileLogger(const char*fileName);

	void Log(const char*, ...) override;

private:

	std::string _fileName;
};
