#include "StdAfx.h"
#include "CoverageInstrumentation.h"

#include <algorithm>

CoverageInstrumentation::CoverageInstrumentation(IMAGE_COR_ILMETHOD* pMethod) : Method(pMethod)
{
}


CoverageInstrumentation::~CoverageInstrumentation(void)
{
}

void CoverageInstrumentation::AddSequenceCoverage(mdMethodDef methodDef, std::vector<SequencePoint> points)
{
#ifdef DEBUG
    int i = 0;
    for (auto it = points.begin(); it != points.end(); it++)
    {    
        ATLTRACE(_T("SEQPT %04d IL_%04X"), i++, (*it).Offset);
    }
#endif

    for (auto it = points.begin(); it != points.end(); it++)
    {    
        InstructionList instructions;

        CreateInstrumentationBlock(instructions, methodDef, (*it).UniqueId);

        InsertInstructionsAtOriginalOffset((*it).Offset, instructions);
    }
}

void CoverageInstrumentation::AddBranchCoverage(mdMethodDef methodDef, std::vector<BranchPoint> points)
{
    if (points.size() == 0) return;

#ifdef DEBUG
    int i = 0;
    for (auto bit = points.begin(); bit != points.end(); bit++)
    {
        ATLTRACE(_T("BRPT %04d IL_%04X %d"), i++, (*bit).Offset, (*bit).Path);
    }
#endif

    for (auto it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        if ((*it)->m_isBranch && ((*it)->m_origOffset != -1))
        {
            OperationDetails &details = Operations::m_mapNameOperationDetails[(*it)->m_operation];
            if (details.controlFlow == COND_BRANCH)
            {
                Instruction *pCurrent = *it;
                ++it;
                Instruction *pNext = *it;

                int idx = 0;

                InstructionList instructions;

                CreateInstrumentationBlock(instructions, methodDef, 
                    (*std::find_if(points.begin(), points.end(), [pCurrent, idx](BranchPoint &bp){return bp.Offset == pCurrent->m_origOffset && bp.Path == idx;})).UniqueId);

                Instruction *pJumpNext = new Instruction(CEE_BR);
                pJumpNext->m_isBranch = true;
                instructions.push_back(pJumpNext);
                pJumpNext->m_branches.push_back(pNext);
               
                for(auto sbit = pCurrent->m_branches.begin(); sbit != pCurrent->m_branches.end(); sbit++)
                {
                    idx++;
                    Instruction *pRecordJmp = CreateInstrumentationBlock(instructions, methodDef, 
                        (*std::find_if(points.begin(), points.end(), [pCurrent, idx](BranchPoint &bp){return bp.Offset == pCurrent->m_origOffset && bp.Path == idx;})).UniqueId); 

                    Instruction *pSwitchJump = new Instruction(CEE_BR);
                    pSwitchJump->m_isBranch = true;
                    instructions.push_back(pSwitchJump);
                    pSwitchJump->m_branches.push_back(*sbit);
                    *sbit = pRecordJmp;
                }

                m_instructions.insert(it, instructions.begin(), instructions.end());
                for (it = m_instructions.begin(); *it != pNext; ++it);
            }
        }
    }
    RecalculateOffsets();
}

Instruction* CoverageInstrumentation::CreateInstrumentationBlock(InstructionList &instructions, 
    mdMethodDef methodDef, ULONGLONG uniqueId)
{
    Instruction *firstInstruction = new Instruction(CEE_LDC_I4, uniqueId);

    instructions.push_back(firstInstruction);
    instructions.push_back(new Instruction(CEE_CALL, methodDef));

    return firstInstruction;
}
