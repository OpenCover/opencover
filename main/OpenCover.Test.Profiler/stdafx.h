// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

#include "targetver.h"

#define _CRT_SECURE_NO_WARNINGS

#ifdef _DEBUG
#define _CRTDBG_MAP_ALLOC
#include <stdlib.h>
#include <crtdbg.h>
#endif

#include <stdio.h>
#include <tchar.h>

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS      // some CString constructors will be explicit

#include <atlbase.h>
#include <atlstr.h>
#include <atlcom.h>
#include <atlctl.h>

#pragma pack(push)
#pragma pack(4)

#include <cor.h>
#include <corsym.h> 
#include <corprof.h>
#include <corhlpr.h>

#pragma pack(pop)

#include <string>
#include <vector>
#include <unordered_map>
#include <memory>

#ifdef UNICODE
#define tstring std::wstring
#else
#define tstring std::string
#endif

// This line ensures that gmock.h can be compiled on its own, even
// when it's fused.
#include <gmock/gmock.h>

// This line ensures that gtest.h can be compiled on its own, even
// when it's fused.
#include <gtest/gtest.h>

/*
#define _TEST_METHOD_EX_EXPANDER(_testMethod)\
_testMethod { try

// Adds support for seeing std::exception in test output. Requires TEST_METHOD_EX_END after test.
// Example:
// TEST_METHOD_EX_BEGIN(MyFailingTest){ throw std::exception("What happened"); } TEST_METHOD_EX_END;
#define TEST_METHOD_EX_BEGIN(_methodName) _TEST_METHOD_EX_EXPANDER(TEST_METHOD(_methodName))

// Use following test declared with TEST_METHOD_EX_BEGIN
#define TEST_METHOD_EX_END\
catch (::std::exception& ex) \
{ \
::std::wstringstream ws; ws << "Unhandled Exception:" << ::std::endl << ex.what(); \
::Microsoft::VisualStudio::CppUnitTestFramework::Assert::Fail(ws.str().c_str());\
} \
}

#define _TEST_METHOD_CLEANUP_EX_EXPANDER(_testCleanUp)\
_testCleanUp { try

// Adds support for seeing std::exception in test output. Requires TEST_METHOD_CLEANUP_EX_END after CleanUp.
// Example:
// TEST_METHOD_CLEANUP_EX_BEGIN(CleanUp){ throw std::exception("What happened"); } TEST_METHOD_CLEANUP_EX_END;
#define TEST_METHOD_CLEANUP_EX_BEGIN(_methodName) _TEST_METHOD_CLEANUP_EX_EXPANDER(TEST_METHOD_CLEANUP(_methodName))

// Use following test declared with TEST_METHOD_CLEANUP_EX_BEGIN
#define TEST_METHOD_CLEANUP_EX_END\
catch (::std::exception& ex) \
{ \
::std::wstringstream ws; ws << "Unhandled Exception:" << ::std::endl << ex.what(); \
::Microsoft::VisualStudio::CppUnitTestFramework::Assert::Fail(ws.str().c_str());\
} \
}

*/
