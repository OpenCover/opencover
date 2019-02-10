#pragma once
#ifndef SONAR_MACROS_h__
#define SONAR_MACROS_h__

/* 
using https://github.com/bjd/sonar-cxx/blob/master/sonar-cxx-plugin/src/main/resources/macros/VS10Macros.h 
as a template as the original has been removed from sonar-cxx project but only adding those we need for now
*/

#define _DLL 1
#define _WINDOWS 1
#define UNICODE 1
#define _UNICODE 1
#define _WINDLL 1
#define _USRDLL 1
#define _ATL_STATIC_REGISTRY 1

// disable assert for production version
#define NDEBUG 1

// STRICT Type Checking: 
#define STRICT 1

// Defines the MFC version.
#if defined(_AFXDLL) || defined(_USRDLL)
#define _MFC_VER 0x0A00
#endif

// Defines the ATL version.
#if defined(_ATL_DLL) || defined(_ATL_STATIC_REGISTRY) || defined(_MFC_VER)
#define _ATL_VER 0x0A00
#endif

// Defined when /Zc:wchar_t is used or if wchar_t is defined in a system header file
// included in your project.
#define _WCHAR_T_DEFINED 1

// Determines the minimum platform SDK required to build your application.
#define WINVER 0x0600
#define _WIN32_WINNT 0x0600
#define NTDDI_VERSION 0x06000000

// Defined for applications for Win32 and Win64. Always defined.
#define WIN32 1
#define _WIN32 1
#define __WIN32 1

// Defined for applications for Win64.
#define _WIN64 

// function calling conventions
//
#define cdecl
#define _cdecl
#define __cdecl
#define _clrcall
#define __clrcall
#define _stdcall
#define __stdcall
#define _fastcall
#define __fastcall
#define __thiscall
#define __vectorcall

// extended storage-class attributes 
//
#define _declspec(...)
#define __declspec(...)

// __w64 keyword
//
#define _w64
#define __w64

// types
//
#define _int8 char 
#define __int8 char 
#define _int16 short 
#define __int16 short 
#define _int32 int 
#define __int32 int 
#define _int64 long long 
#define __int64 long long 
#define __ptr32
#define __ptr64
#define __wchar_t wchar_t
#define __handle

// alignment requirement of the type
//
#define __alignof(type) 1

// try/except statement
//
#define __try try
#define __except(...) catch(int e)
#define __finally catch(...) {}
#define __leave

// conditionally include code depending on whether the specified symbol exists
//
#define __if_exists(v)
#define __if_not_exists(v)

// other keywords
//
#define __assume(a)
#define __uuidof(X) IID_IUnknown
#define __noop (void(0))
#define __pragma(a)
#define __super
#define __debug
#define __emit__(...)

#endif // SONAR_MACROS_h__