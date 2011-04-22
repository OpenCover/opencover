// CodeCoverage.cpp : Implementation of CCodeCoverage

#include "stdafx.h"
#include "CodeCoverage.h"
#include "NativeCallback.h"
#include "Method.h"

CCodeCoverage* CCodeCoverage::g_pProfiler = NULL;
// CCodeCoverage

/// <summary>Handle <c>ICorProfilerCallback::Initialize</c></summary>
/// <remarks>Initialize the profiling environment and establish connection to the host</remarks>
HRESULT STDMETHODCALLTYPE CCodeCoverage::Initialize( 
    /* [in] */ IUnknown *pICorProfilerInfoUnk) 
{
    OLECHAR szGuid[40]={0};
    int nCount = ::StringFromGUID2(CLSID_CodeCoverage, szGuid, 40);

    ATLTRACE(_T("::Initialize - %s"), W2CT(szGuid));

    m_profilerInfo = pICorProfilerInfoUnk;
    if (m_profilerInfo != NULL) ATLTRACE(_T("    ::Initialize (m_profilerInfo OK)"));
    if (m_profilerInfo == NULL) return E_FAIL;
    m_profilerInfo2 = pICorProfilerInfoUnk;
    if (m_profilerInfo2 != NULL) ATLTRACE(_T("    ::Initialize (m_profilerInfo2 OK)"));
    if (m_profilerInfo2 == NULL) return E_FAIL;
    m_profilerInfo3 = pICorProfilerInfoUnk;
    if (m_profilerInfo3 != NULL) ATLTRACE(_T("    ::Initialize (m_profilerInfo3 OK)"));

    WCHAR pszPortNumber[10];
    ::GetEnvironmentVariableW(L"OpenCover_Port", pszPortNumber, 10);
    int portNumber = _wtoi(pszPortNumber);
    ATLTRACE(_T("->Port Number %d"), portNumber);

    m_host = new ProfilerCommunication(portNumber);

    m_host->Start();

    DWORD dwMask = 0;
    dwMask |= COR_PRF_MONITOR_MODULE_LOADS;			// Controls the ModuleLoad, ModuleUnload, and ModuleAttachedToAssembly callbacks.
    dwMask |= COR_PRF_MONITOR_JIT_COMPILATION;	    // Controls the JITCompilation, JITFunctionPitched, and JITInlining callbacks.
    dwMask |= COR_PRF_DISABLE_INLINING;				// Disables all inlining.
    dwMask |= COR_PRF_DISABLE_OPTIMIZATIONS;		// Disables all code optimizations.

    m_profilerInfo2->SetEventMask(dwMask);

    g_pProfiler = this;

    return S_OK; 
}

/// <summary>Handle <c>ICorProfilerCallback::Shutdown</c></summary>
HRESULT STDMETHODCALLTYPE CCodeCoverage::Shutdown( void) 
{ 
    CComCritSecLock<CComAutoCriticalSection> lock(m_cs);
    ATLTRACE(_T("::Shutdown"));
    g_pProfiler = NULL;
    SendVisitPoints();
    m_host->Stop();
    delete m_host;
    return S_OK; 
}

/// <summary>Handle <c>ICorProfilerCallback::ModuleAttachedToAssembly</c></summary>
/// <remarks>Inform the host that we have a new module attached and that it may be 
/// of interest</remarks>
HRESULT STDMETHODCALLTYPE CCodeCoverage::ModuleAttachedToAssembly( 
    /* [in] */ ModuleID moduleId,
    /* [in] */ AssemblyID assemblyId)
{
    std::wstring moduleName = GetModuleName(moduleId);
    std::wstring assemblyName = GetAssemblyName(assemblyId);
    ATLTRACE(_T("::ModuleAttachedToAssembly(%X => %s, %X => %s)"), 
        moduleId, W2CT(moduleName.c_str()), 
        assemblyId, W2CT(assemblyName.c_str()));
    m_host->TrackAssembly((LPWSTR)moduleName.c_str(), (LPWSTR)assemblyName.c_str());
    return S_OK; 
}

/// <summary>An unmanaged callback that can be called from .NET that has a single I4 parameter</summary>
/// <remarks>
/// void (__fastcall *pt)(long) = &SequencePointVisit ;
/// mdSignature pmsig = GetUnmanagedMethodSignatureToken_I4(moduleId);
/// </remarks>
static void __fastcall SequencePointVisit(ULONG seq)
{
    VisitPoint point;
    point.UniqueId = seq;
    point.VisitType = VisitTypeSequencePoint;
    CCodeCoverage::g_pProfiler->AddVisitPoint(point);
}

void CCodeCoverage::AddVisitPoint(VisitPoint &point)
{
    m_ppVisitPoints[m_VisitPointCount]->UniqueId = point.UniqueId;
    m_ppVisitPoints[m_VisitPointCount]->VisitType = point.VisitType;

    if (++m_VisitPointCount==SEQ_BUFFER_SIZE)
    {
        SendVisitPoints();
    }
}

void CCodeCoverage::SendVisitPoints()
{
    CComCritSecLock<CComAutoCriticalSection> lock(m_cs);
    m_host->SendVisitPoints(m_VisitPointCount, m_ppVisitPoints);
    m_VisitPointCount = 0;
}

/// <summary>Handle <c>ICorProfilerCallback::JITCompilationStarted</c></summary>
/// <remarks>The 'workhorse' </remarks>
HRESULT STDMETHODCALLTYPE CCodeCoverage::JITCompilationStarted( 
        /* [in] */ FunctionID functionId,
        /* [in] */ BOOL fIsSafeToBlock)
{
    std::wstring moduleName;
    mdToken functionToken;
    ModuleID moduleId;

    if (GetTokenAndModule(functionId, functionToken, moduleId, moduleName))
    {
        ATLTRACE(_T("::JITCompilationStarted(%X, %d, %s)"), functionId, functionToken, W2CT(moduleName.c_str()));
        unsigned int points;
        SequencePoint ** ppPoints = NULL;
        
        if (m_host->GetSequencePoints(functionToken, (LPWSTR)moduleName.c_str(), &points, &ppPoints))
        {
            LPCBYTE pMethodHeader = NULL;
            ULONG iMethodSize = 0;
            m_profilerInfo2->GetILFunctionBody(moduleId, functionToken, &pMethodHeader, &iMethodSize);

            IMAGE_COR_ILMETHOD* pMethod = (IMAGE_COR_ILMETHOD*)pMethodHeader;
            COR_ILMETHOD_FAT* fatImage = (COR_ILMETHOD_FAT*)&pMethod->Fat;
            
            void (__fastcall *pt)(ULONG) = &SequencePointVisit ;
            mdSignature pmsig = GetUnmanagedMethodSignatureToken_I4(moduleId);

            Method instumentedMethod(pMethod);
            instumentedMethod.SetMinimumStackSize(2);
            
            //points = 0;
            for (unsigned int i=0; i < points; i++)
            {
                //ATLTRACE(_T("SEQPT %02d IL_%04X"), i, ppPoints[i]->Offset);
                InstructionList instructions;
                instructions.push_back(new Instruction(CEE_LDC_I4, ppPoints[i]->UniqueId));
#if _WIN64
                instructions.push_back(new Instruction(CEE_LDC_I8, (ULONGLONG)pt));
#else
                instructions.push_back(new Instruction(CEE_LDC_I4, (ULONG)pt));
#endif
                instructions.push_back(new Instruction(CEE_CALLI, pmsig));

                instumentedMethod.InsertSequenceInstructionsAtOriginalOffset(ppPoints[i]->Offset, instructions);
            }
          
            instumentedMethod.DumpIL();

            CComPtr<IMethodMalloc> methodMalloc;
            m_profilerInfo2->GetILFunctionBodyAllocator(moduleId, &methodMalloc);
            IMAGE_COR_ILMETHOD* pNewMethod = (IMAGE_COR_ILMETHOD*)methodMalloc->Alloc(instumentedMethod.GetMethodSize());
            instumentedMethod.WriteMethod(pNewMethod);
            m_profilerInfo2->SetILFunctionBody(moduleId, functionToken, (LPCBYTE) pNewMethod);
        }
    }
    
    return S_OK; 
}


        