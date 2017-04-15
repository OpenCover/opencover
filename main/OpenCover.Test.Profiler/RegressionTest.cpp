#include "stdafx.h"
#include "..\OpenCover.Profiler\Method.h"
#include "..\OpenCover.Profiler\CoverageInstrumentation.h"

// NOTE: Using pseudo IL code to exercise the code and is not necessarily runnable IL
using namespace Instrumentation;

class RegressionTest : public ::testing::Test {
	void SetUp() override
	{

	}

	void TearDown() override
	{

	}
};

TEST_F(RegressionTest, CanHandleOptimizedCode_Issue663)
{
	// arrange
	BYTE data[] = {
		// header (FAT)
		0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00,
		// code
		// try
		// {
		/*(IL_0000)*/ CEE_LDC_I4_S, 0x2A,
		/*(IL_0002)*/ CEE_NEWOBJ, 0x00, 0x00, 0x00, 0x00,
		/*(IL_0007)*/ CEE_STLOC_0,
		/*(IL_0008)*/ CEE_LEAVE_S, 0x01,
		// }
		// catch(...)
		// { 
		/*(IL_000A)*/ CEE_THROW,
		// }
		// 
		/*(IL_000B)*/ CEE_LDLOC_0,
		/*(IL_000C)*/ CEE_RET,
		0x00, 0x00, 0x00, // align
		// exceptions
		0x01, 0x0C, 0x00, 0x00,
		0x00, 0x00, 0x00, 0x00 /*(IL_0000)*/, 0x0A /*(IL_000A)*/, 0x0A, 0x00 /*(IL_000A)*/, 0x01 /*(IL_000B)*/, 0x00, 0x00, 0x00, 0x00 }; // will be turned into a long exception handler

	auto header = reinterpret_cast<IMAGE_COR_ILMETHOD_FAT*>(data);
	header->Flags = CorILMethod_FatFormat | CorILMethod_MoreSects;
	header->CodeSize = 13;
	header->Size = 3;

	Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

	//instrument.DumpIL();

	// act
	InstructionList instructions;
	CoverageInstrumentation::InsertInjectedMethod(instructions, 0x020FACED, 0x00);
	instrument.InsertInstructionsAtOriginalOffset(0x0, instructions);
	instrument.InsertInstructionsAtOriginalOffset(0xA, instructions);
	instrument.InsertInstructionsAtOriginalOffset(0xB, instructions);

	//instrument.DumpIL();

	// assert
	auto handlerEnd = instrument.m_exceptions.front()->m_handlerEnd;
	for (auto it = instrument.m_instructions.begin(); it != instrument.m_instructions.end(); ++it)
	{
		if ((*it)->m_offset == handlerEnd->m_offset)
		{
			--it;
			ASSERT_EQ(CEE_THROW, (*it)->m_operation);
			return;
		}
	}
	ASSERT_TRUE(false);
}