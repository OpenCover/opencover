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
    void AddBranchCoverage(mdMethodDef methodDef, std::vector<BranchPoint> points);
    void AddSequenceCoverage(mdMethodDef methodDef, std::vector<SequencePoint> points);

private:
    Instruction* CreateInstrumentationBlock(InstructionList &instructions,mdMethodDef methodDef, ULONGLONG uniqueId);
};

