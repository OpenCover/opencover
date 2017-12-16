#include "stdafx.h"
#include "TestProfiler.h"
#include "MockProfiler.h"
#include "ComBaseTest.h"

using ::testing::_;
using ::testing::Return;
using ::testing::Invoke;

class ProfilerBaseTest : public ComBaseTest {
public:
	ProfilerBaseTest()
		: mockProfiler_(nullptr),
		  testProfiler_(nullptr)
	{
	}

private:
	void SetUp() override
	{
		CreateComObject(&testProfiler_);
		CreateComObject(&mockProfiler_);
		testProfiler_->HookChainedProfiler(mockProfiler_);
	}

	void TearDown() override
	{
		ASSERT_EQ(0, testProfiler_->Release());
		ASSERT_EQ(0, mockProfiler_->Release());
	}

protected:
	CComObject<MockProfiler> *mockProfiler_;
	CComObject<CTestProfiler> *testProfiler_;
};

TEST_F(ProfilerBaseTest, ChainedProfiler_HasHookedAllAvailableInterfaces)
{
	ASSERT_EQ(8, mockProfiler_->Release());
	ASSERT_EQ(9, mockProfiler_->AddRef());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_Initialize_AndReturnSuccess)
{
	ON_CALL(*mockProfiler_, Initialize(_))
		.WillByDefault(Invoke([this](/* [in] */ IUnknown *pICorProfilerInfoUnk) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, Initialize(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->Initialize(nullptr));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_Initialize_AndReturnError)
{
	ON_CALL(*mockProfiler_, Initialize(_))
		.WillByDefault(Invoke([this](/* [in] */ IUnknown *pICorProfilerInfoUnk) {
		return E_FAIL;
	}));

	EXPECT_CALL(*mockProfiler_, Initialize(_)).Times(1);
	ASSERT_EQ(E_FAIL, testProfiler_->Initialize(nullptr));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_Shutdown)
{
	ON_CALL(*mockProfiler_, Shutdown())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, Shutdown()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->Shutdown());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_AppDomainCreationStarted)
{
	ON_CALL(*mockProfiler_, AppDomainCreationStarted(_))
		.WillByDefault(Invoke([this](/* [in] */ AppDomainID appDomainId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, AppDomainCreationStarted(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->AppDomainCreationStarted(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_AppDomainCreationFinished)
{
	ON_CALL(*mockProfiler_, AppDomainCreationFinished(_,_))
		.WillByDefault(Invoke([this](/* [in] */ AppDomainID appDomainId,
			/* [in] */ HRESULT hrStatus) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, AppDomainCreationFinished(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->AppDomainCreationFinished(0, E_FAIL));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_AppDomainShutdownStarted)
{
	ON_CALL(*mockProfiler_, AppDomainShutdownStarted(_))
		.WillByDefault(Invoke([this](/* [in] */ AppDomainID appDomainId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, AppDomainShutdownStarted(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->AppDomainShutdownStarted(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_AppDomainShutdownFinished)
{
	ON_CALL(*mockProfiler_, AppDomainShutdownFinished(_, _))
		.WillByDefault(Invoke([this](/* [in] */ AppDomainID appDomainId,
			/* [in] */ HRESULT hrStatus) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, AppDomainShutdownFinished(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->AppDomainShutdownFinished(0, E_FAIL));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_AssemblyLoadStarted)
{
	ON_CALL(*mockProfiler_, AssemblyLoadStarted(_))
		.WillByDefault(Invoke([this](/* [in] */ AssemblyID assemblyId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, AssemblyLoadStarted(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->AssemblyLoadStarted(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_AssemblyLoadFinished)
{
	ON_CALL(*mockProfiler_, AssemblyLoadFinished(_, _))
		.WillByDefault(Invoke([this](/* [in] */ AssemblyID assemblyId,
			/* [in] */ HRESULT hrStatus) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, AssemblyLoadFinished(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->AssemblyLoadFinished(0, E_FAIL));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_AssemblyUnloadStarted)
{
	ON_CALL(*mockProfiler_, AssemblyUnloadStarted(_))
		.WillByDefault(Invoke([this](/* [in] */ AssemblyID assemblyId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, AssemblyUnloadStarted(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->AssemblyUnloadStarted(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_AssemblyUnloadFinished)
{
	ON_CALL(*mockProfiler_, AssemblyUnloadFinished(_, _))
		.WillByDefault(Invoke([this](/* [in] */ AssemblyID assemblyId,
			/* [in] */ HRESULT hrStatus) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, AssemblyUnloadFinished(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->AssemblyUnloadFinished(0, E_FAIL));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ModuleLoadStarted)
{
	ON_CALL(*mockProfiler_, ModuleLoadStarted(_))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ModuleLoadStarted(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ModuleLoadStarted(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ModuleLoadFinished)
{
	ON_CALL(*mockProfiler_, ModuleLoadFinished(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId,
			/* [in] */ HRESULT hrStatus) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ModuleLoadFinished(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ModuleLoadFinished(0, E_FAIL));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ModuleUnloadStarted)
{
	ON_CALL(*mockProfiler_, ModuleUnloadStarted(_))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ModuleUnloadStarted(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ModuleUnloadStarted(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ModuleUnloadFinished)
{
	ON_CALL(*mockProfiler_, ModuleUnloadFinished(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId,
			/* [in] */ HRESULT hrStatus) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ModuleUnloadFinished(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ModuleUnloadFinished(0, E_FAIL));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ModuleAttachedToAssembly)
{
	ON_CALL(*mockProfiler_, ModuleAttachedToAssembly(_,_))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId,
			/* [in] */ AssemblyID assemblyId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ModuleAttachedToAssembly(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ModuleAttachedToAssembly(0, 0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ClassLoadStarted)
{
	ON_CALL(*mockProfiler_, ClassLoadStarted(_))
		.WillByDefault(Invoke([this](/* [in] */ ClassID classId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ClassLoadStarted(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ClassLoadStarted(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ClassLoadFinished)
{
	ON_CALL(*mockProfiler_, ClassLoadFinished(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ClassID classId,
			/* [in] */ HRESULT hrStatus) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ClassLoadFinished(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ClassLoadFinished(0, E_FAIL));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ClassUnloadStarted)
{
	ON_CALL(*mockProfiler_, ClassUnloadStarted(_))
		.WillByDefault(Invoke([this](/* [in] */ ClassID classId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ClassUnloadStarted(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ClassUnloadStarted(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ClassUnloadFinished)
{
	ON_CALL(*mockProfiler_, ClassUnloadFinished(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ClassID classId,
			/* [in] */ HRESULT hrStatus) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ClassUnloadFinished(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ClassUnloadFinished(0, E_FAIL));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_FunctionUnloadStarted)
{
	ON_CALL(*mockProfiler_, FunctionUnloadStarted(_))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, FunctionUnloadStarted(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->FunctionUnloadStarted(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_JITCompilationStarted)
{
	ON_CALL(*mockProfiler_, JITCompilationStarted(_, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [in] */ BOOL fIsSafeToBlock) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, JITCompilationStarted(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->JITCompilationStarted(0, E_FAIL));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_JITCompilationFinished)
{
	ON_CALL(*mockProfiler_, JITCompilationFinished(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [in] */ HRESULT hrStatus,
			/* [in] */ BOOL fIsSafeToBlock) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, JITCompilationFinished(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->JITCompilationFinished(0, E_FAIL, FALSE));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_JITCachedFunctionSearchStarted)
{
	ON_CALL(*mockProfiler_, JITCachedFunctionSearchStarted(_, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [out] */ BOOL *pbUseCachedFunction) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, JITCachedFunctionSearchStarted(_, _)).Times(1);
	BOOL bUseCachedFunction;
	ASSERT_EQ(S_OK, testProfiler_->JITCachedFunctionSearchStarted(0, &bUseCachedFunction));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_JITCachedFunctionSearchFinished)
{
	ON_CALL(*mockProfiler_, JITCachedFunctionSearchFinished(_, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [in] */ COR_PRF_JIT_CACHE result) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, JITCachedFunctionSearchFinished(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->JITCachedFunctionSearchFinished(0, COR_PRF_CACHED_FUNCTION_FOUND));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_JITFunctionPitched)
{
	ON_CALL(*mockProfiler_, JITFunctionPitched(_))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, JITFunctionPitched(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->JITFunctionPitched(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_JITInlining)
{
	ON_CALL(*mockProfiler_, JITInlining(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID callerId,
			/* [in] */ FunctionID calleeId,
			/* [out] */ BOOL *pfShouldInline) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, JITInlining(_, _, _)).Times(1);
	BOOL fShouldInline;
	ASSERT_EQ(S_OK, testProfiler_->JITInlining(0, 0, &fShouldInline));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ThreadCreated)
{
	ON_CALL(*mockProfiler_, ThreadCreated(_))
		.WillByDefault(Invoke([this](/* [in] */ ThreadID functionId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ThreadCreated(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ThreadCreated(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ThreadDestroyed)
{
	ON_CALL(*mockProfiler_, ThreadDestroyed(_))
		.WillByDefault(Invoke([this](/* [in] */ ThreadID functionId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ThreadDestroyed(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ThreadDestroyed(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ThreadAssignedToOSThread)
{
	ON_CALL(*mockProfiler_, ThreadAssignedToOSThread(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ThreadID managedThreadId,
			/* [in] */ DWORD osThreadId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ThreadAssignedToOSThread(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ThreadAssignedToOSThread(0, 0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_RemotingClientInvocationStarted)
{
	ON_CALL(*mockProfiler_, RemotingClientInvocationStarted())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, RemotingClientInvocationStarted()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->RemotingClientInvocationStarted());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_RemotingClientSendingMessage)
{
	ON_CALL(*mockProfiler_, RemotingClientSendingMessage(_, _))
		.WillByDefault(Invoke([this](/* [in] */ GUID *pCookie,
			/* [in] */ BOOL fIsAsync) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, RemotingClientSendingMessage(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->RemotingClientSendingMessage(nullptr, FALSE));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_RemotingClientReceivingReply)
{
	ON_CALL(*mockProfiler_, RemotingClientReceivingReply(_, _))
		.WillByDefault(Invoke([this](/* [in] */ GUID *pCookie,
			/* [in] */ BOOL fIsAsync) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, RemotingClientReceivingReply(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->RemotingClientReceivingReply(nullptr, FALSE));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_RemotingClientInvocationFinished)
{
	ON_CALL(*mockProfiler_, RemotingClientInvocationFinished())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, RemotingClientInvocationFinished()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->RemotingClientInvocationFinished());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_RemotingServerReceivingMessage)
{
	ON_CALL(*mockProfiler_, RemotingServerReceivingMessage(_, _))
		.WillByDefault(Invoke([this](/* [in] */ GUID *pCookie,
			/* [in] */ BOOL fIsAsync) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, RemotingServerReceivingMessage(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->RemotingServerReceivingMessage(nullptr, FALSE));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_RemotingServerInvocationStarted)
{
	ON_CALL(*mockProfiler_, RemotingServerInvocationStarted())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, RemotingServerInvocationStarted()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->RemotingServerInvocationStarted());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_RemotingServerInvocationReturned)
{
	ON_CALL(*mockProfiler_, RemotingServerInvocationReturned())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, RemotingServerInvocationReturned()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->RemotingServerInvocationReturned());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_RemotingServerSendingReply)
{
	ON_CALL(*mockProfiler_, RemotingServerSendingReply(_, _))
		.WillByDefault(Invoke([this](/* [in] */ GUID *pCookie,
			/* [in] */ BOOL fIsAsync) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, RemotingServerSendingReply(_, _)).Times(1);
	GUID *pCookie = nullptr;
	ASSERT_EQ(S_OK, testProfiler_->RemotingServerSendingReply(pCookie, FALSE));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_UnmanagedToManagedTransition)
{
	ON_CALL(*mockProfiler_, UnmanagedToManagedTransition(_, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [in] */ COR_PRF_TRANSITION_REASON reason) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, UnmanagedToManagedTransition(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->UnmanagedToManagedTransition(0, COR_PRF_TRANSITION_CALL));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ManagedToUnmanagedTransition)
{
	ON_CALL(*mockProfiler_, ManagedToUnmanagedTransition(_, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [in] */ COR_PRF_TRANSITION_REASON reason) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ManagedToUnmanagedTransition(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ManagedToUnmanagedTransition(0, COR_PRF_TRANSITION_CALL));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_RuntimeSuspendStarted)
{
	ON_CALL(*mockProfiler_, RuntimeSuspendStarted(_))
		.WillByDefault(Invoke([this](/* [in] */ COR_PRF_SUSPEND_REASON suspendReason) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, RuntimeSuspendStarted(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->RuntimeSuspendStarted(COR_PRF_SUSPEND_OTHER));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_RuntimeSuspendFinished)
{
	ON_CALL(*mockProfiler_, RuntimeSuspendFinished())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, RuntimeSuspendFinished()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->RuntimeSuspendFinished());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_RuntimeSuspendAborted)
{
	ON_CALL(*mockProfiler_, RuntimeSuspendAborted())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, RuntimeSuspendAborted()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->RuntimeSuspendAborted());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_RuntimeResumeStarted)
{
	ON_CALL(*mockProfiler_, RuntimeResumeStarted())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, RuntimeResumeStarted()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->RuntimeResumeStarted());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_RuntimeResumeFinished)
{
	ON_CALL(*mockProfiler_, RuntimeResumeFinished())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, RuntimeResumeFinished()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->RuntimeResumeFinished());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_RuntimeThreadSuspended)
{
	ON_CALL(*mockProfiler_, RuntimeThreadSuspended(_))
		.WillByDefault(Invoke([this](/* [in] */ ThreadID threadId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, RuntimeThreadSuspended(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->RuntimeThreadSuspended(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_RuntimeThreadResumed)
{
	ON_CALL(*mockProfiler_, RuntimeThreadResumed(_))
		.WillByDefault(Invoke([this](/* [in] */ ThreadID threadId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, RuntimeThreadResumed(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->RuntimeThreadResumed(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_MovedReferences)
{
	ON_CALL(*mockProfiler_, MovedReferences(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ULONG cMovedObjectIDRanges,
			/* [size_is][in] */ ObjectID oldObjectIDRangeStart[],
			/* [size_is][in] */ ObjectID newObjectIDRangeStart[],
			/* [size_is][in] */ ULONG cObjectIDRangeLength[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, MovedReferences(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->MovedReferences(0, nullptr, nullptr, nullptr));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ObjectAllocated)
{
	ON_CALL(*mockProfiler_, ObjectAllocated(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ObjectID objectId,
			/* [in] */ ClassID classId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ObjectAllocated(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ObjectAllocated(0, 0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ObjectsAllocatedByClass)
{
	ON_CALL(*mockProfiler_, ObjectsAllocatedByClass(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ULONG cClassCount,
			/* [size_is][in] */ ClassID classIds[],
			/* [size_is][in] */ ULONG cObjects[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ObjectsAllocatedByClass(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ObjectsAllocatedByClass(0, nullptr, nullptr));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ObjectReferences)
{
	ON_CALL(*mockProfiler_, ObjectReferences(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ObjectID objectId,
			/* [in] */ ClassID classId,
			/* [in] */ ULONG cObjectRefs,
			/* [size_is][in] */ ObjectID objectRefIds[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ObjectReferences(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ObjectReferences(0, 0, 0, nullptr));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_RootReferences)
{
	ON_CALL(*mockProfiler_, RootReferences(_, _))
		.WillByDefault(Invoke([this](/* [in] */ ULONG cRootRefs,
			/* [size_is][in] */ ObjectID rootRefIds[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, RootReferences(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->RootReferences(0, nullptr));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ExceptionThrown)
{
	ON_CALL(*mockProfiler_, ExceptionThrown(_))
		.WillByDefault(Invoke([this](/* [in] */ ObjectID thrownObjectId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ExceptionThrown(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ExceptionThrown(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ExceptionSearchFunctionEnter)
{
	ON_CALL(*mockProfiler_, ExceptionSearchFunctionEnter(_))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ExceptionSearchFunctionEnter(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ExceptionSearchFunctionEnter(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ExceptionSearchFunctionLeave)
{
	ON_CALL(*mockProfiler_, ExceptionSearchFunctionLeave())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ExceptionSearchFunctionLeave()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ExceptionSearchFunctionLeave());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ExceptionSearchFilterEnter)
{
	ON_CALL(*mockProfiler_, ExceptionSearchFilterEnter(_))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ExceptionSearchFilterEnter(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ExceptionSearchFilterEnter(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ExceptionSearchFilterLeave)
{
	ON_CALL(*mockProfiler_, ExceptionSearchFilterLeave())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ExceptionSearchFilterLeave()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ExceptionSearchFilterLeave());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ExceptionSearchCatcherFound)
{
	ON_CALL(*mockProfiler_, ExceptionSearchCatcherFound(_))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ExceptionSearchCatcherFound(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ExceptionSearchCatcherFound(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ExceptionOSHandlerEnter)
{
	ON_CALL(*mockProfiler_, ExceptionOSHandlerEnter(_))
		.WillByDefault(Invoke([this](/* [in] */  UINT_PTR __unused) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ExceptionOSHandlerEnter(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ExceptionOSHandlerEnter(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ExceptionOSHandlerLeave)
{
	ON_CALL(*mockProfiler_, ExceptionOSHandlerLeave(_))
		.WillByDefault(Invoke([this](/* [in] */  UINT_PTR __unused) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ExceptionOSHandlerLeave(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ExceptionOSHandlerLeave(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ExceptionUnwindFunctionEnter)
{
	ON_CALL(*mockProfiler_, ExceptionUnwindFunctionEnter(_))
		.WillByDefault(Invoke([this](/* [in] */  FunctionID functionId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ExceptionUnwindFunctionEnter(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ExceptionUnwindFunctionEnter(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ExceptionUnwindFunctionLeave)
{
	ON_CALL(*mockProfiler_, ExceptionUnwindFunctionLeave())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ExceptionUnwindFunctionLeave()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ExceptionUnwindFunctionLeave());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ExceptionUnwindFinallyEnter)
{
	ON_CALL(*mockProfiler_, ExceptionUnwindFinallyEnter(_))
		.WillByDefault(Invoke([this](/* [in] */  FunctionID functionId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ExceptionUnwindFinallyEnter(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ExceptionUnwindFinallyEnter(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ExceptionUnwindFinallyLeave)
{
	ON_CALL(*mockProfiler_, ExceptionUnwindFinallyLeave())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ExceptionUnwindFinallyLeave()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ExceptionUnwindFinallyLeave());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ExceptionCatcherEnter)
{
	ON_CALL(*mockProfiler_, ExceptionCatcherEnter(_, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [in] */ ObjectID objectId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ExceptionCatcherEnter(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ExceptionCatcherEnter(0, 0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ExceptionCatcherLeave)
{
	ON_CALL(*mockProfiler_, ExceptionCatcherLeave())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ExceptionCatcherLeave()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ExceptionCatcherLeave());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_COMClassicVTableCreated)
{
	ON_CALL(*mockProfiler_, COMClassicVTableCreated(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ClassID wrappedClassId,
			/* [in] */ REFGUID implementedIID,
			/* [in] */ void *pVTable,
			/* [in] */ ULONG cSlots) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, COMClassicVTableCreated(_, _, _, _)).Times(1);
	GUID guid;
	ASSERT_EQ(S_OK, testProfiler_->COMClassicVTableCreated(0, guid, nullptr, 0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_COMClassicVTableDestroyed)
{
	ON_CALL(*mockProfiler_, COMClassicVTableDestroyed(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ClassID wrappedClassId,
			/* [in] */ REFGUID implementedIID,
			/* [in] */ void *pVTable) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, COMClassicVTableDestroyed(_, _, _)).Times(1);
	GUID guid;
	ASSERT_EQ(S_OK, testProfiler_->COMClassicVTableDestroyed(0, guid, nullptr));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ExceptionCLRCatcherFound)
{
	ON_CALL(*mockProfiler_, ExceptionCLRCatcherFound())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ExceptionCLRCatcherFound()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ExceptionCLRCatcherFound());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ExceptionCLRCatcherExecute)
{
	ON_CALL(*mockProfiler_, ExceptionCLRCatcherExecute())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ExceptionCLRCatcherExecute()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ExceptionCLRCatcherExecute());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ThreadNameChanged)
{
	ON_CALL(*mockProfiler_, ThreadNameChanged(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ThreadID threadId,
			/* [in] */ ULONG cchName,
			/* [in] */
			__in_ecount_opt(cchName)  WCHAR name[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ThreadNameChanged(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ThreadNameChanged(0, 0, nullptr));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_GarbageCollectionStarted)
{
	ON_CALL(*mockProfiler_, GarbageCollectionStarted(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ int cGenerations,
			/* [size_is][in] */ BOOL generationCollected[],
			/* [in] */ COR_PRF_GC_REASON reason) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, GarbageCollectionStarted(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->GarbageCollectionStarted(0, FALSE, COR_PRF_GC_INDUCED));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_SurvivingReferences)
{
	ON_CALL(*mockProfiler_, SurvivingReferences(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ULONG cSurvivingObjectIDRanges,
			/* [size_is][in] */ ObjectID objectIDRangeStart[],
			/* [size_is][in] */ ULONG cObjectIDRangeLength[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, SurvivingReferences(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->SurvivingReferences(0, nullptr, nullptr));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_GarbageCollectionFinished)
{
	ON_CALL(*mockProfiler_, GarbageCollectionFinished())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, GarbageCollectionFinished()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->GarbageCollectionFinished());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_FinalizeableObjectQueued)
{
	ON_CALL(*mockProfiler_, FinalizeableObjectQueued(_, _))
		.WillByDefault(Invoke([this](/* [in] */ DWORD finalizerFlags,
			/* [in] */ ObjectID objectID) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, FinalizeableObjectQueued(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->FinalizeableObjectQueued(0, 0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_RootReferences2)
{
	ON_CALL(*mockProfiler_, RootReferences2(_, _, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ULONG cRootRefs,
			/* [size_is][in] */ ObjectID rootRefIds[],
			/* [size_is][in] */ COR_PRF_GC_ROOT_KIND rootKinds[],
			/* [size_is][in] */ COR_PRF_GC_ROOT_FLAGS rootFlags[],
			/* [size_is][in] */ UINT_PTR rootIds[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, RootReferences2(_, _, _,_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->RootReferences2(0, nullptr, nullptr, nullptr, nullptr));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_HandleCreated)
{
	ON_CALL(*mockProfiler_, HandleCreated(_, _))
		.WillByDefault(Invoke([this](/* [in] */ GCHandleID handleId,
			/* [in] */ ObjectID initialObjectId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, HandleCreated(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->HandleCreated(0, 0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_HandleDestroyed)
{
	ON_CALL(*mockProfiler_, HandleDestroyed(_))
		.WillByDefault(Invoke([this](/* [in] */ GCHandleID handleId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, HandleDestroyed(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->HandleDestroyed(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_InitializeForAttach)
{
	ON_CALL(*mockProfiler_, InitializeForAttach(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ IUnknown *pCorProfilerInfoUnk,
			/* [in] */ void *pvClientData,
			/* [in] */ UINT cbClientData) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, InitializeForAttach(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->InitializeForAttach(nullptr, nullptr, 0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ProfilerAttachComplete)
{
	ON_CALL(*mockProfiler_, ProfilerAttachComplete())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ProfilerAttachComplete()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ProfilerAttachComplete());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ProfilerDetachSucceeded)
{
	ON_CALL(*mockProfiler_, ProfilerDetachSucceeded())
		.WillByDefault(Invoke([this]() {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ProfilerDetachSucceeded()).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ProfilerDetachSucceeded());
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ReJITCompilationStarted)
{
	ON_CALL(*mockProfiler_, ReJITCompilationStarted(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [in] */ ReJITID rejitId,
			/* [in] */ BOOL fIsSafeToBlock) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ReJITCompilationStarted(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ReJITCompilationStarted(0, 0, FALSE));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_GetReJITParameters)
{
	ON_CALL(*mockProfiler_, GetReJITParameters(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId,
			/* [in] */ mdMethodDef methodId,
			/* [in] */ ICorProfilerFunctionControl *pFunctionControl) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, GetReJITParameters(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->GetReJITParameters(0, 0, nullptr));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ReJITCompilationFinished)
{
	ON_CALL(*mockProfiler_, ReJITCompilationFinished(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [in] */ ReJITID rejitId,
			/* [in] */ HRESULT hrStatus,
			/* [in] */ BOOL fIsSafeToBlock) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ReJITCompilationFinished(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ReJITCompilationFinished(0, 0, S_OK, FALSE));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ReJITError)
{
	ON_CALL(*mockProfiler_, ReJITError(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId,
			/* [in] */ mdMethodDef methodId,
			/* [in] */ FunctionID functionId,
			/* [in] */ HRESULT hrStatus) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ReJITError(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ReJITError(0, 0, 0, S_OK));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_MovedReferences2)
{
	ON_CALL(*mockProfiler_, MovedReferences2(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ULONG cMovedObjectIDRanges,
			/* [size_is][in] */ ObjectID oldObjectIDRangeStart[],
			/* [size_is][in] */ ObjectID newObjectIDRangeStart[],
			/* [size_is][in] */ SIZE_T cObjectIDRangeLength[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, MovedReferences2(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->MovedReferences2(0, nullptr, nullptr, nullptr));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_SurvivingReferences2)
{
	ON_CALL(*mockProfiler_, SurvivingReferences2(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ULONG cSurvivingObjectIDRanges,
			/* [size_is][in] */ ObjectID objectIDRangeStart[],
			/* [size_is][in] */ SIZE_T cObjectIDRangeLength[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, SurvivingReferences2(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->SurvivingReferences2(0, nullptr, nullptr));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ConditionalWeakTableElementReferences)
{
	ON_CALL(*mockProfiler_, ConditionalWeakTableElementReferences(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ ULONG cRootRefs,
			/* [size_is][in] */ ObjectID keyRefIds[],
			/* [size_is][in] */ ObjectID valueRefIds[],
			/* [size_is][in] */ GCHandleID rootIds[]) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ConditionalWeakTableElementReferences(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ConditionalWeakTableElementReferences(0, nullptr, nullptr, nullptr));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_GetAssemblyReferences)
{
	ON_CALL(*mockProfiler_, GetAssemblyReferences(_, _))
		.WillByDefault(Invoke([this](/* [string][in] */ const WCHAR *wszAssemblyPath,
			/* [in] */ ICorProfilerAssemblyReferenceProvider *pAsmRefProvider) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, GetAssemblyReferences(_, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->GetAssemblyReferences(nullptr, nullptr));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_ModuleInMemorySymbolsUpdated)
{
	ON_CALL(*mockProfiler_, ModuleInMemorySymbolsUpdated(_))
		.WillByDefault(Invoke([this](/* [in] */ ModuleID moduleId) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, ModuleInMemorySymbolsUpdated(_)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->ModuleInMemorySymbolsUpdated(0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_DynamicMethodJITCompilationStarted)
{
	ON_CALL(*mockProfiler_, DynamicMethodJITCompilationStarted(_, _, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [in] */ BOOL fIsSafeToBlock,
			/* [in] */ LPCBYTE pILHeader,
			/* [in] */ ULONG cbILHeader) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, DynamicMethodJITCompilationStarted(_, _, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->DynamicMethodJITCompilationStarted(0, 0, nullptr, 0));
}

TEST_F(ProfilerBaseTest, ChainedProfiler_WillForwardCallsTo_DynamicMethodJITCompilationFinished)
{
	ON_CALL(*mockProfiler_, DynamicMethodJITCompilationFinished(_, _, _))
		.WillByDefault(Invoke([this](/* [in] */ FunctionID functionId,
			/* [in] */ HRESULT hrStatus,
			/* [in] */ BOOL fIsSafeToBlock) {
		return S_OK;
	}));

	EXPECT_CALL(*mockProfiler_, DynamicMethodJITCompilationFinished(_, _, _)).Times(1);
	ASSERT_EQ(S_OK, testProfiler_->DynamicMethodJITCompilationFinished(0, 0, 0));
}
