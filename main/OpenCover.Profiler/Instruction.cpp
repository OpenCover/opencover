//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#include "StdAfx.h"
#include "Instruction.h"

namespace Instrumentation
{
	Instruction::Instruction()
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

	Instruction& Instruction::operator = (const Instruction& b)
	{
		m_offset = b.m_offset;
		m_operation = b.m_operation;
		m_operand = b.m_operand;
		m_isBranch = b.m_isBranch;
		m_branchOffsets = b.m_branchOffsets;
		m_branches = b.m_branches;
		m_joins = b.m_joins;
		m_origOffset = b.m_origOffset;
		return *this;
	}

	bool Instruction::Equivalent(const Instruction&b)
	{
		if (m_operation != b.m_operation) 
			return false;
		if (m_operand != b.m_operand) 
			return false;
		if (m_branches.size() != b.m_branches.size()) 
			return false;
		auto it2 = b.m_branches.begin();
		for (auto it = m_branches.begin(); it != m_branches.end(); ++it, ++it2)
		{
			if (!(*it)->Equivalent(*(*it2)))
				return false;
		}
		return true;
	}
}