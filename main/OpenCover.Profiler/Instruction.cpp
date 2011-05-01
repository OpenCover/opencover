#include "StdAfx.h"
#include "Instruction.h"

Instruction::Instruction(void)
{
    m_operation = CEE_NOP;
    m_operand = 0;
    m_offset = -1;
    m_isBranch = false;
    m_origOffset = -1;
}

Instruction::Instruction(CanonicalName operation, ULONGLONG operand)
{
    m_operation = operation;
    m_operand = operand;
    m_offset = -1;
    m_isBranch = false;
    m_origOffset = -1;
}

Instruction::Instruction(CanonicalName operation)
{
    m_operation = operation;
    m_operand = 0;
    m_offset = -1;
    m_isBranch = false;
    m_origOffset = -1;
}

Instruction::~Instruction(void)
{
}

Instruction& Instruction::operator = (const Instruction& b)
{
    m_offset = b.m_offset;
    m_operation = b.m_operation;
    m_operand = b.m_operand;
    m_isBranch = b.m_isBranch;
    m_branchOffsets = b.m_branchOffsets;
    m_branches = b.m_branches;
    m_origOffset = b.m_origOffset;
    return *this;
}


