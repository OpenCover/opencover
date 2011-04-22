// CodeCoverage.h : Declaration of the CCodeCoverage

#pragma once
#include "resource.h"       // main symbols

#ifdef _WIN64
#include "OpenCoverProfiler64_i.h"
#else
#include "OpenCoverProfiler_i.h"
#endif

#include "ProfilerCommunication.h"
#include "ProfileBase.h"

using namespace ATL;

#define COM_FAIL_RETURN(hr, ret) if (!SUCCEEDED(hr)) return (ret)
#define COM_FAIL(hr) if (!SUCCEEDED(hr)) return

#define SEQ_BUFFER_SIZE 8000

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
        m_ppVisitPoints = NULL;
        AllocateCommunicationBuffer();
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
        DeallocateCommunicationBuffer();
        if (m_profilerInfo!=NULL) m_profilerInfo.Release();
        if (m_profilerInfo2!=NULL) m_profilerInfo2.Release();
        if (m_profilerInfo3!=NULL) m_profilerInfo3.Release();
    }
private:
    void AllocateCommunicationBuffer()
    {
        DeallocateCommunicationBuffer();
        m_ppVisitPoints = new VisitPoint*[SEQ_BUFFER_SIZE];
        for (int i=0;i<SEQ_BUFFER_SIZE;i++)
        {
            m_ppVisitPoints[i] = new VisitPoint;
        }
        m_VisitPointCount = 0;
    }

    void DeallocateCommunicationBuffer()
    {
        if (m_ppVisitPoints==NULL) return;
        for (int i=0;i<SEQ_BUFFER_SIZE;i++)
        {
            delete m_ppVisitPoints[i];
        }
        delete [] m_ppVisitPoints;
        m_ppVisitPoints = NULL;
    }

public:
    CComQIPtr<ICorProfilerInfo> m_profilerInfo;
    CComQIPtr<ICorProfilerInfo2> m_profilerInfo2;
    CComQIPtr<ICorProfilerInfo3> m_profilerInfo3;

    std::wstring GetModuleName(ModuleID moduleId);
    std::wstring GetAssemblyName(AssemblyID assemblyId);
    BOOL GetTokenAndModule(FunctionID funcId, mdToken& functionToken, ModuleID& moduleId, std::wstring &moduleName);
    void AddVisitPoint(VisitPoint &point);

private:
    ProfilerCommunication * m_host;
    VisitPoint **m_ppVisitPoints;
    unsigned int m_VisitPointCount;
     CComAutoCriticalSection m_cs;
private:
    mdSignature GetUnmanagedMethodSignatureToken_I4(ModuleID moduleID); 
    void SendVisitPoints();

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
