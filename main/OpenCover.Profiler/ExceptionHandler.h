//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#include "Instruction.h"

#pragma once

namespace Instrumentation
{
	class ExceptionHandler;
	class Method;

	typedef std::vector<ExceptionHandler *> ExceptionHandlerList;

	/// <summary>A representation of a try/catch section handler</summary>
	class ExceptionHandler
	{
	public:
		ExceptionHandler();

#ifdef TEST_FRAMEWORK
	public:
#else
	private:
#endif
		CorExceptionFlag m_handlerType;
		Instruction * m_tryStart;
		Instruction * m_tryEnd;
		Instruction * m_handlerStart;
		Instruction * m_handlerEnd;
		Instruction * m_filterStart;

		ULONG m_token;

	public:
		friend class Method;
	};
}