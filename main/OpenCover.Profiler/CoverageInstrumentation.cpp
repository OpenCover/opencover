#include "StdAfx.h"
#include "CoverageInstrumentation.h"

#include <algorithm>

CoverageInstrumentation::CoverageInstrumentation(IMAGE_COR_ILMETHOD* pMethod) : Method(pMethod)
{
}


CoverageInstrumentation::~CoverageInstrumentation(void)
{
}

void CoverageInstrumentation::AddSequenceCoverage(mdSignature pvsig, FPTR pt, std::vector<SequencePoint> points)
{
    for (std::vector<SequencePoint>::iterator it = points.begin(); it != points.end(); it++)
    {    
        //ATLTRACE(_T("SEQPT IL_%04X"), (*it).Offset);
        InstructionList instructions;

        CreateInstrumentationBlock(instructions, pvsig, pt, (*it).UniqueId);

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

void CoverageInstrumentation::AddBranchCoverage(mdSignature pvsig, FPTR pt, std::vector<BranchPoint> points)
{
    if (points.size() == 0) return;

    /*
    for (std::vector<BranchPoint>::iterator bit = points.begin(); bit != points.end(); bit++)
    {
        ATLTRACE(_T("BRPT IL_%04X %d"), (*bit).Offset, (*bit).Path);
    }
    */

    for (InstructionListIter it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        if ((*it)->m_isBranch && ((*it)->m_origOffset != -1))
        {
            OperationDetails &details = Operations::m_mapNameOperationDetails[(*it)->m_operation];
            if (details.controlFlow == COND_BRANCH)
            {
                Instruction *pCurrent = *it;
                ++it;
                Instruction *pNext = *it;

                FindBranchPoint fpb0(pCurrent->m_origOffset, 0);

                InstructionList instructions;

                CreateInstrumentationBlock(instructions, pvsig, pt, 
                    (*std::find_if(points.begin(), points.end(), fpb0)).UniqueId);

                Instruction *pJumpNext = new Instruction(CEE_BR);
                pJumpNext->m_isBranch = true;
                instructions.push_back(pJumpNext);
                pJumpNext->m_branches.push_back(pNext);

                int idx = 1;
                for(InstructionListIter sbit = pCurrent->m_branches.begin(); sbit != pCurrent->m_branches.end(); sbit++)
                {
                    FindBranchPoint fpb(pCurrent->m_origOffset, idx++);

                    Instruction *pRecordJmp = CreateInstrumentationBlock(instructions, pvsig, pt, 
                        (*std::find_if(points.begin(), points.end(), fpb)).UniqueId); 

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
    RecalculateOffsets();
}

Instruction* CoverageInstrumentation::CreateInstrumentationBlock(InstructionList &instructions, 
    mdSignature pvsig, FPTR pt, ULONGLONG uniqueId)
{
    Instruction *firstInstruction = new Instruction(CEE_LDC_I4, uniqueId);

    instructions.push_back(firstInstruction);
#if _WIN64
    instructions.push_back(new Instruction(CEE_LDC_I8, (ULONGLONG)pt));
#else
    instructions.push_back(new Instruction(CEE_LDC_I4, (ULONG)pt));
#endif
    instructions.push_back(new Instruction(CEE_CALLI, pvsig));

    return firstInstruction;
}
