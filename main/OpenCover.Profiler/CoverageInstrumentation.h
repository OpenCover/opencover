#pragma once

#include "method.h"
#include "Messages.h"

#if _WIN64
typedef unsigned __int64 FPTR; 
#else
typedef unsigned long FPTR;
#endif


class CoverageInstrumentation :
    public Method
{
public:
    CoverageInstrumentation(IMAGE_COR_ILMETHOD* pMethod);
    ~CoverageInstrumentation(void);

public:
    void AddBranchCoverage(mdSignature pvsig, FPTR pt, std::vector<BranchPoint> points);
    void AddSequenceCoverage(mdSignature pvsig, FPTR pt, std::vector<SequencePoint> points);

private:
    Instruction* CreateInstrumentationBlock(InstructionList &instructions, mdSignature pvsig, FPTR pt, ULONGLONG uniqueId);
};

