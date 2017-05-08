#include <windows.h>
#include "TraceLogger.h"
#include "FormattedString.h"
#include <debugapi.h>

using namespace std;

void TraceLogger::Log(const char* format, ...)
{
	va_list args;
	va_start(args, format);

	OutputDebugStringA(
		static_cast<const char*>(FormattedString(FormatArgs(args, format))));

	va_end(args);	
}
