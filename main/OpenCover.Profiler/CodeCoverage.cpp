//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
// CodeCoverage.cpp : Implementation of CCodeCoverage

#include "stdafx.h"
#include "CodeCoverage.h"
#include "NativeCallback.h"
#include "dllmain.h"

CCodeCoverage* CCodeCoverage::g_pProfiler = NULL;
// CCodeCoverage

/// <summary>Handle <c>ICorProfilerCallback::Initialize</c></summary>
/// <remarks>Initialize the profiling environment and establish connection to the host</remarks>
HRESULT STDMETHODCALLTYPE CCodeCoverage::Initialize(
	/* [in] */ IUnknown *pICorProfilerInfoUnk)
{
	return OpenCoverInitialise(pICorProfilerInfoUnk);
}

HRESULT CCodeCoverage::OpenCoverInitialise(IUnknown *pICorProfilerInfoUnk){
	ATLTRACE(_T("::OpenCoverInitialise"));

    OLECHAR szGuid[40]={0};
    int nCount = ::StringFromGUID2(CLSID_CodeCoverage, szGuid, 40);
    RELTRACE(L"    ::Initialize(...) => CLSID == %s", szGuid);
    //::OutputDebugStringW(szGuid);

    WCHAR szExeName[MAX_PATH];
    GetModuleFileNameW(NULL, szExeName, MAX_PATH);
    RELTRACE(L"    ::Initialize(...) => EXE = %s", szExeName);

    WCHAR szModuleName[MAX_PATH];
    GetModuleFileNameW(_AtlModule.m_hModule, szModuleName, MAX_PATH);
    RELTRACE(L"    ::Initialize(...) => PROFILER = %s", szModuleName);
    //::OutputDebugStringW(szModuleName);

    if (g_pProfiler!=NULL) 
        RELTRACE(_T("Another instance of the profiler is running under this process..."));

    m_profilerInfo = pICorProfilerInfoUnk;
    if (m_profilerInfo != NULL) ATLTRACE(_T("    ::Initialize (m_profilerInfo OK)"));
    if (m_profilerInfo == NULL) return E_FAIL;
    m_profilerInfo2 = pICorProfilerInfoUnk;
    if (m_profilerInfo2 != NULL) ATLTRACE(_T("    ::Initialize (m_profilerInfo2 OK)"));
    if (m_profilerInfo2 == NULL) return E_FAIL;
    m_profilerInfo3 = pICorProfilerInfoUnk;
#ifndef _TOOLSETV71
    m_profilerInfo4 = pICorProfilerInfoUnk;
#endif

    ZeroMemory(&m_runtimeVersion, sizeof(m_runtimeVersion));
    if (m_profilerInfo3 != NULL) 
    {
        ATLTRACE(_T("    ::Initialize (m_profilerInfo3 OK)"));
        
        ZeroMemory(&m_runtimeVersion, sizeof(m_runtimeVersion));
        m_profilerInfo3->GetRuntimeInformation(NULL, &m_runtimeType, 
            &m_runtimeVersion.usMajorVersion, 
            &m_runtimeVersion.usMinorVersion, 
            &m_runtimeVersion.usBuildNumber, 
            &m_runtimeVersion.usRevisionNumber, 0, NULL, NULL); 

        ATLTRACE(_T("    ::Initialize (Runtime %d)"), m_runtimeType);
    }

    TCHAR key[1024] = {0};
    ::GetEnvironmentVariable(_T("OpenCover_Profiler_Key"), key, 1024);
    RELTRACE(_T("    ::Initialize(...) => key = %s"), key);

    TCHAR ns[1024] = {0};
    ::GetEnvironmentVariable(_T("OpenCover_Profiler_Namespace"), ns, 1024);
    ATLTRACE(_T("    ::Initialize(...) => ns = %s"), ns);

    TCHAR instrumentation[1024] = {0};
    ::GetEnvironmentVariable(_T("OpenCover_Profiler_Instrumentation"), instrumentation, 1024);
    ATLTRACE(_T("    ::Initialize(...) => instrumentation = %s"), instrumentation);

    TCHAR threshold[1024] = {0};
    ::GetEnvironmentVariable(_T("OpenCover_Profiler_Threshold"), threshold, 1024);
    m_threshold = _tcstoul(threshold, NULL, 10);
    ATLTRACE(_T("    ::Initialize(...) => threshold = %ul"), m_threshold);

    TCHAR tracebyTest[1024] = {0};
    ::GetEnvironmentVariable(_T("OpenCover_Profiler_TraceByTest"), tracebyTest, 1024);
    m_tracingEnabled = _tcslen(tracebyTest) != 0;
	ATLTRACE(_T("    ::Initialize(...) => tracingEnabled = %s (%s)"), m_tracingEnabled ? _T("true") : _T("false"), tracebyTest);


    m_useOldStyle = (tstring(instrumentation) == _T("oldSchool"));

    if (!m_host.Initialise(key, ns))
    {
        RELTRACE(_T("    ::Initialize => Failed to initialise the profiler communications - profiler will not run for this process."));
        return E_FAIL;
    }

    OpenCoverSupportInitialize(pICorProfilerInfoUnk);

	if (m_chainedProfiler == NULL){
		DWORD dwMask = AppendProfilerEventMask(0); 

		COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->SetEventMask(dwMask),
			_T("    ::Initialize(...) => SetEventMask => 0x%X"));
	}

    if(m_profilerInfo3 != NULL)
    {
        COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo3->SetFunctionIDMapper2(FunctionMapper2, this), 
            _T("    ::Initialize(...) => SetFunctionIDMapper2 => 0x%X"));
    }
    else
    {
        COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->SetFunctionIDMapper(FunctionMapper), 
            _T("    ::Initialize(...) => SetFunctionIDMapper => 0x%X"));
    }

    g_pProfiler = this;

#ifndef _TOOLSETV71
    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->SetEnterLeaveFunctionHooks2(
        _FunctionEnter2, _FunctionLeave2, _FunctionTailcall2), 
        _T("    ::Initialize(...) => SetEnterLeaveFunctionHooks2 => 0x%X"));
#endif
    RELTRACE(_T("::Initialize - Done!"));
    
    return S_OK; 
}

DWORD CCodeCoverage::AppendProfilerEventMask(DWORD currentEventMask)
{
	DWORD dwMask = currentEventMask;
	dwMask |= COR_PRF_MONITOR_MODULE_LOADS;			// Controls the ModuleLoad, ModuleUnload, and ModuleAttachedToAssembly callbacks.
	dwMask |= COR_PRF_MONITOR_JIT_COMPILATION;	    // Controls the JITCompilation, JITFunctionPitched, and JITInlining callbacks.
	dwMask |= COR_PRF_DISABLE_INLINING;				// Disables all inlining.
	dwMask |= COR_PRF_DISABLE_OPTIMIZATIONS;		// Disables all code optimizations.
	dwMask |= COR_PRF_USE_PROFILE_IMAGES;           // Causes the native image search to look for profiler-enhanced images

	if (m_tracingEnabled)
		dwMask |= COR_PRF_MONITOR_ENTERLEAVE;       // Controls the FunctionEnter, FunctionLeave, and FunctionTailcall callbacks.

	if (m_useOldStyle)
		dwMask |= COR_PRF_DISABLE_TRANSPARENCY_CHECKS_UNDER_FULL_TRUST;      // Disables security transparency checks that are normally done during just-in-time (JIT) compilation and class loading for full-trust assemblies. This can make some instrumentation easier to perform.

#ifndef _TOOLSETV71
	if (m_profilerInfo4 != NULL)
	{
		ATLTRACE(_T("    ::Initialize (m_profilerInfo4 OK)"));
		dwMask |= COR_PRF_DISABLE_ALL_NGEN_IMAGES;
	}
#endif

    dwMask |= COR_PRF_MONITOR_THREADS;

	return dwMask;
}

/// <summary>Handle <c>ICorProfilerCallback::Shutdown</c></summary>
HRESULT STDMETHODCALLTYPE CCodeCoverage::Shutdown( void) 
{ 
    RELTRACE(_T("::Shutdown - Starting"));

    if (m_chainedProfiler != NULL)
		m_chainedProfiler->Shutdown();

    m_host.CloseChannel(m_tracingEnabled);

    WCHAR szExeName[MAX_PATH];
    GetModuleFileNameW(NULL, szExeName, MAX_PATH);
    RELTRACE(_T("::Shutdown - Nothing left to do but return S_OK(%s)"), szExeName);
    g_pProfiler = NULL;
    return S_OK; 
}

/// <summary>An unmanaged callback that can be called from .NET that has a single I4 parameter</summary>
/// <remarks>
/// void (__fastcall *pt)(long) = &amp;SequencePointVisit ;
/// mdSignature pmsig = GetUnmanagedMethodSignatureToken_I4(moduleId);
/// </remarks>
static void __fastcall InstrumentPointVisit(ULONG seq)
{
    CCodeCoverage::g_pProfiler->AddVisitPoint(seq);
}

void __fastcall CCodeCoverage::AddVisitPoint(ULONG uniqueId)
{ 
    if (uniqueId == 0) return;
    if (m_threshold != 0)
    {
        ULONG& threshold = m_thresholds.at(uniqueId);
        if (threshold >= m_threshold)
            return;
        threshold++;
    }

    if (m_tracingEnabled){
        m_host.AddVisitPoint(uniqueId);
    }
    else {
        m_host.AddVisitPointToThreadBuffer(uniqueId, IT_VisitPoint);
    }
}

void CCodeCoverage::Resize(ULONG minSize) {
    if (minSize > m_thresholds.size()){
        ULONG newSize = ((minSize / BUFFER_SIZE) + 1) * BUFFER_SIZE;
        m_thresholds.resize(newSize);
    }
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::ModuleLoadFinished(
	/* [in] */ ModuleID moduleId,
	/* [in] */ HRESULT hrStatus)
{
	if (m_chainedProfiler != NULL)
		m_chainedProfiler->ModuleLoadFinished(moduleId, hrStatus);

	return RegisterCuckoos(moduleId);
}

/// <summary>Handle <c>ICorProfilerCallback::ModuleAttachedToAssembly</c></summary>
/// <remarks>Inform the host that we have a new module attached and that it may be 
/// of interest</remarks>
HRESULT STDMETHODCALLTYPE CCodeCoverage::ModuleAttachedToAssembly( 
    /* [in] */ ModuleID moduleId,
    /* [in] */ AssemblyID assemblyId)
{
	if (m_chainedProfiler != NULL)
		m_chainedProfiler->ModuleAttachedToAssembly(moduleId, assemblyId);

    std::wstring modulePath = GetModulePath(moduleId);
    std::wstring assemblyName = GetAssemblyName(assemblyId);
    /*ATLTRACE(_T("::ModuleAttachedToAssembly(...) => (%X => %s, %X => %s)"), 
        moduleId, W2CT(modulePath.c_str()), 
        assemblyId, W2CT(assemblyName.c_str()));*/
    m_allowModules[modulePath] = m_host.TrackAssembly((LPWSTR)modulePath.c_str(), (LPWSTR)assemblyName.c_str());
    m_allowModulesAssemblyMap[modulePath] = assemblyName;

    if (m_allowModules[modulePath]){
        ATLTRACE(_T("::ModuleAttachedToAssembly(...) => (%X => %s, %X => %s)"),
            moduleId, W2CT(modulePath.c_str()),
            assemblyId, W2CT(assemblyName.c_str()));
    }

    return S_OK; 
}

/// <summary>Handle <c>ICorProfilerCallback::JITCompilationStarted</c></summary>
/// <remarks>The 'workhorse' </remarks>
HRESULT STDMETHODCALLTYPE CCodeCoverage::JITCompilationStarted( 
        /* [in] */ FunctionID functionId,
        /* [in] */ BOOL fIsSafeToBlock)
{
    std::wstring modulePath;
    mdToken functionToken;
    ModuleID moduleId;
    AssemblyID assemblyId;

    if (GetTokenAndModule(functionId, functionToken, moduleId, modulePath, &assemblyId))
    {
        if (OpenCoverSupportRequired(assemblyId, functionId))
            OpenCoverSupportCompilation(functionId, functionToken, moduleId, assemblyId, modulePath);

        CuckooSupportCompilation(assemblyId, functionToken, moduleId);

        if (m_allowModules[modulePath])
        {
            ATLTRACE(_T("::JITCompilationStarted(%X, ...) => %d, %X => %s"), functionId, functionToken, moduleId, W2CT(modulePath.c_str()));

            std::vector<SequencePoint> seqPoints;
            std::vector<BranchPoint> brPoints;

            if (m_host.GetPoints(functionToken, (LPWSTR)modulePath.c_str(),
                (LPWSTR)m_allowModulesAssemblyMap[modulePath].c_str(), seqPoints, brPoints))
            {
                if (seqPoints.size() != 0)
                {
                    LPCBYTE pMethodHeader = NULL;
                    ULONG iMethodSize = 0;
                    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->GetILFunctionBody(moduleId, functionToken, &pMethodHeader, &iMethodSize),
                        _T("    ::JITCompilationStarted(...) => GetILFunctionBody => 0x%X"));

                    IMAGE_COR_ILMETHOD* pMethod = (IMAGE_COR_ILMETHOD*)pMethodHeader;

                    Method instumentedMethod(pMethod);
                    instumentedMethod.IncrementStackSize(2);

                    ATLTRACE(_T("::JITCompilationStarted(...) => Instrumenting..."));
                    //seqPoints.clear();
                    //brPoints.clear();

                    // Instrument method
                    InstrumentMethod(moduleId, instumentedMethod, seqPoints, brPoints);

                    //instumentedMethod.DumpIL();

                    CComPtr<IMethodMalloc> methodMalloc;
                    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->GetILFunctionBodyAllocator(moduleId, &methodMalloc),
                        _T("    ::JITCompilationStarted(...) => GetILFunctionBodyAllocator=> 0x%X"));

                    IMAGE_COR_ILMETHOD* pNewMethod = (IMAGE_COR_ILMETHOD*)methodMalloc->Alloc(instumentedMethod.GetMethodSize());
                    instumentedMethod.WriteMethod(pNewMethod);
                    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->SetILFunctionBody(moduleId, functionToken, (LPCBYTE)pNewMethod),
                        _T("    ::JITCompilationStarted(...) => SetILFunctionBody => 0x%X"));

                    ULONG mapSize = instumentedMethod.GetILMapSize();
                    COR_IL_MAP * pMap = (COR_IL_MAP *)CoTaskMemAlloc(mapSize * sizeof(COR_IL_MAP));
                    instumentedMethod.PopulateILMap(mapSize, pMap);
                    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->SetILInstrumentedCodeMap(functionId, TRUE, mapSize, pMap),
                        _T("    ::JITCompilationStarted(...) => SetILInstrumentedCodeMap => 0x%X"));

                    // only do this for .NET4 and above as there are issues with earlier runtimes (Access Violations)
                    if (m_runtimeVersion.usMajorVersion >= 4)
                        CoTaskMemFree(pMap);

                    // resize the threshold array 
                    if (m_threshold != 0)
                    {
                        if (seqPoints.size() > 0)
                            Resize(seqPoints.back().UniqueId + 1);
                        if (brPoints.size() > 0)
                            Resize(brPoints.back().UniqueId + 1);
                    }
                }
            }
        }
    }
    
    if (m_chainedProfiler != NULL)
        return m_chainedProfiler->JITCompilationStarted(functionId, fIsSafeToBlock);

    return S_OK; 
}

ipv CCodeCoverage::GetInstrumentPointVisit(){
	return &InstrumentPointVisit;
}

void CCodeCoverage::InstrumentMethod(ModuleID moduleId, Method& method,  std::vector<SequencePoint> seqPoints, std::vector<BranchPoint> brPoints)
{
    if (m_useOldStyle)
    {
        mdSignature pvsig = GetMethodSignatureToken_I4(moduleId);
		void(__fastcall *pt)(ULONG) = GetInstrumentPointVisit();

        InstructionList instructions;
        if (seqPoints.size() > 0)
            CoverageInstrumentation::InsertFunctionCall(instructions, pvsig, (FPTR)pt, seqPoints[0].UniqueId);
        if (method.IsInstrumented(0, instructions)) return;
  
        CoverageInstrumentation::AddBranchCoverage([pvsig, pt](InstructionList& instructions, ULONG uniqueId)->Instruction*
        {
            return CoverageInstrumentation::InsertFunctionCall(instructions, pvsig, (FPTR)pt, uniqueId);
        }, method, brPoints, seqPoints);

        CoverageInstrumentation::AddSequenceCoverage([pvsig, pt](InstructionList& instructions, ULONG uniqueId)->Instruction*
        {
            return CoverageInstrumentation::InsertFunctionCall(instructions, pvsig, (FPTR)pt, uniqueId);
        }, method, seqPoints);
    }
    else
    {
        mdMethodDef injectedVisitedMethod = RegisterSafeCuckooMethod(moduleId);

        InstructionList instructions;
        if (seqPoints.size() > 0)
            CoverageInstrumentation::InsertInjectedMethod(instructions, injectedVisitedMethod, seqPoints[0].UniqueId);
        if (method.IsInstrumented(0, instructions)) return;
  
        CoverageInstrumentation::AddBranchCoverage([injectedVisitedMethod](InstructionList& instructions, ULONG uniqueId)->Instruction*
        {
            return CoverageInstrumentation::InsertInjectedMethod(instructions, injectedVisitedMethod, uniqueId);
        }, method, brPoints, seqPoints);
        
        CoverageInstrumentation::AddSequenceCoverage([injectedVisitedMethod](InstructionList& instructions, ULONG uniqueId)->Instruction*
        {
            return CoverageInstrumentation::InsertInjectedMethod(instructions, injectedVisitedMethod, uniqueId);
        }, method, seqPoints);
    }
}

HRESULT CCodeCoverage::InstrumentMethodWith(ModuleID moduleId, mdToken functionToken, InstructionList &instructions){

	LPCBYTE pMethodHeader = NULL;
	ULONG iMethodSize = 0;
	COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo->GetILFunctionBody(moduleId, functionToken, &pMethodHeader, &iMethodSize),
		_T("    ::InstrumentMethodWith(...) => GetILFunctionBody => 0x%X"));

	IMAGE_COR_ILMETHOD* pMethod = (IMAGE_COR_ILMETHOD*)pMethodHeader;
	Method instumentedMethod(pMethod);

	instumentedMethod.InsertInstructionsAtOriginalOffset(0, instructions);

	//instumentedMethod.DumpIL();

	// now to write the method back
	CComPtr<IMethodMalloc> methodMalloc;
	COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo->GetILFunctionBodyAllocator(moduleId, &methodMalloc),
		_T("    ::InstrumentMethodWith(...) => GetILFunctionBodyAllocator=> 0x%X"));

	IMAGE_COR_ILMETHOD* pNewMethod = (IMAGE_COR_ILMETHOD*)methodMalloc->Alloc(instumentedMethod.GetMethodSize());
	instumentedMethod.WriteMethod(pNewMethod);
	COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo->SetILFunctionBody(moduleId, functionToken, (LPCBYTE)pNewMethod),
		_T("    ::InstrumentMethodWith(...) => SetILFunctionBody => 0x%X"));

    return S_OK;
}
