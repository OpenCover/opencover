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


#include <unordered_map>

#include "ReleaseTrace.h"

using namespace ATL;

#define COM_FAIL_RETURNMSG(hr, msg) if (!SUCCEEDED(hr)) { RELTRACE(msg, hr); return (hr); }

#define COM_FAILMSG(hr, msg) if (!SUCCEEDED(hr)) { RELTRACE(msg, hr); return; }

#define COM_FAIL(hr, ret) if (!SUCCEEDED(hr)) { return (ret); }

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
    }

DECLARE_REGISTRY_RESOURCEID(IDR_CODECOVERAGE)

BEGIN_COM_MAP(CCodeCoverage)
    COM_INTERFACE_ENTRY(ICorProfilerCallback)
    COM_INTERFACE_ENTRY(ICorProfilerCallback2)
    COM_INTERFACE_ENTRY(ICorProfilerCallback3)
END_COM_MAP()

    DECLARE_PROTECT_FINAL_CONSTRUCT()

    HRESULT FinalConstruct()
    {
        return S_OK;
    }

    void FinalRelease()
    {
        if (m_profilerInfo!=NULL) m_profilerInfo.Release();
        if (m_profilerInfo2!=NULL) m_profilerInfo2.Release();
        if (m_profilerInfo3!=NULL) m_profilerInfo3.Release();
    }

public:
    CComQIPtr<ICorProfilerInfo> m_profilerInfo;
    CComQIPtr<ICorProfilerInfo2> m_profilerInfo2;
    CComQIPtr<ICorProfilerInfo3> m_profilerInfo3;

    std::wstring GetModulePath(ModuleID moduleId);
    std::wstring GetModulePath(ModuleID moduleId, AssemblyID *pAssemblyId);
    std::wstring GetAssemblyName(AssemblyID assemblyId);
    BOOL GetTokenAndModule(FunctionID funcId, mdToken& functionToken, ModuleID& moduleId, std::wstring &modulePath, AssemblyID *pAssemblyId);

public:
    ProfilerCommunication m_host;

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
    std::tr1::unordered_map<std::wstring, bool> m_allowModules;
    std::tr1::unordered_map<std::wstring, std::wstring> m_allowModulesAssemblyMap;

    COR_PRF_RUNTIME_TYPE m_runtimeType;
    ASSEMBLYMETADATA m_runtimeVersion;

private:
    mdSignature GetMethodSignatureToken_I4(ModuleID moduleID); 
    HRESULT GetModuleRef(ModuleID moduleId, WCHAR*moduleName, mdModuleRef &mscorlibRef);

    HRESULT GetModuleRef4000(IMetaDataAssemblyEmit *metaDataAssemblyEmit, WCHAR*moduleName, mdModuleRef &mscorlibRef);
    HRESULT GetModuleRef2000(IMetaDataAssemblyEmit *metaDataAssemblyEmit, WCHAR*moduleName, mdModuleRef &mscorlibRef);
    HRESULT GetModuleRef2050(IMetaDataAssemblyEmit *metaDataAssemblyEmit, WCHAR*moduleName, mdModuleRef &mscorlibRef);

private:
    mdMethodDef m_cuckooSafeToken;
    mdMethodDef m_cuckooCriticalToken;
    HRESULT AddCriticalCuckooBody(ModuleID moduleId);
    HRESULT AddSafeCuckooBody(ModuleID moduleId);
    mdMemberRef RegisterSafeCuckooMethod(ModuleID moduleId);

public:
    static CCodeCoverage* g_pProfiler;

public:
    virtual HRESULT STDMETHODCALLTYPE Initialize( 
        /* [in] */ IUnknown *pICorProfilerInfoUnk);
        
    virtual HRESULT STDMETHODCALLTYPE Shutdown( void);

    virtual HRESULT STDMETHODCALLTYPE ModuleAttachedToAssembly( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ AssemblyID assemblyId);
    
     virtual HRESULT STDMETHODCALLTYPE ModuleLoadFinished( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ HRESULT hrStatus);

    virtual HRESULT STDMETHODCALLTYPE JITCompilationStarted( 
        /* [in] */ FunctionID functionId,
        /* [in] */ BOOL fIsSafeToBlock);

};

OBJECT_ENTRY_AUTO(__uuidof(CodeCoverage), CCodeCoverage)
