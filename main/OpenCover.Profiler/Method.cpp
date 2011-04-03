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
        Instruction * pInstruction = new Instruction();
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
            pInstruction->m_operand = Read<BYTE>(&pCode, &position);
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

        // handle the branches
        if (pInstruction->m_operation == CEE_SWITCH)
        {
            __int64 numbranches = pInstruction->m_operand;
            while(numbranches-- != 0) pInstruction->m_branchOffsets.push_back(Read<short>(&pCode, &position));
        }

        if (details.controlFlow == BRANCH || details.controlFlow == COND_BRANCH)
        {
            pInstruction->m_branchOffsets.push_back(pInstruction->m_operand);
        }

        m_instructions.push_back(pInstruction);
    }

    // resolve the branches
    for (InstructionListConstIter it = m_instructions.begin(); it != m_instructions.end() ; ++it)
    {
        OperationDetails &details = Operations::m_mapNameOperationDetails[(*it)->m_operation];
        if (details.operandSize == Null)
        {
            ATLTRACE(_T("%x %s"), (*it)->m_offset, details.stringName);
        }
        else
        {
            ATLTRACE(_T("%x %s %X"), (*it)->m_offset, details.stringName, (*it)->m_operand);
        }
    }

}



