#include "StdAfx.h"
#include "CoverageInstrumentation.h"

#include <algorithm>

namespace CoverageInstrumentation
{
	using namespace Instrumentation;

	Instruction* InsertInjectedMethod(InstructionList &instructions, mdMethodDef injectedMethodDef, ULONG uniqueId)
	{
		Instruction *firstInstruction = new Instruction(CEE_LDC_I4, uniqueId);
		instructions.push_back(firstInstruction);
		instructions.push_back(new Instruction(CEE_CALL, injectedMethodDef));
		return firstInstruction;
	}

	Instruction* InsertFunctionCall(InstructionList &instructions, mdSignature pvsig, FPTR pt, ULONGLONG uniqueId)
	{
		Instruction *firstInstruction = new Instruction(CEE_LDC_I4, uniqueId);
		instructions.push_back(firstInstruction);
#ifdef _WIN64
		instructions.push_back(new Instruction(CEE_LDC_I8, (ULONGLONG)pt));
#else
		instructions.push_back(new Instruction(CEE_LDC_I4, (ULONG)pt));
#endif
		instructions.push_back(new Instruction(CEE_CALLI, pvsig));

		return firstInstruction;
	}
}