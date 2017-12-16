//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#pragma once

using namespace ATL;

class CProfilerBase : public ICorProfilerCallback8
{
public:
    virtual ~CProfilerBase()
    {
		ReleaseChainedProfiler();
	}

	void ReleaseChainedProfiler()
    {
		if (chainedProfiler_ != nullptr) chainedProfiler_.Release();
		if (chainedProfiler2_ != nullptr) chainedProfiler2_.Release();
		if (chainedProfiler3_ != nullptr) chainedProfiler3_.Release();
		if (chainedProfiler4_ != nullptr) chainedProfiler4_.Release();
		if (chainedProfiler5_ != nullptr) chainedProfiler5_.Release();
		if (chainedProfiler6_ != nullptr) chainedProfiler6_.Release();
		if (chainedProfiler7_ != nullptr) chainedProfiler7_.Release();
		if (chainedProfiler8_ != nullptr) chainedProfiler8_.Release();
	}

private:
	CComQIPtr<ICorProfilerCallback> chainedProfiler_;
	CComQIPtr<ICorProfilerCallback2> chainedProfiler2_;
	CComQIPtr<ICorProfilerCallback3> chainedProfiler3_;
	CComQIPtr<ICorProfilerCallback4> chainedProfiler4_;
	CComQIPtr<ICorProfilerCallback5> chainedProfiler5_;
	CComQIPtr<ICorProfilerCallback6> chainedProfiler6_;
	CComQIPtr<ICorProfilerCallback7> chainedProfiler7_;
	CComQIPtr<ICorProfilerCallback8> chainedProfiler8_;

public:
	void HookChainedProfiler(IUnknown* hookedProfiler)
	{
		chainedProfiler_ = hookedProfiler;
		chainedProfiler2_ = hookedProfiler;
		chainedProfiler3_ = hookedProfiler;
		chainedProfiler4_ = hookedProfiler;
		chainedProfiler5_ = hookedProfiler;
		chainedProfiler6_ = hookedProfiler;
		chainedProfiler7_ = hookedProfiler;
		chainedProfiler8_ = hookedProfiler;
	}

	bool IsChainedProfilerHooked() const { return chainedProfiler_ != nullptr; }

protected:
	template<class A, class B>
	static HRESULT ChainCall(A callA, B callBIfAReturnS_OK)
	{
		HRESULT hr = callA();
		return hr != S_OK ? hr : callBIfAReturnS_OK();
	}
	
	template<class P, class CCP, class CLP>
	static HRESULT ChainProfiler(P profiler, CCP callChainedProfiler, CLP callLocalProfilerIfS_OK)
	{
		return ChainCall([&]() { return profiler != nullptr ? callChainedProfiler(profiler) : S_OK;	}, 
			[&]() { return callLocalProfilerIfS_OK(); });
	}

// ICorProfilerCallback
public:
    virtual HRESULT STDMETHODCALLTYPE Initialize( 
        /* [in] */ IUnknown *pICorProfilerInfoUnk) override
    { 
		return ChainProfiler(chainedProfiler_, 
			[&](ICorProfilerCallback *profiler) { return profiler->Initialize(pICorProfilerInfoUnk); },
			[]() { return S_OK; });
    }
        
    virtual HRESULT STDMETHODCALLTYPE Shutdown() override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->Shutdown(); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE AppDomainCreationStarted( 
        /* [in] */ AppDomainID appDomainId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->AppDomainCreationStarted(appDomainId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE AppDomainCreationFinished( 
        /* [in] */ AppDomainID appDomainId,
        /* [in] */ HRESULT hrStatus) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->AppDomainCreationFinished(appDomainId, hrStatus); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE AppDomainShutdownStarted( 
        /* [in] */ AppDomainID appDomainId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->AppDomainShutdownStarted(appDomainId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE AppDomainShutdownFinished( 
        /* [in] */ AppDomainID appDomainId,
        /* [in] */ HRESULT hrStatus) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->AppDomainShutdownFinished(appDomainId, hrStatus); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE AssemblyLoadStarted( 
        /* [in] */ AssemblyID assemblyId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->AssemblyLoadStarted(assemblyId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE AssemblyLoadFinished( 
        /* [in] */ AssemblyID assemblyId,
        /* [in] */ HRESULT hrStatus) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->AssemblyLoadFinished(assemblyId, hrStatus); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE AssemblyUnloadStarted( 
        /* [in] */ AssemblyID assemblyId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->AssemblyUnloadStarted(assemblyId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE AssemblyUnloadFinished( 
        /* [in] */ AssemblyID assemblyId,
        /* [in] */ HRESULT hrStatus) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->AssemblyUnloadFinished(assemblyId, hrStatus); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ModuleLoadStarted( 
        /* [in] */ ModuleID moduleId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ModuleLoadStarted(moduleId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ModuleLoadFinished( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ HRESULT hrStatus) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ModuleLoadFinished(moduleId, hrStatus); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ModuleUnloadStarted( 
        /* [in] */ ModuleID moduleId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ModuleUnloadStarted(moduleId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ModuleUnloadFinished( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ HRESULT hrStatus) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ModuleUnloadFinished(moduleId, hrStatus); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ModuleAttachedToAssembly( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ AssemblyID assemblyId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ModuleAttachedToAssembly(moduleId, assemblyId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ClassLoadStarted( 
        /* [in] */ ClassID classId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ClassLoadStarted(classId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ClassLoadFinished( 
        /* [in] */ ClassID classId,
        /* [in] */ HRESULT hrStatus) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ClassLoadFinished(classId, hrStatus); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ClassUnloadStarted( 
        /* [in] */ ClassID classId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ClassUnloadStarted(classId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ClassUnloadFinished( 
        /* [in] */ ClassID classId,
        /* [in] */ HRESULT hrStatus) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ClassUnloadFinished(classId, hrStatus); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE FunctionUnloadStarted( 
        /* [in] */ FunctionID functionId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->FunctionUnloadStarted(functionId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE JITCompilationStarted( 
        /* [in] */ FunctionID functionId,
        /* [in] */ BOOL fIsSafeToBlock) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->JITCompilationStarted(functionId, fIsSafeToBlock); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE JITCompilationFinished( 
        /* [in] */ FunctionID functionId,
        /* [in] */ HRESULT hrStatus,
        /* [in] */ BOOL fIsSafeToBlock) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->JITCompilationFinished(functionId, hrStatus, fIsSafeToBlock); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE JITCachedFunctionSearchStarted( 
        /* [in] */ FunctionID functionId,
        /* [out] */ BOOL *pbUseCachedFunction) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->JITCachedFunctionSearchStarted(functionId, pbUseCachedFunction); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE JITCachedFunctionSearchFinished( 
        /* [in] */ FunctionID functionId,
        /* [in] */ COR_PRF_JIT_CACHE result) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->JITCachedFunctionSearchFinished(functionId, result); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE JITFunctionPitched( 
        /* [in] */ FunctionID functionId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->JITFunctionPitched(functionId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE JITInlining( 
        /* [in] */ FunctionID callerId,
        /* [in] */ FunctionID calleeId,
        /* [out] */ BOOL *pfShouldInline) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->JITInlining(callerId, calleeId, pfShouldInline); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ThreadCreated( 
        /* [in] */ ThreadID threadId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ThreadCreated(threadId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ThreadDestroyed( 
        /* [in] */ ThreadID threadId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ThreadDestroyed(threadId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ThreadAssignedToOSThread( 
        /* [in] */ ThreadID managedThreadId,
        /* [in] */ DWORD osThreadId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ThreadAssignedToOSThread(managedThreadId, osThreadId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE RemotingClientInvocationStarted( void) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->RemotingClientInvocationStarted(); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE RemotingClientSendingMessage( 
        /* [in] */ GUID *pCookie,
        /* [in] */ BOOL fIsAsync) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->RemotingClientSendingMessage(pCookie, fIsAsync); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE RemotingClientReceivingReply( 
        /* [in] */ GUID *pCookie,
        /* [in] */ BOOL fIsAsync) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->RemotingClientReceivingReply(pCookie, fIsAsync); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE RemotingClientInvocationFinished( void) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->RemotingClientInvocationFinished(); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE RemotingServerReceivingMessage( 
        /* [in] */ GUID *pCookie,
        /* [in] */ BOOL fIsAsync) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->RemotingServerReceivingMessage(pCookie, fIsAsync); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE RemotingServerInvocationStarted( void) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->RemotingServerInvocationStarted(); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE RemotingServerInvocationReturned( void) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->RemotingServerInvocationReturned(); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE RemotingServerSendingReply( 
        /* [in] */ GUID *pCookie,
        /* [in] */ BOOL fIsAsync) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->RemotingServerSendingReply(pCookie, fIsAsync); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE UnmanagedToManagedTransition( 
        /* [in] */ FunctionID functionId,
        /* [in] */ COR_PRF_TRANSITION_REASON reason ) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->UnmanagedToManagedTransition(functionId, reason); },
			[]() { return S_OK; });
    }
        
    virtual HRESULT STDMETHODCALLTYPE ManagedToUnmanagedTransition( 
        /* [in] */ FunctionID functionId,
        /* [in] */ COR_PRF_TRANSITION_REASON reason) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ManagedToUnmanagedTransition(functionId, reason); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeSuspendStarted( 
        /* [in] */ COR_PRF_SUSPEND_REASON suspendReason) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->RuntimeSuspendStarted(suspendReason); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeSuspendFinished( void) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->RuntimeSuspendFinished(); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeSuspendAborted( void) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->RuntimeSuspendAborted(); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeResumeStarted( void) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->RuntimeResumeStarted(); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeResumeFinished( void) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->RuntimeResumeFinished(); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeThreadSuspended( 
        /* [in] */ ThreadID threadId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->RuntimeThreadSuspended(threadId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE RuntimeThreadResumed( 
        /* [in] */ ThreadID threadId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->RuntimeThreadResumed(threadId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE MovedReferences( 
        /* [in] */ ULONG cMovedObjectIDRanges,
        /* [size_is][in] */ ObjectID oldObjectIDRangeStart[  ],
        /* [size_is][in] */ ObjectID newObjectIDRangeStart[  ],
        /* [size_is][in] */ ULONG cObjectIDRangeLength[  ]) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->MovedReferences(cMovedObjectIDRanges, oldObjectIDRangeStart, newObjectIDRangeStart, cObjectIDRangeLength); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ObjectAllocated( 
        /* [in] */ ObjectID objectId,
        /* [in] */ ClassID classId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ObjectAllocated(objectId, classId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ObjectsAllocatedByClass( 
        /* [in] */ ULONG cClassCount,
        /* [size_is][in] */ ClassID classIds[  ],
        /* [size_is][in] */ ULONG cObjects[  ]) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ObjectsAllocatedByClass(cClassCount, classIds, cObjects); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ObjectReferences( 
        /* [in] */ ObjectID objectId,
        /* [in] */ ClassID classId,
        /* [in] */ ULONG cObjectRefs,
        /* [size_is][in] */ ObjectID objectRefIds[  ]) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ObjectReferences(objectId, classId, cObjectRefs, objectRefIds); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE RootReferences( 
        /* [in] */ ULONG cRootRefs,
        /* [size_is][in] */ ObjectID rootRefIds[  ]) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->RootReferences(cRootRefs, rootRefIds); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionThrown( 
        /* [in] */ ObjectID thrownObjectId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ExceptionThrown(thrownObjectId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFunctionEnter( 
        /* [in] */ FunctionID functionId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ExceptionSearchFunctionEnter(functionId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFunctionLeave( void) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ExceptionSearchFunctionLeave(); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFilterEnter( 
        /* [in] */ FunctionID functionId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ExceptionSearchFilterEnter(functionId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchFilterLeave( void) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ExceptionSearchFilterLeave(); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionSearchCatcherFound( 
        /* [in] */ FunctionID functionId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ExceptionSearchCatcherFound(functionId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionOSHandlerEnter( 
        /* [in] */ UINT_PTR __unused) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ExceptionOSHandlerEnter(__unused); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionOSHandlerLeave( 
        /* [in] */ UINT_PTR __unused) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ExceptionOSHandlerLeave(__unused); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFunctionEnter( 
        /* [in] */ FunctionID functionId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ExceptionUnwindFunctionEnter(functionId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFunctionLeave( void) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ExceptionUnwindFunctionLeave(); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFinallyEnter( 
        /* [in] */ FunctionID functionId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ExceptionUnwindFinallyEnter(functionId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionUnwindFinallyLeave( void) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ExceptionUnwindFinallyLeave(); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionCatcherEnter( 
        /* [in] */ FunctionID functionId,
        /* [in] */ ObjectID objectId) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ExceptionCatcherEnter(functionId, objectId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionCatcherLeave( void) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ExceptionCatcherLeave(); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE COMClassicVTableCreated( 
        /* [in] */ ClassID wrappedClassId,
        /* [in] */ REFGUID implementedIID,
        /* [in] */ void *pVTable,
        /* [in] */ ULONG cSlots) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->COMClassicVTableCreated(wrappedClassId, implementedIID, pVTable, cSlots); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE COMClassicVTableDestroyed( 
        /* [in] */ ClassID wrappedClassId,
        /* [in] */ REFGUID implementedIID,
        /* [in] */ void *pVTable) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->COMClassicVTableDestroyed(wrappedClassId, implementedIID, pVTable); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionCLRCatcherFound( void) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ExceptionCLRCatcherFound(); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ExceptionCLRCatcherExecute( void) override
    {
		return ChainProfiler(chainedProfiler_,
			[&](ICorProfilerCallback *profiler) { return profiler->ExceptionCLRCatcherExecute(); },
			[]() { return S_OK; });
	}

// ICorProfilerCallback2
public:
    virtual HRESULT STDMETHODCALLTYPE ThreadNameChanged( 
        /* [in] */ ThreadID threadId,
        /* [in] */ ULONG cchName,
        /* [in] */ 
        __in_ecount_opt(cchName)  WCHAR name[  ]) override
    {
		return ChainProfiler(chainedProfiler2_,
			[&](ICorProfilerCallback2 *profiler) { return profiler->ThreadNameChanged(threadId, cchName, name); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE GarbageCollectionStarted( 
        /* [in] */ int cGenerations,
        /* [size_is][in] */ BOOL generationCollected[  ],
        /* [in] */ COR_PRF_GC_REASON reason) override
    {
		return ChainProfiler(chainedProfiler2_,
			[&](ICorProfilerCallback2 *profiler) { return profiler->GarbageCollectionStarted(cGenerations, generationCollected, reason); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE SurvivingReferences( 
        /* [in] */ ULONG cSurvivingObjectIDRanges,
        /* [size_is][in] */ ObjectID objectIDRangeStart[  ],
        /* [size_is][in] */ ULONG cObjectIDRangeLength[  ]) override
    {
		return ChainProfiler(chainedProfiler2_,
			[&](ICorProfilerCallback2 *profiler) { return profiler->SurvivingReferences(cSurvivingObjectIDRanges, objectIDRangeStart, cObjectIDRangeLength); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE GarbageCollectionFinished( void) override
    {
		return ChainProfiler(chainedProfiler2_,
			[&](ICorProfilerCallback2 *profiler) { return profiler->GarbageCollectionFinished(); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE FinalizeableObjectQueued( 
        /* [in] */ DWORD finalizerFlags,
        /* [in] */ ObjectID objectID) override
    {
		return ChainProfiler(chainedProfiler2_,
			[&](ICorProfilerCallback2 *profiler) { return profiler->FinalizeableObjectQueued(finalizerFlags, objectID); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE RootReferences2( 
        /* [in] */ ULONG cRootRefs,
        /* [size_is][in] */ ObjectID rootRefIds[  ],
        /* [size_is][in] */ COR_PRF_GC_ROOT_KIND rootKinds[  ],
        /* [size_is][in] */ COR_PRF_GC_ROOT_FLAGS rootFlags[  ],
        /* [size_is][in] */ UINT_PTR rootIds[  ]) override
    {
		return ChainProfiler(chainedProfiler2_,
			[&](ICorProfilerCallback2 *profiler) { return profiler->RootReferences2(cRootRefs, rootRefIds, rootKinds, rootFlags, rootIds); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE HandleCreated( 
        /* [in] */ GCHandleID handleId,
        /* [in] */ ObjectID initialObjectId) override
    {
		return ChainProfiler(chainedProfiler2_,
			[&](ICorProfilerCallback2 *profiler) { return profiler->HandleCreated(handleId, initialObjectId); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE HandleDestroyed( 
        /* [in] */ GCHandleID handleId) override
    {
		return ChainProfiler(chainedProfiler2_,
			[&](ICorProfilerCallback2 *profiler) { return profiler->HandleDestroyed(handleId); },
			[]() { return S_OK; });
	}

// ICorProfilerCallback3
public:
    virtual HRESULT STDMETHODCALLTYPE InitializeForAttach( 
        /* [in] */ IUnknown *pCorProfilerInfoUnk,
        /* [in] */ void *pvClientData,
        /* [in] */ UINT cbClientData) override
    {
		return ChainProfiler(chainedProfiler3_,
			[&](ICorProfilerCallback3 *profiler) { return profiler->InitializeForAttach(pCorProfilerInfoUnk, pvClientData, cbClientData); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ProfilerAttachComplete( void) override
    {
		return ChainProfiler(chainedProfiler3_,
			[&](ICorProfilerCallback3 *profiler) { return profiler->ProfilerAttachComplete(); },
			[]() { return S_OK; });
	}
        
    virtual HRESULT STDMETHODCALLTYPE ProfilerDetachSucceeded( void) override
    {
		return ChainProfiler(chainedProfiler3_,
			[&](ICorProfilerCallback3 *profiler) { return profiler->ProfilerDetachSucceeded(); },
			[]() { return S_OK; });
	}

// ICorProfilerCallback4
public:
    virtual HRESULT STDMETHODCALLTYPE ReJITCompilationStarted( 
        /* [in] */ FunctionID functionId,
        /* [in] */ ReJITID rejitId,
        /* [in] */ BOOL fIsSafeToBlock) override
    {
		return ChainProfiler(chainedProfiler4_,
			[&](ICorProfilerCallback4 *profiler) { return profiler->ReJITCompilationStarted(functionId, rejitId, fIsSafeToBlock); },
			[]() { return S_OK; });
	}

    virtual HRESULT STDMETHODCALLTYPE GetReJITParameters( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ mdMethodDef methodId,
        /* [in] */ ICorProfilerFunctionControl *pFunctionControl) override
    {
		return ChainProfiler(chainedProfiler4_,
			[&](ICorProfilerCallback4 *profiler) { return profiler->GetReJITParameters(moduleId, methodId, pFunctionControl); },
			[]() { return S_OK; });
	}
        
	virtual HRESULT STDMETHODCALLTYPE ReJITCompilationFinished( 
        /* [in] */ FunctionID functionId,
        /* [in] */ ReJITID rejitId,
        /* [in] */ HRESULT hrStatus,
        /* [in] */ BOOL fIsSafeToBlock) override
	{
		return ChainProfiler(chainedProfiler4_,
			[&](ICorProfilerCallback4 *profiler) { return profiler->ReJITCompilationFinished(functionId, rejitId, hrStatus, fIsSafeToBlock); },
			[]() { return S_OK; });
	}
        
	virtual HRESULT STDMETHODCALLTYPE ReJITError( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ mdMethodDef methodId,
        /* [in] */ FunctionID functionId,
        /* [in] */ HRESULT hrStatus) override
	{
		return ChainProfiler(chainedProfiler4_,
			[&](ICorProfilerCallback4 *profiler) { return profiler->ReJITError(moduleId, methodId, functionId, hrStatus); },
			[]() { return S_OK; });
	}
        
	virtual HRESULT STDMETHODCALLTYPE MovedReferences2( 
        /* [in] */ ULONG cMovedObjectIDRanges,
        /* [size_is][in] */ ObjectID oldObjectIDRangeStart[  ],
        /* [size_is][in] */ ObjectID newObjectIDRangeStart[  ],
        /* [size_is][in] */ SIZE_T cObjectIDRangeLength[  ]) override
	{
		return ChainProfiler(chainedProfiler4_,
			[&](ICorProfilerCallback4 *profiler) { return profiler->MovedReferences2(cMovedObjectIDRanges, oldObjectIDRangeStart, newObjectIDRangeStart, cObjectIDRangeLength); },
			[]() { return S_OK; });
	}

    virtual HRESULT STDMETHODCALLTYPE SurvivingReferences2( 
        /* [in] */ ULONG cSurvivingObjectIDRanges,
        /* [size_is][in] */ ObjectID objectIDRangeStart[  ],
        /* [size_is][in] */ SIZE_T cObjectIDRangeLength[  ]) override
    {
		return ChainProfiler(chainedProfiler4_,
			[&](ICorProfilerCallback4 *profiler) { return profiler->SurvivingReferences2(cSurvivingObjectIDRanges, objectIDRangeStart, cObjectIDRangeLength); },
			[]() { return S_OK; });
	}

// ICorProfilerCallback5
public:
	virtual HRESULT STDMETHODCALLTYPE ConditionalWeakTableElementReferences( 
		/* [in] */ ULONG cRootRefs,
		/* [size_is][in] */ ObjectID keyRefIds[  ],
		/* [size_is][in] */ ObjectID valueRefIds[  ],
		/* [size_is][in] */ GCHandleID rootIds[  ]) override
	{
		return ChainProfiler(chainedProfiler5_,
			[&](ICorProfilerCallback5 *profiler) { return profiler->ConditionalWeakTableElementReferences(cRootRefs, keyRefIds, valueRefIds, rootIds); },
			[]() { return S_OK; });
	}

// ICorProfilerCallback6
public:
    virtual HRESULT STDMETHODCALLTYPE GetAssemblyReferences(
        /* [string][in] */ const WCHAR *wszAssemblyPath,
        /* [in] */ ICorProfilerAssemblyReferenceProvider *pAsmRefProvider) override
    {
		return ChainProfiler(chainedProfiler6_,
			[&](ICorProfilerCallback6 *profiler) { return profiler->GetAssemblyReferences(wszAssemblyPath, pAsmRefProvider); },
			[]() { return S_OK; });
    }

// ICorProfilerCallback7
public:
    virtual HRESULT STDMETHODCALLTYPE ModuleInMemorySymbolsUpdated(
        ModuleID moduleId) override
    {
		return ChainProfiler(chainedProfiler7_,
			[&](ICorProfilerCallback7 *profiler) { return profiler->ModuleInMemorySymbolsUpdated(moduleId); },
			[]() { return S_OK; });
    }

// ICorProfilerCallback8
public:
	virtual HRESULT STDMETHODCALLTYPE DynamicMethodJITCompilationStarted(
		/* [in] */ FunctionID functionId,
		/* [in] */ BOOL fIsSafeToBlock,
		/* [in] */ LPCBYTE pILHeader,
		/* [in] */ ULONG cbILHeader) override 
	{
		return ChainProfiler(chainedProfiler8_,
			[&](ICorProfilerCallback8 *profiler) { return profiler->DynamicMethodJITCompilationStarted(functionId, fIsSafeToBlock, pILHeader, cbILHeader); },
			[]() { return S_OK; });
	}

	virtual HRESULT STDMETHODCALLTYPE DynamicMethodJITCompilationFinished(
		/* [in] */ FunctionID functionId,
		/* [in] */ HRESULT hrStatus,
		/* [in] */ BOOL fIsSafeToBlock) override
	{
		return ChainProfiler(chainedProfiler8_,
			[&](ICorProfilerCallback8 *profiler) { return profiler->DynamicMethodJITCompilationFinished(functionId, hrStatus, fIsSafeToBlock); },
			[]() { return S_OK; });
	}
};