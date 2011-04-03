#include "Operations.h"

#pragma once

class Instruction;

typedef std::vector<Instruction *> InstructionList;
typedef InstructionList::iterator InstructionListIter;
typedef InstructionList::const_iterator InstructionListConstIter;

class Instruction
{
public:
    Instruction(void);
    ~Instruction(void);

public:
    unsigned int m_offset;
    CanonicalName m_operation;
    __int64 m_operand;
    bool m_isBranch;

public:
    std::vector<short> m_branchOffsets;
    InstructionList m_branches;
};


