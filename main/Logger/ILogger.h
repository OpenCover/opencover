#pragma once

class ILogger
{
public:
	virtual ~ILogger() {};
	virtual void Log(const char*, ...) = 0;
};

__declspec (dllexport) ILogger& GetFileLogger(const char* fileName);
__declspec (dllexport) ILogger& GetTraceLogger();
__declspec (dllexport) ILogger& GetNullLogger();
