#include "Operations.h"

#pragma once

class Instruction;
class Method;

typedef std::vector<Instruction *> InstructionList;
typedef InstructionList::iterator InstructionListIter;
typedef InstructionList::const_iterator InstructionListConstIter;

#define UNSAFE_BRANCH_OPERAND 0xDEADBABE

class Instruction
{
public:
    Instruction(void);
    ~Instruction(void);

private:
    long m_offset;
    CanonicalName m_operation;
    ULONGLONG m_operand;
    bool m_isBranch;

    std::vector<long> m_branchOffsets;
    InstructionList m_branches;

public:

    friend class Method;
};


