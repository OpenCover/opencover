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

using namespace ATL;

#define COM_FAIL_RETURN(hr, ret) if (!SUCCEEDED(hr)) return (ret)
#define COM_FAIL(hr) if (!SUCCEEDED(hr)) return
#define MAGIC_NUMBER  0x9e3779b9

template<>
struct std::hash<std::pair<std::wstring, ULONG32> > {
private:
   const std::hash<std::wstring> _hash;
   const std::hash<ULONG32> _hash2;
public:
   std::hash<std::pair<std::wstring, ULONG32>>() {}
   size_t operator()(const std::pair<std::wstring, ULONG32> &p) const {
      size_t seed = _hash(p.first);
      return _hash2(p.second) + MAGIC_NUMBER + (seed<<6) + (seed>>2);
   }
};

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
    std::wstring GetAssemblyName(AssemblyID assemblyId);
    BOOL GetTokenAndModule(FunctionID funcId, mdToken& functionToken, ModuleID& moduleId, std::wstring &modulePath);

public:
    ProfilerCommunication m_host;

private:
    std::tr1::unordered_map<std::wstring, bool> m_allowModules;
    std::tr1::unordered_map<std::wstring, std::wstring> m_allowModulesAssemblyMap;
    BOOL m_isV4;

    std::tr1::unordered_map<std::pair<std::wstring, ULONG32>, bool> m_jitdMethods;

private:
    mdSignature GetUnmanagedMethodSignatureToken_I4(ModuleID moduleID); 

public:
    static CCodeCoverage* g_pProfiler;

public:
    virtual HRESULT STDMETHODCALLTYPE Initialize( 
        /* [in] */ IUnknown *pICorProfilerInfoUnk);
        
    virtual HRESULT STDMETHODCALLTYPE Shutdown( void);

    virtual HRESULT STDMETHODCALLTYPE CCodeCoverage::ModuleAttachedToAssembly( 
        /* [in] */ ModuleID moduleId,
        /* [in] */ AssemblyID assemblyId);
    
    virtual HRESULT STDMETHODCALLTYPE CCodeCoverage::JITCompilationStarted( 
        /* [in] */ FunctionID functionId,
        /* [in] */ BOOL fIsSafeToBlock);

};

OBJECT_ENTRY_AUTO(__uuidof(CodeCoverage), CCodeCoverage)
