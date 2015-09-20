//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
// CodeCoverage.h : Declaration of the CCodeCoverage

#pragma once
#include "resource.h"       // main symbols

#include "OpenCoverProfiler_i.h"

#include "ProfilerCommunication.h"
#include "ProfileBase.h"
#include "ProfilerInfo.h"

#include <unordered_map>

#include "ReleaseTrace.h"

using namespace ATL;

#define COM_FAIL_MSG_RETURN_ERROR(hr, msg) if (!SUCCEEDED(hr)) { RELTRACE(msg, hr); return (hr); }

//#define COM_FAILMSG(hr, msg) if (!SUCCEEDED(hr)) { RELTRACE(msg, hr); return; }

#define COM_FAIL_MSG_RETURN_OTHER(hr, ret, msg) if (!SUCCEEDED(hr)) { RELTRACE(msg, hr); return (ret); }

#define MSCORLIB_NAME L"mscorlib"

#include "CoverageInstrumentation.h"

typedef void(__fastcall *ipv)(ULONG);

#define BUFFER_SIZE 16384

// CCodeCoverage

/// <summary>The main profiler COM object</summary>
class ATL_NO_VTABLE CCodeCoverage :
    public CComObjectRootEx<CComMultiThreadModel>,
    public CComCoClass<CCodeCoverage, &CLSID_CodeCoverage>,
    public CProfilerBase
{
public:
    CCodeCoverage() 
    {
        m_runtimeType = COR_PRF_DESKTOP_CLR;
        m_useOldStyle = false;
		m_threshold = 0U;
        m_tracingEnabled = false;
        m_cuckooCriticalToken = 0;
        m_cuckooSafeToken = 0;
        m_infoHook = nullptr;
    }

DECLARE_REGISTRY_RESOURCEID(IDR_CODECOVERAGE)

BEGIN_COM_MAP(CCodeCoverage)
    COM_INTERFACE_ENTRY(ICorProfilerCallback)
    COM_INTERFACE_ENTRY(ICorProfilerCallback2)
    COM_INTERFACE_ENTRY(ICorProfilerCallback3)
#ifndef _TOOLSETV71
    COM_INTERFACE_ENTRY(ICorProfilerCallback4)
    COM_INTERFACE_ENTRY(ICorProfilerCallback5)
#endif
END_COM_MAP()

    DECLARE_PROTECT_FINAL_CONSTRUCT()

    HRESULT FinalConstruct()
    {
        return S_OK;
    }

    void FinalRelease()
    {
        if (m_profilerInfo != nullptr) m_profilerInfo.Release();
        if (m_profilerInfo2 != nullptr) m_profilerInfo2.Release();
        if (m_profilerInfo3 != nullptr) m_profilerInfo3.Release();
#ifndef _TOOLSETV71
        if (m_profilerInfo4 != nullptr) m_profilerInfo4.Release();
#endif
	}

public:
    CComQIPtr<ICorProfilerInfo> m_profilerInfo;
    CComQIPtr<ICorProfilerInfo2> m_profilerInfo2;
    CComQIPtr<ICorProfilerInfo3> m_profilerInfo3;
#ifndef _TOOLSETV71
    CComQIPtr<ICorProfilerInfo4> m_profilerInfo4;
#endif

    std::wstring GetModulePath(ModuleID moduleId);
    std::wstring GetModulePath(ModuleID moduleId, AssemblyID *pAssemblyId);
    std::wstring GetAssemblyName(AssemblyID assemblyId);
    BOOL GetTokenAndModule(FunctionID funcId, mdToken& functionToken, ModuleID& moduleId, std::wstring &modulePath, AssemblyID *pAssemblyId);
	std::wstring GetTypeAndMethodName(FunctionID functionId);
    void __fastcall AddVisitPoint(ULONG uniqueId);

private:
    ProfilerCommunication m_host;
	HRESULT OpenCoverInitialise(IUnknown *pICorProfilerInfoUnk);
	DWORD AppendProfilerEventMask(DWORD currentEventMask);

	ipv static GetInstrumentPointVisit();

private:
    static UINT_PTR _stdcall FunctionMapper2(FunctionID functionId, void* clientData, BOOL* pbHookFunction);
    static UINT_PTR _stdcall FunctionMapper(FunctionID functionId, BOOL* pbHookFunction);

public:
    void FunctionEnter2(
    /*[in]*/FunctionID                          funcID, 
    /*[in]*/UINT_PTR                            clientData, 
    /*[in]*/COR_PRF_FRAME_INFO                  func, 
    /*[in]*/COR_PRF_FUNCTION_ARGUMENT_INFO      *argumentInfo);

    void FunctionLeave2(
    /*[in]*/FunctionID                          funcID, 
    /*[in]*/UINT_PTR                            clientData, 
    /*[in]*/COR_PRF_FRAME_INFO                  func, 
    /*[in]*/COR_PRF_FUNCTION_ARGUMENT_RANGE     *retvalRange);

    void FunctionTailcall2(
    /*[in]*/FunctionID                          funcID, 
    /*[in]*/UINT_PTR                            clientData, 
    /*[in]*/COR_PRF_FRAME_INFO                  func);

private:
    std::unordered_map<std::wstring, bool> m_allowModules;
    std::unordered_map<std::wstring, std::wstring> m_allowModulesAssemblyMap;

    COR_PRF_RUNTIME_TYPE m_runtimeType;
    ASSEMBLYMETADATA m_runtimeVersion;

    bool m_useOldStyle;
	ULONG m_threshold;
	bool m_tracingEnabled;

private:
    std::vector<ULONG> m_thresholds;
    void Resize(ULONG minSize);



private:
    mdSignature GetMethodSignatureToken_I4(ModuleID moduleID); 
    HRESULT GetModuleRef(ModuleID moduleId, WCHAR*moduleName, mdModuleRef &mscorlibRef);

    HRESULT GetModuleRef4000(IMetaDataAssemblyEmit *metaDataAssemblyEmit, WCHAR*moduleName, mdModuleRef &mscorlibRef);
    HRESULT GetModuleRef2000(IMetaDataAssemblyEmit *metaDataAssemblyEmit, WCHAR*moduleName, mdModuleRef &mscorlibRef);
    HRESULT GetModuleRef2050(IMetaDataAssemblyEmit *metaDataAssemblyEmit, WCHAR*moduleName, mdModuleRef &mscorlibRef);

private:
	HRESULT CCodeCoverage::RegisterCuckoos(ModuleID moduleId);
    mdMethodDef m_cuckooSafeToken;
    mdMethodDef m_cuckooCriticalToken;
    HRESULT AddCriticalCuckooBody(ModuleID moduleId);
    HRESULT AddSafeCuckooBody(ModuleID moduleId);
    mdMemberRef RegisterSafeCuckooMethod(ModuleID moduleId);
    void InstrumentMethod(ModuleID moduleId, Method& method,  std::vector<SequencePoint> seqPoints, std::vector<BranchPoint> brPoints);
	HRESULT CuckooSupportCompilation(
		AssemblyID assemblyId,
		mdToken functionToken,
		ModuleID moduleId);

private:
    CComPtr<ICorProfilerCallback4> m_chainedProfiler;
    CComObject<CProfilerInfo> *m_infoHook;

    HRESULT OpenCoverSupportInitialize(IUnknown *pICorProfilerInfoUnk);
    HRESULT GetOpenCoverSupportRef(ModuleID moduleId, mdModuleRef &supportRef);
    mdMethodDef CreatePInvokeHook(ModuleID moduleId);
    HRESULT OpenCoverSupportCompilation(FunctionID functionId, mdToken functionToken, ModuleID moduleId, AssemblyID assemblyId, std::wstring &modulePath);
	mdMethodDef Get_CurrentDomainMethod(ModuleID moduleID);
	HRESULT InstrumentMethodWith(ModuleID moduleId, mdToken functionToken, InstructionList &instructions);

    bool OpenCoverSupportRequired(AssemblyID assemblyId, FunctionID functionId);

    mdMethodDef GetFakesHelperMethodRef(TCHAR* methodName, ModuleID moduleId);
    void InstrumentTestPlatformUtilities(FunctionID functionId, mdToken functionToken, ModuleID moduleId, AssemblyID assemblyId);
    void InstrumentTestPlatformTestExecutor(FunctionID functionId, mdToken functionToken, ModuleID moduleId, AssemblyID assemblyId);

    mdMethodDef GetUITestingHelperMethodRef(TCHAR* methodName, ModuleID moduleId);
    void InstrumentTestToolsUITesting(FunctionID functionId, mdToken functionToken, ModuleID moduleId, AssemblyID assemblyId);

	friend class CProfilerInfo;

public:
    static CCodeCoverage* g_pProfiler;

public:
    virtual HRESULT STDMETHODCALLTYPE Initialize( 
        /* [in] */ IUnknown *pICorProfilerInfoUnk) override;
        
    virtual HRESULT STDMETHODCALLTYPE Shutdown( void) override;

    virtual HRESULT STDMETHODCALLTYPE ModuleAttachedToAssembly( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ AssemblyID assemblyId) override;
    
     virtual HRESULT STDMETHODCALLTYPE ModuleLoadFinished( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ HRESULT hrStatus) override;

    virtual HRESULT STDMETHODCALLTYPE JITCompilationStarted( 
        /* [in] */ FunctionID functionId,
        /* [in] */ BOOL fIsSafeToBlock) override;

public:
	// COR_PRF_MONITOR_APPDOMAIN_LOADS
	virtual HRESULT STDMETHODCALLTYPE AppDomainCreationStarted(
		/* [in] */ AppDomainID appDomainId) override
	{
		if (m_chainedProfiler != nullptr)
			return m_chainedProfiler->AppDomainCreationStarted(appDomainId);
		return S_OK;
	}

	virtual HRESULT STDMETHODCALLTYPE AppDomainCreationFinished(
		/* [in] */ AppDomainID appDomainId,
		/* [in] */ HRESULT hrStatus) override
	{
		if (m_chainedProfiler != nullptr)
			return m_chainedProfiler->AppDomainCreationFinished(appDomainId, hrStatus);
		return S_OK;
	}

	virtual HRESULT STDMETHODCALLTYPE AppDomainShutdownStarted(
		/* [in] */ AppDomainID appDomainId) override
	{
		if (m_chainedProfiler != nullptr)
			return m_chainedProfiler->AppDomainShutdownStarted(appDomainId);
		return S_OK;
	}

	virtual HRESULT STDMETHODCALLTYPE AppDomainShutdownFinished(
		/* [in] */ AppDomainID appDomainId,
		/* [in] */ HRESULT hrStatus) override
	{
		if (m_chainedProfiler != nullptr)
			return m_chainedProfiler->AppDomainShutdownFinished(appDomainId, hrStatus);
		return S_OK;
	}

	// COR_PRF_MONITOR_ASSEMBLY_LOADS
	virtual HRESULT STDMETHODCALLTYPE AssemblyLoadStarted(
		/* [in] */ AssemblyID assemblyId) override
	{
		if (m_chainedProfiler != nullptr)
			return m_chainedProfiler->AssemblyLoadStarted(assemblyId);
		return S_OK;
	}

	virtual HRESULT STDMETHODCALLTYPE AssemblyLoadFinished(
		/* [in] */ AssemblyID assemblyId,
		/* [in] */ HRESULT hrStatus) override
	{
		if (m_chainedProfiler != nullptr)
			return m_chainedProfiler->AssemblyLoadFinished(assemblyId, hrStatus);
		return S_OK;
	}

	virtual HRESULT STDMETHODCALLTYPE AssemblyUnloadStarted(
		/* [in] */ AssemblyID assemblyId) override
	{
		if (m_chainedProfiler != nullptr)
			return m_chainedProfiler->AssemblyUnloadStarted(assemblyId);
		return S_OK;
	}

	virtual HRESULT STDMETHODCALLTYPE AssemblyUnloadFinished(
		/* [in] */ AssemblyID assemblyId,
		/* [in] */ HRESULT hrStatus) override
	{
		if (m_chainedProfiler != nullptr)
			return m_chainedProfiler->AssemblyUnloadFinished(assemblyId, hrStatus);
		return S_OK;
	}

	// COR_PRF_MONITOR_MODULE_LOADS
	virtual HRESULT STDMETHODCALLTYPE ModuleLoadStarted(
		/* [in] */ ModuleID moduleId) override
	{
		if (m_chainedProfiler != nullptr)
			return m_chainedProfiler->ModuleLoadStarted(moduleId);
		return S_OK;
	}

	//virtual HRESULT STDMETHODCALLTYPE ModuleLoadFinished(
	//	/* [in] */ ModuleID moduleId,
	//	/* [in] */ HRESULT hrStatus)
	//{
	//	if (m_chainedProfiler != NULL)
	//		return m_chainedProfiler->ModuleLoadFinished(moduleId, hrStatus);
	//	return S_OK;
	//}

	virtual HRESULT STDMETHODCALLTYPE ModuleUnloadStarted(
		/* [in] */ ModuleID moduleId) override
	{
		if (m_chainedProfiler != nullptr)
			return m_chainedProfiler->ModuleUnloadStarted(moduleId);
		return S_OK;
	}

	virtual HRESULT STDMETHODCALLTYPE ModuleUnloadFinished(
		/* [in] */ ModuleID moduleId,
		/* [in] */ HRESULT hrStatus) override
	{
		if (m_chainedProfiler != nullptr)
			return m_chainedProfiler->ModuleUnloadFinished(moduleId, hrStatus);
		return S_OK;
	}

	//virtual HRESULT STDMETHODCALLTYPE ModuleAttachedToAssembly(
	//	/* [in] */ ModuleID moduleId,
	//	/* [in] */ AssemblyID assemblyId)
	//{
	//	return S_OK;
	//}

	//COR_PRF_MONITOR_JIT_COMPILATION
	//virtual HRESULT STDMETHODCALLTYPE JITCompilationStarted(
	//	/* [in] */ FunctionID functionId,
	//	/* [in] */ BOOL fIsSafeToBlock)
	//{
	//	return S_OK;
	//}

	virtual HRESULT STDMETHODCALLTYPE JITCompilationFinished(
		/* [in] */ FunctionID functionId,
		/* [in] */ HRESULT hrStatus,
		/* [in] */ BOOL fIsSafeToBlock) override
	{
		if (m_chainedProfiler != nullptr)
			return m_chainedProfiler->JITCompilationFinished(functionId, hrStatus, fIsSafeToBlock);
		return S_OK;
	}

	virtual HRESULT STDMETHODCALLTYPE JITFunctionPitched(
		/* [in] */ FunctionID functionId) override
	{
		if (m_chainedProfiler != nullptr)
			return m_chainedProfiler->JITFunctionPitched(functionId);
		return S_OK;
	}

	virtual HRESULT STDMETHODCALLTYPE JITInlining(
		/* [in] */ FunctionID callerId,
		/* [in] */ FunctionID calleeId,
		/* [out] */ BOOL *pfShouldInline) override
	{
		if (m_chainedProfiler != nullptr)
			return m_chainedProfiler->JITInlining(callerId, calleeId, pfShouldInline);
		return S_OK;
	}

	// COR_PRF_MONITOR_THREADS
    virtual HRESULT STDMETHODCALLTYPE ThreadCreated(
        /* [in] */ ThreadID threadId) override;

    virtual HRESULT STDMETHODCALLTYPE ThreadDestroyed(
        /* [in] */ ThreadID threadId) override;

    virtual HRESULT STDMETHODCALLTYPE ThreadAssignedToOSThread(
        /* [in] */ ThreadID managedThreadId,
        /* [in] */ DWORD osThreadId) override;

    virtual HRESULT STDMETHODCALLTYPE ThreadNameChanged(
        /* [in] */ ThreadID threadId,
        /* [in] */ ULONG cchName,
        /* [in] */
        __in_ecount_opt(cchName)  WCHAR name[]) override;
};

OBJECT_ENTRY_AUTO(__uuidof(CodeCoverage), CCodeCoverage)
