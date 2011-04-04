#include "Operations.h"

#pragma once

class Instruction;

typedef std::vector<Instruction *> InstructionList;
typedef InstructionList::iterator InstructionListIter;
typedef InstructionList::const_iterator InstructionListConstIter;

#define UNSAFE_BRANCH_OPERAND 0xDEADBABE

class Instruction
{
public:
    Instruction(void);
    ~Instruction(void);

public:
    long m_offset;
    CanonicalName m_operation;
    ULONGLONG m_operand;
    bool m_isBranch;

public:
    std::vector<long> m_branchOffsets;
    InstructionList m_branches;
};


