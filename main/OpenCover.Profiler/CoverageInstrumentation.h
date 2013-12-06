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
    void AddBranchCoverage(IM instrumentMethod, Method& method, std::vector<BranchPoint> points, std::vector<SequencePoint> seqPoints)
    {
        if (points.size() == 0) return;

        bool seqPointsBegin = false;
        auto iseqp = seqPoints.begin();
        SequencePoint currentSeqPoint;
        for (auto it = method.m_instructions.begin(); it != method.m_instructions.end(); ++it)
        {
            if (!seqPointsBegin) {
                if (iseqp != seqPoints.end()) {
                    if ((*it)->m_origOffset == (*iseqp).Offset) {
                        seqPointsBegin = true;
                        currentSeqPoint = (*iseqp);
                        ++iseqp;
            }   }   }
            else {
                if (iseqp != seqPoints.end()) {
                    if ((*it)->m_origOffset == (*iseqp).Offset) {
                        currentSeqPoint = (*iseqp);
                        ++iseqp;
            }   }   }
            if (seqPointsBegin) { (*it)->m_seqp = currentSeqPoint; }


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
                    ULONG storedId = uniqueId; // store branch 0 ID (default/else)

                    Instruction *pJumpNext = new Instruction(CEE_BR);
                    pJumpNext->m_isBranch = true;
                    pJumpNext->m_branches.push_back(pNext);

                    instructions.push_back(pJumpNext);

                    // collect branches instrumentation
                    for(auto sbit = pCurrent->m_branches.begin(); sbit != pCurrent->m_branches.end(); sbit++)
                    {
                        idx++;
                        uniqueId = (*std::find_if(points.begin(), points.end(), [pCurrent, idx](BranchPoint &bp){return bp.Offset == pCurrent->m_origOffset && bp.Path == idx;})).UniqueId;
                        Instruction* pBranchInstrument = instrumentMethod(instructions, uniqueId);
                        Instruction *pBranchJump = new Instruction(CEE_BR);
                        pBranchJump->m_isBranch = true;
                        pBranchJump->m_branches.push_back(*sbit);
                        instructions.push_back(pBranchJump);
                        *sbit = pBranchInstrument; // rewire conditional branch to instrumentation
                        
                        if (pCurrent->m_operation != CEE_SWITCH)
                        {
                            // add join
                            Instruction* pBranchEnd = method.EndOfBranch(pBranchJump->m_branches[0]);
                            if (pBranchEnd->m_jump == NULL)
                            {   // if no branch-chain exists, then add current branch to joins
                                _ASSERTE(pBranchJump != pBranchEnd);
                                _ASSERTE(pBranchJump->m_branches[0] == pBranchEnd);
                                _ASSERTE(pBranchJump->m_isBranch);
                                _ASSERTE(pBranchJump->m_branches.size() == 1);
                                _ASSERTE(pBranchJump->m_branches[0] == pBranchEnd);
                                pBranchEnd->m_seqp = pCurrent->m_seqp;
                                pBranchEnd->m_joins.push_back(pBranchJump);
                            }
                            else
                            {   // if branch-chain exists, then add last jump-branch to joins
                                _ASSERTE(pBranchEnd->m_jump != NULL);
                                _ASSERTE(pBranchEnd != pBranchJump->m_branches[0]);
                                _ASSERTE(pBranchEnd->m_jump->m_isBranch);
                                _ASSERTE(pBranchEnd->m_jump->m_branches.size() == 1);
                                _ASSERTE(pBranchEnd->m_jump->m_branches[0] == pBranchEnd);
                                pBranchEnd->m_jump->m_seqp = pCurrent->m_seqp;
                                pBranchEnd->m_joins.push_back(pBranchEnd->m_jump);
                            }
                        }
                    }
                    
                    // now instrument "default:" or "else" branch
                    Instruction* pDefaultEnd = method.EndOfBranch( pNext );
                    if (pCurrent->m_operation == CEE_SWITCH 
                        && pDefaultEnd->m_jump != NULL 
                        && pDefaultEnd->m_joins.size() != 0
                        && seqPointsBegin )
                    {   // switch "default:" branch ends up into "join" point

                        // add final join to be rewired
                        _ASSERTE(pDefaultEnd != pNext);
                        _ASSERTE(pDefaultEnd->m_jump->m_isBranch);
                        _ASSERTE(pDefaultEnd->m_jump->m_branches.size() == 1);
                        _ASSERTE(pDefaultEnd->m_jump->m_branches[0] == pDefaultEnd);
                        pDefaultEnd->m_joins.push_back(pDefaultEnd->m_jump);

                        // goal: join "default:" instrumentation with branch created before IL-switch
                        // why? compiler sometimes excludes Path 0 by BR instruction before IL-switch 

                        // insert not "default:" instrumentation at pNext
                        // ----------------------------------------------
                        //        IL_xx Conditional Branch instruction with arguments (at BranchPoint.Offset)
                        //        IL_xx BR pNext (Path 0)
                        //        IL_xx Path 1 Instrument
                        //        IL_xx pBranchJump back to original Path 1 Instruction
                        //        IL_xx Path 2 Instrument
                        //        IL_xx pBranchJump back to original Path 2 Instruction
                        //        IL_xx Path N.. Instrument
                        //        IL_xx pBranchJump back to original Path N.. Instruction
                        // pNext: IL_xx BR jump [chain] to pDefaultEnd join point 
                        
                        // insert not "default:" instrumentation at pNext
                        for (it = method.m_instructions.begin(); *it != pNext; ++it);
                        method.m_instructions.insert(it, instructions.begin(), instructions.end());
                        
                        // insert "default:" instrumentation at pDefaultEnd
                        // ------------------------------------------------
                        // pNext:       IL_xx BR jump to [optional] or to pDefaultEnd (=>last jump)
                        // ....
                        // [optional]   IL_xx BR jump to next [optional] jump
                        // ....
                        // [optional]   IL_xx BR jump to pDefaultEnd (=>last jump)
                        // ....
                        // pDefault:    IL_xx Path 0 Instrument
                        // pDefaultEnd: IL_xx whatever it is <- rewire incoming (joined) jumps from here to pDefault 

                        instructions.clear();
                        Instruction* pDefault = instrumentMethod(instructions, storedId);

                        // rewire pDefaultEnd joins
                        for(auto join = pDefaultEnd->m_joins.begin(); join != pDefaultEnd->m_joins.end(); join++)
                        {
                            _ASSERTE((*join)->m_isBranch);
                            _ASSERTE((*join)->m_branches.size() == 1);

                            // rewire only pCurrent-SequencePoint joins
                            if ((*join)->m_seqp.UniqueId == pCurrent->m_seqp.UniqueId)
                            {
                                // deal with rewired duplicates (two branches merging into same join path)
                                _ASSERTE((*join)->m_branches[0] == pDefaultEnd || (*join)->m_branches[0] == pDefault);
                                if ((*join)->m_branches[0] != pDefault)
                                    (*join)->m_branches[0] = pDefault; // rewire incoming branch to instrumentation
                            }                                    
                        }

                        // insert "default:" part of instrumentation at pDefaultEnd
                        for (it = method.m_instructions.begin(); *it != pDefaultEnd; ++it);
                        method.m_instructions.insert(it, instructions.begin(), instructions.end());
                    }
                    else
                    {
                        // here insert all instrumentation at pNext
                        // ----------------------------------------
                        //        IL_xx Conditional Branch instruction with arguments (at BranchPoint.Offset)
                        //        IL_xx BR pNext -> rewired to pElse (Path 0)
                        //        IL_xx Path 1 Instrument
                        //        IL_xx pBranchJump back to original Path 1 Instruction
                        //        IL_xx Path 2 Instrument
                        //        IL_xx pBranchJump back to original Path 2 Instruction
                        //        IL_xx Path N.. Instrument
                        //        IL_xx pBranchJump back to original Path N.. Instruction
                        // pElse: IL_xx Path 0 Instrument 
                        // pNext: IL_xx Whatever it is 
                        
                        Instruction* pElse = instrumentMethod(instructions, storedId);
                        pJumpNext->m_branches[0] = pElse; // rewire pJumpNext

                        for (it = method.m_instructions.begin(); *it != pNext; ++it);
                        method.m_instructions.insert(it, instructions.begin(), instructions.end());
                    }
                    
                    // restore 'it' position
                    for (it = method.m_instructions.begin(); *it != pNext; ++it);
                }
            }
        }
        method.RecalculateOffsets();
    }

    Instruction* InsertInjectedMethod(InstructionList &instructions, mdMethodDef injectedMethodDef, ULONG uniqueId);
    Instruction* InsertFunctionCall(InstructionList &instructions, mdSignature pvsig, FPTR pt, ULONGLONG uniqueId);

};

