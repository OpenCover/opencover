#include "FileLogger.h"
#include "TraceLogger.h"

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
