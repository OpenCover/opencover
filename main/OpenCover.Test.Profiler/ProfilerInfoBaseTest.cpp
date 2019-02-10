#include "stdafx.h"
#include "TestProfilerInfo.h"
#include "MockProfilerInfo.h"
#include "ComBaseTest.h"

using ::testing::_;
using ::testing::Return;
using ::testing::Invoke;

class ProfilerInfoBaseTest : public ComBaseTest {
public:
	ProfilerInfoBaseTest() : mockProfilerInfo_(nullptr),
		testProfilerInfo_(nullptr)
	{
	}

private:
	void SetUp() override
	{
		CreateComObject(&testProfilerInfo_);
		CreateComObject(&mockProfilerInfo_);
		testProfilerInfo_->ChainProfilerInfo(mockProfilerInfo_);
	}

	void TearDown() override
	{
		ASSERT_EQ(0, testProfilerInfo_->Release());
		ASSERT_EQ(0, mockProfilerInfo_->Release());
	}

protected:
	CComObject<MockProfilerInfo> *mockProfilerInfo_;
	CComObject<CTestProfilerInfo> *testProfilerInfo_;
};

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_HasHookedAllAvailableInterfaces)
{
	ASSERT_EQ(8, mockProfilerInfo_->Release());
	ASSERT_EQ(9, mockProfilerInfo_->AddRef());
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetClassFromObject_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetClassFromObject(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ObjectID objectId,
			/* [out] */ ClassID *pClassId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetClassFromObject(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetClassFromObject(0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetClassFromToken_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetClassFromToken(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId,
			/* [in] */ mdTypeDef typeDef,
			/* [out] */ ClassID *pClassId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetClassFromToken(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetClassFromToken(0, 0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetCodeInfo_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetCodeInfo(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [out] */ LPCBYTE *pStart,
			/* [out] */ ULONG *pcSize) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetCodeInfo(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetCodeInfo(0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetEventMask_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetEventMask(_))
		.WillByDefault(Invoke([this](/* [out] */ DWORD *pdwEvents) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetEventMask(_)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetEventMask(nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetFunctionFromIP_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetFunctionFromIP(_, _))
		.WillByDefault(Invoke([this](/* [in] */ LPCBYTE ip,
			/* [out] */ FunctionID *pFunctionId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetFunctionFromIP(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetFunctionFromIP(nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetFunctionFromToken_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetFunctionFromToken(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId,
			/* [in] */ mdToken token,
			/* [out] */ FunctionID *pFunctionId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetFunctionFromToken(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetFunctionFromToken(0, 0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetHandleFromThread_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetHandleFromThread(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ThreadID threadId,
			/* [out] */ HANDLE *phThread) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetHandleFromThread(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetHandleFromThread(0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetObjectSize_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetObjectSize(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ObjectID objectId,
			/* [out] */ ULONG *pcSize) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetObjectSize(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetObjectSize(0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_IsArrayClass_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, IsArrayClass(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ClassID classId,
			/* [out] */ CorElementType *pBaseElemType,
			/* [out] */ ClassID *pBaseClassId,
			/* [out] */ ULONG *pcRank) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, IsArrayClass(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->IsArrayClass(0, nullptr, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetThreadInfo_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetThreadInfo(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ThreadID threadId,
			/* [out] */ DWORD *pdwWin32ThreadId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetThreadInfo(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetThreadInfo(0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetCurrentThreadID_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetCurrentThreadID(_))
		.WillByDefault(Invoke([this](/* [out] */ ThreadID *pThreadId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetCurrentThreadID(_)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetCurrentThreadID(nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetClassIDInfo_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetClassIDInfo(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ClassID classId,
			/* [out] */ ModuleID *pModuleId,
			/* [out] */ mdTypeDef *pTypeDefToken) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetClassIDInfo(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetClassIDInfo(0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetFunctionInfo_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetFunctionInfo(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [out] */ ClassID *pClassId,
			/* [out] */ ModuleID *pModuleId,
			/* [out] */ mdToken *pToken) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetFunctionInfo(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetFunctionInfo(0, nullptr, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_SetEventMask_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, SetEventMask(_))
		.WillByDefault(Invoke([this](/* [in] */ DWORD dwEvents) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, SetEventMask(_)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->SetEventMask(0));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_SetEnterLeaveFunctionHooks_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, SetEnterLeaveFunctionHooks(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionEnter *pFuncEnter,
			/* [in] */ FunctionLeave *pFuncLeave,
			/* [in] */ FunctionTailcall *pFuncTailcall) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, SetEnterLeaveFunctionHooks(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->SetEnterLeaveFunctionHooks(nullptr, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_SetFunctionIDMapper_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, SetFunctionIDMapper(_))
		.WillByDefault(Invoke([this](/* [in] */ FunctionIDMapper *pFunc) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, SetFunctionIDMapper(_)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->SetFunctionIDMapper(nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetTokenAndMetaDataFromFunction_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetTokenAndMetaDataFromFunction(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [in] */ REFIID riid,
			/* [out] */ IUnknown **ppImport,
			/* [out] */ mdToken *pToken) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetTokenAndMetaDataFromFunction(_, _, _, _)).Times(1);
	GUID guid;
	ASSERT_EQ(S_OK, testProfilerInfo_->GetTokenAndMetaDataFromFunction(0, guid, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetModuleInfo_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetModuleInfo(_, _, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId,
			/* [out] */ LPCBYTE *ppBaseLoadAddress,
			/* [in] */ ULONG cchName,
			/* [out] */ ULONG *pcchName,
			/* [annotation][out] */
			_Out_writes_to_(cchName, *pcchName)  WCHAR szName[],
			/* [out] */ AssemblyID *pAssemblyId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetModuleInfo(_, _, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetModuleInfo(0, nullptr, 0, nullptr, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetModuleMetaData_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetModuleMetaData(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId,
			/* [in] */ DWORD dwOpenFlags,
			/* [in] */ REFIID riid,
			/* [out] */ IUnknown **ppOut) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetModuleMetaData(_, _, _, _)).Times(1);
	GUID guid;
	ASSERT_EQ(S_OK, testProfilerInfo_->GetModuleMetaData(0, 0, guid, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetILFunctionBody_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetILFunctionBody(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId,
			/* [in] */ mdMethodDef methodId,
			/* [out] */ LPCBYTE *ppMethodHeader,
			/* [out] */ ULONG *pcbMethodSize) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetILFunctionBody(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetILFunctionBody(0, 0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetILFunctionBodyAllocator_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetILFunctionBodyAllocator(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId,
			/* [out] */ IMethodMalloc **ppMalloc) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetILFunctionBodyAllocator(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetILFunctionBodyAllocator(0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_SetILFunctionBody_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, SetILFunctionBody(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId,
			/* [in] */ mdMethodDef methodid,
			/* [in] */ LPCBYTE pbNewILMethodHeader) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, SetILFunctionBody(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->SetILFunctionBody(0, 0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetAppDomainInfo_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetAppDomainInfo(_, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ AppDomainID appDomainId,
			/* [in] */ ULONG cchName,
			/* [out] */ ULONG *pcchName,
			/* [annotation][out] */
			_Out_writes_to_(cchName, *pcchName)  WCHAR szName[],
			/* [out] */ ProcessID *pProcessId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetAppDomainInfo(_, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetAppDomainInfo(0, 0, nullptr, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetAssemblyInfo_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetAssemblyInfo(_, _, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ AssemblyID assemblyId,
			/* [in] */ ULONG cchName,
			/* [out] */ ULONG *pcchName,
			/* [annotation][out] */
			_Out_writes_to_(cchName, *pcchName)  WCHAR szName[],
			/* [out] */ AppDomainID *pAppDomainId,
			/* [out] */ ModuleID *pModuleId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetAssemblyInfo(_, _, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetAssemblyInfo(0, 0, nullptr, nullptr, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_SetFunctionReJIT_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, SetFunctionReJIT(_))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, SetFunctionReJIT(_)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->SetFunctionReJIT(0));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_ForceGC_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, ForceGC())
		.WillByDefault(Invoke([this](void) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, ForceGC()).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->ForceGC());
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_SetILInstrumentedCodeMap_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, SetILInstrumentedCodeMap(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [in] */ BOOL fStartJit,
			/* [in] */ ULONG cILMapEntries,
			/* [size_is][in] */ COR_IL_MAP rgILMapEntries[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, SetILInstrumentedCodeMap(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->SetILInstrumentedCodeMap(0, 0, 0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetInprocInspectionInterface_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetInprocInspectionInterface(_))
		.WillByDefault(Invoke([this](/* [out] */ IUnknown **ppicd) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetInprocInspectionInterface(_)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetInprocInspectionInterface(nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetInprocInspectionIThisThread_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetInprocInspectionIThisThread(_))
		.WillByDefault(Invoke([this](/* [out] */ IUnknown **ppicd) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetInprocInspectionIThisThread(_)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetInprocInspectionIThisThread(nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetThreadContext_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetThreadContext(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ThreadID threadId,
			/* [out] */ ContextID *pContextId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetThreadContext(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetThreadContext(0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_BeginInprocDebugging_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, BeginInprocDebugging(_, _))
		.WillByDefault(Invoke([this](/* [in] */ BOOL fThisThreadOnly,
			/* [out] */ DWORD *pdwProfilerContext) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, BeginInprocDebugging(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->BeginInprocDebugging(0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_EndInprocDebugging_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, EndInprocDebugging(_))
		.WillByDefault(Invoke([this](/* [in] */ DWORD dwProfilerContext) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, EndInprocDebugging(_)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->EndInprocDebugging(0));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetILToNativeMapping_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetILToNativeMapping(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [in] */ ULONG32 cMap,
			/* [out] */ ULONG32 *pcMap,
			/* [length_is][size_is][out] */ COR_DEBUG_IL_TO_NATIVE_MAP map[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetILToNativeMapping(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetILToNativeMapping(0, 0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_DoStackSnapshot_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, DoStackSnapshot(_, _, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ThreadID thread,
			/* [in] */ StackSnapshotCallback *callback,
			/* [in] */ ULONG32 infoFlags,
			/* [in] */ void *clientData,
			/* [size_is][in] */ BYTE context[],
			/* [in] */ ULONG32 contextSize) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, DoStackSnapshot(_, _, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->DoStackSnapshot(0, nullptr, 0, nullptr, nullptr, 0));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_SetEnterLeaveFunctionHooks2_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, SetEnterLeaveFunctionHooks2(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionEnter2 *pFuncEnter,
			/* [in] */ FunctionLeave2 *pFuncLeave,
			/* [in] */ FunctionTailcall2 *pFuncTailcall) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, SetEnterLeaveFunctionHooks2(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->SetEnterLeaveFunctionHooks2(nullptr, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetFunctionInfo2_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetFunctionInfo2(_, _, _, _, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID funcId,
			/* [in] */ COR_PRF_FRAME_INFO frameInfo,
			/* [out] */ ClassID *pClassId,
			/* [out] */ ModuleID *pModuleId,
			/* [out] */ mdToken *pToken,
			/* [in] */ ULONG32 cTypeArgs,
			/* [out] */ ULONG32 *pcTypeArgs,
			/* [out] */ ClassID typeArgs[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetFunctionInfo2(_, _, _, _, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetFunctionInfo2(0, 0, nullptr, nullptr, nullptr, 0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetStringLayout_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetStringLayout(_, _, _))
		.WillByDefault(Invoke([this](/* [out] */ ULONG *pBufferLengthOffset,
			/* [out] */ ULONG *pStringLengthOffset,
			/* [out] */ ULONG *pBufferOffset) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetStringLayout(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetStringLayout(nullptr, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetClassLayout_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetClassLayout(_, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ClassID classID,
			/* [out][in] */ COR_FIELD_OFFSET rFieldOffset[],
			/* [in] */ ULONG cFieldOffset,
			/* [out] */ ULONG *pcFieldOffset,
			/* [out] */ ULONG *pulClassSize) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetClassLayout(_, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetClassLayout(0, nullptr, 0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetClassIDInfo2_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetClassIDInfo2(_, _, _, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ClassID classId,
			/* [out] */ ModuleID *pModuleId,
			/* [out] */ mdTypeDef *pTypeDefToken,
			/* [out] */ ClassID *pParentClassId,
			/* [in] */ ULONG32 cNumTypeArgs,
			/* [out] */ ULONG32 *pcNumTypeArgs,
			/* [out] */ ClassID typeArgs[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetClassIDInfo2(_, _, _, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetClassIDInfo2(0, nullptr, nullptr, nullptr, 0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetCodeInfo2_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetCodeInfo2(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionID,
			/* [in] */ ULONG32 cCodeInfos,
			/* [out] */ ULONG32 *pcCodeInfos,
			/* [length_is][size_is][out] */ COR_PRF_CODE_INFO codeInfos[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetCodeInfo2(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetCodeInfo2(0, 0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetClassFromTokenAndTypeArgs_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetClassFromTokenAndTypeArgs(_, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleID,
			/* [in] */ mdTypeDef typeDef,
			/* [in] */ ULONG32 cTypeArgs,
			/* [size_is][in] */ ClassID typeArgs[],
			/* [out] */ ClassID *pClassID) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetClassFromTokenAndTypeArgs(_, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetClassFromTokenAndTypeArgs(0, 0, 0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetFunctionFromTokenAndTypeArgs_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetFunctionFromTokenAndTypeArgs(_, _, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleID,
			/* [in] */ mdMethodDef funcDef,
			/* [in] */ ClassID classId,
			/* [in] */ ULONG32 cTypeArgs,
			/* [size_is][in] */ ClassID typeArgs[],
			/* [out] */ FunctionID *pFunctionID) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetFunctionFromTokenAndTypeArgs(_, _, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetFunctionFromTokenAndTypeArgs(0, 0, 0, 0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_EnumModuleFrozenObjects_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, EnumModuleFrozenObjects(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleID,
			/* [out] */ ICorProfilerObjectEnum **ppEnum) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, EnumModuleFrozenObjects(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->EnumModuleFrozenObjects(0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetArrayObjectInfo_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetArrayObjectInfo(_, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ObjectID objectId,
			/* [in] */ ULONG32 cDimensions,
			/* [size_is][out] */ ULONG32 pDimensionSizes[],
			/* [size_is][out] */ int pDimensionLowerBounds[],
			/* [out] */ BYTE **ppData) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetArrayObjectInfo(_, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetArrayObjectInfo(0, 0, nullptr, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetBoxClassLayout_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetBoxClassLayout(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ClassID classId,
			/* [out] */ ULONG32 *pBufferOffset) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetBoxClassLayout(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetBoxClassLayout(0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetThreadAppDomain_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetThreadAppDomain(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ThreadID threadId,
			/* [out] */ AppDomainID *pAppDomainId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetThreadAppDomain(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetThreadAppDomain(0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetRVAStaticAddress_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetRVAStaticAddress(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ClassID classId,
			/* [in] */ mdFieldDef fieldToken,
			/* [out] */ void **ppAddress) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetRVAStaticAddress(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetRVAStaticAddress(0, 0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetAppDomainStaticAddress_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetAppDomainStaticAddress(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ClassID classId,
			/* [in] */ mdFieldDef fieldToken,
			/* [in] */ AppDomainID appDomainId,
			/* [out] */ void **ppAddress) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetAppDomainStaticAddress(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetAppDomainStaticAddress(0, 0, 0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetThreadStaticAddress_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetThreadStaticAddress(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ClassID classId,
			/* [in] */ mdFieldDef fieldToken,
			/* [in] */ ThreadID threadId,
			/* [out] */ void **ppAddress) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetThreadStaticAddress(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetThreadStaticAddress(0, 0, 0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetContextStaticAddress_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetContextStaticAddress(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ClassID classId,
			/* [in] */ mdFieldDef fieldToken,
			/* [in] */ ContextID contextId,
			/* [out] */ void **ppAddress) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetContextStaticAddress(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetContextStaticAddress(0, 0, 0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetStaticFieldInfo_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetStaticFieldInfo(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ClassID classId,
			/* [in] */ mdFieldDef fieldToken,
			/* [out] */ COR_PRF_STATIC_TYPE *pFieldInfo) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetStaticFieldInfo(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetStaticFieldInfo(0, 0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetGenerationBounds_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetGenerationBounds(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ULONG cObjectRanges,
			/* [out] */ ULONG *pcObjectRanges,
			/* [length_is][size_is][out] */ COR_PRF_GC_GENERATION_RANGE ranges[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetGenerationBounds(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetGenerationBounds(0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetObjectGeneration_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetObjectGeneration(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ObjectID objectId,
			/* [out] */ COR_PRF_GC_GENERATION_RANGE *range) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetObjectGeneration(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetObjectGeneration(0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetNotifiedExceptionClauseInfo_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetNotifiedExceptionClauseInfo(_))
		.WillByDefault(Invoke([this](/* [out] */ COR_PRF_EX_CLAUSE_INFO *pinfo) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetNotifiedExceptionClauseInfo(_)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetNotifiedExceptionClauseInfo(nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_EnumJITedFunctions_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, EnumJITedFunctions(_))
		.WillByDefault(Invoke([this](/* [out] */ ICorProfilerFunctionEnum **ppEnum) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, EnumJITedFunctions(_)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->EnumJITedFunctions(nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_RequestProfilerDetach_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, RequestProfilerDetach(_))
		.WillByDefault(Invoke([this](/* [in] */ DWORD dwExpectedCompletionMilliseconds) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, RequestProfilerDetach(_)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->RequestProfilerDetach(0));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_SetFunctionIDMapper2_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, SetFunctionIDMapper2(_, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionIDMapper2 *pFunc,
			/* [in] */ void *clientData) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, SetFunctionIDMapper2(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->SetFunctionIDMapper2(nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetStringLayout2_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetStringLayout2(_, _))
		.WillByDefault(Invoke([this](/* [out] */ ULONG *pStringLengthOffset,
			/* [out] */ ULONG *pBufferOffset) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetStringLayout2(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetStringLayout2(nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_SetEnterLeaveFunctionHooks3_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, SetEnterLeaveFunctionHooks3(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionEnter3 *pFuncEnter3,
			/* [in] */ FunctionLeave3 *pFuncLeave3,
			/* [in] */ FunctionTailcall3 *pFuncTailcall3) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, SetEnterLeaveFunctionHooks3(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->SetEnterLeaveFunctionHooks3(nullptr, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_SetEnterLeaveFunctionHooks3WithInfo_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, SetEnterLeaveFunctionHooks3WithInfo(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionEnter3WithInfo *pFuncEnter3WithInfo,
			/* [in] */ FunctionLeave3WithInfo *pFuncLeave3WithInfo,
			/* [in] */ FunctionTailcall3WithInfo *pFuncTailcall3WithInfo) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, SetEnterLeaveFunctionHooks3WithInfo(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->SetEnterLeaveFunctionHooks3WithInfo(nullptr, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetFunctionEnter3Info_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetFunctionEnter3Info(_, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [in] */ COR_PRF_ELT_INFO eltInfo,
			/* [out] */ COR_PRF_FRAME_INFO *pFrameInfo,
			/* [out][in] */ ULONG *pcbArgumentInfo,
			/* [size_is][out] */ COR_PRF_FUNCTION_ARGUMENT_INFO *pArgumentInfo) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetFunctionEnter3Info(_, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetFunctionEnter3Info(0, 0, nullptr, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetFunctionLeave3Info_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetFunctionLeave3Info(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [in] */ COR_PRF_ELT_INFO eltInfo,
			/* [out] */ COR_PRF_FRAME_INFO *pFrameInfo,
			/* [out] */ COR_PRF_FUNCTION_ARGUMENT_RANGE *pRetvalRange) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetFunctionLeave3Info(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetFunctionLeave3Info(0, 0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetFunctionTailcall3Info_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetFunctionTailcall3Info(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [in] */ COR_PRF_ELT_INFO eltInfo,
			/* [out] */ COR_PRF_FRAME_INFO *pFrameInfo) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetFunctionTailcall3Info(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetFunctionTailcall3Info(0, 0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_EnumModules_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, EnumModules(_))
		.WillByDefault(Invoke([this](/* [out] */ ICorProfilerModuleEnum **ppEnum) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, EnumModules(_)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->EnumModules(nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetRuntimeInformation_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetRuntimeInformation(_, _, _, _, _, _, _, _, _))
		.WillByDefault(Invoke([this](/* [out] */ USHORT *pClrInstanceId,
			/* [out] */ COR_PRF_RUNTIME_TYPE *pRuntimeType,
			/* [out] */ USHORT *pMajorVersion,
			/* [out] */ USHORT *pMinorVersion,
			/* [out] */ USHORT *pBuildNumber,
			/* [out] */ USHORT *pQFEVersion,
			/* [in] */ ULONG cchVersionString,
			/* [out] */ ULONG *pcchVersionString,
			/* [annotation][out] */
			_Out_writes_to_(cchVersionString, *pcchVersionString)  WCHAR szVersionString[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetRuntimeInformation(_, _, _, _, _, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetRuntimeInformation(nullptr, nullptr, nullptr, nullptr, nullptr, nullptr, 0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetThreadStaticAddress2_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetThreadStaticAddress2(_, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ClassID classId,
			/* [in] */ mdFieldDef fieldToken,
			/* [in] */ AppDomainID appDomainId,
			/* [in] */ ThreadID threadId,
			/* [out] */ void **ppAddress) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetThreadStaticAddress2(_, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetThreadStaticAddress2(0, 0, 0, 0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetAppDomainsContainingModule_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetAppDomainsContainingModule(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId,
			/* [in] */ ULONG32 cAppDomainIds,
			/* [out] */ ULONG32 *pcAppDomainIds,
			/* [length_is][size_is][out] */ AppDomainID appDomainIds[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetAppDomainsContainingModule(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetAppDomainsContainingModule(0, 0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetModuleInfo2_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetModuleInfo2(_, _, _, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId,
			/* [out] */ LPCBYTE *ppBaseLoadAddress,
			/* [in] */ ULONG cchName,
			/* [out] */ ULONG *pcchName,
			/* [annotation][out] */
			_Out_writes_to_(cchName, *pcchName)  WCHAR szName[],
			/* [out] */ AssemblyID *pAssemblyId,
			/* [out] */ DWORD *pdwModuleFlags) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetModuleInfo2(_, _, _, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetModuleInfo2(0, nullptr, 0, nullptr, nullptr, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_EnumThreads_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, EnumThreads(_))
		.WillByDefault(Invoke([this](/* [out] */ ICorProfilerThreadEnum **ppEnum) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, EnumThreads(_)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->EnumThreads(nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_InitializeCurrentThread_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, InitializeCurrentThread())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, InitializeCurrentThread()).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->InitializeCurrentThread());
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_RequestReJIT_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, RequestReJIT(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ULONG cFunctions,
			/* [size_is][in] */ ModuleID moduleIds[],
			/* [size_is][in] */ mdMethodDef methodIds[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, RequestReJIT(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->RequestReJIT(0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_RequestRevert_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, RequestRevert(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ULONG cFunctions,
			/* [size_is][in] */ ModuleID moduleIds[],
			/* [size_is][in] */ mdMethodDef methodIds[],
			/* [size_is][out] */ HRESULT status[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, RequestRevert(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->RequestRevert(0, nullptr, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetCodeInfo3_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetCodeInfo3(_, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionID,
			/* [in] */ ReJITID reJitId,
			/* [in] */ ULONG32 cCodeInfos,
			/* [out] */ ULONG32 *pcCodeInfos,
			/* [length_is][size_is][out] */ COR_PRF_CODE_INFO codeInfos[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetCodeInfo3(_, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetCodeInfo3(0, 0, 0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetFunctionFromIP2_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetFunctionFromIP2(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ LPCBYTE ip,
			/* [out] */ FunctionID *pFunctionId,
			/* [out] */ ReJITID *pReJitId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetFunctionFromIP2(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetFunctionFromIP2(nullptr, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetReJITIDs_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetReJITIDs(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [in] */ ULONG cReJitIds,
			/* [out] */ ULONG *pcReJitIds,
			/* [length_is][size_is][out] */ ReJITID reJitIds[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetReJITIDs(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetReJITIDs(0, 0, 0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetILToNativeMapping2_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetILToNativeMapping2(_, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [in] */ ReJITID reJitId,
			/* [in] */ ULONG32 cMap,
			/* [out] */ ULONG32 *pcMap,
			/* [length_is][size_is][out] */ COR_DEBUG_IL_TO_NATIVE_MAP map[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetILToNativeMapping2(_, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetILToNativeMapping2(0, 0, 0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_EnumJITedFunctions2_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, EnumJITedFunctions2(_))
		.WillByDefault(Invoke([this](/* [out] */ ICorProfilerFunctionEnum **ppEnum) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, EnumJITedFunctions2(_)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->EnumJITedFunctions2(nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetObjectSize2_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetObjectSize2(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ObjectID objectId,
			/* [out] */ SIZE_T *pcSize) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetObjectSize2(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetObjectSize2(0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetEventMask2_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetEventMask2(_, _))
		.WillByDefault(Invoke([this](/* [out] */ DWORD *pdwEventsLow,
			/* [out] */ DWORD *pdwEventsHigh) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetEventMask2(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetEventMask2(nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_SetEventMask2_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, SetEventMask2(_, _))
		.WillByDefault(Invoke([this](/* [in] */ DWORD dwEventsLow,
			/* [in] */ DWORD dwEventsHigh) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, SetEventMask2(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->SetEventMask2(0, 0));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_EnumNgenModuleMethodsInliningThisMethod_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, EnumNgenModuleMethodsInliningThisMethod(_, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID inlinersModuleId,
			/* [in] */ ModuleID inlineeModuleId,
			/* [in] */ mdMethodDef inlineeMethodId,
			/* [out] */ BOOL *incompleteData,
			/* [out] */ ICorProfilerMethodEnum **ppEnum) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, EnumNgenModuleMethodsInliningThisMethod(_, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->EnumNgenModuleMethodsInliningThisMethod(0, 0, 0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_ApplyMetaData_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, ApplyMetaData(_))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, ApplyMetaData(_)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->ApplyMetaData(0));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetInMemorySymbolsLength_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetInMemorySymbolsLength(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId,
			/* [out] */ DWORD *pCountSymbolBytes) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetInMemorySymbolsLength(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetInMemorySymbolsLength(0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_ReadInMemorySymbols_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, ReadInMemorySymbols(_, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId,
			/* [in] */ DWORD symbolsReadOffset,
			/* [out] */ BYTE *pSymbolBytes,
			/* [in] */ DWORD countSymbolBytes,
			/* [out] */ DWORD *pCountSymbolBytesRead) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, ReadInMemorySymbols(_, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->ReadInMemorySymbols(0, 0, nullptr, 0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_IsFunctionDynamic_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, IsFunctionDynamic(_, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [out] */ BOOL *isDynamic) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, IsFunctionDynamic(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->IsFunctionDynamic(0, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetFunctionFromIP3_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetFunctionFromIP3(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ LPCBYTE ip,
			/* [out] */ FunctionID *functionId,
			/* [out] */ ReJITID *pReJitId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetFunctionFromIP3(_, _, 0)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetFunctionFromIP3(0, nullptr, nullptr));
}

TEST_F(ProfilerInfoBaseTest, ChainedProfilerInfo_WillForwardCallsTo_GetDynamicFunctionInfo_AndReturnSuccess)
{
	ON_CALL(*mockProfilerInfo_, GetDynamicFunctionInfo(_, _, _, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [out] */ ModuleID *moduleId,
			/* [out] */ PCCOR_SIGNATURE *ppvSig,
			/* [out] */ ULONG *pbSig,
			/* [in] */ ULONG cchName,
			/* [out] */ ULONG *pcchName,
			/* [out] */ WCHAR wszName[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfilerInfo_, GetDynamicFunctionInfo(_, _, _, _, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfilerInfo_->GetDynamicFunctionInfo(0, nullptr, nullptr, nullptr, 0, nullptr, nullptr));
}
