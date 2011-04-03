#include "stdafx.h"
#include "Method.h"

Method::Method() {}
Method::~Method()
{
    for (InstructionListConstIter it = m_instructions.begin(); it != m_instructions.end() ; ++it)
    {
        delete *it;
    }
}

void Method::ReadMethod(IMAGE_COR_ILMETHOD* pMethod)
{
    BYTE* pCode;
    unsigned int codeSize = 0;
    COR_ILMETHOD_FAT* fatImage = (COR_ILMETHOD_FAT*)&pMethod->Fat;
    if(!fatImage->IsFat())
    {
        COR_ILMETHOD_TINY* tinyImage = (COR_ILMETHOD_TINY*)&pMethod->Tiny;
        memset(&m_header, 0, 3 * sizeof(DWORD));
        m_header.Size = 3;
        m_header.Flags = CorILMethod_FatFormat;
        m_header.CodeSize = tinyImage->GetCodeSize();
        m_header.MaxStack = 8;
        pCode = tinyImage->GetCode();
        codeSize = tinyImage->GetCodeSize();
    }
    else
    {
        memcpy(&m_header, pMethod, fatImage->Size * sizeof(DWORD));
        pCode = fatImage->GetCode();
        codeSize = fatImage->GetCodeSize();
    }

    ReadBody(pCode, codeSize);

}

// build the instruction list
void Method::ReadBody(BYTE* pCode, unsigned int codeSize)
{
    unsigned int position = 0;
    
    while (position < codeSize)
    {
        Instruction* pInstruction = new Instruction();
        pInstruction->m_offset = position;
        BYTE op1 = 0xFF;
        BYTE op2 = Read<BYTE>(&pCode, &position);
        if (op2 == 0xFE)
        {
            op1 = 0xFE;
            op2 = Read<BYTE>(&pCode, &position);
        }
        OperationDetails &details = Operations::m_mapOpsOperationDetails[MAKEWORD(op1, op2)];
        pInstruction->m_operation = details.canonicalName;
        switch(details.operandSize)
        {
        case Null:
            break;
        case Byte:
            pInstruction->m_operand = Read<char>(&pCode, &position);
            break;
        case Word:
            pInstruction->m_operand = Read<short>(&pCode, &position);
            break;
        case Dword:
            pInstruction->m_operand = Read<long>(&pCode, &position);
            break;
        case Qword:
            pInstruction->m_operand = Read<__int64>(&pCode, &position);
            break;
        default:
            break;
        }

        // are we a branch or a switch
        pInstruction->m_isBranch = (details.controlFlow == BRANCH || details.controlFlow == COND_BRANCH);

        if (pInstruction->m_isBranch && pInstruction->m_operation != CEE_SWITCH)
        {
            pInstruction->m_branchOffsets.push_back(pInstruction->m_operand);
        }

        if (pInstruction->m_operation == CEE_SWITCH)
        {
            __int64 numbranches = pInstruction->m_operand;
            ATLTRACE(_T("xxxxxxxxx %d"), numbranches);
            while(numbranches-- != 0) pInstruction->m_branchOffsets.push_back(Read<long>(&pCode, &position));
        }

        m_instructions.push_back(pInstruction);
    }

    // resolve the branches
    for (InstructionListConstIter it = m_instructions.begin(); it != m_instructions.end() ; ++it)
    {
        OperationDetails &details = Operations::m_mapNameOperationDetails[(*it)->m_operation];
#if DUMP_IL 
        if (details.operandSize == Null)
        {
            ATLTRACE(_T("IL_%04X %s"), (*it)->m_offset, details.stringName);
        }
        else
        {
            if ((*it)->m_isBranch && (*it)->m_operation != CEE_SWITCH)
            {
                int offset = (*it)->m_offset + (*it)->m_operand + details.length + details.operandSize;
                ATLTRACE(_T("IL_%04X %s IL_%04X"), (*it)->m_offset, details.stringName, offset);
            }
            else
            {
                ATLTRACE(_T("IL_%04X %s %X"), (*it)->m_offset, details.stringName, (*it)->m_operand);
            }
        }
        for (std::vector<short>::iterator offsetIter = (*it)->m_branchOffsets.begin(); offsetIter != (*it)->m_branchOffsets.end() ; offsetIter++)
        {
            if ((*it)->m_operation == CEE_SWITCH)
            {
                int offset = (*it)->m_offset + (4 * (*it)->m_operand) + (*offsetIter) + details.length + details.operandSize;
                ATLTRACE(_T("    IL_%04X"), offset);
            }
        }
#endif
        for (std::vector<short>::iterator offsetIter = (*it)->m_branchOffsets.begin(); offsetIter != (*it)->m_branchOffsets.end() ; offsetIter++)
        {
            int offset = 0;
            if ((*it)->m_operation == CEE_SWITCH)
            {
                offset = (*it)->m_offset + (4 * (*it)->m_operand) + (*offsetIter) + details.length + details.operandSize;
            }
            else
            {
                offset = (*it)->m_offset + *offsetIter + details.length + details.operandSize;
            }
            
            for (InstructionListConstIter it2 = m_instructions.begin(); it2 != m_instructions.end() ; ++it2)
            {
                if ((*it2)->m_offset == offset)
                {
                    (*it)->m_branches.push_back(*it2);
                }
            }
        }
        _ASSERTE((*it)->m_branchOffsets.size() == (*it)->m_branches.size());
    }
}



