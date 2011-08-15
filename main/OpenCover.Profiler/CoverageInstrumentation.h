#pragma once

#include "method.h"
#include "Messages.h"

class CoverageInstrumentation :
    public Method
{
public:
    CoverageInstrumentation(IMAGE_COR_ILMETHOD* pMethod);
    ~CoverageInstrumentation(void);

public:
#if _WIN64
    void AddBranchCoverage(mdSignature pvsig, ULONGLONG pt, std::vector<BranchPoint> points);
    void AddSequenceCoverage(mdSignature pvsig, ULONGLONG pt, std::vector<BranchPoint> points);
#else
    void AddBranchCoverage(mdSignature pvsig, ULONG pt, std::vector<BranchPoint> points);
    void AddSequenceCoverage(mdSignature pvsig, ULONG pt, std::vector<SequencePoint> points);
#endif

};

