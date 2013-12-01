#pragma once

#include "method.h"
#include "Messages.h"
#include <algorithm>

#if _WIN64
typedef unsigned __int64 FPTR; 
#else
typedef unsigned long FPTR;
#endif

namespace CoverageInstrumentation
{
    template<class IM>
    inline void AddSequenceCoverage(IM instrumentMethod, Method& method, std::vector<SequencePoint> points)
    {
        if (points.size() == 0) return;
        for (auto it = points.begin(); it != points.end(); it++)
        {    
            InstructionList instructions;
            instrumentMethod(instructions, (*it).UniqueId);
            method.InsertInstructionsAtOriginalOffset((*it).Offset, instructions);
        }
    }

    template<class IM>
    void AddBranchCoverage(IM instrumentMethod, Method& method, std::vector<BranchPoint> points)
    {
#define MOVE_TO_ENDOFBRANCH FALSE
        if (points.size() == 0) return;

        for (auto it = method.m_instructions.begin(); it != method.m_instructions.end(); ++it)
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

                    ULONG uniqueId = (*std::find_if(points.begin(), points.end(), [pCurrent, idx](BranchPoint &bp){return bp.Offset == pCurrent->m_origOffset && bp.Path == idx;})).UniqueId; 

#if MOVE_TO_ENDOFBRANCH
                    instructions.clear();
                    Instruction* pElse = instrumentMethod(instructions, uniqueId);
                    Instruction* toInstrument = method.EndOfBranch( pNext );
                    if ( toInstrument->m_prev != NULL ) // rewire last BR instruction?
                        toInstrument->m_prev->m_branches[0] = pElse; 
                    method.InsertInstructionsAtOffset( toInstrument->m_offset, instructions );
#else
                    instrumentMethod(instructions, uniqueId);

                    Instruction *pJumpNext = new Instruction(CEE_BR);
                    pJumpNext->m_isBranch = true;
                    instructions.push_back(pJumpNext);
                    pJumpNext->m_branches.push_back(pNext);
#endif

                    for(auto sbit = pCurrent->m_branches.begin(); sbit != pCurrent->m_branches.end(); sbit++)
                    {
                        idx++;
                        uniqueId = (*std::find_if(points.begin(), points.end(), [pCurrent, idx](BranchPoint &bp){return bp.Offset == pCurrent->m_origOffset && bp.Path == idx;})).UniqueId;

#if MOVE_TO_ENDOFBRANCH
                        instructions.clear();
                        Instruction* pRecordJmp = instrumentMethod(instructions, uniqueId);
                        Instruction* toInstrument = method.EndOfBranch( *sbit );
                        if ( toInstrument->m_prev == NULL ) // rewire branch direct to instrumentation?
                            *sbit = pRecordJmp; 
                        else // rewire indirect via last BR instruction
                            toInstrument->m_prev->m_branches[0] = pRecordJmp; 
                        method.InsertInstructionsAtOffset( toInstrument->m_offset, instructions );
#else
                        Instruction *pRecordJmp = instrumentMethod(instructions, uniqueId); 

                        Instruction *pSwitchJump = new Instruction(CEE_BR);
                        pSwitchJump->m_isBranch = true;
                        instructions.push_back(pSwitchJump);
                        pSwitchJump->m_branches.push_back(*sbit);
                        *sbit = pRecordJmp;
#endif
                    }

#if MOVE_TO_ENDOFBRANCH
#else
                    // *it points here at pNext
                    method.m_instructions.insert(it, instructions.begin(), instructions.end());
                    // restore 'it' position
                    for (it = method.m_instructions.begin(); *it != pNext; ++it);
#endif
                }
            }
        }
        method.RecalculateOffsets();
    }

    Instruction* InsertInjectedMethod(InstructionList &instructions, mdMethodDef injectedMethodDef, ULONG uniqueId);
    Instruction* InsertFunctionCall(InstructionList &instructions, mdSignature pvsig, FPTR pt, ULONGLONG uniqueId);

#undef MOVE_TO_ENDOFBRANCH
};

