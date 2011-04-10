#pragma once

class CProfilerBase : public ICorProfilerCallback3
{
    // ICorProfilerCallback
public:
    virtual HRESULT STDMETHODCALLTYPE Initialize( 
        /* [in] */ IUnknown *pICorProfilerInfoUnk)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE Shutdown( void)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE AppDomainCreationStarted( 
        /* [in] */ AppDomainID appDomainId)	
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE AppDomainCreationFinished( 
        /* [in] */ AppDomainID appDomainId,
        /* [in] */ HRESULT hrStatus) 
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE AppDomainShutdownStarted( 
        /* [in] */ AppDomainID appDomainId)	
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE AppDomainShutdownFinished( 
        /* [in] */ AppDomainID appDomainId,
        /* [in] */ HRESULT hrStatus) 
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE AssemblyLoadStarted( 
        /* [in] */ AssemblyID assemblyId) 
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE AssemblyLoadFinished( 
        /* [in] */ AssemblyID assemblyId,
        /* [in] */ HRESULT hrStatus) 
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE AssemblyUnloadStarted( 
        /* [in] */ AssemblyID assemblyId) 
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE AssemblyUnloadFinished( 
        /* [in] */ AssemblyID assemblyId,
        /* [in] */ HRESULT hrStatus) 
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ModuleLoadStarted( 
        /* [in] */ ModuleID moduleId) 
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ModuleLoadFinished( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ HRESULT hrStatus) 
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ModuleUnloadStarted( 
        /* [in] */ ModuleID moduleId) 
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ModuleUnloadFinished( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ HRESULT hrStatus) 
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ModuleAttachedToAssembly( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ AssemblyID assemblyId)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ClassLoadStarted( 
        /* [in] */ ClassID classId)	
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ClassLoadFinished( 
        /* [in] */ ClassID classId,
        /* [in] */ HRESULT hrStatus) 
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ClassUnloadStarted( 
        /* [in] */ ClassID classId)	
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ClassUnloadFinished( 
        /* [in] */ ClassID classId,
        /* [in] */ HRESULT hrStatus) 
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE FunctionUnloadStarted( 
        /* [in] */ FunctionID functionId) 
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE JITCompilationStarted( 
        /* [in] */ FunctionID functionId,
        /* [in] */ BOOL fIsSafeToBlock)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE JITCompilationFinished( 
        /* [in] */ FunctionID functionId,
        /* [in] */ HRESULT hrStatus,
        /* [in] */ BOOL fIsSafeToBlock) 
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE JITCachedFunctionSearchStarted( 
        /* [in] */ FunctionID functionId,
        /* [out] */ BOOL *pbUseCachedFunction)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE JITCachedFunctionSearchFinished( 
        /* [in] */ FunctionID functionId,
        /* [in] */ COR_PRF_JIT_CACHE result)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE JITFunctionPitched( 
        /* [in] */ FunctionID functionId)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE JITInlining( 
        /* [in] */ FunctionID callerId,
        /* [in] */ FunctionID calleeId,
        /* [out] */ BOOL *pfShouldInline)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ThreadCreated( 
        /* [in] */ ThreadID threadId)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ThreadDestroyed( 
        /* [in] */ ThreadID threadId)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ThreadAssignedToOSThread( 
        /* [in] */ ThreadID managedThreadId,
        /* [in] */ DWORD osThreadId)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RemotingClientInvocationStarted( void)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RemotingClientSendingMessage( 
        /* [in] */ GUID *pCookie,
        /* [in] */ BOOL fIsAsync)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RemotingClientReceivingReply( 
        /* [in] */ GUID *pCookie,
        /* [in] */ BOOL fIsAsync)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RemotingClientInvocationFinished( void)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RemotingServerReceivingMessage( 
        /* [in] */ GUID *pCookie,
        /* [in] */ BOOL fIsAsync)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RemotingServerInvocationStarted( void)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RemotingServerInvocationReturned( void)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RemotingServerSendingReply( 
        /* [in] */ GUID *pCookie,
        /* [in] */ BOOL fIsAsync)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE UnmanagedToManagedTransition( 
        /* [in] */ FunctionID functionId,
        /* [in] */ COR_PRF_TRANSITION_REASON reason )
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ManagedToUnmanagedTransition( 
        /* [in] */ FunctionID functionId,
        /* [in] */ COR_PRF_TRANSITION_REASON reason)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeSuspendStarted( 
        /* [in] */ COR_PRF_SUSPEND_REASON suspendReason)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeSuspendFinished( void)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeSuspendAborted( void)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeResumeStarted( void)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeResumeFinished( void)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeThreadSuspended( 
        /* [in] */ ThreadID threadId)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeThreadResumed( 
        /* [in] */ ThreadID threadId)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE MovedReferences( 
        /* [in] */ ULONG cMovedObjectIDRanges,
        /* [size_is][in] */ ObjectID oldObjectIDRangeStart[  ],
        /* [size_is][in] */ ObjectID newObjectIDRangeStart[  ],
        /* [size_is][in] */ ULONG cObjectIDRangeLength[  ])
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ObjectAllocated( 
        /* [in] */ ObjectID objectId,
        /* [in] */ ClassID classId)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ObjectsAllocatedByClass( 
        /* [in] */ ULONG cClassCount,
        /* [size_is][in] */ ClassID classIds[  ],
        /* [size_is][in] */ ULONG cObjects[  ])
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ObjectReferences( 
        /* [in] */ ObjectID objectId,
        /* [in] */ ClassID classId,
        /* [in] */ ULONG cObjectRefs,
        /* [size_is][in] */ ObjectID objectRefIds[  ])
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RootReferences( 
        /* [in] */ ULONG cRootRefs,
        /* [size_is][in] */ ObjectID rootRefIds[  ])
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionThrown( 
        /* [in] */ ObjectID thrownObjectId)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFunctionEnter( 
        /* [in] */ FunctionID functionId)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFunctionLeave( void)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFilterEnter( 
        /* [in] */ FunctionID functionId)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFilterLeave( void)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchCatcherFound( 
        /* [in] */ FunctionID functionId)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionOSHandlerEnter( 
        /* [in] */ UINT_PTR __unused)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionOSHandlerLeave( 
        /* [in] */ UINT_PTR __unused)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFunctionEnter( 
        /* [in] */ FunctionID functionId)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFunctionLeave( void)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFinallyEnter( 
        /* [in] */ FunctionID functionId)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFinallyLeave( void)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionCatcherEnter( 
        /* [in] */ FunctionID functionId,
        /* [in] */ ObjectID objectId)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionCatcherLeave( void)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE COMClassicVTableCreated( 
        /* [in] */ ClassID wrappedClassId,
        /* [in] */ REFGUID implementedIID,
        /* [in] */ void *pVTable,
        /* [in] */ ULONG cSlots)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE COMClassicVTableDestroyed( 
        /* [in] */ ClassID wrappedClassId,
        /* [in] */ REFGUID implementedIID,
        /* [in] */ void *pVTable)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionCLRCatcherFound( void)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionCLRCatcherExecute( void)
    { return S_OK; }

// ICorProfilerCallback2
public:
    virtual HRESULT STDMETHODCALLTYPE ThreadNameChanged( 
        /* [in] */ ThreadID threadId,
        /* [in] */ ULONG cchName,
        /* [in] */ 
        __in_ecount_opt(cchName)  WCHAR name[  ])
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE GarbageCollectionStarted( 
        /* [in] */ int cGenerations,
        /* [size_is][in] */ BOOL generationCollected[  ],
        /* [in] */ COR_PRF_GC_REASON reason)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE SurvivingReferences( 
        /* [in] */ ULONG cSurvivingObjectIDRanges,
        /* [size_is][in] */ ObjectID objectIDRangeStart[  ],
        /* [size_is][in] */ ULONG cObjectIDRangeLength[  ])
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE GarbageCollectionFinished( void)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE FinalizeableObjectQueued( 
        /* [in] */ DWORD finalizerFlags,
        /* [in] */ ObjectID objectID)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE RootReferences2( 
        /* [in] */ ULONG cRootRefs,
        /* [size_is][in] */ ObjectID rootRefIds[  ],
        /* [size_is][in] */ COR_PRF_GC_ROOT_KIND rootKinds[  ],
        /* [size_is][in] */ COR_PRF_GC_ROOT_FLAGS rootFlags[  ],
        /* [size_is][in] */ UINT_PTR rootIds[  ])
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE HandleCreated( 
        /* [in] */ GCHandleID handleId,
        /* [in] */ ObjectID initialObjectId)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE HandleDestroyed( 
        /* [in] */ GCHandleID handleId)
    { return S_OK; }

// ICorProfilerCallback3
public:
    virtual HRESULT STDMETHODCALLTYPE InitializeForAttach( 
        /* [in] */ IUnknown *pCorProfilerInfoUnk,
        /* [in] */ void *pvClientData,
        /* [in] */ UINT cbClientData)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ProfilerAttachComplete( void)
    { return S_OK; }
        
    virtual HRESULT STDMETHODCALLTYPE ProfilerDetachSucceeded( void)
    { return S_OK; }

};
