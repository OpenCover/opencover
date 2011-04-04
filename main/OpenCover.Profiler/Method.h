#include "Instruction.h"
#include "ExceptionHandler.h"

#pragma once


class Method
{
public:
    Method();
    ~Method();

    void ReadMethod(IMAGE_COR_ILMETHOD* pMethod);

private:
    void ReadBody(BYTE* pCode, long codeSize);
    void ConvertShortBranches();
    void DumpIL();
    void ResolveBranches();
    Instruction * GetInstructionAtOffset(long offset);
    void ReadSections(BYTE *pCode, long position);

private:
    // all instrumented methods will be FAT regardless
    IMAGE_COR_ILMETHOD_FAT m_header;

    template<typename value_type> value_type Read(BYTE** buffer, long * position) {
        value_type value = *(value_type*)(*buffer);
        *buffer += sizeof(value_type);
        *position += sizeof(value_type);
        return value;
    }

    template<typename value_type> void Align(BYTE** buffer, long * position) {
        long i = sizeof(value_type) - 1;
        long incr = ((*position + i) & ~3) - *position;
        *buffer += incr;
        *position += incr;
    }

    void Advance(long num, BYTE** buffer, long * position) {
        *buffer += num;
        *position += num;
    }

    ExceptionHandlerList m_exceptions;

public:
    InstructionList m_instructions;
};

 
 

