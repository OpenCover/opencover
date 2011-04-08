#include "Instruction.h"
#include "ExceptionHandler.h"

#pragma once

/// <summary>The <c>MethodBuffer</c> encapsulates all the buffer handling</summary>
class MethodBuffer
{
protected:
    void SetBuffer(BYTE* pBuffer) {
        m_bufferBase = pBuffer;
        m_bufferCurrent = pBuffer;
        m_position = 0;
    }

    long GetPosition() { return m_position; }

    template<typename value_type> value_type Read() {
        _ASSERTE(m_bufferCurrent != NULL);
        value_type value = *(value_type*)(m_bufferCurrent);
        Advance(sizeof(value_type));
        return value;
    }

    template<typename value_type> void Write(value_type value) {
        _ASSERTE(m_bufferCurrent != NULL);
        *(value_type*)(m_bufferCurrent) = value;
        Advance(sizeof(value_type));
    }

    template<typename value_type> void Align() {
        _ASSERTE(m_bufferCurrent != NULL);
        long i = sizeof(value_type) - 1;
        long incr = ((m_position + i) & ~3) - m_position;
        Advance(incr);
    }

    void Advance(long num) {
        _ASSERTE(m_bufferCurrent != NULL);
        m_bufferCurrent += num;
        m_position += num;
    }

private:
    BYTE * m_bufferBase;
    BYTE * m_bufferCurrent;
    long m_position;
};

/// <summary>The <c>Method</c> entity builds a 'model' of the IL that can then be modified</summary>
class Method : MethodBuffer
{
public:
    Method();
    ~Method();

    void ReadMethod(IMAGE_COR_ILMETHOD* pMethod);
    long GetMethodSize();
    void WriteMethod(IMAGE_COR_ILMETHOD* pMethod);

private:
    void ReadBody();
    void ConvertShortBranches();
    void DumpIL();
    void ResolveBranches();
    Instruction * GetInstructionAtOffset(long offset);
    void ReadSections();
    void RecalculateOffsets();

private:
    // all instrumented methods will be FAT (with FAT SECTIONS if exist) regardless
    IMAGE_COR_ILMETHOD_FAT m_header;

    ExceptionHandlerList m_exceptions;
    InstructionList m_instructions;

public:
};

 
 

