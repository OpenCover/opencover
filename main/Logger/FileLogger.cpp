#include "FileLogger.h"
#include <fstream>
#include "FormattedString.h"

using namespace std;

FileLogger::FileLogger(const char* fileName) :
	_fileName (fileName)
{
}

void FileLogger::Log(const char* format, ...)
{
	va_list args;
	va_start(args, format);

	ofstream out(_fileName, ios::app);
	out << static_cast<const char*>(FormattedString(FormatArgs(args, format)));

	va_end(args);	
}
