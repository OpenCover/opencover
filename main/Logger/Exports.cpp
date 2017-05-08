#include "FileLogger.h"
#include "TraceLogger.h"
#include "NullLogger.h"

__declspec (dllexport) ILogger& GetFileLogger(const char* fileName)
{
	static FileLogger logger(fileName);
	return logger;
}

__declspec (dllexport) ILogger& GetTraceLogger()
{
	static TraceLogger logger;
	return logger;
}

__declspec (dllexport) ILogger& GetNullLogger()
{
	static NullLogger logger;
	return logger;
}
