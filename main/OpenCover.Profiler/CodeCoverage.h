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

#include <memory>

#define COM_FAIL_MSG_RETURN_ERROR(hr, msg) if (!SUCCEEDED(hr)) { RELTRACE(msg, hr); return (hr); }

#define COM_FAIL_MSG_RETURN_OTHER(hr, ret, msg) if (!SUCCEEDED(hr)) { RELTRACE(msg, hr); return (ret); }

#define MSCORLIB_NAME L"mscorlib"
#define DNCORLIB_NAME L"System.Private.CoreLib"

#include "CoverageInstrumentation.h"

typedef void(__fastcall *ipv)(ULONG);

#define BUFFER_SIZE 16384

// CCodeCoverage

/// <summary>The main profiler COM object</summary>
class ATL_NO_VTABLE CCodeCoverage :
    public CComObjectRootEx<CComMultiThreadModel>,
    public CComCoClass<CCodeCoverage, &CLSID_CodeCoverage>,
    public CProfilerBase, CProfilerHook
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
        _shortwait = 10000;
        chained_module_ = nullptr;
    }

DECLARE_REGISTRY_RESOURCEID(IDR_CODECOVERAGE)

BEGIN_COM_MAP(CCodeCoverage)
    COM_INTERFACE_ENTRY(ICorProfilerCallback)
    COM_INTERFACE_ENTRY(ICorProfilerCallback2)
    COM_INTERFACE_ENTRY(ICorProfilerCallback3)
    COM_INTERFACE_ENTRY(ICorProfilerCallback4)
    COM_INTERFACE_ENTRY(ICorProfilerCallback5)
    COM_INTERFACE_ENTRY(ICorProfilerCallback6)
	COM_INTERFACE_ENTRY(ICorProfilerCallback7)
	COM_INTERFACE_ENTRY(ICorProfilerCallback8)
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
        if (m_profilerInfo4 != nullptr) m_profilerInfo4.Release();
	}

public:
    CComQIPtr<ICorProfilerInfo> m_profilerInfo;
    CComQIPtr<ICorProfilerInfo2> m_profilerInfo2;
    CComQIPtr<ICorProfilerInfo3> m_profilerInfo3;
    CComQIPtr<ICorProfilerInfo4> m_profilerInfo4;

    std::wstring GetModulePath(ModuleID moduleId);
    std::wstring GetModulePath(ModuleID moduleId, AssemblyID *pAssemblyId);
    std::wstring GetAssemblyName(AssemblyID assemblyId);
    BOOL GetTokenAndModule(FunctionID funcId, mdToken& functionToken, ModuleID& moduleId, std::wstring &modulePath, AssemblyID *pAssemblyId);
	std::wstring GetTypeAndMethodName(FunctionID functionId);
    void __fastcall AddVisitPoint(ULONG uniqueId);

private:
	DWORD AppendProfilerEventMask(DWORD currentEventMask) override;

private:
    std::shared_ptr<Communication::ProfilerCommunication> _host;
    ULONG _shortwait;
	HRESULT OpenCoverInitialise(IUnknown *pICorProfilerInfoUnk);

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
    bool safe_mode_;
	bool enableDiagnostics_;

private:
    std::vector<ULONG> m_thresholds;
    void Resize(ULONG minSize);



private:
    mdSignature GetMethodSignatureToken_I4(ModuleID moduleID); 
    HRESULT GetModuleRef(ModuleID moduleId, const WCHAR*moduleName, mdModuleRef &mscorlibRef);

    HRESULT GetModuleRef4000(IMetaDataAssemblyEmit *metaDataAssemblyEmit, const WCHAR* moduleName, mdModuleRef &mscorlibRef);
    HRESULT GetModuleRef2000(IMetaDataAssemblyEmit *metaDataAssemblyEmit, const WCHAR* moduleName, mdModuleRef &mscorlibRef);
    HRESULT GetModuleRef2050(IMetaDataAssemblyEmit *metaDataAssemblyEmit, const WCHAR* moduleName, mdModuleRef &mscorlibRef);

private:
	HRESULT CCodeCoverage::RegisterCuckoos(ModuleID moduleId);
    mdMethodDef m_cuckooSafeToken;
    mdMethodDef m_cuckooCriticalToken;
    HRESULT AddCriticalCuckooBody(ModuleID moduleId);
    HRESULT AddSafeCuckooBody(ModuleID moduleId);
    mdMemberRef RegisterSafeCuckooMethod(ModuleID moduleId, const WCHAR* moduleName);
    void InstrumentMethod(ModuleID moduleId, Instrumentation::Method& method,  std::vector<SequencePoint> seqPoints, std::vector<BranchPoint> brPoints);
	HRESULT CuckooSupportCompilation(
		AssemblyID assemblyId,
		mdToken functionToken,
		ModuleID moduleId);
	std::wstring cuckoo_module_;

private:
	HMODULE chained_module_;

    CComObject<CProfilerInfo> *m_infoHook;

    HRESULT OpenCoverSupportInitialize(IUnknown *pICorProfilerInfoUnk);
    HRESULT GetOpenCoverSupportRef(ModuleID moduleId, mdModuleRef &supportRef);
    mdMethodDef CreatePInvokeHook(ModuleID moduleId);
    HRESULT OpenCoverSupportCompilation(FunctionID functionId, mdToken functionToken, ModuleID moduleId, AssemblyID assemblyId, std::wstring &modulePath);
	mdMethodDef Get_CurrentDomainMethod(ModuleID moduleID);
	HRESULT InstrumentMethodWith(ModuleID moduleId, mdToken functionToken, Instrumentation::InstructionList &instructions);

    bool OpenCoverSupportRequired(AssemblyID assemblyId, FunctionID functionId);

    mdMethodDef GetFakesHelperMethodRef(TCHAR* methodName, ModuleID moduleId);
    void InstrumentTestPlatformUtilities(FunctionID functionId, mdToken functionToken, ModuleID moduleId, AssemblyID assemblyId);
    void InstrumentTestPlatformTestExecutor(FunctionID functionId, mdToken functionToken, ModuleID moduleId, AssemblyID assemblyId);

    mdMethodDef GetUITestingHelperMethodRef(TCHAR* methodName, ModuleID moduleId);
    void InstrumentTestToolsUITesting(FunctionID functionId, mdToken functionToken, ModuleID moduleId, AssemblyID assemblyId);

	int getSendVisitPointsTimerInterval();

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

    virtual HRESULT STDMETHODCALLTYPE ThreadDestroyed(
        /* [in] */ ThreadID threadId) override;

    virtual HRESULT STDMETHODCALLTYPE ThreadAssignedToOSThread(
        /* [in] */ ThreadID managedThreadId,
        /* [in] */ DWORD osThreadId) override;
};

OBJECT_ENTRY_AUTO(__uuidof(CodeCoverage), CCodeCoverage)
