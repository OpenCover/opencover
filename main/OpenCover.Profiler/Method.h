//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#pragma once
#include "Instruction.h"
#include "ExceptionHandler.h"
#include "MethodBuffer.h"

namespace Instrumentation
{
	/// <summary>The <c>Method</c> entity builds a 'model' of the IL that can then be modified</summary>
	class Method :
		public MethodBuffer
	{
	public:
		explicit Method(IMAGE_COR_ILMETHOD* pMethod);
		~Method();

	public:
		long GetMethodSize();
		void WriteMethod(IMAGE_COR_ILMETHOD* pMethod);
		void InsertInstructionsAtOriginalOffset(long origOffset, const InstructionList &instructions);
		void InsertInstructionsAtOffset(long offset, const InstructionList &instructions);
		void DumpIL(bool enableDump);
		ULONG GetILMapSize();
		void PopulateILMap(ULONG mapSize, COR_IL_MAP* maps);

		bool IsInstrumented(long offset, const InstructionList &instructions);

	public:
		void SetMinimumStackSize(unsigned int minimumStackSize)
		{
			if (m_header.MaxStack < minimumStackSize)
			{
				m_header.MaxStack = minimumStackSize;
			}
		}

		void IncrementStackSize(unsigned int extraStackSize)
		{
			m_header.MaxStack += extraStackSize;
		}

		DWORD GetCodeSize() const { return m_header.CodeSize; }


	public:
		void RecalculateOffsets();

	private:
		void ReadMethod(IMAGE_COR_ILMETHOD* pMethod);
		void ReadBody();

		void ConvertShortBranches();
		void ResolveBranches();
		void DumpExceptionFilters();
		void DumpInstructions();
		Instruction * GetInstructionAtOffset(long offset);
		Instruction * GetInstructionAtOffset(long offset, bool isFinally, bool isFault, bool isFilter, bool isTyped);
		void ReadSections();

		template<class flag, class start, class end>
		void ReadExceptionHandlers(int count);

		ExceptionHandler* ReadExceptionHandler(enum CorExceptionFlag type, long tryStart, long tryEnd, long handlerStart, long handlerEnd, long filterStart, ULONG token);

		void WriteSections();
		bool DoesTryHandlerPointToOffset(long offset);

	private:
		// all instrumented methods will be FAT (with FAT SECTIONS if exist) regardless
		IMAGE_COR_ILMETHOD_FAT m_header;

#ifdef TEST_FRAMEWORK
	public:
		ExceptionHandlerList m_exceptions;
#else
	private:
		ExceptionHandlerList m_exceptions;
#endif
	public:
		InstructionList m_instructions;

		int GetNumberOfInstructions() const
		{
			return static_cast<int>(m_instructions.size());
		}

		int GetNumberOfExceptions() const
		{
			return static_cast<int>(m_exceptions.size());
		}
	};
}