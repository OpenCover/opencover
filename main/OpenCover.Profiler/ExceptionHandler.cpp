//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#include "StdAfx.h"
#include "ExceptionHandler.h"

namespace Instrumentation
{
	ExceptionHandler::ExceptionHandler()
	{
		m_tryStart = nullptr;
		m_tryEnd = nullptr;
		m_handlerStart = nullptr;
		m_handlerEnd = nullptr;
		m_filterStart = nullptr;
		m_token = 0;
		m_handlerType = COR_ILEXCEPTION_CLAUSE_NONE;
	}
}