#include "StdAfx.h"
#include "Instruction.h"

Instruction::Instruction(void)
{
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


Instruction::~Instruction(void)
{
}


