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

                    // here we istrument branch 0 == fall-through branch =>
                    //        switch instruction DEFAULT branch
                    //        br(if) instruction ELSE branch
                    ULONG uniqueId = (*std::find_if(points.begin(), points.end(), [pCurrent, idx](BranchPoint &bp){return bp.Offset == pCurrent->m_origOffset && bp.Path == idx;})).UniqueId;

                    instructions.clear();
                    Instruction* pElseOrDefault = instrumentMethod(instructions, uniqueId);
                    Instruction* pToInstrument = pNext;
                    if (pCurrent->m_operation == CEE_SWITCH)
                    {   // we got "default:" branch here
                        Instruction* pEndOfDefaultBranch = method.EndOfBranch( pNext );
                        if ( pEndOfDefaultBranch->m_joints.size() != 0 )
                        {   // cool, our "default:" branch ends up into "joint" point

                            // update instruction to instrument
                            if ( pToInstrument != pEndOfDefaultBranch )
                            {
                                pToInstrument = pEndOfDefaultBranch;
                            }
                            // rewire other joints
                            for(auto joint = pToInstrument->m_joints.begin(); joint != pToInstrument->m_joints.end(); joint++)
                            {
                                Instruction* pToRewire = *joint; // :)
                                _ASSERTE(pToRewire->m_isBranch);
                                _ASSERTE(pToRewire->m_branches.size() == 1);
                                _ASSERTE(pToRewire->m_branches[0] == pToInstrument);
                                pToRewire->m_branches[0] = pElseOrDefault; // rewire pointing branch to instrumentation 
                            }
                        }
                    }
                    
                    for (it = method.m_instructions.begin(); *it != pToInstrument; ++it);
                    method.m_instructions.insert(it, instructions.begin(), instructions.end());

                    // here we instrument
                    for(auto sbit = pCurrent->m_branches.begin(); sbit != pCurrent->m_branches.end(); sbit++)
                    {
                        idx++;
                        uniqueId = (*std::find_if(points.begin(), points.end(), [pCurrent, idx](BranchPoint &bp){return bp.Offset == pCurrent->m_origOffset && bp.Path == idx;})).UniqueId;

                        instructions.clear();
                        Instruction* pRecordJmp = instrumentMethod(instructions, uniqueId);

                        // insert instrumentation point as usual 
						// do not push-down anything here, 
                        Instruction* toInstrument = *sbit;
                        *sbit = pRecordJmp; // rewire switch branch to instrumentation

                        if (pCurrent->m_operation != CEE_SWITCH)
                        {
                            // Current operation is any conditional branch except switch
                            Instruction* endOfCondBranch = method.EndOfBranch( toInstrument );
                            if (endOfCondBranch->m_jump == NULL)
                            {
                                // if no branch-chain exists, then add current branch to joints
                                endOfCondBranch->m_joints.push_back(pCurrent);
                            }
                            else
                            {
                                // if branch-chain exists, then add last jump-branch to joints
                                endOfCondBranch->m_joints.push_back(endOfCondBranch->m_jump);
                            }
                        }

                        for (it = method.m_instructions.begin(); *it != toInstrument; ++it);
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

