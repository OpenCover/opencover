#include "StdAfx.h"
#include "CoverageInstrumentation.h"

#include <algorithm>

#ifdef DEBUG
// uncommment to get debug builds to dump out instrumented functions (slow)
#define DUMP_PT 1
#endif

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