//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#include "stdafx.h"
#include "Method.h"

#ifdef DEBUG
// uncommment to get debug builds to dump out instrumented functions (slow)
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
    for (auto it = m_instructions.begin(); it != m_instructions.end() ; ++it)
    {
        delete *it;
    }

    for (auto it = m_exceptions.begin(); it != m_exceptions.end() ; ++it)
    {
        delete *it;
    }
}

/// <summary>Read the full method from the supplied buffer.</summary>
void Method::ReadMethod(IMAGE_COR_ILMETHOD* pMethod)
{
    BYTE* pCode;
    auto fatImage = static_cast<COR_ILMETHOD_FAT*>(&pMethod->Fat);
    if(!fatImage->IsFat())
    {
        ATLTRACE(_T("TINY"));
        auto tinyImage = static_cast<COR_ILMETHOD_TINY*>(&pMethod->Tiny);
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
    auto fatImage = static_cast<COR_ILMETHOD_FAT*>(&pMethod->Fat);
    
    m_header.Flags &= ~CorILMethod_MoreSects;
    if (m_exceptions.size() > 0)
    {
        m_header.Flags |= CorILMethod_MoreSects;
    }
    
    memcpy(fatImage, &m_header, m_header.Size * sizeof(DWORD));

    pCode = fatImage->GetCode();

    SetBuffer(pCode);

    for (auto it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        auto& details = Operations::m_mapNameOperationDetails[(*it)->m_operation];
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
            Write<BYTE>(static_cast<BYTE>((*it)->m_operand));
            break;
        case Word:
            Write<USHORT>(static_cast<USHORT>((*it)->m_operand));
            break;
        case Dword:
            Write<ULONG>(static_cast<ULONG>((*it)->m_operand));
            break;
        case Qword:
            Write<ULONGLONG>((*it)->m_operand);
            break;
        default:
            break;
        }

        if ((*it)->m_operation == CEE_SWITCH)
        {
            for (auto offsetIter = (*it)->m_branchOffsets.begin(); offsetIter != (*it)->m_branchOffsets.end(); ++offsetIter)
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
        for (auto it = m_exceptions.begin(); it != m_exceptions.end() ; ++it)
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
                pInstruction->m_branchOffsets.push_back(static_cast<char>(static_cast<BYTE>(pInstruction->m_operand)));
            }
            else
            {
                pInstruction->m_branchOffsets.push_back(static_cast<ULONG>(pInstruction->m_operand));
            }
        }

        if (pInstruction->m_operation == CEE_SWITCH)
        {
            auto numbranches = static_cast<DWORD>(pInstruction->m_operand);
            while(numbranches-- != 0) pInstruction->m_branchOffsets.push_back(Read<long>());
        }

        m_instructions.push_back(pInstruction);
    }

    ReadSections();

    SetBuffer(nullptr); 

    //DumpIL();

    ResolveBranches();
    
    ConvertShortBranches();

    RecalculateOffsets();

    //DumpIL();
}

/// <summary>Read the section handler section.</summary>
/// <remarks>All 'Small' sections are to be converted to 'Fat' sections.</remarks>
void Method::ReadSections()
{
    if ((m_header.Flags & CorILMethod_MoreSects) == CorILMethod_MoreSects)
    {
        BYTE flags;
        do
        {
            Align<DWORD>(); // must be DWORD aligned
            flags = Read<BYTE>();
            _ASSERTE((flags & CorILMethod_Sect_EHTable) == CorILMethod_Sect_EHTable);
            if ((flags & CorILMethod_Sect_FatFormat) == CorILMethod_Sect_FatFormat)
            {
                Advance(-1);
                int count = ((Read<ULONG>() >> 8) / 24);
                for (auto i = 0; i < count; i++)
                {
                    auto type = static_cast<CorExceptionFlag>(Read<ULONG>());
                    auto tryStart = Read<long>();
                    auto tryEnd = Read<long>();
                    auto handlerStart = Read<long>();
                    auto handlerEnd = Read<long>();
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
                    auto pSection = new ExceptionHandler();
                    pSection->m_handlerType = type;
                    pSection->m_tryStart = GetInstructionAtOffset(tryStart);
                    pSection->m_tryEnd = GetInstructionAtOffset(tryStart + tryEnd);
                    pSection->m_handlerStart = GetInstructionAtOffset(handlerStart);
                    pSection->m_handlerEnd = GetInstructionAtOffset(handlerStart + handlerEnd, 
                        (type & COR_ILEXCEPTION_CLAUSE_FINALLY) == COR_ILEXCEPTION_CLAUSE_FINALLY,
                        (type & COR_ILEXCEPTION_CLAUSE_FAULT) == COR_ILEXCEPTION_CLAUSE_FAULT,
                        (type & COR_ILEXCEPTION_CLAUSE_FILTER) == COR_ILEXCEPTION_CLAUSE_FILTER,
                        (type & COR_ILEXCEPTION_CLAUSE_NONE) == COR_ILEXCEPTION_CLAUSE_NONE);
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
                auto count = static_cast<int>(Read<BYTE>() / 12);
                Advance(2);
                for (auto i = 0; i < count; i++)
                {
                    auto type = static_cast<CorExceptionFlag>(Read<USHORT>());
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
                        (type & COR_ILEXCEPTION_CLAUSE_FINALLY) == COR_ILEXCEPTION_CLAUSE_FINALLY,
                        (type & COR_ILEXCEPTION_CLAUSE_FAULT) == COR_ILEXCEPTION_CLAUSE_FAULT,
                        (type & COR_ILEXCEPTION_CLAUSE_FILTER) == COR_ILEXCEPTION_CLAUSE_FILTER,
                        (type & COR_ILEXCEPTION_CLAUSE_NONE) == COR_ILEXCEPTION_CLAUSE_NONE);
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
    for (auto it = m_instructions.begin(); it != m_instructions.end() ; ++it)
    {
        if ((*it)->m_offset == offset)
        {
            return (*it);
        }
    }
    _ASSERTE(FALSE);
    return nullptr;
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
Instruction * Method::GetInstructionAtOffset(long offset, bool isFinally, bool isFault, bool isFilter, bool isTyped)
{
    for (auto it = m_instructions.begin(); it != m_instructions.end() ; ++it)
    {
        if ((*it)->m_offset == offset)
        {
            return (*it);
        }
    }

    if (isFinally || isFault || isFilter || isTyped)
    {
        auto pLast = m_instructions.back();
        auto& details = Operations::m_mapNameOperationDetails[pLast->m_operation];
        if (offset == pLast->m_offset + details.length + details.operandSize)
        {
            // add a code label to hang the clause handler end off
            auto pInstruction = new Instruction(CEE_CODE_LABEL); 
            pInstruction->m_offset = offset;
            m_instructions.push_back(pInstruction);
            return pInstruction;
        }
    }  
    _ASSERTE(FALSE);
    return nullptr;
}

/// <summary>Uses the current offsets and locates the instructions that reside that offset to 
/// build a new list</summary>
/// <remarks>This allows us to insert (or modify) instructions without losing the intended 'goto' 
/// point. <c>RecalculateOffsets</c> is used to rebuild the new required operand(s) based on the
/// offsets of the instructions being referenced</remarks>
void Method::ResolveBranches()
{
    for (auto it = m_instructions.begin(); it != m_instructions.end() ; ++it)
    {
        (*it)->m_branches.clear();
        auto& details = Operations::m_mapNameOperationDetails[(*it)->m_operation];
        auto baseOffset = (*it)->m_offset + details.length + details.operandSize;
        if ((*it)->m_operation == CEE_SWITCH)
        {
            baseOffset += (4*static_cast<long>((*it)->m_operand));
        }
        
        for (auto offsetIter = (*it)->m_branchOffsets.begin(); offsetIter != (*it)->m_branchOffsets.end() ; ++offsetIter)
        {
            auto offset = baseOffset + (*offsetIter);
            auto instruction = GetInstructionAtOffset(offset);
            if (instruction != nullptr) 
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
    for (auto it = m_instructions.begin(); it != m_instructions.end() ; ++it)
    {
        auto& details = Operations::m_mapNameOperationDetails[(*it)->m_operation];
        if (details.operandSize == Null)
        {
            ATLTRACE(_T("IL_%04X %s"), (*it)->m_offset, details.stringName);
        }
        else
        {
            if (details.operandParam == ShortInlineBrTarget || details.operandParam == InlineBrTarget)
            {
                auto offset = (*it)->m_offset + (*it)->m_branchOffsets[0] + details.length + details.operandSize;
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
        for (auto offsetIter = (*it)->m_branchOffsets.begin(); offsetIter != (*it)->m_branchOffsets.end() ; ++offsetIter)
        {
            if ((*it)->m_operation == CEE_SWITCH)
            {
                auto offset = (*it)->m_offset + (4*static_cast<long>((*it)->m_operand)) + (*offsetIter) + details.length + details.operandSize;
                ATLTRACE(_T("    IL_%04X"), offset);
            }
        }
    }

    int i = 0;
    for (auto it = m_exceptions.begin(); it != m_exceptions.end() ; ++it)
    {
        ATLTRACE(_T("Section %d: %d %04X %04X %04X %04X %04X %08X"), 
            i++, (*it)->m_handlerType, 
            (*it)->m_tryStart != nullptr ? (*it)->m_tryStart->m_offset : 0, 
            (*it)->m_tryEnd != nullptr ? (*it)->m_tryEnd->m_offset : 0, 
            (*it)->m_handlerStart != nullptr ? (*it)->m_handlerStart->m_offset : 0, 
            (*it)->m_handlerEnd != nullptr ? (*it)->m_handlerEnd->m_offset : 0, 
            (*it)->m_filterStart != nullptr ? (*it)->m_filterStart->m_offset : 0, 
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
    for (auto it = m_instructions.begin(); it != m_instructions.end(); ++it)
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
    for (auto it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        auto& details = Operations::m_mapNameOperationDetails[(*it)->m_operation];
        (*it)->m_offset = position;
        position += details.length;
        position += details.operandSize;
        if((*it)->m_operation == CEE_SWITCH)
        {
            position += 4*static_cast<long>((*it)->m_operand);
        }
    }

    for (auto it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        auto& details = Operations::m_mapNameOperationDetails[(*it)->m_operation];
        if ((*it)->m_isBranch)
        {
            (*it)->m_branchOffsets.clear();
            if((*it)->m_operation == CEE_SWITCH)
            {
                auto offset = ((*it)->m_offset + details.length + details.operandSize + (4*static_cast<long>((*it)->m_operand)));                    
                for (auto bit = (*it)->m_branches.begin(); bit != (*it)->m_branches.end(); ++bit)
                {
                    (*it)->m_branchOffsets.push_back((*bit)->m_offset - offset);
                }
            }
            else
            {
                (*it)->m_operand = (*it)->m_branches[0]->m_offset - ((*it)->m_offset + details.length + details.operandSize);
                (*it)->m_branchOffsets.push_back(static_cast<long>((*it)->m_operand));
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
    auto lastInstruction = m_instructions.back();
    auto& details = Operations::m_mapNameOperationDetails[lastInstruction->m_operation];
    
    m_header.CodeSize = lastInstruction->m_offset + details.length + details.operandSize;
    long size = sizeof(IMAGE_COR_ILMETHOD_FAT) + m_header.CodeSize;

    m_header.Flags &= ~CorILMethod_MoreSects;
    if (m_exceptions.size() > 0)
    {
        m_header.Flags |= CorILMethod_MoreSects;
        long align = sizeof(DWORD) - 1;
        size = ((size + align) & ~align);
        size += ((static_cast<long>(m_exceptions.size()) * 6) + 1) * sizeof(long);
    }

    return size;
}

/// <summary>Test if a method has already been instrumented by comparing a list of instructions at that location</summary>
/// <param name="offset">The offset to look for.</param>
/// <param name="instructions">The list of instructions to compare with at that location.</param>
bool Method::IsInstrumented(long offset, const InstructionList &instructions)
{
    bool foundInstructionAtOffset = false;
    for (auto it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        if ((*it)->m_origOffset == offset)
        {
            foundInstructionAtOffset = true;
            for (auto it2 = instructions.begin(); it2 != instructions.end(); ++it2, ++it)
            {
                if (!(*it2)->Equivalent(*(*it))) 
                    return false;
            }
            break;
        }
    } 

    return foundInstructionAtOffset;
}

/// <summary>Insert a sequence of instructions at a specific offset</summary>
/// <param name="offset">The offset to look for.</param>
/// <param name="instructions">The list of instructions to insert at that location.</param>
/// <remarks>Original pointer references are maintained by inserting the sequence of instructions 
/// after the intended target and then using a copy operator on the <c>Instruction</c> objects to 
/// copy the data between them</remarks>
void Method::InsertInstructionsAtOffset(long offset, const InstructionList &instructions)
{
    InstructionList clone;
    for (auto it = instructions.begin(); it != instructions.end(); ++it)
    {
        clone.push_back(new Instruction(*(*it)));
    }

    //long actualOffset;
    for (auto it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        if ((*it)->m_offset == offset)
        {
            //actualOffset = (*it)->m_offset;
            m_instructions.insert(++it, clone.begin(), clone.end());
            break;
        }
    } 

    for (auto it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        if ((*it)->m_origOffset == offset)
        {
            auto orig = *(*it);
            for (unsigned int i=0;i<clone.size();i++)
            {
                auto temp = it++;
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
/// <param name="origOffset">The original (as in before any instrumentation) offset to look for.</param>
/// <param name="instructions">The list of instructions to insert at that location.</param>
/// <remarks>Original pointer references are maintained by inserting the sequence of instructions 
/// after the intended target and then using a copy operator on the <c>Instruction</c> objects to 
/// copy the data between them</remarks>
void Method::InsertInstructionsAtOriginalOffset(long origOffset, const InstructionList &instructions)
{
#define MOVE_TO_ENDOFBRANCH FALSE
    InstructionList clone;
    for (auto it = instructions.begin(); it != instructions.end(); ++it)
    {
        clone.push_back(new Instruction(*(*it)));
    }

    long actualOffset = 0;
    Instruction* actualInstruction = nullptr;
    for (auto it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        if ((*it)->m_origOffset == origOffset)
        {
            actualInstruction = *it;
            actualOffset = (actualInstruction)->m_offset;
            m_instructions.insert(++it, clone.begin(), clone.end());
            break;
        }
    } 

    if (!DoesTryHandlerPointToOffset (actualOffset))
    {
        for (auto it = m_instructions.begin(); it != m_instructions.end(); ++it)
        {
            if (*it == actualInstruction)
             {            
                Instruction orig = *(*it);
                for (unsigned int i=0;i<clone.size();i++)
                {
                    auto temp = it++;
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
/// <param name="offset">The offset to look for.</param>
/// <returns>An <c>Instruction</c> that exists at that location.</returns>
bool Method::DoesTryHandlerPointToOffset(long offset)
{
    for (auto it = m_exceptions.begin(); it != m_exceptions.end() ; ++it)
    {
        if (((*it)->m_handlerType == COR_ILEXCEPTION_CLAUSE_NONE) 
            && ((*it)->m_handlerStart->m_offset == offset)) return true;
    }
    return false;
}

/// <summary>Get the size of the COR_IL_MAP block</summary>
/// <returns>The size (number of elements) of the array to allocate</returns>
/// <remarks>Used with PopulateILMap to allocate and populate an array of COR_IL_MAP
/// which when used with SetILInstrumentedCodeMap can be used to inform any attached 
/// debugger where the new debug points are.</remarks>
ULONG Method::GetILMapSize()
{
    ULONG mapSize = 0;
    for (auto it = m_instructions.begin(); it != m_instructions.end(); ++it)
    {
        if ((*it)->m_origOffset != -1) 
            ++mapSize;
    }
    return mapSize;
}

/// <summary>Populate a supplied COR_IL_MAP block</summary>
/// <param name="mapSize">The size of the array.</param>
/// <param name="maps">The preallocated (CoTaskMemAlloc) array to populate.</param>
/// <remarks>Used with ULONG Method::GetILMapSize to allocate and populate an array of COR_IL_MAP
/// which when used with SetILInstrumentedCodeMap can be used to inform any attached 
/// debugger where the new debug points are.</remarks>
void Method::PopulateILMap(ULONG mapSize, COR_IL_MAP* maps)
{
    _ASSERTE(GetILMapSize() == mapSize);

    mapSize = 0;
    for (auto it = m_instructions.begin(); it != m_instructions.end(); ++it)
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


Instruction* Method::EndOfBranch(Instruction* toFollow)
    {
        Instruction* next = toFollow;
        next->m_jump = nullptr;
        Instruction* jump;
        Instruction* jumpTo;
        if (next != nullptr)
        {
            while ( next->m_operation == CEE_BR || next->m_operation == CEE_BR_S ) {
                _ASSERTE(next->m_isBranch);
                _ASSERTE(next->m_branches.size() == 1);
                _ASSERTE(next->m_branches[0] != NULL);
                jumpTo = next->m_branches[0];
                _ASSERTE(jumpTo != NULL);
                if ( jumpTo == nullptr ) break;
                jump = next; // store last BR instruction
                next = jumpTo; // store last BR jump-target instruction found (so far) 
                next->m_jump = jump; // set m_jump to last BR instruction
            }
        }
        return next == nullptr ? toFollow : next;
    }

