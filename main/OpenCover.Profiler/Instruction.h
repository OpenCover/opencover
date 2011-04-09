#include "Operations.h"

#pragma once

class Instruction;
class Method;

typedef std::vector<Instruction *> InstructionList;
typedef InstructionList::iterator InstructionListIter;
typedef InstructionList::const_iterator InstructionListConstIter;

#define UNSAFE_BRANCH_OPERAND 0xDEADBABE

/// <summary>A representation of an IL instruction.</summary>
class Instruction
{
public:
    Instruction(CanonicalName operation, ULONGLONG operand);
    ~Instruction(void);

protected:
    Instruction(void);
    Instruction& operator = (const Instruction& b);

private:
    long m_offset;
    CanonicalName m_operation;
    ULONGLONG m_operand;
    bool m_isBranch;

    std::vector<long> m_branchOffsets;
    InstructionList m_branches;

    long m_origOffset;

public:

    friend class Method;
};


