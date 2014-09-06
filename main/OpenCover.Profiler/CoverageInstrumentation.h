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

					auto bpp = std::find_if(points.begin(), points.end(), [pCurrent, idx](BranchPoint &bp){return bp.Offset == pCurrent->m_origOffset && bp.Path == idx;});
					if (bpp == points.end()) // we can't find information on a branch to instrument (this may happen if it was skipped/ignored during initial investigation by the host process)
						continue;

                    ULONG uniqueId = (*bpp).UniqueId;
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
                        
                    }
                    
                    // now instrument "default:" or "else" branch
                    // insert all instrumentation at pNext
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

