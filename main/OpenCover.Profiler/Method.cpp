#include "stdafx.h"
#include "Method.h"

#ifdef DEBUG
#define DUMP_IL 1
#endif

Method::Method(IMAGE_COR_ILMETHOD* pMethod) 
{
    memset(&m_header, 0, 3 * sizeof(DWORD));
    m_header.Size = 3;
    m_header.Flags = CorILMethod_FatFormat;
    m_header.MaxStack = 8; 

    ReadMethod(pMethod);
}

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

/// <summary>Read the full method from the supplied buffer.</summary>
void Method::ReadMethod(IMAGE_COR_ILMETHOD* pMethod)
{
    BYTE* pCode;
    COR_ILMETHOD_FAT* fatImage = (COR_ILMETHOD_FAT*)&pMethod->Fat;
    if(!fatImage->IsFat())
    {
        ATLTRACE(_T("TINY"));
        COR_ILMETHOD_TINY* tinyImage = (COR_ILMETHOD_TINY*)&pMethod->Tiny;
        m_header.CodeSize = tinyImage->GetCodeSize();
        pCode = tinyImage->GetCode();
        ATLTRACE(_T("TINY(%X) => (%d + 1) : %d"), m_header.CodeSize, m_header.CodeSize, m_header.MaxStack);
    }
    else
    {
        memcpy(&m_header, pMethod, fatImage->Size * sizeof(DWORD));
        pCode = fatImage->GetCode();
        ATLTRACE(_T("FAT(%X) => (%d + 12) : %d"), m_header.CodeSize, m_header.CodeSize, m_header.MaxStack);
    }
    SetBuffer(pCode);
    ReadBody();
}

/// <summary>Write the method to a supplied buffer</summary>
/// <remarks><para>The buffer must be of the size supplied by <c>GetMethodSize</c>.</para>
/// <para>Currently only write methods with 'Fat' headers and 'Fat' Sections - simpler.</para>
/// <para>The buffer will normally be allocated by a call to <c>IMethodMalloc::Alloc</c></para></remarks>
void Method::WriteMethod(IMAGE_COR_ILMETHOD* pMethod)
{
    BYTE* pCode;
    COR_ILMETHOD_FAT* fatImage = (COR_ILMETHOD_FAT*)&pMethod->Fat;
    
    m_header.Flags &= ~CorILMethod_MoreSects;
    if (m_exceptions.size() > 0)
    {
        m_header.Flags |= CorILMethod_MoreSects;
    }
    
    memcpy(fatImage, &m_header, m_header.Size * sizeof(DWORD));

    pCode = fatImage->GetCode();

    SetBuffer(pCode);

    for (InstructionListConstIter it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        OperationDetails &details = Operations::m_mapNameOperationDetails[(*it)->m_operation];
        if (details.op1 == REFPRE)
        {
            Write<BYTE>(details.op2);
        }
        else if (details.op1 == MOOT)
        {
            continue;
        }
        else
        {
            Write<BYTE>(details.op1);
            Write<BYTE>(details.op2);
        }

        switch(details.operandSize)
        {
        case Null:
            break;
        case Byte:
            Write<BYTE>((BYTE)(*it)->m_operand);
            break;
        case Word:
            Write<USHORT>((USHORT)(*it)->m_operand);
            break;
        case Dword:
            Write<ULONG>((ULONG)(*it)->m_operand);
            break;
        case Qword:
            Write<ULONGLONG>((*it)->m_operand);
            break;
        default:
            break;
        }

        if ((*it)->m_operation == CEE_SWITCH)
        {
            for (std::vector<long>::iterator offsetIter = (*it)->m_branchOffsets.begin(); offsetIter != (*it)->m_branchOffsets.end() ; offsetIter++)
            {
                Write<long>(*offsetIter);
            }
        }
    }

    WriteSections();
}

/// <summary>Write out the FAT sections</summary>
void Method::WriteSections()
{
    if (m_exceptions.size() > 0)
    {
        Align<DWORD>();
        IMAGE_COR_ILMETHOD_SECT_FAT section;
        section.Kind = CorILMethod_Sect_FatFormat;
        section.Kind |= CorILMethod_Sect_EHTable;
        section.DataSize = (m_exceptions.size() * 24) + 4;
        Write<IMAGE_COR_ILMETHOD_SECT_FAT>(section);
        for (ExceptionHandlerListConstIter it = m_exceptions.begin(); it != m_exceptions.end() ; ++it)
        {
            Write<ULONG>((*it)->m_handlerType);
            Write<long>((*it)->m_tryStart->m_offset);
            Write<long>((*it)->m_tryEnd->m_offset - (*it)->m_tryStart->m_offset);
            Write<long>((*it)->m_handlerStart->m_offset);
            Write<long>((*it)->m_handlerEnd->m_offset - (*it)->m_handlerStart->m_offset);

            switch ((*it)->m_handlerType)
            {
            case COR_ILEXCEPTION_CLAUSE_FILTER:
                Write<long>((*it)->m_filterStart->m_offset);
                break;
            default:
                Write<ULONG>((*it)->m_token);
                break;
            }
        }
    }
}

/// <summary>Read in a method body and any section handlers.</summary>
/// <remarks>Also converts all short branches to long branches and calls <c>RecalculateOffsets</c></remarks>
void Method::ReadBody()
{
    _ASSERTE(m_header.CodeSize != 0);
    _ASSERTE(GetPosition() == 0);

    while (GetPosition() < m_header.CodeSize)
    {
        Instruction* pInstruction = new Instruction();
        pInstruction->m_origOffset = pInstruction->m_offset = GetPosition();
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
            DWORD numbranches = (DWORD)pInstruction->m_operand;
            while(numbranches-- != 0) pInstruction->m_branchOffsets.push_back(Read<long>());
        }

        m_instructions.push_back(pInstruction);
    }

    ReadSections();

    SetBuffer(NULL); 

    DumpIL();

    ResolveBranches();
    
    ConvertShortBranches();

    RecalculateOffsets();

    DumpIL();
}

/// <summary>Read the section handler section.</summary>
/// <remarks>All 'Small' sections are to be converted to 'Fat' sections.</remarks>
void Method::ReadSections()
{
    if ((m_header.Flags & CorILMethod_MoreSects) == CorILMethod_MoreSects)
    {
        BYTE flags = 0;
        do
        {
            Align<DWORD>(); // must be DWORD aligned
            flags = Read<BYTE>();
            _ASSERTE((flags & CorILMethod_Sect_EHTable) == CorILMethod_Sect_EHTable);
            if ((flags & CorILMethod_Sect_FatFormat) == CorILMethod_Sect_FatFormat)
            {
                Advance(-1);
                int count = ((Read<ULONG>() >> 8) / 24);
                for (int i = 0; i < count; i++)
                {
                    CorExceptionFlag type = (CorExceptionFlag)Read<ULONG>();
                    long tryStart = Read<long>();
                    long tryEnd = Read<long>();
                    long handlerStart = Read<long>();
                    long handlerEnd = Read<long>();
                    long filterStart = 0;
                    ULONG token = 0;
                    switch (type)
                    {
                    case COR_ILEXCEPTION_CLAUSE_FILTER:
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
                    pSection->m_handlerEnd = GetInstructionAtOffset(handlerStart + handlerEnd, 
                        (type & COR_ILEXCEPTION_CLAUSE_FINALLY) == COR_ILEXCEPTION_CLAUSE_FINALLY);
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
                    CorExceptionFlag type = (CorExceptionFlag)Read<USHORT>();
                    long tryStart = Read<USHORT>();
                    long tryEnd = Read<BYTE>();
                    long handlerStart = Read<USHORT>();
                    long handlerEnd = Read<BYTE>();
                    long filterStart = 0;
                    ULONG token = 0;
                    switch (type)
                    {
                    case COR_ILEXCEPTION_CLAUSE_FILTER:
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
                    pSection->m_handlerEnd = GetInstructionAtOffset(handlerStart + handlerEnd, 
                        (type & COR_ILEXCEPTION_CLAUSE_FINALLY) == COR_ILEXCEPTION_CLAUSE_FINALLY);
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
}

/// <summary>Gets the <c>Instruction</c> that has (is at) the specified offset.</summary>
/// <param name="offset">The offset to look for.</param>
/// <returns>An <c>Instruction</c> that exists at that location.</returns>
/// <remarks>Ensure that the offsets are current by executing <c>RecalculateOffsets</c>
/// beforehand</remarks>
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

/// <summary>Gets the <c>Instruction</c> that has (is at) the specified offset.</summary>
/// <param name="offset">The offset to look for.</param>
/// <returns>An <c>Instruction</c> that exists at that location.</returns>
/// <remarks>Ensure that the offsets are current by executing <c>RecalculateOffsets</c>
/// beforehand. Only should be used when trying to find the instruction pointed to be a finally 
/// block which may be beyond the bounds of the method itself</remarks>
/// <example>
///     void Method()
///     {
///            try
///            {
///                throw new Exception();
///            }
///            finally
///            {
///                Console.WriteLine("Finally");
///            }
///     }
/// </example>
Instruction * Method::GetInstructionAtOffset(long offset, bool isFinally)
{
    for (InstructionListConstIter it = m_instructions.begin(); it != m_instructions.end() ; ++it)
    {
        if ((*it)->m_offset == offset)
        {
            return (*it);
        }
    }

    if (isFinally)
    {
        Instruction *pLast = m_instructions.back();
        OperationDetails &details = Operations::m_mapNameOperationDetails[pLast->m_operation];
        if (offset == pLast->m_offset + details.length + details.operandSize)
        {
            // add a code label to hang the try/finally handler end off
            Instruction *pInstruction = new Instruction(CEE_CODE_LABEL); 
            pInstruction->m_offset = offset;
            m_instructions.push_back(pInstruction);
            return pInstruction;
        }
    }  
    _ASSERTE(FALSE);
    return NULL;
}

/// <summary>Uses the current offsets and locates the instructions that reside that offset to 
/// build a new list</summary>
/// <remarks>This allows us to insert (or modify) instructions without losing the intended 'goto' 
/// point. <c>RecalculateOffsets</c> is used to rebuild the new required operand(s) based on the
/// offsets of the instructions being referenced</remarks>
void Method::ResolveBranches()
{
    for (InstructionListConstIter it = m_instructions.begin(); it != m_instructions.end() ; ++it)
    {
        (*it)->m_branches.clear();
        OperationDetails &details = Operations::m_mapNameOperationDetails[(*it)->m_operation];
        long baseOffset = (*it)->m_offset + details.length + details.operandSize;
        if ((*it)->m_operation == CEE_SWITCH)
        {
            baseOffset += (4*(long)(*it)->m_operand);
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

/// <summary>Pretty print the IL</summary>
/// <remarks>Only works for Debug builds.</remarks>
void Method::DumpIL()
{
#ifdef DUMP_IL
    ATLTRACE(_T("-+-+-+-+-+-+-+-+-+-+-+-+- START -+-+-+-+-+-+-+-+-+-+-+-+"));
    for (InstructionListConstIter it = m_instructions.begin(); it != m_instructions.end() ; ++it)
    {
        OperationDetails &details = Operations::m_mapNameOperationDetails[(*it)->m_operation];
        if (details.operandSize == Null)
        {
            ATLTRACE(_T("IL_%04X %s"), (*it)->m_offset, details.stringName);
        }
        else
        {
            if (details.operandParam == ShortInlineBrTarget || details.operandParam == InlineBrTarget)
            {
                long offset = (*it)->m_offset + (*it)->m_branchOffsets[0] + details.length + details.operandSize;
                ATLTRACE(_T("IL_%04X %s IL_%04X"), 
                    (*it)->m_offset, details.stringName, offset);
            }
            else if (details.operandParam == InlineMethod || details.operandParam == InlineString)
            {
                ATLTRACE(_T("IL_%04X %s (%02X)%02X%02X%02X"), 
                    (*it)->m_offset, details.stringName, 
                    (BYTE)((*it)->m_operand >> 24),
                    (BYTE)((*it)->m_operand >> 16),
                    (BYTE)((*it)->m_operand >> 8),
                    (BYTE)((*it)->m_operand & 0xFF));
            }
            else if (details.operandSize == Byte)
            {
                ATLTRACE(_T("IL_%04X %s %02X"), 
                    (*it)->m_offset, details.stringName, (*it)->m_operand);
            }
            else if (details.operandSize == Word)
            {
                ATLTRACE(_T("IL_%04X %s %04X"), 
                    (*it)->m_offset, details.stringName, (*it)->m_operand);
            }
            else if (details.operandSize == Dword)
            {
                ATLTRACE(_T("IL_%04X %s %08X"), 
                    (*it)->m_offset, details.stringName, (*it)->m_operand);
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
                long offset = (*it)->m_offset + (4*(long)(*it)->m_operand) + (*offsetIter) + details.length + details.operandSize;
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
    ATLTRACE(_T("-+-+-+-+-+-+-+-+-+-+-+-+-  END  -+-+-+-+-+-+-+-+-+-+-+-+"));
#endif
}

/// <summary>Converts all short branches to long branches.</summary>
/// <remarks><para>After instrumentation most short branches will not have the capacity for
/// the new required offset. Save time/effort and make all branches long ones.</para> 
/// <para>Could add the capability to optimise long to short at a later date but consider 
/// the benefits dubious after all the new instrumentation has been added.</para></remarks>
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

/// <summary>Recalculate the offsets of each instruction taking into account the instruction
/// size, the operand size and any extra datablocks CEE_SWITCH</summary>
void Method::RecalculateOffsets()
{
    int position = 0;
    for (InstructionListConstIter it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        OperationDetails &details = Operations::m_mapNameOperationDetails[(*it)->m_operation];
        (*it)->m_offset = position;
        position += details.length;
        position += details.operandSize;
        if((*it)->m_operation == CEE_SWITCH)
        {
            position += 4*(long)(*it)->m_operand;
        }
    }

    for (InstructionListConstIter it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        OperationDetails &details = Operations::m_mapNameOperationDetails[(*it)->m_operation];
        if ((*it)->m_isBranch)
        {
            (*it)->m_branchOffsets.clear();
            if((*it)->m_operation == CEE_SWITCH)
            {
                long offset = ((*it)->m_offset + details.length + details.operandSize + (4*(long)(*it)->m_operand));                    
                for (InstructionListIter bit = (*it)->m_branches.begin(); bit != (*it)->m_branches.end(); ++bit)
                {
                    (*it)->m_branchOffsets.push_back((*bit)->m_offset - offset);
                }
            }
            else
            {
                (*it)->m_operand = (*it)->m_branches[0]->m_offset - ((*it)->m_offset + details.length + details.operandSize);
                (*it)->m_branchOffsets.push_back((long)(*it)->m_operand);
            }
        }
    }
}

/// <summary>Calculates the size of the method which include the header size, 
/// the code size and the (aligned) creitical sections if they exist. Use this
/// to get the size required for allocating memory.</summary>
/// <returns>The size of the method.</returns>
/// <remarks>It is recomended that <c>RecalculateOffsets</c> should be called 
/// beforehand if any instrumentation has been done</remarks>
long Method::GetMethodSize()
{
    Instruction * lastInstruction = m_instructions.back();
    OperationDetails &details = Operations::m_mapNameOperationDetails[lastInstruction->m_operation];
    
    m_header.CodeSize = lastInstruction->m_offset + details.length + details.operandSize;
    long size = sizeof(IMAGE_COR_ILMETHOD_FAT) + m_header.CodeSize;

    m_header.Flags &= ~CorILMethod_MoreSects;
    if (m_exceptions.size() > 0)
    {
        m_header.Flags |= CorILMethod_MoreSects;
        long align = sizeof(DWORD) - 1;
        size = ((size + align) & ~align);
        size += (((long)m_exceptions.size() * 6) + 1) * sizeof(long);
    }

    return size;
}

void Method::InsertSequenceInstructionsAtOffset(long offset, InstructionList &instructions)
{
    long actualOffset = 0;
    for (InstructionListConstIter it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        if ((*it)->m_offset == offset)
        {
            actualOffset = (*it)->m_offset;
            m_instructions.insert(++it, instructions.begin(), instructions.end());
            break;
        }
    } 

    for (InstructionListIter it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        if ((*it)->m_origOffset == offset)
        {            
            Instruction orig = *(*it);
            for (unsigned int i=0;i<instructions.size();i++)
            {
                InstructionListIter temp = it++;
                *(*temp) = *(*it);
            }
            *(*it) = orig;
            break;
        }
    }

    RecalculateOffsets();
    return;
}

/// <summary>Insert a sequence of instructions at a sequence point</summary>
/// <remarks>Original pointer references are maintianed by inserting the sequence of instructions 
/// after the intended target and then using a copy operator on the <c>Instruction</c> objects to 
/// copy the data between them</remarks>
void Method::InsertSequenceInstructionsAtOriginalOffset(long offset, InstructionList &instructions)
{
    long actualOffset = 0;
    for (InstructionListConstIter it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        if ((*it)->m_origOffset == offset)
        {
            actualOffset = (*it)->m_offset;
            m_instructions.insert(++it, instructions.begin(), instructions.end());
            break;
        }
    } 

    if (!DoesTryHandlerPointToOffset (actualOffset))
    {
        for (InstructionListIter it = m_instructions.begin(); it != m_instructions.end(); ++it)
        {
            if ((*it)->m_origOffset == offset)
            {            
                Instruction orig = *(*it);
                for (unsigned int i=0;i<instructions.size();i++)
                {
                    InstructionListIter temp = it++;
                    *(*temp) = *(*it);
                }
                *(*it) = orig;
                break;
            }
        }
    }
    RecalculateOffsets();
    return;
}

/// <summary>Test if we have an exception where the handler start points to the 
/// instruction at the supplied offset</summary>
bool Method::DoesTryHandlerPointToOffset(long offset)
{
    for (ExceptionHandlerListConstIter it = m_exceptions.begin(); it != m_exceptions.end() ; ++it)
    {
        if (((*it)->m_handlerType == COR_ILEXCEPTION_CLAUSE_NONE) 
            && ((*it)->m_handlerStart->m_offset == offset)) return true;
    }
    return false;
}

ULONG Method::GetILMapSize()
{
    ULONG mapSize = 0;
    for (InstructionListIter it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        if ((*it)->m_origOffset != -1) 
            ++mapSize;
    }
    return mapSize;
}

void Method::PopulateILMap(ULONG mapSize, COR_IL_MAP* maps)
{
    _ASSERTE(GetILMapSize() == mapSize);

    mapSize = 0;
    for (InstructionListIter it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        if ((*it)->m_origOffset != -1)
        {
            maps[mapSize].fAccurate = TRUE;
            maps[mapSize].oldOffset = (*it)->m_origOffset;
            maps[mapSize].newOffset = (*it)->m_offset;

            ++mapSize;
        }
    }
}

