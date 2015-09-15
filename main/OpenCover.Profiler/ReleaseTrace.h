//
// OpenCover - S Wilde
//
// This code was unashamedly cobbled from atltrace.h to provide release build available tracing
//

#pragma once

class CReleaseTrace
{
    public:
	    CReleaseTrace()
	    {
	}

	const char* PREFIX = "OpenCover: (Profiler) ";
	const wchar_t* WPREFIX = L"OpenCover: (Profiler) ";

#pragma warning(push)
#pragma warning(disable : 4793)
	void __cdecl operator()(
		const char *pszFmt, 
		...) const
	{		
		va_list ptr; va_start(ptr, pszFmt);
        int nBytes = _vscprintf(pszFmt, ptr) + 1;
        va_end(ptr);
				
		auto prefixLength = strlen(PREFIX);
		std::vector<char> buffer(nBytes + prefixLength);
		sprintf_s(&buffer[0], prefixLength + 1, "%s", PREFIX);

        va_start(ptr, pszFmt);
		_vsnprintf_s(&buffer[prefixLength], nBytes, nBytes - 1, pszFmt, ptr);
        va_end(ptr);

        ::OutputDebugStringA(&buffer[0]);
	}
#pragma warning(pop)

#pragma warning(push)
#pragma warning(disable : 4793)
	void __cdecl operator()(
		const wchar_t *pszFmt, 
		...) const
	{
		va_list ptr; va_start(ptr, pszFmt);
        int nBytes = _vscwprintf(pszFmt, ptr) + 1;
        va_end(ptr);

		auto prefixLength = wcslen(WPREFIX);
		std::vector<wchar_t> buffer(nBytes + prefixLength);
		swprintf_s(&buffer[0], prefixLength + 1, L"%s", WPREFIX);
		
        va_start(ptr, pszFmt);
		_vsnwprintf_s(&buffer[prefixLength], nBytes, nBytes - 1, pszFmt, ptr);
        va_end(ptr);

        ::OutputDebugStringW(&buffer[0]);
	}
#pragma warning(pop)

};

#define RELTRACE CReleaseTrace()

#ifdef _DEBUG
#undef ATLTRACE
#define ATLTRACE CReleaseTrace()
#endif