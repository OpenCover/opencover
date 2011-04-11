#include "stdafx.h"
#include "..\OpenCover.Profiler\Method.h"

class InstrumentationTest : public ::testing::Test {
    virtual void SetUp() {
        
    }

    virtual void TearDown() {
        
    }
};

TEST_F(InstrumentationTest, CanReadMethodWithTinyHeader)
{
    BYTE data[] = {(0x02 << 2) + CorILMethod_TinyFormat, 
        CEE_NOP, CEE_RET};

    Method instrument((IMAGE_COR_ILMETHOD*)data);

    ASSERT_EQ(2, instrument.m_instructions.size());
}

TEST_F(InstrumentationTest, CanReadMethodWithFatHeader)
{
    BYTE data[] = {
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        CEE_NOP, CEE_RET};

    IMAGE_COR_ILMETHOD_FAT * pHeader = (IMAGE_COR_ILMETHOD_FAT*)data;

    pHeader->Flags = CorILMethod_FatFormat;
    pHeader->CodeSize = 2;
    pHeader->Size = 3;

    Method instrument((IMAGE_COR_ILMETHOD*)data);

    ASSERT_EQ(2, instrument.m_instructions.size());
}

TEST_F(InstrumentationTest, CanConvertSmallBranchesToLongBranches)
{
    // This is pseudo IL code to exercise the code and is not runnable IL
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

    Method instrument((IMAGE_COR_ILMETHOD*)data);

    ASSERT_EQ(15, instrument.m_instructions.size());

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
    // This is pseudo IL code to exercise the code and is not runnable IL
    BYTE data[] = {(11 << 2) + CorILMethod_TinyFormat, 
        CEE_BR, 0x05, 0x00, 0x00, 0x00,
        CEE_BR, 0x00, 0x00, 0x00, 0x00,
        CEE_RET};

    Method instrument((IMAGE_COR_ILMETHOD*)data);

    ASSERT_EQ(3, instrument.m_instructions.size());

    ASSERT_EQ(instrument.m_instructions[2]->m_operation, 
        instrument.m_instructions[0]->m_branches[0]->m_operation);
    ASSERT_EQ(instrument.m_instructions[2]->m_operation, 
        instrument.m_instructions[1]->m_branches[0]->m_operation);
}

TEST_F(InstrumentationTest, ConvertedBranchesPointToCorrectTargets)
{
    // This is pseudo IL code to exercise the code and is not runnable IL
    BYTE data[] = {(8 << 2) + CorILMethod_TinyFormat, 
        CEE_BR_S, 0x05,
        CEE_BR, 0x00, 0x00, 0x00, 0x00,
        CEE_RET};

    Method instrument((IMAGE_COR_ILMETHOD*)data);

    ASSERT_EQ(3, instrument.m_instructions.size());
    ASSERT_EQ(CEE_BR, instrument.m_instructions[0]->m_operation);

    ASSERT_EQ(instrument.m_instructions[2]->m_operation, 
        instrument.m_instructions[0]->m_branches[0]->m_operation);
    ASSERT_EQ(instrument.m_instructions[2]->m_operation, 
        instrument.m_instructions[1]->m_branches[0]->m_operation);
}

TEST_F(InstrumentationTest, HandlesSwitchBranches)
{
    // This is pseudo IL code to exercise the code and is not runnable IL
    BYTE data[] = {(14 << 2) + CorILMethod_TinyFormat, 
        CEE_SWITCH, 0x02, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,
        CEE_RET};

    Method instrument((IMAGE_COR_ILMETHOD*)data);

    ASSERT_EQ(2, instrument.m_instructions.size());

    ASSERT_EQ(instrument.m_instructions[1]->m_operation, 
        instrument.m_instructions[0]->m_branches[0]->m_operation);
    ASSERT_EQ(instrument.m_instructions[1]->m_operation, 
        instrument.m_instructions[0]->m_branches[1]->m_operation);
}

TEST_F(InstrumentationTest, CanReadShortExceptions)
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
        0x00, 0x24, 0x00, 0x00,
        0x00, 0x00, 0x01, 0x00, 0x04, 0x05, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00,  
        0x00, 0x00, 0x01, 0x00, 0x04, 0x0a, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00,  
        0x02, 0x00, 0x01, 0x00, 0x11, 0x12, 0x00, 0x03, 0x00, 0x00, 0x00, 0x00,  
        
    };

    IMAGE_COR_ILMETHOD_FAT * pHeader = (IMAGE_COR_ILMETHOD_FAT*)data;

    pHeader->Flags = CorILMethod_FatFormat | CorILMethod_MoreSects;
    pHeader->CodeSize = 23;
    pHeader->Size = 3;

    Method instrument((IMAGE_COR_ILMETHOD*)data);

    ASSERT_EQ(19, instrument.m_instructions.size());
    ASSERT_EQ(3, instrument.m_exceptions.size());
}

TEST_F(InstrumentationTest, CanReadFatExceptions)
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
        0x40, 0x48, 0x00, 0x00,
        0x00, 0x00, 0x00, 0x00,  0x01, 0x00, 0x00, 0x00,  0x04, 0x00, 0x00, 0x00,  0x05, 0x00, 0x00, 0x00,  0x05, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  
        0x00, 0x00, 0x00, 0x00,  0x01, 0x00, 0x00, 0x00,  0x04, 0x00, 0x00, 0x00,  0x0a, 0x00, 0x00, 0x00,  0x05, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  
        0x02, 0x00, 0x00, 0x00,  0x01, 0x00, 0x00, 0x00,  0x11, 0x00, 0x00, 0x00,  0x12, 0x00, 0x00, 0x00,  0x03, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  
        
    };

    IMAGE_COR_ILMETHOD_FAT * pHeader = (IMAGE_COR_ILMETHOD_FAT*)data;

    pHeader->Flags = CorILMethod_FatFormat | CorILMethod_MoreSects;
    pHeader->CodeSize = 23;
    pHeader->Size = 3;

    Method instrument((IMAGE_COR_ILMETHOD*)data);

    ASSERT_EQ(19, instrument.m_instructions.size());
    ASSERT_EQ(3, instrument.m_exceptions.size());
}