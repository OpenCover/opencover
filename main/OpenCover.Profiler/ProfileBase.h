//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#pragma once

#ifndef _TOOLSETV71
class CProfilerBase : public ICorProfilerCallback7
{
#else
class CProfilerBase : public ICorProfilerCallback3
{
#endif
    // ICorProfilerCallback
public:
    virtual ~CProfilerBase()
    {
    }

    virtual HRESULT STDMETHODCALLTYPE Initialize( 
        /* [in] */ IUnknown *pICorProfilerInfoUnk) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE Shutdown( void) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE AppDomainCreationStarted( 
        /* [in] */ AppDomainID appDomainId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE AppDomainCreationFinished( 
        /* [in] */ AppDomainID appDomainId,
        /* [in] */ HRESULT hrStatus) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE AppDomainShutdownStarted( 
        /* [in] */ AppDomainID appDomainId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE AppDomainShutdownFinished( 
        /* [in] */ AppDomainID appDomainId,
        /* [in] */ HRESULT hrStatus) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE AssemblyLoadStarted( 
        /* [in] */ AssemblyID assemblyId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE AssemblyLoadFinished( 
        /* [in] */ AssemblyID assemblyId,
        /* [in] */ HRESULT hrStatus) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE AssemblyUnloadStarted( 
        /* [in] */ AssemblyID assemblyId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE AssemblyUnloadFinished( 
        /* [in] */ AssemblyID assemblyId,
        /* [in] */ HRESULT hrStatus) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ModuleLoadStarted( 
        /* [in] */ ModuleID moduleId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ModuleLoadFinished( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ HRESULT hrStatus) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ModuleUnloadStarted( 
        /* [in] */ ModuleID moduleId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ModuleUnloadFinished( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ HRESULT hrStatus) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ModuleAttachedToAssembly( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ AssemblyID assemblyId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ClassLoadStarted( 
        /* [in] */ ClassID classId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ClassLoadFinished( 
        /* [in] */ ClassID classId,
        /* [in] */ HRESULT hrStatus) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ClassUnloadStarted( 
        /* [in] */ ClassID classId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ClassUnloadFinished( 
        /* [in] */ ClassID classId,
        /* [in] */ HRESULT hrStatus) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE FunctionUnloadStarted( 
        /* [in] */ FunctionID functionId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE JITCompilationStarted( 
        /* [in] */ FunctionID functionId,
        /* [in] */ BOOL fIsSafeToBlock) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE JITCompilationFinished( 
        /* [in] */ FunctionID functionId,
        /* [in] */ HRESULT hrStatus,
        /* [in] */ BOOL fIsSafeToBlock) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE JITCachedFunctionSearchStarted( 
        /* [in] */ FunctionID functionId,
        /* [out] */ BOOL *pbUseCachedFunction) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE JITCachedFunctionSearchFinished( 
        /* [in] */ FunctionID functionId,
        /* [in] */ COR_PRF_JIT_CACHE result) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE JITFunctionPitched( 
        /* [in] */ FunctionID functionId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE JITInlining( 
        /* [in] */ FunctionID callerId,
        /* [in] */ FunctionID calleeId,
        /* [out] */ BOOL *pfShouldInline) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ThreadCreated( 
        /* [in] */ ThreadID threadId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ThreadDestroyed( 
        /* [in] */ ThreadID threadId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ThreadAssignedToOSThread( 
        /* [in] */ ThreadID managedThreadId,
        /* [in] */ DWORD osThreadId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RemotingClientInvocationStarted( void) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RemotingClientSendingMessage( 
        /* [in] */ GUID *pCookie,
        /* [in] */ BOOL fIsAsync) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RemotingClientReceivingReply( 
        /* [in] */ GUID *pCookie,
        /* [in] */ BOOL fIsAsync) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RemotingClientInvocationFinished( void) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RemotingServerReceivingMessage( 
        /* [in] */ GUID *pCookie,
        /* [in] */ BOOL fIsAsync) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RemotingServerInvocationStarted( void) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RemotingServerInvocationReturned( void) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RemotingServerSendingReply( 
        /* [in] */ GUID *pCookie,
        /* [in] */ BOOL fIsAsync) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE UnmanagedToManagedTransition( 
        /* [in] */ FunctionID functionId,
        /* [in] */ COR_PRF_TRANSITION_REASON reason ) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ManagedToUnmanagedTransition( 
        /* [in] */ FunctionID functionId,
        /* [in] */ COR_PRF_TRANSITION_REASON reason) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeSuspendStarted( 
        /* [in] */ COR_PRF_SUSPEND_REASON suspendReason) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeSuspendFinished( void) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeSuspendAborted( void) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeResumeStarted( void) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeResumeFinished( void) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeThreadSuspended( 
        /* [in] */ ThreadID threadId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeThreadResumed( 
        /* [in] */ ThreadID threadId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE MovedReferences( 
        /* [in] */ ULONG cMovedObjectIDRanges,
        /* [size_is][in] */ ObjectID oldObjectIDRangeStart[  ],
        /* [size_is][in] */ ObjectID newObjectIDRangeStart[  ],
        /* [size_is][in] */ ULONG cObjectIDRangeLength[  ]) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ObjectAllocated( 
        /* [in] */ ObjectID objectId,
        /* [in] */ ClassID classId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ObjectsAllocatedByClass( 
        /* [in] */ ULONG cClassCount,
        /* [size_is][in] */ ClassID classIds[  ],
        /* [size_is][in] */ ULONG cObjects[  ]) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ObjectReferences( 
        /* [in] */ ObjectID objectId,
        /* [in] */ ClassID classId,
        /* [in] */ ULONG cObjectRefs,
        /* [size_is][in] */ ObjectID objectRefIds[  ]) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RootReferences( 
        /* [in] */ ULONG cRootRefs,
        /* [size_is][in] */ ObjectID rootRefIds[  ]) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionThrown( 
        /* [in] */ ObjectID thrownObjectId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFunctionEnter( 
        /* [in] */ FunctionID functionId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFunctionLeave( void) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFilterEnter( 
        /* [in] */ FunctionID functionId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFilterLeave( void) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchCatcherFound( 
        /* [in] */ FunctionID functionId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionOSHandlerEnter( 
        /* [in] */ UINT_PTR __unused) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionOSHandlerLeave( 
        /* [in] */ UINT_PTR __unused) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFunctionEnter( 
        /* [in] */ FunctionID functionId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFunctionLeave( void) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFinallyEnter( 
        /* [in] */ FunctionID functionId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFinallyLeave( void) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionCatcherEnter( 
        /* [in] */ FunctionID functionId,
        /* [in] */ ObjectID objectId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionCatcherLeave( void) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE COMClassicVTableCreated( 
        /* [in] */ ClassID wrappedClassId,
        /* [in] */ REFGUID implementedIID,
        /* [in] */ void *pVTable,
        /* [in] */ ULONG cSlots) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE COMClassicVTableDestroyed( 
        /* [in] */ ClassID wrappedClassId,
        /* [in] */ REFGUID implementedIID,
        /* [in] */ void *pVTable) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionCLRCatcherFound( void) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionCLRCatcherExecute( void) override
    { return S_OK; }

// ICorProfilerCallback2
public:
    virtual HRESULT STDMETHODCALLTYPE ThreadNameChanged( 
        /* [in] */ ThreadID threadId,
        /* [in] */ ULONG cchName,
        /* [in] */ 
        __in_ecount_opt(cchName)  WCHAR name[  ]) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE GarbageCollectionStarted( 
        /* [in] */ int cGenerations,
        /* [size_is][in] */ BOOL generationCollected[  ],
        /* [in] */ COR_PRF_GC_REASON reason) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE SurvivingReferences( 
        /* [in] */ ULONG cSurvivingObjectIDRanges,
        /* [size_is][in] */ ObjectID objectIDRangeStart[  ],
        /* [size_is][in] */ ULONG cObjectIDRangeLength[  ]) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE GarbageCollectionFinished( void) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE FinalizeableObjectQueued( 
        /* [in] */ DWORD finalizerFlags,
        /* [in] */ ObjectID objectID) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RootReferences2( 
        /* [in] */ ULONG cRootRefs,
        /* [size_is][in] */ ObjectID rootRefIds[  ],
        /* [size_is][in] */ COR_PRF_GC_ROOT_KIND rootKinds[  ],
        /* [size_is][in] */ COR_PRF_GC_ROOT_FLAGS rootFlags[  ],
        /* [size_is][in] */ UINT_PTR rootIds[  ]) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE HandleCreated( 
        /* [in] */ GCHandleID handleId,
        /* [in] */ ObjectID initialObjectId) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE HandleDestroyed( 
        /* [in] */ GCHandleID handleId) override
    { return S_OK; }

// ICorProfilerCallback3
public:
    virtual HRESULT STDMETHODCALLTYPE InitializeForAttach( 
        /* [in] */ IUnknown *pCorProfilerInfoUnk,
        /* [in] */ void *pvClientData,
        /* [in] */ UINT cbClientData) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ProfilerAttachComplete( void) override
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ProfilerDetachSucceeded( void) override
    { return S_OK; }

#ifndef _TOOLSETV71
// ICorProfilerCallback4
public:
    virtual HRESULT STDMETHODCALLTYPE ReJITCompilationStarted( 
        /* [in] */ FunctionID functionId,
        /* [in] */ ReJITID rejitId,
        /* [in] */ BOOL fIsSafeToBlock) override
    { return S_OK; }

    virtual HRESULT STDMETHODCALLTYPE GetReJITParameters( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ mdMethodDef methodId,
        /* [in] */ ICorProfilerFunctionControl *pFunctionControl) override
    { return S_OK; }
        
	virtual HRESULT STDMETHODCALLTYPE ReJITCompilationFinished( 
        /* [in] */ FunctionID functionId,
        /* [in] */ ReJITID rejitId,
        /* [in] */ HRESULT hrStatus,
        /* [in] */ BOOL fIsSafeToBlock) override
	{ return S_OK; }
        
	virtual HRESULT STDMETHODCALLTYPE ReJITError( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ mdMethodDef methodId,
        /* [in] */ FunctionID functionId,
        /* [in] */ HRESULT hrStatus) override
	{ return S_OK; }
        
	virtual HRESULT STDMETHODCALLTYPE MovedReferences2( 
        /* [in] */ ULONG cMovedObjectIDRanges,
        /* [size_is][in] */ ObjectID oldObjectIDRangeStart[  ],
        /* [size_is][in] */ ObjectID newObjectIDRangeStart[  ],
        /* [size_is][in] */ SIZE_T cObjectIDRangeLength[  ]) override
	{ return S_OK; }

    virtual HRESULT STDMETHODCALLTYPE SurvivingReferences2( 
        /* [in] */ ULONG cSurvivingObjectIDRanges,
        /* [size_is][in] */ ObjectID objectIDRangeStart[  ],
        /* [size_is][in] */ SIZE_T cObjectIDRangeLength[  ]) override
    { return S_OK; }

// ICorProfilerCallback5
public:
	virtual HRESULT STDMETHODCALLTYPE ConditionalWeakTableElementReferences( 
		/* [in] */ ULONG cRootRefs,
		/* [size_is][in] */ ObjectID keyRefIds[  ],
		/* [size_is][in] */ ObjectID valueRefIds[  ],
		/* [size_is][in] */ GCHandleID rootIds[  ]) override
	{ return S_OK; }
#endif

// ICorProfilerCallback6
public:
    virtual HRESULT STDMETHODCALLTYPE GetAssemblyReferences(
        /* [string][in] */ const WCHAR *wszAssemblyPath,
        /* [in] */ ICorProfilerAssemblyReferenceProvider *pAsmRefProvider) override
    {
        return S_OK;
    }

    // ICorProfilerCallback7
public:
    virtual HRESULT STDMETHODCALLTYPE ModuleInMemorySymbolsUpdated(
        ModuleID moduleId) override
    {
        return S_OK;
    }
};