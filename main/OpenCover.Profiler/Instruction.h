//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#include "Operations.h"

#pragma once
#include "Messages.h"

class Instruction;
class Method;

typedef std::vector<Instruction*> InstructionList;

#define UNSAFE_BRANCH_OPERAND 0xDEADBABE

/// <summary>A representation of an IL instruction.</summary>
class Instruction
{
public:
    Instruction(CanonicalName operation, ULONGLONG operand);
    Instruction(CanonicalName operation);
    ~Instruction(void);
protected:
    Instruction(void);
    Instruction& operator = (const Instruction& b);
    bool Equivalent(const Instruction& b);

#ifdef TEST_FRAMEWORK
public:
#else
public:
#endif
    long m_offset;
    CanonicalName m_operation;
    ULONGLONG m_operand;
    bool m_isBranch;

    std::vector<long> m_branchOffsets;
    InstructionList m_branches;
    InstructionList m_joins;
    Instruction* m_jump;
    SequencePoint m_seqp;

    long m_origOffset;

public:

    friend class Method;
};


