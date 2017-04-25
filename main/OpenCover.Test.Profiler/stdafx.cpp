// stdafx.cpp : source file that includes just the standard includes
// OpenCover.Test.Profiler.pch will be the pre-compiled header
// stdafx.obj will contain the pre-compiled type information

#include "stdafx.h"

// TODO: reference any additional headers you need in STDAFX.H
// and not in this file

CComModule _Module;
extern __declspec(selectany) CAtlModule* _pAtlModule = &_Module;

// The following lines pull in the real gmock *.cc files.
#include "src/gmock-cardinalities.cc"
#include "src/gmock-internal-utils.cc"
#include "src/gmock-matchers.cc"
#include "src/gmock-spec-builders.cc"
#include "src/gmock.cc"

// The following lines pull in the real gtest *.cc files.
#include <src/gtest.cc>
#include <src/gtest-death-test.cc>
#include <src/gtest-filepath.cc>
#include <src/gtest-port.cc>
#include <src/gtest-printers.cc>
#include <src/gtest-test-part.cc>
#include <src/gtest-typed-test.cc>

#include <src/gtest_main.cc>