#include "Instruction.h"

#pragma once

class Method
{
public:
    Method();
    ~Method();

    void ReadMethod(IMAGE_COR_ILMETHOD* pMethod);

private:
    void ReadBody(BYTE* pCode, unsigned int codeSize);

private:
    // all instrumented methods will be FAT regardless
    IMAGE_COR_ILMETHOD_FAT m_header;

    template<typename value_type> value_type Read(BYTE** buffer, unsigned int * position) {
        value_type value = *(value_type*)(*buffer);
        *buffer += sizeof(value_type);
        *position += sizeof(value_type);
        return value;
    }

public:
    InstructionList m_instructions;
};

 
 

