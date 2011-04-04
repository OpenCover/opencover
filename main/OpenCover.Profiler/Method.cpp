#include "stdafx.h"
#include "Method.h"

#define DUMP_IL 1

Method::Method() {}
Method::~Method()
{
    for (InstructionListConstIter it = m_instructions.begin(); it != m_instructions.end() ; ++it)
    {
        delete *it;
    }

    for (ExceptionHandlerListConstIter it = m_exceptions.begin(); it != m_exceptions.end() ; ++it)
    {
        delete *it;
    }
}

void Method::ReadMethod(IMAGE_COR_ILMETHOD* pMethod)
{
    BYTE* pCode;
    m_codeSize = 0;
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
        m_codeSize = tinyImage->GetCodeSize();
    }
    else
    {
        memcpy(&m_header, pMethod, fatImage->Size * sizeof(DWORD));
        pCode = fatImage->GetCode();
        m_codeSize = fatImage->GetCodeSize();
    }
    SetBuffer(pCode);
    ReadBody();
}

// build the instruction list
void Method::ReadBody()
{
    _ASSERTE(m_codeSize != 0);
    _ASSERTE(GetPosition() == 0);

    while (GetPosition() < m_codeSize)
    {
        Instruction* pInstruction = new Instruction();
        pInstruction->m_offset = GetPosition();
        BYTE op1 = REFPRE;
        BYTE op2 = Read<BYTE>();
        switch (op2)
        {
        case STP1:
            op1 = STP1;
            op2 = Read<BYTE>();
            break;
        default: 
            break;
        }
        OperationDetails &details = Operations::m_mapOpsOperationDetails[MAKEWORD(op1, op2)];
        pInstruction->m_operation = details.canonicalName;
        switch(details.operandSize)
        {
        case Null:
            break;
        case Byte:
            pInstruction->m_operand = Read<BYTE>();
            break;
        case Word:
            pInstruction->m_operand = Read<USHORT>();
            break;
        case Dword:
            pInstruction->m_operand = Read<ULONG>();
            break;
        case Qword:
            pInstruction->m_operand = Read<ULONGLONG>();
            break;
        default:
            break;
        }

        // are we a branch or a switch
        pInstruction->m_isBranch = (details.controlFlow == BRANCH || details.controlFlow == COND_BRANCH);

        if (pInstruction->m_isBranch && pInstruction->m_operation != CEE_SWITCH)
        {
            if (details.operandSize==1)
            {
                pInstruction->m_branchOffsets.push_back((char)(BYTE)pInstruction->m_operand);
            }
            else
            {
                pInstruction->m_branchOffsets.push_back((ULONG)pInstruction->m_operand);
            }
        }

        if (pInstruction->m_operation == CEE_SWITCH)
        {
            __int64 numbranches = pInstruction->m_operand;
            while(numbranches-- != 0) pInstruction->m_branchOffsets.push_back(Read<long>());
        }

        m_instructions.push_back(pInstruction);
    }

    if ((m_header.Flags & CorILMethod_MoreSects) == CorILMethod_MoreSects)
    {
        ReadSections();
    }

    SetBuffer(NULL); // we have read all we can 

    DumpIL();

    ResolveBranches();
    
    ConvertShortBranches();
}

void Method::ReadSections()
{
    BYTE flags = 0;
    do
    {
        Align<DWORD>(); // must be DWORD aligned
        flags = Read<BYTE>();
        if ((flags & CorILMethod_Sect_FatFormat) == CorILMethod_Sect_FatFormat)
        {
            Advance(-1);
            int count = ((Read<ULONG>() >> 8) / 24);
            for (int i = 0; i < count; i++)
            {
                ExceptionHandlerType type = (ExceptionHandlerType)Read<ULONG>();
                long tryStart = Read<long>();
                long tryEnd = Read<long>();
                long handlerStart = Read<long>();
                long handlerEnd = Read<long>();
                long filterStart = 0;
                ULONG token = 0;
                switch (type)
                {
                case CLAUSE_FILTER:
                    filterStart = Read<long>();
                    break;
                default:
                    token = Read<ULONG>();
                    break;
                }
                ExceptionHandler * pSection = new ExceptionHandler();
                pSection->m_handlerType = type;
                pSection->m_tryStart = GetInstructionAtOffset(tryStart);
                pSection->m_tryEnd = GetInstructionAtOffset(tryStart + tryEnd);
                pSection->m_handlerStart = GetInstructionAtOffset(handlerStart);
                pSection->m_handlerEnd = GetInstructionAtOffset(handlerStart + handlerEnd);
                if (filterStart!=0)
                {
                    pSection->m_filterStart = GetInstructionAtOffset(filterStart);
                }
                pSection->m_token = token;
                m_exceptions.push_back(pSection);
            }
        }
        else
        {
            int count = (int)(Read<BYTE>() / 12);
            Advance(2);
            for (int i = 0; i < count; i++)
            {
                ExceptionHandlerType type = (ExceptionHandlerType)Read<USHORT>();
                long tryStart = Read<short>();
                long tryEnd = Read<char>();
                long handlerStart = Read<short>();
                long handlerEnd = Read<char>();
                long filterStart = 0;
                ULONG token = 0;
                switch (type)
                {
                case CLAUSE_FILTER:
                    filterStart = Read<long>();
                    break;
                default:
                    token = Read<ULONG>();
                    break;
                }
                ExceptionHandler * pSection = new ExceptionHandler();
                pSection->m_handlerType = type;
                pSection->m_tryStart = GetInstructionAtOffset(tryStart);
                pSection->m_tryEnd = GetInstructionAtOffset(tryStart + tryEnd);
                pSection->m_handlerStart = GetInstructionAtOffset(handlerStart);
                pSection->m_handlerEnd = GetInstructionAtOffset(handlerStart + handlerEnd);
                if (filterStart!=0)
                {
                    pSection->m_filterStart = GetInstructionAtOffset(filterStart);
                }
                pSection->m_token = token;
                m_exceptions.push_back(pSection);
            }
        }
    } while((flags & CorILMethod_Sect_MoreSects) == CorILMethod_Sect_MoreSects);
}

Instruction * Method::GetInstructionAtOffset(long offset)
{
    for (InstructionListConstIter it = m_instructions.begin(); it != m_instructions.end() ; ++it)
    {
        if ((*it)->m_offset == offset)
        {
            return (*it);
        }
    }
    _ASSERTE(FALSE);
    return NULL;
}

void Method::ResolveBranches()
{
    for (InstructionListConstIter it = m_instructions.begin(); it != m_instructions.end() ; ++it)
    {
        OperationDetails &details = Operations::m_mapNameOperationDetails[(*it)->m_operation];
        long baseOffset = (*it)->m_offset + details.length + details.operandSize;
        if ((*it)->m_operation == CEE_SWITCH)
        {
            baseOffset += (4 * (long)(*it)->m_operand);
        }
        
        for (std::vector<long>::iterator offsetIter = (*it)->m_branchOffsets.begin(); offsetIter != (*it)->m_branchOffsets.end() ; offsetIter++)
        {
            long offset = baseOffset + (*offsetIter);
            Instruction * instruction = GetInstructionAtOffset(offset);
            if (instruction != NULL) 
            {
                (*it)->m_branches.push_back(instruction);
            }
        }
        _ASSERTE((*it)->m_branchOffsets.size() == (*it)->m_branches.size());
    }
}

void Method::DumpIL()
{
    for (InstructionListConstIter it = m_instructions.begin(); it != m_instructions.end() ; ++it)
    {
        OperationDetails &details = Operations::m_mapNameOperationDetails[(*it)->m_operation];
        if (details.operandSize == Null)
        {
            ATLTRACE(_T("IL_%04X %s"), (*it)->m_offset, details.stringName);
        }
        else
        {
            if ((*it)->m_isBranch && (*it)->m_operation != CEE_SWITCH)
            {
                long offset = (*it)->m_offset + (*it)->m_branchOffsets[0] + details.length + details.operandSize;
                ATLTRACE(_T("IL_%04X %s IL_%04X"), (*it)->m_offset, details.stringName, offset);
            }
            else
            {
                ATLTRACE(_T("IL_%04X %s %X"), (*it)->m_offset, details.stringName, (*it)->m_operand);
            }
        }
        for (std::vector<long>::iterator offsetIter = (*it)->m_branchOffsets.begin(); offsetIter != (*it)->m_branchOffsets.end() ; offsetIter++)
        {
            if ((*it)->m_operation == CEE_SWITCH)
            {
                long offset = (*it)->m_offset + (4 * (long)(*it)->m_operand) + (*offsetIter) + details.length + details.operandSize;
                ATLTRACE(_T("    IL_%04X"), offset);
            }
        }
    }

    int i = 0;
    for (ExceptionHandlerListConstIter it = m_exceptions.begin(); it != m_exceptions.end() ; ++it)
    {
        ATLTRACE(_T("Section %d: %d %04X %04X %04X %04X %04X %08X"), 
            i++, (*it)->m_handlerType, 
            (*it)->m_tryStart != NULL ? (*it)->m_tryStart->m_offset : 0, 
            (*it)->m_tryEnd != NULL ? (*it)->m_tryEnd->m_offset : 0, 
            (*it)->m_handlerStart != NULL ? (*it)->m_handlerStart->m_offset : 0, 
            (*it)->m_handlerEnd != NULL ? (*it)->m_handlerEnd->m_offset : 0, 
            (*it)->m_filterStart != NULL ? (*it)->m_filterStart->m_offset : 0, 
            (*it)->m_token);
    }            
}

void Method::ConvertShortBranches()
{
    for (InstructionListConstIter it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        OperationDetails &details = Operations::m_mapNameOperationDetails[(*it)->m_operation];
        if ((*it)->m_isBranch && details.operandSize == 1)
        {
            CanonicalName newOperation = (*it)->m_operation;
            switch((*it)->m_operation)
            {
            case CEE_BR_S:
                newOperation = CEE_BR;
                break;
            case CEE_BRFALSE_S:
                newOperation = CEE_BRFALSE;
                break;
            case CEE_BRTRUE_S:
                newOperation = CEE_BRTRUE;
                break;
            case CEE_BEQ_S:
                newOperation = CEE_BEQ;
                break;
            case CEE_BGE_S:
                newOperation = CEE_BGE;
                break;
            case CEE_BGT_S:
                newOperation = CEE_BGT;
                break;
            case CEE_BLE_S:
                newOperation = CEE_BLE;
                break;
            case CEE_BLT_S:
                newOperation = CEE_BLT;
                break;
            case CEE_BNE_UN_S:
                newOperation = CEE_BNE_UN;
                break;
            case CEE_BGE_UN_S:
                newOperation = CEE_BGE_UN;
                break;
            case CEE_BGT_UN_S:
                newOperation = CEE_BGT_UN;
                break;
            case CEE_BLE_UN_S:
                newOperation = CEE_BLE_UN;
                break;
            case CEE_BLT_UN_S:
                newOperation = CEE_BLT_UN;
                break;
            case CEE_LEAVE_S:
                newOperation = CEE_LEAVE;
                break;
            default:
                break;
            }
            (*it)->m_operation = newOperation;
            (*it)->m_operand = UNSAFE_BRANCH_OPERAND;
        }

        (*it)->m_branchOffsets.clear();
    }
}



