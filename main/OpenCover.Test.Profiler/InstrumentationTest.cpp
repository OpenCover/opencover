#include "stdafx.h"
#include "..\OpenCover.Profiler\Method.h"
#include <memory>

// NOTE: Using pseudo IL code to exercise the code and is not necessarily runnable IL
using namespace Instrumentation;

class InstrumentationTest : public ::testing::Test {
	void SetUp() override
    {
        
    }

	void TearDown() override
    {
        
    }
};

TEST_F(InstrumentationTest, CanReadMethodWithTinyHeader)
{
    BYTE data[] = {(0x02 << 2) + CorILMethod_TinyFormat, 
        CEE_NOP, CEE_RET};

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    ASSERT_EQ(2, instrument.GetNumberOfInstructions());
}

TEST_F(InstrumentationTest, CanReadMethodWithFatHeader)
{
    BYTE data[] = {
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        CEE_NOP, CEE_RET};

	auto pHeader = reinterpret_cast<IMAGE_COR_ILMETHOD_FAT*>(data);

    pHeader->Flags = CorILMethod_FatFormat;
    pHeader->CodeSize = 2;
    pHeader->Size = 3;

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    ASSERT_EQ(2, instrument.GetNumberOfInstructions());
}

TEST_F(InstrumentationTest, CanConvertSmallBranchesToLongBranches)
{
    BYTE data[] = {(29 << 2) + CorILMethod_TinyFormat, 
        CEE_BR_S, 0x00,
        CEE_BRFALSE_S, 0x00,
        CEE_BRTRUE_S, 0x00,
        CEE_BEQ_S, 0x00,
        CEE_BGE_S, 0x00,
        CEE_BGT_S, 0x00,
        CEE_BLE_S, 0x00,
        CEE_BLT_S, 0x00,
        CEE_BNE_UN_S, 0x00,
        CEE_BGE_UN_S, 0x00,
        CEE_BGT_UN_S, 0x00,
        CEE_BLE_UN_S, 0x00,
        CEE_BLT_UN_S, 0x00,
        CEE_LEAVE_S, 0x00,
        CEE_RET};

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    ASSERT_EQ(15, instrument.GetNumberOfInstructions());

    ASSERT_EQ(CEE_BR, instrument.m_instructions[0]->m_operation);
    ASSERT_EQ(CEE_BRFALSE, instrument.m_instructions[1]->m_operation);
    ASSERT_EQ(CEE_BRTRUE, instrument.m_instructions[2]->m_operation);
    ASSERT_EQ(CEE_BEQ, instrument.m_instructions[3]->m_operation);
    ASSERT_EQ(CEE_BGE, instrument.m_instructions[4]->m_operation);
    ASSERT_EQ(CEE_BGT, instrument.m_instructions[5]->m_operation);
    ASSERT_EQ(CEE_BLE, instrument.m_instructions[6]->m_operation);
    ASSERT_EQ(CEE_BLT, instrument.m_instructions[7]->m_operation);
    ASSERT_EQ(CEE_BNE_UN, instrument.m_instructions[8]->m_operation);
    ASSERT_EQ(CEE_BGE_UN, instrument.m_instructions[9]->m_operation);
    ASSERT_EQ(CEE_BGT_UN, instrument.m_instructions[10]->m_operation);
    ASSERT_EQ(CEE_BLE_UN, instrument.m_instructions[11]->m_operation);
    ASSERT_EQ(CEE_BLT_UN, instrument.m_instructions[12]->m_operation);
    ASSERT_EQ(CEE_LEAVE, instrument.m_instructions[13]->m_operation);
}

TEST_F(InstrumentationTest, BranchesPointToCorrectTargets)
{
    BYTE data[] = {(11 << 2) + CorILMethod_TinyFormat, 
        CEE_BR, 0x05, 0x00, 0x00, 0x00,
        CEE_BR, 0x00, 0x00, 0x00, 0x00,
        CEE_RET};

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    ASSERT_EQ(3, instrument.GetNumberOfInstructions());

    ASSERT_EQ(instrument.m_instructions[2]->m_operation, 
        instrument.m_instructions[0]->m_branches[0]->m_operation);
    ASSERT_EQ(instrument.m_instructions[2]->m_operation, 
        instrument.m_instructions[1]->m_branches[0]->m_operation);
}

TEST_F(InstrumentationTest, ConvertedBranchesPointToCorrectTargets)
{
    BYTE data[] = {(8 << 2) + CorILMethod_TinyFormat, 
        CEE_BR_S, 0x05,
        CEE_BR, 0x00, 0x00, 0x00, 0x00,
        CEE_RET};

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    ASSERT_EQ(3, instrument.GetNumberOfInstructions());
    ASSERT_EQ(CEE_BR, instrument.m_instructions[0]->m_operation);

    ASSERT_EQ(instrument.m_instructions[2]->m_operation, 
        instrument.m_instructions[0]->m_branches[0]->m_operation);
    ASSERT_EQ(instrument.m_instructions[2]->m_operation, 
        instrument.m_instructions[1]->m_branches[0]->m_operation);
}

TEST_F(InstrumentationTest, HandlesSwitchBranches)
{
    BYTE data[] = {(14 << 2) + CorILMethod_TinyFormat, 
        CEE_SWITCH, 0x02, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        CEE_RET};

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    ASSERT_EQ(2, instrument.GetNumberOfInstructions());

    ASSERT_EQ(instrument.m_instructions[1]->m_operation, 
        instrument.m_instructions[0]->m_branches[0]->m_operation);
    ASSERT_EQ(instrument.m_instructions[1]->m_operation, 
        instrument.m_instructions[0]->m_branches[1]->m_operation);
}

TEST_F(InstrumentationTest, CanReadShortExceptionsWithFinally)
{
    BYTE data[] = {
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        CEE_NOP,  
        CEE_NOP, //<--
        CEE_NOP, 
        CEE_LEAVE_S, 0X0A,
        CEE_POP, 
        CEE_NOP, 
        CEE_NOP,
        CEE_LEAVE_S, 0X05,
        CEE_POP, 
        CEE_NOP, 
        CEE_NOP,
        CEE_LEAVE_S, 0X00,
        CEE_NOP, 
        CEE_LEAVE_S, 0X03,
        CEE_NOP, 
        CEE_NOP, 
        CEE_ENDFINALLY,
        CEE_NOP, 
        CEE_RET,
        0x00, // align
        0x01, 0x24, 0x00, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x04, 0x05, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00,  
        0x00, 0x00, 0x01, 0x00, 0x04, 0x0a, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00,  
        0x02, 0x00, 0x01, 0x00, 0x11, 0x12, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00,  
        
    };

	auto pHeader = reinterpret_cast<IMAGE_COR_ILMETHOD_FAT*>(data);

    pHeader->Flags = CorILMethod_FatFormat | CorILMethod_MoreSects;
    pHeader->CodeSize = 23;
    pHeader->Size = 3;

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    ASSERT_EQ(19, instrument.GetNumberOfInstructions());
    ASSERT_EQ(3, instrument.GetNumberOfExceptions());
}

TEST_F(InstrumentationTest, CanReadShortExceptionsEndingWithFinally)
{
    BYTE data[] = {
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        CEE_NOP,  
        CEE_NOP, //<--
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP,
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP,
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_ENDFINALLY,
        0x00, // align
        0x00, // align
        0x00, // align
        0x01, 0x0c, 0x00, 0x00,
        0x02, 0x00, 0x01, 0x00, 0x11, 0x12, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00,  
    };

	auto pHeader = reinterpret_cast<IMAGE_COR_ILMETHOD_FAT*>(data);

    pHeader->Flags = CorILMethod_FatFormat | CorILMethod_MoreSects;
    pHeader->CodeSize = 21;
    pHeader->Size = 3;

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    ASSERT_EQ(22, instrument.GetNumberOfInstructions());
    ASSERT_EQ(1, instrument.GetNumberOfExceptions());
}

TEST_F(InstrumentationTest, CanReadShortExceptionsWithFault)
{
    BYTE data[] = {
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        CEE_NOP,  
        CEE_NOP, //<--
        CEE_NOP, 
        CEE_LEAVE_S, 0X0A,
        CEE_POP, 
        CEE_NOP, 
        CEE_NOP,
        CEE_LEAVE_S, 0X05,
        CEE_POP, 
        CEE_NOP, 
        CEE_NOP,
        CEE_LEAVE_S, 0X00,
        CEE_NOP, 
        CEE_LEAVE_S, 0X03,
        CEE_NOP, 
        CEE_NOP, 
        CEE_ENDFINALLY,
        CEE_NOP, 
        CEE_RET,
        0x00, // align
        0x01, 0x24, 0x00, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x04, 0x05, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00,  
        0x00, 0x00, 0x01, 0x00, 0x04, 0x0a, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00,  
        0x02, 0x00, 0x01, 0x00, 0x11, 0x12, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00,  
        
    };

	auto pHeader = reinterpret_cast<IMAGE_COR_ILMETHOD_FAT*>(data);

    pHeader->Flags = CorILMethod_FatFormat | CorILMethod_MoreSects;
    pHeader->CodeSize = 23;
    pHeader->Size = 3;

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    ASSERT_EQ(19, instrument.GetNumberOfInstructions());
    ASSERT_EQ(3, instrument.GetNumberOfExceptions());
}

TEST_F(InstrumentationTest, CanReadShortExceptionsEndingWithFault)
{
    BYTE data[] = {
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        CEE_NOP,  
        CEE_NOP, //<--
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP,
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP,
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_ENDFINALLY,
        0x00, // align
        0x00, // align
        0x00, // align
        0x01, 0x0c, 0x00, 0x00,
        0x04, 0x00, 0x01, 0x00, 0x11, 0x12, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00,  
    };

	auto pHeader = reinterpret_cast<IMAGE_COR_ILMETHOD_FAT*>(data);

    pHeader->Flags = CorILMethod_FatFormat | CorILMethod_MoreSects;
    pHeader->CodeSize = 21;
    pHeader->Size = 3;

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    ASSERT_EQ(22, instrument.GetNumberOfInstructions());
    ASSERT_EQ(1, instrument.GetNumberOfExceptions());
}

TEST_F(InstrumentationTest, CanReadFatExceptionsWithFinally)
{
    BYTE data[] = {
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        CEE_NOP,  
        CEE_NOP, //<--
        CEE_NOP, 
        CEE_LEAVE_S, 0X0A,
        CEE_POP, 
        CEE_NOP, 
        CEE_NOP,
        CEE_LEAVE_S, 0X05,
        CEE_POP, 
        CEE_NOP, 
        CEE_NOP,
        CEE_LEAVE_S, 0X00,
        CEE_NOP, 
        CEE_LEAVE_S, 0X03,
        CEE_NOP, 
        CEE_NOP, 
        CEE_ENDFINALLY,
        CEE_NOP, 
        CEE_RET,
        0x00, // align
        0x41, 0x48, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,  0x01, 0x00, 0x00, 0x00,  0x04, 0x00, 0x00, 0x00,  0x05, 0x00, 0x00, 0x00,  0x05, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  
        0x00, 0x00, 0x00, 0x00,  0x01, 0x00, 0x00, 0x00,  0x04, 0x00, 0x00, 0x00,  0x0a, 0x00, 0x00, 0x00,  0x05, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  
        0x02, 0x00, 0x00, 0x00,  0x01, 0x00, 0x00, 0x00,  0x11, 0x00, 0x00, 0x00,  0x12, 0x00, 0x00, 0x00,  0x03, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  
        
    };

	auto pHeader = reinterpret_cast<IMAGE_COR_ILMETHOD_FAT*>(data);

    pHeader->Flags = CorILMethod_FatFormat | CorILMethod_MoreSects;
    pHeader->CodeSize = 23;
    pHeader->Size = 3;

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    ASSERT_EQ(19, instrument.GetNumberOfInstructions());
    ASSERT_EQ(3, instrument.GetNumberOfExceptions());
}

TEST_F(InstrumentationTest, CanReadFatExceptionsEndingWithFinally)
{
    BYTE data[] = {
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        CEE_NOP,  
        CEE_NOP, //<--
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP,
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP,
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_ENDFINALLY,
        0x00, // align
        0x00, // align
        0x00, // align
        0x41, 0x18, 0x00, 0x00,
        0x02, 0x00, 0x00, 0x00,  0x01, 0x00, 0x00, 0x00,  0x11, 0x00, 0x00, 0x00,  0x12, 0x00, 0x00, 0x00,  0x03, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  
        
    };

    IMAGE_COR_ILMETHOD_FAT * pHeader = reinterpret_cast<IMAGE_COR_ILMETHOD_FAT*>(data);

    pHeader->Flags = CorILMethod_FatFormat | CorILMethod_MoreSects;
    pHeader->CodeSize = 21;
    pHeader->Size = 3;

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    ASSERT_EQ(22, instrument.GetNumberOfInstructions());
    ASSERT_EQ(1, instrument.GetNumberOfExceptions());
}

TEST_F(InstrumentationTest, CanReadFatExceptionsWithFault)
{
    BYTE data[] = {
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        CEE_NOP,  
        CEE_NOP, //<--
        CEE_NOP, 
        CEE_LEAVE_S, 0X0A,
        CEE_POP, 
        CEE_NOP, 
        CEE_NOP,
        CEE_LEAVE_S, 0X05,
        CEE_POP, 
        CEE_NOP, 
        CEE_NOP,
        CEE_LEAVE_S, 0X00,
        CEE_NOP, 
        CEE_LEAVE_S, 0X03,
        CEE_NOP, 
        CEE_NOP, 
        CEE_ENDFINALLY,
        CEE_NOP, 
        CEE_RET,
        0x00, // align
        0x41, 0x48, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,  0x01, 0x00, 0x00, 0x00,  0x04, 0x00, 0x00, 0x00,  0x05, 0x00, 0x00, 0x00,  0x05, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  
        0x00, 0x00, 0x00, 0x00,  0x01, 0x00, 0x00, 0x00,  0x04, 0x00, 0x00, 0x00,  0x0a, 0x00, 0x00, 0x00,  0x05, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  
        0x04, 0x00, 0x00, 0x00,  0x01, 0x00, 0x00, 0x00,  0x11, 0x00, 0x00, 0x00,  0x12, 0x00, 0x00, 0x00,  0x03, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  
    };

    IMAGE_COR_ILMETHOD_FAT * pHeader = reinterpret_cast<IMAGE_COR_ILMETHOD_FAT*>(data);

    pHeader->Flags = CorILMethod_FatFormat | CorILMethod_MoreSects;
    pHeader->CodeSize = 23;
    pHeader->Size = 3;

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    ASSERT_EQ(19, instrument.GetNumberOfInstructions());
    ASSERT_EQ(3, instrument.GetNumberOfExceptions());
}

TEST_F(InstrumentationTest, CanReadFatExceptionsEndingWithFault)
{
    BYTE data[] = {
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        CEE_NOP,  
        CEE_NOP, //<--
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP,
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP,
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_NOP, 
        CEE_ENDFINALLY,
        0x00, // align
        0x00, // align
        0x00, // align
        0x41, 0x18, 0x00, 0x00,
        0x04, 0x00, 0x00, 0x00,  0x01, 0x00, 0x00, 0x00,  0x11, 0x00, 0x00, 0x00,  0x12, 0x00, 0x00, 0x00,  0x03, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  
    };

    IMAGE_COR_ILMETHOD_FAT * pHeader = reinterpret_cast<IMAGE_COR_ILMETHOD_FAT*>(data);

    pHeader->Flags = CorILMethod_FatFormat | CorILMethod_MoreSects;
    pHeader->CodeSize = 21;
    pHeader->Size = 3;

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    ASSERT_EQ(22, instrument.GetNumberOfInstructions());
    ASSERT_EQ(1, instrument.GetNumberOfExceptions());
}

TEST_F(InstrumentationTest, Calculates_Size_NoExceptionHandlers)
{
    BYTE data[] = {(8 << 2) + CorILMethod_TinyFormat, 
        CEE_BR_S, 0x05,
        CEE_BR, 0x00, 0x00, 0x00, 0x00,
        CEE_RET};
    
    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    
    ASSERT_EQ(23, static_cast<int>(instrument.GetMethodSize()));
}

TEST_F(InstrumentationTest, Calculates_Size_WithExceptionHandlers)
{
    BYTE data[] = {
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,      
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,       
        CEE_RET,
        0x00, 0x00, 0x00, // align
        0x01, 0x0C, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}; // will be turned into a long exception handler

	auto pHeader = reinterpret_cast<IMAGE_COR_ILMETHOD_FAT*>(data);
    pHeader->Flags = CorILMethod_FatFormat | CorILMethod_MoreSects;
    pHeader->CodeSize = 25;
    pHeader->Size = 3;

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    ASSERT_EQ(68, static_cast<int>(instrument.GetMethodSize()));
}

TEST_F(InstrumentationTest, CanInsertInstructions_Whilst_Maintaining_Pointer)
{
    BYTE data[] = {(8 << 2) + CorILMethod_TinyFormat, 
        CEE_BR_S, 0x05,
        CEE_BR, 0x00, 0x00, 0x00, 0x00,
        CEE_RET};

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    InstructionList instructions;
    instructions.push_back(new Instruction(CEE_NOP, 0));

    instrument.InsertInstructionsAtOriginalOffset(7, instructions);

    instrument.DumpIL(true);

    ASSERT_EQ(4, instrument.GetNumberOfInstructions());

    ASSERT_EQ(CEE_NOP, instrument.m_instructions[2]->m_operation);
    ASSERT_EQ(CEE_RET, instrument.m_instructions[3]->m_operation);
    ASSERT_EQ(CEE_NOP, instrument.m_instructions[0]->m_branches[0]->m_operation);
    ASSERT_EQ(CEE_NOP, instrument.m_instructions[1]->m_branches[0]->m_operation);
}

TEST_F(InstrumentationTest, CanWriteMethod)
{
    BYTE data[] = {(8 << 2) + CorILMethod_TinyFormat, 
        CEE_BR_S, 0x05,
        CEE_BR, 0x00, 0x00, 0x00, 0x00,
        CEE_RET};

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    int size = instrument.GetMethodSize();

	std::unique_ptr<BYTE[]> buffer(new BYTE[size]);

	auto newMethod = reinterpret_cast<COR_ILMETHOD_FAT*>(buffer.get());

    instrument.WriteMethod(reinterpret_cast<IMAGE_COR_ILMETHOD*>(newMethod));
    
    ASSERT_TRUE(newMethod->IsFat());
    ASSERT_EQ(0, newMethod->GetFlags() & CorILMethod_MoreSects);
    ASSERT_EQ(3, newMethod->GetSize());
    ASSERT_EQ(11, newMethod->GetCodeSize());
}

TEST_F(InstrumentationTest, CanWriteMethodWithExceptions)
{
    BYTE data[] = {
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,      
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,       
        CEE_RET,
        0x00, 0x00, 0x00, // align
        0x01, 0x0C, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00}; // will be turned into a long exception handler

	auto pHeader = reinterpret_cast<IMAGE_COR_ILMETHOD_FAT*>(data);
    pHeader->Flags = CorILMethod_FatFormat | CorILMethod_MoreSects;
    pHeader->CodeSize = 25;
    pHeader->Size = 3;

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    int size = instrument.GetMethodSize();

	std::unique_ptr<BYTE[]> buffer(new BYTE[size]);

	auto newMethod = reinterpret_cast<COR_ILMETHOD_FAT*>(buffer.get());

    instrument.WriteMethod(reinterpret_cast<IMAGE_COR_ILMETHOD*>(newMethod));
    
    ASSERT_TRUE(newMethod->IsFat());
    ASSERT_EQ(CorILMethod_MoreSects, newMethod->GetFlags() & CorILMethod_MoreSects);
    ASSERT_EQ(3, newMethod->GetSize());
    ASSERT_EQ(25, newMethod->GetCodeSize());

	auto pSect = newMethod->GetSect();
    ASSERT_TRUE(pSect->IsFat());
    ASSERT_EQ(CorILMethod_Sect_EHTable, pSect->Kind());
    ASSERT_EQ(28, pSect->DataSize()); // 24 (1 FAT section) + 4
}

TEST_F(InstrumentationTest, CanCalculateCorrectILMapSize)
{
    BYTE data[] = {(8 << 2) + CorILMethod_TinyFormat, 
        CEE_BR_S, 0x05,
        CEE_BR, 0x00, 0x00, 0x00, 0x00,
        CEE_RET};

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    InstructionList instructions;
    instructions.push_back(new Instruction(CEE_NOP, 0));
    instructions.push_back(new Instruction(CEE_NOP, 0));

    instrument.InsertInstructionsAtOriginalOffset(7, instructions);

    ASSERT_EQ(3, static_cast<int>(instrument.GetILMapSize()));
}

TEST_F(InstrumentationTest, CanPopulateSuppliedILMapSize)
{
    BYTE data[] = {(8 << 2) + CorILMethod_TinyFormat, 
        CEE_BR_S, 0x05,
        CEE_BR, 0x00, 0x00, 0x00, 0x00,
        CEE_RET};

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

    InstructionList instructions;
    instructions.push_back(new Instruction(CEE_NOP, 0));
    instructions.push_back(new Instruction(CEE_NOP, 0));

    instrument.InsertInstructionsAtOriginalOffset(7, instructions);

	std::unique_ptr<COR_IL_MAP[]> map(new COR_IL_MAP[instrument.GetILMapSize()]);

    instrument.PopulateILMap(3, map.get());

    ASSERT_EQ(0, map[0].oldOffset);
    ASSERT_EQ(0, map[0].newOffset);

    ASSERT_EQ(2, map[1].oldOffset);
    ASSERT_EQ(5, map[1].newOffset);

    ASSERT_EQ(7, map[2].oldOffset);
    ASSERT_EQ(12, map[2].newOffset);
}

TEST_F(InstrumentationTest, WillAddCodeLabelWhenClauseExtendsLastOpcode)
{
    BYTE data[] = {
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,      
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,       
        0xFE, 0x1A, // CEE_RETHROW
        0x00, 0x00, // align
        0x01, 0x0C, 0x00, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x11, 0x12, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00}; 

	DWORD codeSize = 26;

	auto pHeader = reinterpret_cast<IMAGE_COR_ILMETHOD_FAT*>(data);
    pHeader->Flags = CorILMethod_FatFormat | CorILMethod_MoreSects;
    pHeader->CodeSize = codeSize;
    pHeader->Size = 3;

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));

	ASSERT_EQ(codeSize, static_cast<DWORD>(instrument.GetCodeSize())); // no change in size	
	ASSERT_EQ(CEE_CODE_LABEL, instrument.m_instructions.back()->m_operation);
}

TEST_F(InstrumentationTest, CanIdentifyInstrumentedMethods)
{
    BYTE data[] = {(0x02 << 2) + CorILMethod_TinyFormat, 
        CEE_NOP, CEE_RET};

    Method instrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(data));
    ASSERT_EQ(2, instrument.GetNumberOfInstructions());

	InstructionList instructions;
	instructions.push_back(new Instruction(CEE_LDC_I4, 1234));
	instructions.push_back(new Instruction(CEE_CALL, 1234));

	ASSERT_FALSE(instrument.IsInstrumented(0, instructions));
	instrument.InsertInstructionsAtOffset(0, instructions);

	// now we need to pretend we are rejitting 
    int size = instrument.GetMethodSize();

	std::unique_ptr<BYTE[]> buffer(new BYTE[size]);
	auto newMethod = reinterpret_cast<COR_ILMETHOD_FAT*>(buffer.get());
    instrument.WriteMethod(reinterpret_cast<IMAGE_COR_ILMETHOD*>(newMethod));

    Method newInstrument(reinterpret_cast<IMAGE_COR_ILMETHOD*>(newMethod));

	ASSERT_TRUE(newInstrument.IsInstrumented(0, instructions));
	ASSERT_FALSE(newInstrument.IsInstrumented(5, instructions));
	ASSERT_FALSE(newInstrument.IsInstrumented(1, instructions)); // no instruction at offset 0

}
