#include "StdAfx.h"
#include "CoverageInstrumentation.h"

#include <algorithm>

CoverageInstrumentation::CoverageInstrumentation(IMAGE_COR_ILMETHOD* pMethod) : Method(pMethod)
{
}


CoverageInstrumentation::~CoverageInstrumentation(void)
{
}

#if _WIN64
void CoverageInstrumentation::AddSequenceCoverage(mdSignature pvsig, ULONGLONG pt, std::vector<SequencePoint> points)
#else
void CoverageInstrumentation::AddSequenceCoverage(mdSignature pvsig, ULONG pt, std::vector<SequencePoint> points)
#endif
{
    for (std::vector<SequencePoint>::iterator it = points.begin(); it != points.end(); it++)
    {    
        //ATLTRACE(_T("SEQPT %02d IL_%04X"), i, ppPoints[i]->Offset);
        InstructionList instructions;
        instructions.push_back(new Instruction(CEE_LDC_I4, (*it).UniqueId));
#if _WIN64
        instructions.push_back(new Instruction(CEE_LDC_I8, (ULONGLONG)pt));
#else
        instructions.push_back(new Instruction(CEE_LDC_I4, (ULONG)pt));
#endif
        instructions.push_back(new Instruction(CEE_CALLI, pvsig));

        InsertSequenceInstructionsAtOriginalOffset((*it).Offset, instructions);
    }
}

struct FindBranchPoint
{
    long offset;
    long path;

    FindBranchPoint(long offset, long path) : offset(offset), path(path) {}

    bool operator()(BranchPoint& bp) {
        return (bp.Offset == offset && bp.Path == path);
    }
};

#if _WIN64
void CoverageInstrumentation::AddBranchCoverage(mdSignature pvsig, ULONGLONG pt, std::vector<BranchPoint> points)
#else
void CoverageInstrumentation::AddBranchCoverage(mdSignature pvsig, ULONG pt, std::vector<BranchPoint> points)
#endif
{
    if (points.size() == 0) return;
    for (InstructionListIter it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        if ((*it)->m_isBranch && ((*it)->m_origOffset != -1))
        {
            OperationDetails &details = Operations::m_mapNameOperationDetails[(*it)->m_operation];
            if (details.controlFlow == COND_BRANCH)
            {
                if (details.canonicalName != CEE_SWITCH)
                {
                    Instruction *pCurrent = *it;
                    Instruction *pOriginalTarget = pCurrent->m_branches[0];
                    
                    FindBranchPoint fpb0(pCurrent->m_origOffset, 0);
                    FindBranchPoint fpb1(pCurrent->m_origOffset, 1);
                    
                    ++it;
                    Instruction *pNext = *it;

                    InstructionList instructions;
                    instructions.push_back(new Instruction(CEE_LDC_I4, (*std::find_if(points.begin(), points.end(), fpb0)).UniqueId));
#if _WIN64
                    instructions.push_back(new Instruction(CEE_LDC_I8, (ULONGLONG)pt));
#else
                    instructions.push_back(new Instruction(CEE_LDC_I4, (ULONG)pt));
#endif
                    instructions.push_back(new Instruction(CEE_CALLI, pvsig));

                    Instruction *pJumpNext = new Instruction(CEE_BR);
                    pJumpNext->m_isBranch = true;
                    instructions.push_back(pJumpNext);

                    Instruction *pRecordJmp = new Instruction(CEE_LDC_I4, (*std::find_if(points.begin(), points.end(), fpb1)).UniqueId);
                    instructions.push_back(pRecordJmp);
#if _WIN64
                    instructions.push_back(new Instruction(CEE_LDC_I8, (ULONGLONG)pt));
#else
                    instructions.push_back(new Instruction(CEE_LDC_I4, (ULONG)pt));
#endif
                    instructions.push_back(new Instruction(CEE_CALLI, pvsig));

                    Instruction *pJumpTarget = new Instruction(CEE_BR);
                    pJumpTarget->m_isBranch = true;
                    instructions.push_back(pJumpTarget);
                    
                    // wire up
                    pJumpNext->m_branches.push_back(pNext);
                    pJumpTarget->m_branches.push_back(pOriginalTarget);
                    pCurrent->m_branches[0] = pRecordJmp;
                    
                    m_instructions.insert(it, instructions.begin(), instructions.end());
                    for (it = m_instructions.begin(); *it != pJumpTarget; ++it);
                    ++it;
                }
                else
                {
                    Instruction *pCurrent = *it;
                    ++it;
                    Instruction *pNext = *it;

                    FindBranchPoint fpb0(pCurrent->m_origOffset, -1);

                    InstructionList instructions;

                    instructions.push_back(new Instruction(CEE_LDC_I4, (*std::find_if(points.begin(), points.end(), fpb0)).UniqueId));
#if _WIN64
                    instructions.push_back(new Instruction(CEE_LDC_I8, (ULONGLONG)pt));
#else
                    instructions.push_back(new Instruction(CEE_LDC_I4, (ULONG)pt));
#endif
                    instructions.push_back(new Instruction(CEE_CALLI, pvsig));

                    Instruction *pJumpNext = new Instruction(CEE_BR);
                    pJumpNext->m_isBranch = true;
                    instructions.push_back(pJumpNext);
                    pJumpNext->m_branches.push_back(pNext);

                    int idx = 0;
                    for(InstructionListIter sbit = pCurrent->m_branches.begin(); sbit != pCurrent->m_branches.end(); sbit++)
                    {
                        FindBranchPoint fpb(pCurrent->m_origOffset, idx++);
                        Instruction *pRecordJmp = new Instruction(CEE_LDC_I4, (*std::find_if(points.begin(), points.end(), fpb)).UniqueId); 
                        instructions.push_back(pRecordJmp);
#if _WIN64
                        instructions.push_back(new Instruction(CEE_LDC_I8, (ULONGLONG)pt));
#else
                        instructions.push_back(new Instruction(CEE_LDC_I4, (ULONG)pt));
#endif
                        instructions.push_back(new Instruction(CEE_CALLI, pvsig));

                        Instruction *pSwitchJump = new Instruction(CEE_BR);
                        pSwitchJump->m_isBranch = true;
                        instructions.push_back(pSwitchJump);
                        pSwitchJump->m_branches.push_back(*sbit);
                        *sbit = pRecordJmp;
                    }

                    m_instructions.insert(it, instructions.begin(), instructions.end());
                    for (it = m_instructions.begin(); *it != pNext; ++it);
                    ++it;
                }
            }
        }
    }
    RecalculateOffsets();
}
