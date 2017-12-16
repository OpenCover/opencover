#pragma once
#include "TestProfiler.h"

class MockProfiler :
	public CTestProfiler
{
    // ICorProfilerCallback
public:
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, Initialize, HRESULT(
		/* [in] */ IUnknown *pICorProfilerInfoUnk));
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, Shutdown, HRESULT());
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, AppDomainCreationStarted, HRESULT(
		/* [in] */ AppDomainID appDomainId));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, AppDomainCreationFinished, HRESULT(
		/* [in] */ AppDomainID appDomainId,
		/* [in] */ HRESULT hrStatus));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, AppDomainShutdownStarted, HRESULT(
		/* [in] */ AppDomainID appDomainId));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, AppDomainShutdownFinished, HRESULT(
		/* [in] */ AppDomainID appDomainId,
		/* [in] */ HRESULT hrStatus));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, AssemblyLoadStarted, HRESULT(
		/* [in] */ AssemblyID assemblyId));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, AssemblyLoadFinished, HRESULT(
		/* [in] */ AssemblyID assemblyId,
		/* [in] */ HRESULT hrStatus));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, AssemblyUnloadStarted, HRESULT(
		/* [in] */ AssemblyID assemblyId));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, AssemblyUnloadFinished, HRESULT(
		/* [in] */ AssemblyID assemblyId,
		/* [in] */ HRESULT hrStatus));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, ModuleLoadStarted, HRESULT(
		/* [in] */ ModuleID moduleId));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, ModuleLoadFinished, HRESULT(
		/* [in] */ ModuleID moduleId,
		/* [in] */ HRESULT hrStatus));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, ModuleUnloadStarted, HRESULT(
		/* [in] */ ModuleID moduleId));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, ModuleUnloadFinished, HRESULT(
		/* [in] */ ModuleID moduleId,
		/* [in] */ HRESULT hrStatus));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, ModuleAttachedToAssembly, HRESULT(
		/* [in] */ ModuleID moduleId,
		/* [in] */ AssemblyID assemblyId));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, ClassLoadStarted, HRESULT(
		/* [in] */ ClassID classId));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, ClassLoadFinished, HRESULT(
		/* [in] */ ClassID classId,
		/* [in] */ HRESULT hrStatus));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, ClassUnloadStarted, HRESULT(
		/* [in] */ ClassID classId));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, ClassUnloadFinished, HRESULT(
		/* [in] */ ClassID classId,
		/* [in] */ HRESULT hrStatus));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, FunctionUnloadStarted, HRESULT(
		/* [in] */ FunctionID functionId));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, JITCompilationStarted, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [in] */ BOOL fIsSafeToBlock));
	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, JITCompilationFinished, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [in] */ HRESULT hrStatus,
		/* [in] */ BOOL fIsSafeToBlock));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, JITCachedFunctionSearchStarted, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [out] */ BOOL *pbUseCachedFunction));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, JITCachedFunctionSearchFinished, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [in] */ COR_PRF_JIT_CACHE result));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, JITFunctionPitched, HRESULT(
		/* [in] */ FunctionID functionId));
	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, JITInlining, HRESULT(
		/* [in] */ FunctionID callerId,
		/* [in] */ FunctionID calleeId,
		/* [out] */ BOOL *pfShouldInline));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, ThreadCreated, HRESULT(
		/* [in] */ ThreadID functionId));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, ThreadDestroyed, HRESULT(
		/* [in] */ ThreadID functionId));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, ThreadAssignedToOSThread, HRESULT(
		/* [in] */ ThreadID managedThreadId,
		/* [in] */ DWORD osThreadId));
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, RemotingClientInvocationStarted, HRESULT());
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, RemotingClientSendingMessage, HRESULT(
		/* [in] */ GUID *pCookie,
		/* [in] */ BOOL fIsAsync));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, RemotingClientReceivingReply, HRESULT(
		/* [in] */ GUID *pCookie,
		/* [in] */ BOOL fIsAsync));
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, RemotingClientInvocationFinished, HRESULT());
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, RemotingServerReceivingMessage, HRESULT(
		/* [in] */ GUID *pCookie,
		/* [in] */ BOOL fIsAsync));
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, RemotingServerInvocationStarted, HRESULT());
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, RemotingServerInvocationReturned, HRESULT());
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, RemotingServerSendingReply, HRESULT(
		/* [in] */ GUID *pCookie,
		/* [in] */ BOOL fIsAsync));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, UnmanagedToManagedTransition, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [in] */ COR_PRF_TRANSITION_REASON reason));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, ManagedToUnmanagedTransition, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [in] */ COR_PRF_TRANSITION_REASON reason));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, RuntimeSuspendStarted, HRESULT(
		/* [in] */ COR_PRF_SUSPEND_REASON suspendReason));
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, RuntimeSuspendFinished, HRESULT());
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, RuntimeSuspendAborted, HRESULT());
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, RuntimeResumeStarted, HRESULT());
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, RuntimeResumeFinished, HRESULT());
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, RuntimeThreadSuspended, HRESULT(
		/* [in] */ ThreadID threadId));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, RuntimeThreadResumed, HRESULT(
		/* [in] */ ThreadID threadId));
	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, MovedReferences, HRESULT(
		/* [in] */ ULONG cMovedObjectIDRanges,
		/* [size_is][in] */ ObjectID oldObjectIDRangeStart[],
		/* [size_is][in] */ ObjectID newObjectIDRangeStart[],
		/* [size_is][in] */ ULONG cObjectIDRangeLength[]));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, ObjectAllocated, HRESULT(
		/* [in] */ ObjectID objectId,
		/* [in] */ ClassID classId));
	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, ObjectsAllocatedByClass, HRESULT(
		/* [in] */ ULONG cClassCount,
		/* [size_is][in] */ ClassID classIds[],
		/* [size_is][in] */ ULONG cObjects[]));
	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, ObjectReferences, HRESULT(
		/* [in] */ ObjectID objectId,
		/* [in] */ ClassID classId,
		/* [in] */ ULONG cObjectRefs,
		/* [size_is][in] */ ObjectID objectRefIds[]));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, RootReferences, HRESULT(
		/* [in] */ ULONG cRootRefs,
		/* [size_is][in] */ ObjectID rootRefIds[]));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, ExceptionThrown, HRESULT(
		/* [in] */ ObjectID thrownObjectId));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, ExceptionSearchFunctionEnter, HRESULT(
		/* [in] */ FunctionID functionId));
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, ExceptionSearchFunctionLeave, HRESULT());
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, ExceptionSearchFilterEnter, HRESULT(
		/* [in] */ FunctionID functionId));
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, ExceptionSearchFilterLeave, HRESULT());
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, ExceptionSearchCatcherFound, HRESULT(
		/* [in] */ FunctionID functionId));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, ExceptionOSHandlerEnter, HRESULT(
		/* [in] */  UINT_PTR __unused));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, ExceptionOSHandlerLeave, HRESULT(
		/* [in] */  UINT_PTR __unused));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, ExceptionUnwindFunctionEnter, HRESULT(
		/* [in] */  FunctionID functionId));
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, ExceptionUnwindFunctionLeave, HRESULT());
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, ExceptionUnwindFinallyEnter, HRESULT(
		/* [in] */  FunctionID functionId));
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, ExceptionUnwindFinallyLeave, HRESULT());
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, ExceptionCatcherEnter, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [in] */ ObjectID objectId));
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, ExceptionCatcherLeave, HRESULT());
	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, COMClassicVTableCreated, HRESULT(
		/* [in] */ ClassID wrappedClassId,
		/* [in] */ REFGUID implementedIID,
		/* [in] */ void *pVTable,
		/* [in] */ ULONG cSlots));
	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, COMClassicVTableDestroyed, HRESULT(
		/* [in] */ ClassID wrappedClassId,
		/* [in] */ REFGUID implementedIID,
		/* [in] */ void *pVTable));
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, ExceptionCLRCatcherFound, HRESULT());
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, ExceptionCLRCatcherExecute, HRESULT());

	// ICorProfilerCallback2
public:
	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, ThreadNameChanged, HRESULT(
		/* [in] */ ThreadID threadId,
		/* [in] */ ULONG cchName,
		/* [in] */
		__in_ecount_opt(cchName)  WCHAR name[]));
	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, GarbageCollectionStarted, HRESULT(
		/* [in] */ int cGenerations,
		/* [size_is][in] */ BOOL generationCollected[],
		/* [in] */ COR_PRF_GC_REASON reason));
	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, SurvivingReferences, HRESULT(
		/* [in] */ ULONG cSurvivingObjectIDRanges,
		/* [size_is][in] */ ObjectID objectIDRangeStart[],
		/* [size_is][in] */ ULONG cObjectIDRangeLength[]));
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, GarbageCollectionFinished, HRESULT());
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, FinalizeableObjectQueued, HRESULT(
		/* [in] */ DWORD finalizerFlags,
		/* [in] */ ObjectID objectID));
	MOCK_METHOD5_WITH_CALLTYPE(STDMETHODCALLTYPE, RootReferences2, HRESULT(
		/* [in] */ ULONG cRootRefs,
		/* [size_is][in] */ ObjectID rootRefIds[],
		/* [size_is][in] */ COR_PRF_GC_ROOT_KIND rootKinds[],
		/* [size_is][in] */ COR_PRF_GC_ROOT_FLAGS rootFlags[],
		/* [size_is][in] */ UINT_PTR rootIds[]));
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, HandleCreated, HRESULT(
		/* [in] */ GCHandleID handleId,
		/* [in] */ ObjectID initialObjectId));
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, HandleDestroyed, HRESULT(
		/* [in] */ GCHandleID handleId));

	// ICorProfilerCallback3
public:	
	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, InitializeForAttach, HRESULT(
		/* [in] */ IUnknown *pCorProfilerInfoUnk,
		/* [in] */ void *pvClientData,
		/* [in] */ UINT cbClientData));
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, ProfilerAttachComplete, HRESULT());
	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, ProfilerDetachSucceeded, HRESULT());

	// ICorProfilerCallback4
public:
	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, ReJITCompilationStarted, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [in] */ ReJITID rejitId,
		/* [in] */ BOOL fIsSafeToBlock));
	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, GetReJITParameters, HRESULT(
		/* [in] */ ModuleID moduleId,
		/* [in] */ mdMethodDef methodId,
		/* [in] */ ICorProfilerFunctionControl *pFunctionControl));
	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, ReJITCompilationFinished, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [in] */ ReJITID rejitId,
		/* [in] */ HRESULT hrStatus,
		/* [in] */ BOOL fIsSafeToBlock));
	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, ReJITError, HRESULT(
		/* [in] */ ModuleID moduleId,
		/* [in] */ mdMethodDef methodId,
		/* [in] */ FunctionID functionId,
		/* [in] */ HRESULT hrStatus));
	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, MovedReferences2, HRESULT(
		/* [in] */ ULONG cMovedObjectIDRanges,
		/* [size_is][in] */ ObjectID oldObjectIDRangeStart[],
		/* [size_is][in] */ ObjectID newObjectIDRangeStart[],
		/* [size_is][in] */ SIZE_T cObjectIDRangeLength[]));
	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, SurvivingReferences2, HRESULT(
		/* [in] */ ULONG cSurvivingObjectIDRanges,
		/* [size_is][in] */ ObjectID objectIDRangeStart[],
		/* [size_is][in] */ SIZE_T cObjectIDRangeLength[]));

	// ICorProfilerCallback5
public:
	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, ConditionalWeakTableElementReferences, HRESULT(
		/* [in] */ ULONG cRootRefs,
		/* [size_is][in] */ ObjectID keyRefIds[],
		/* [size_is][in] */ ObjectID valueRefIds[],
		/* [size_is][in] */ GCHandleID rootIds[]));

	// ICorProfilerCallback6
public:
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, GetAssemblyReferences, HRESULT(
		/* [string][in] */ const WCHAR *wszAssemblyPath,
		/* [in] */ ICorProfilerAssemblyReferenceProvider *pAsmRefProvider));

	// ICorProfilerCallback7
public:
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, ModuleInMemorySymbolsUpdated, HRESULT(
		/* [in] */ ModuleID moduleId));

	// ICorProfilerCallback8
public:
	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, DynamicMethodJITCompilationStarted, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [in] */ BOOL fIsSafeToBlock,
		/* [in] */ LPCBYTE pILHeader,
		/* [in] */ ULONG cbILHeader));
	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, DynamicMethodJITCompilationFinished, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [in] */ HRESULT hrStatus,
		/* [in] */ BOOL fIsSafeToBlock));

};
