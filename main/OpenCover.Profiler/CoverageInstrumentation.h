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
#define MOVE_TO_ENDOFBRANCH TRUE
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

                    // istrument branch 0 == fall-through branch => 
                    //		switch instruction DEFAULT branch
                    //		br(if) instruction ELSE branch
                    ULONG uniqueId = (*std::find_if(points.begin(), points.end(), [pCurrent, idx](BranchPoint &bp){return bp.Offset == pCurrent->m_origOffset && bp.Path == idx;})).UniqueId; 

#if MOVE_TO_ENDOFBRANCH
                    // push-down instrumentation by following links (unconditional BR instructions)
                    // if two or more end-of-branch link to same instruction (branch-join-point),
                    // then visit counts will merge. ie:
                    // IL_100 branch instrumentation 1) - count visits for 1)
                    // IL_101 branch instrumentation 2) - count visits for 1) and 2)
                    // IL_102 branch instrumentation 3) - count visits for 1) and 2) and 3)
                    // IL_103 branch join-point
                    // But, at branch instrumentation visit-count doesn't really count :)
                    // What does count is visited or not-visited.
                    instructions.clear();
                    Instruction* pElse = instrumentMethod(instructions, uniqueId);
                    Instruction* toInstrument = pNext;
                    if (pCurrent->m_operation == CEE_SWITCH) {
                        // to minimize unwanted branch join/merge
                        // push down only (switch)DEFAULT branch
                        toInstrument = method.EndOfBranch( pNext ); // push SWITCH default branch down
                        if ( toInstrument->m_jump != NULL ) 
                        {
                            _ASSERTE(toInstrument != pNext);
                            _ASSERTE(toInstrument->m_jump->m_isBranch && toInstrument->m_jump->m_operation==CEE_BR);
                            _ASSERTE(toInstrument->m_jump->m_branches.size() == 1);
                            _ASSERTE(toInstrument->m_jump->m_branches[0] == toInstrument);
                            toInstrument->m_jump->m_branches[0] = pElse; // rewire BR/jump instruction to instrumentation
                        }
                        else
                        {
                            _ASSERTE(toInstrument == pNext);
                        }
                    }
                    for (it = method.m_instructions.begin(); *it != toInstrument; ++it);
                    method.m_instructions.insert(it, instructions.begin(), instructions.end());
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
                        Instruction* toInstrument = *sbit;
                        if (pCurrent->m_operation == CEE_SWITCH)
                        {
                            // to minimize unwanted join-merge
                            // do not push-down instrumentation for switch branches
                            *sbit = pRecordJmp; // rewire switch branch to instrumentation
                        }
                        else
                        {
                            toInstrument = method.EndOfBranch( *sbit );
                            if ( toInstrument->m_jump == NULL )
                            {
                                _ASSERTE(toInstrument == *sbit);
                                *sbit = pRecordJmp; // rewire branch to instrumentation
                            }
                            else 
                            {
                                _ASSERTE(toInstrument != *sbit);
                                _ASSERTE(toInstrument->m_jump->m_isBranch && toInstrument->m_jump->m_operation==CEE_BR );
                                _ASSERTE(toInstrument->m_jump->m_branches.size() == 1);
                                _ASSERTE(toInstrument->m_jump->m_branches[0] == toInstrument);
                                toInstrument->m_jump->m_branches[0] = pRecordJmp; // rewire last BR to instrumentation
                            }
                        }
                        for (it = method.m_instructions.begin(); *it != toInstrument; ++it);
                        method.m_instructions.insert(it, instructions.begin(), instructions.end());
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
#endif
                    // restore 'it' position
                    for (it = method.m_instructions.begin(); *it != pNext; ++it);
                }
            }
        }
        method.RecalculateOffsets();
    }

    Instruction* InsertInjectedMethod(InstructionList &instructions, mdMethodDef injectedMethodDef, ULONG uniqueId);
    Instruction* InsertFunctionCall(InstructionList &instructions, mdSignature pvsig, FPTR pt, ULONGLONG uniqueId);

#undef MOVE_TO_ENDOFBRANCH
};

