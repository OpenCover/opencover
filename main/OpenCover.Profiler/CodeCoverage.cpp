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

CCodeCoverage* CCodeCoverage::g_pProfiler = nullptr;
// CCodeCoverage

using namespace Instrumentation;

/// <summary>Handle <c>ICorProfilerCallback::Initialize</c></summary>
/// <remarks>Initialize the profiling environment and establish connection to the host</remarks>
HRESULT STDMETHODCALLTYPE CCodeCoverage::Initialize(
	/* [in] */ IUnknown *pICorProfilerInfoUnk)
{
	return OpenCoverInitialise(pICorProfilerInfoUnk);
}

void GetVersion(LPTSTR szVersionFile, DWORD *dwVersionHigh, DWORD *dwVersionLow) {
	DWORD  verHandle = NULL;
	UINT   size = 0;
	VS_FIXEDFILEINFO *verInfo = NULL;
	auto  verSize = GetFileVersionInfoSize(szVersionFile, &verHandle);

	if (verSize != NULL)
	{
		auto verData = new BYTE[verSize];

		if (GetFileVersionInfo(szVersionFile, 0, verSize, verData))
		{
			if (VerQueryValue(verData, _T("\\"), (VOID FAR* FAR*)&verInfo, &size))
			{
				if (size >= sizeof(VS_FIXEDFILEINFO))
				{
					if (verInfo->dwSignature == 0xfeef04bd)
					{
						RELTRACE(_T("File Version: %d.%d.%d.%d\n"),
							(verInfo->dwFileVersionMS >> 16) & 0xffff,
							(verInfo->dwFileVersionMS >> 0) & 0xffff,
							(verInfo->dwFileVersionLS >> 16) & 0xffff,
							(verInfo->dwFileVersionLS >> 0) & 0xffff
						);

						*dwVersionHigh = verInfo->dwFileVersionMS;
						*dwVersionLow = verInfo->dwFileVersionLS;
					}
				}
			}
		}
		delete[] verData;
	}
}

#pragma warning (suppress : 6262) // Function uses '17528' bytes of stack; heap not wanted
HRESULT CCodeCoverage::OpenCoverInitialise(IUnknown *pICorProfilerInfoUnk){
	ATLTRACE(_T("::OpenCoverInitialise"));

    OLECHAR szGuid[40]={0};
    (void) ::StringFromGUID2(CLSID_CodeCoverage, szGuid, 40);
    RELTRACE(L"    ::Initialize(...) => CLSID == %s", szGuid);
    //::OutputDebugStringW(szGuid);

	TCHAR szExeName[MAX_PATH];
    GetModuleFileName(nullptr, szExeName, MAX_PATH);
    RELTRACE(_T("    ::Initialize(...) => EXE = %s"), szExeName);

    TCHAR szModuleName[MAX_PATH];
    GetModuleFileName(_AtlModule.m_hModule, szModuleName, MAX_PATH);
    RELTRACE(_T("    ::Initialize(...) => PROFILER = %s"), szModuleName);
    //::OutputDebugStringW(szModuleName);

    if (g_pProfiler!=nullptr) 
        RELTRACE(_T("Another instance of the profiler is running under this process..."));

    m_profilerInfo = pICorProfilerInfoUnk;
    if (m_profilerInfo != nullptr) ATLTRACE(_T("    ::Initialize (m_profilerInfo OK)"));
    if (m_profilerInfo == nullptr) return E_FAIL;
    m_profilerInfo2 = pICorProfilerInfoUnk;
    if (m_profilerInfo2 != nullptr) ATLTRACE(_T("    ::Initialize (m_profilerInfo2 OK)"));
    if (m_profilerInfo2 == nullptr) return E_FAIL;
    m_profilerInfo3 = pICorProfilerInfoUnk;
    m_profilerInfo4 = pICorProfilerInfoUnk;

    ZeroMemory(&m_runtimeVersion, sizeof(m_runtimeVersion));
    if (m_profilerInfo3 != nullptr) 
    {
        ATLTRACE(_T("    ::Initialize (m_profilerInfo3 OK)"));
        
        ZeroMemory(&m_runtimeVersion, sizeof(m_runtimeVersion));
        m_profilerInfo3->GetRuntimeInformation(nullptr, &m_runtimeType, 
            &m_runtimeVersion.usMajorVersion, 
            &m_runtimeVersion.usMinorVersion, 
            &m_runtimeVersion.usBuildNumber, 
            &m_runtimeVersion.usRevisionNumber, 0, nullptr, nullptr); 

        ATLTRACE(_T("    ::Initialize (Runtime %d)"), m_runtimeType);
    }

    TCHAR key[1024] = {0};
    ::GetEnvironmentVariable(_T("OpenCover_Profiler_Key"), key, 1024);
    RELTRACE(_T("    ::Initialize(...) => key = %s"), key);

    TCHAR ns[1024] = {0};
    ::GetEnvironmentVariable(_T("OpenCover_Profiler_Namespace"), ns, 1024);
    ATLTRACE(_T("    ::Initialize(...) => ns = %s"), ns);

	TCHAR instrumentation[1024] = { 0 };
	::GetEnvironmentVariable(_T("OpenCover_Profiler_Instrumentation"), instrumentation, 1024);
	ATLTRACE(_T("    ::Initialize(...) => instrumentation = %s"), instrumentation);

	TCHAR diagnostics[1024] = { 0 };
	::GetEnvironmentVariable(_T("OpenCover_Profiler_Diagnostics"), diagnostics, 1024);
	ATLTRACE(_T("    ::Initialize(...) => Diagnostics = %s"), diagnostics);

    TCHAR threshold[1024] = {0};
    ::GetEnvironmentVariable(_T("OpenCover_Profiler_Threshold"), threshold, 1024);
    m_threshold = _tcstoul(threshold, nullptr, 10);
    ATLTRACE(_T("    ::Initialize(...) => threshold = %ul"), m_threshold);

    TCHAR tracebyTest[1024] = {0};
    ::GetEnvironmentVariable(_T("OpenCover_Profiler_TraceByTest"), tracebyTest, 1024);
    m_tracingEnabled = _tcslen(tracebyTest) != 0;
	ATLTRACE(_T("    ::Initialize(...) => tracingEnabled = %s (%s)"), m_tracingEnabled ? _T("true") : _T("false"), tracebyTest);

    TCHAR safeMode[1024] = { 0 };
    ::GetEnvironmentVariable(_T("OpenCover_Profiler_SafeMode"), safeMode, 1024);
    safe_mode_ = m_tracingEnabled || (_tcslen(safeMode) != 0);
    ATLTRACE(_T("    ::Initialize(...) => safeMode = %s (%s)"), safe_mode_ ? _T("true") : _T("false"), safeMode);

    TCHAR shortwait[1024] = { 0 };
    if (::GetEnvironmentVariable(_T("OpenCover_Profiler_ShortWait"), shortwait, 1024) > 0) {
        _shortwait = _tcstoul(shortwait, nullptr, 10);
        if (_shortwait < 10000) 
            _shortwait = 10000;
        if (_shortwait > 60000)
            _shortwait = 60000;
        ATLTRACE(_T("    ::Initialize(...) => shortwait = %ul"), _shortwait);
    }

	DWORD dwVersionHigh, dwVersionLow;
	GetVersion(szModuleName, &dwVersionHigh, &dwVersionLow);

	m_useOldStyle = (tstring(instrumentation) == _T("oldSchool"));

	enableDiagnostics_ = (tstring(diagnostics) == _T("true"));

    _host = std::make_shared<Communication::ProfilerCommunication>(_shortwait, dwVersionHigh, dwVersionLow);

	int sendVisitPointsTimerInterval = getSendVisitPointsTimerInterval();

	if (!_host->Initialise(key, ns, szExeName,
		safe_mode_, sendVisitPointsTimerInterval))
    {
        RELTRACE(_T("    ::Initialize => Profiler will not run for this process."));
        return E_FAIL;
    }

    OpenCoverSupportInitialize(pICorProfilerInfoUnk);

	if (!IsChainedProfilerHooked()){
		DWORD dwMask = AppendProfilerEventMask(0); 

		COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->SetEventMask(dwMask),
			_T("    ::Initialize(...) => SetEventMask => 0x%X"));
	}

    if(m_profilerInfo3 != nullptr)
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

    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->SetEnterLeaveFunctionHooks2(
        _FunctionEnter2, _FunctionLeave2, _FunctionTailcall2), 
        _T("    ::Initialize(...) => SetEnterLeaveFunctionHooks2 => 0x%X"));

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

	if (m_profilerInfo4 != nullptr)
	{
		ATLTRACE(_T("    ::Initialize (m_profilerInfo4 OK)"));
		dwMask |= COR_PRF_DISABLE_ALL_NGEN_IMAGES;
	}

    dwMask |= COR_PRF_MONITOR_THREADS;

	return dwMask;
}

/// <summary>Handle <c>ICorProfilerCallback::Shutdown</c></summary>
HRESULT STDMETHODCALLTYPE CCodeCoverage::Shutdown( void) 
{ 
    RELTRACE(_T("::Shutdown - Starting"));

	return ChainCall([&]() {return CProfilerBase::Shutdown(); }, 
		[&]()
	{
		if (chained_module_ != nullptr)
			FreeLibrary(chained_module_);

		_host->CloseChannel(safe_mode_);

		WCHAR szExeName[MAX_PATH];
		GetModuleFileNameW(nullptr, szExeName, MAX_PATH);
		RELTRACE(_T("::Shutdown - Nothing left to do but return S_OK(%s)"), W2CT(szExeName));
		g_pProfiler = nullptr;
		return S_OK;
	});
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

    if (safe_mode_) {
        _host->AddVisitPoint(uniqueId);
    }
    else {
        _host->AddVisitPointToThreadBuffer(uniqueId, IT_VisitPoint);
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

	return ChainCall([&]() { return CProfilerBase::ModuleLoadFinished(moduleId, hrStatus); },
		[&]() { return RegisterCuckoos(moduleId); });
}

/// <summary>Handle <c>ICorProfilerCallback::ModuleAttachedToAssembly</c></summary>
/// <remarks>Inform the host that we have a new module attached and that it may be 
/// of interest</remarks>
HRESULT STDMETHODCALLTYPE CCodeCoverage::ModuleAttachedToAssembly( 
    /* [in] */ ModuleID moduleId,
    /* [in] */ AssemblyID assemblyId)
{
	return ChainCall([&]() { return CProfilerBase::ModuleAttachedToAssembly(moduleId, assemblyId); }, 
		[&]() {
		std::wstring modulePath = GetModulePath(moduleId);
		std::wstring assemblyName = GetAssemblyName(assemblyId);
		/*ATLTRACE(_T("::ModuleAttachedToAssembly(...) => (%X => %s, %X => %s)"),
		moduleId, W2CT(modulePath.c_str()),
		assemblyId, W2CT(assemblyName.c_str()));*/
		m_allowModules[modulePath] = _host->TrackAssembly(const_cast<LPWSTR>(modulePath.c_str()), const_cast<LPWSTR>(assemblyName.c_str()));
		m_allowModulesAssemblyMap[modulePath] = assemblyName;

		if (m_allowModules[modulePath]) {
			ATLTRACE(_T("::ModuleAttachedToAssembly(...) => (%X => %s, %X => %s)"),
				moduleId, W2CT(modulePath.c_str()),
				assemblyId, W2CT(assemblyName.c_str()));
		}

		if (MSCORLIB_NAME == assemblyName || DNCORLIB_NAME == assemblyName) {
			cuckoo_module_ = assemblyName;
			RELTRACE(_T("cuckoo nest => %s"), W2CT(cuckoo_module_.c_str()));
		}

		return S_OK;
	});
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
            RELTRACE(_T("::JITCompilationStarted(%X, ...) => %d, %X => %s"), functionId, functionToken, moduleId, W2CT(modulePath.c_str()));

            std::vector<SequencePoint> seqPoints;
            std::vector<BranchPoint> brPoints;

            if (_host->GetPoints(functionToken, const_cast<LPWSTR>(modulePath.c_str()),
                const_cast<LPWSTR>(m_allowModulesAssemblyMap[modulePath].c_str()), seqPoints, brPoints))
            {
                if (seqPoints.size() != 0)
                {
                    IMAGE_COR_ILMETHOD* pMethodHeader = nullptr;
                    ULONG iMethodSize = 0;
                    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->GetILFunctionBody(moduleId, functionToken, (LPCBYTE*)&pMethodHeader, &iMethodSize),
                        _T("    ::JITCompilationStarted(...) => GetILFunctionBody => 0x%X"));

                    Instrumentation::Method instumentedMethod(pMethodHeader);
                    instumentedMethod.IncrementStackSize(2);

                    ATLTRACE(_T("::JITCompilationStarted(...) => Instrumenting..."));

                    // Instrument method
					instumentedMethod.DumpIL(enableDiagnostics_);
					if (enableDiagnostics_)
					{
						RELTRACE(_T("Sequence points:"));
						for (auto seq_point : seqPoints)
						{
							RELTRACE(_T("IL_%04X %ld"), seq_point.Offset, seq_point.UniqueId);
						}

						RELTRACE(_T("Branch points:"));
						for (auto br_point : brPoints)
						{
							RELTRACE(_T("IL_%04X (%ld) %ld"), br_point.Offset, br_point.Path, br_point.UniqueId);
						}
					}
					InstrumentMethod(moduleId, instumentedMethod, seqPoints, brPoints);
                    instumentedMethod.DumpIL(enableDiagnostics_);

                    CComPtr<IMethodMalloc> methodMalloc;
                    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->GetILFunctionBodyAllocator(moduleId, &methodMalloc),
                        _T("    ::JITCompilationStarted(...) => GetILFunctionBodyAllocator=> 0x%X"));

                    auto pNewMethod = static_cast<IMAGE_COR_ILMETHOD*>(methodMalloc->Alloc(instumentedMethod.GetMethodSize()));
                    instumentedMethod.WriteMethod(pNewMethod);
                    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo2->SetILFunctionBody(moduleId, functionToken, (LPCBYTE)pNewMethod),
                        _T("    ::JITCompilationStarted(...) => SetILFunctionBody => 0x%X"));

                    ULONG mapSize = instumentedMethod.GetILMapSize();
                    COR_IL_MAP * pMap = static_cast<COR_IL_MAP *>(CoTaskMemAlloc(mapSize * sizeof(COR_IL_MAP)));
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
    
    return CProfilerBase::JITCompilationStarted(functionId, fIsSafeToBlock); 
}

ipv CCodeCoverage::GetInstrumentPointVisit(){
	return &InstrumentPointVisit;
}

void CCodeCoverage::InstrumentMethod(ModuleID moduleId, Instrumentation::Method& method,  std::vector<SequencePoint> seqPoints, std::vector<BranchPoint> brPoints)
{
    if (m_useOldStyle)
    {
        auto pvsig = GetMethodSignatureToken_I4(moduleId);
        auto pt = GetInstrumentPointVisit();

        InstructionList instructions;
        if (seqPoints.size() > 0)
            CoverageInstrumentation::InsertFunctionCall(instructions, pvsig, (FPTR)pt, seqPoints[0].UniqueId);
        if (method.IsInstrumented(0, instructions)) return;
  
        CoverageInstrumentation::AddBranchCoverage([pvsig, pt](InstructionList& brinstructions, ULONG uniqueId)->Instruction*
        {
            return CoverageInstrumentation::InsertFunctionCall(brinstructions, pvsig, (FPTR)pt, uniqueId);
        }, method, brPoints, seqPoints);

        CoverageInstrumentation::AddSequenceCoverage([pvsig, pt](InstructionList& seqinstructions, ULONG uniqueId)->Instruction*
        {
            return CoverageInstrumentation::InsertFunctionCall(seqinstructions, pvsig, (FPTR)pt, uniqueId);
        }, method, seqPoints);
    }
    else
    {
        auto injectedVisitedMethod = RegisterSafeCuckooMethod(moduleId, cuckoo_module_.c_str());

        InstructionList instructions;
        if (seqPoints.size() > 0)
            CoverageInstrumentation::InsertInjectedMethod(instructions, injectedVisitedMethod, seqPoints[0].UniqueId);
        if (method.IsInstrumented(0, instructions)) return;
  
        CoverageInstrumentation::AddBranchCoverage([injectedVisitedMethod](InstructionList& brinstructions, ULONG uniqueId)->Instruction*
        {
            return CoverageInstrumentation::InsertInjectedMethod(brinstructions, injectedVisitedMethod, uniqueId);
        }, method, brPoints, seqPoints);
        
        CoverageInstrumentation::AddSequenceCoverage([injectedVisitedMethod](InstructionList& seqinstructions, ULONG uniqueId)->Instruction*
        {
            return CoverageInstrumentation::InsertInjectedMethod(seqinstructions, injectedVisitedMethod, uniqueId);
        }, method, seqPoints);
    }
}

HRESULT CCodeCoverage::InstrumentMethodWith(ModuleID moduleId, mdToken functionToken, InstructionList &instructions){

    IMAGE_COR_ILMETHOD* pMethodHeader = nullptr;
	ULONG iMethodSize = 0;
    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo->GetILFunctionBody(moduleId, functionToken, (LPCBYTE*)&pMethodHeader, &iMethodSize),
		_T("    ::InstrumentMethodWith(...) => GetILFunctionBody => 0x%X"));

	Instrumentation::Method instumentedMethod(pMethodHeader);

	//instumentedMethod.DumpIL();

	instumentedMethod.InsertInstructionsAtOriginalOffset(0, instructions);

	//instumentedMethod.DumpIL();

	// now to write the method back
	CComPtr<IMethodMalloc> methodMalloc;
	COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo->GetILFunctionBodyAllocator(moduleId, &methodMalloc),
		_T("    ::InstrumentMethodWith(...) => GetILFunctionBodyAllocator=> 0x%X"));

	IMAGE_COR_ILMETHOD* pNewMethod = static_cast<IMAGE_COR_ILMETHOD*>(methodMalloc->Alloc(instumentedMethod.GetMethodSize()));
	instumentedMethod.WriteMethod(pNewMethod);
	COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo->SetILFunctionBody(moduleId, functionToken, (LPCBYTE)pNewMethod),
		_T("    ::InstrumentMethodWith(...) => SetILFunctionBody => 0x%X"));

    return S_OK;
}

int CCodeCoverage::getSendVisitPointsTimerInterval()
{
	int timerIntervalValue = 0;
	TCHAR timerIntervalString[1024] = { 0 };
	if (::GetEnvironmentVariable(_T("OpenCover_SendVisitPointsTimerInterval"), timerIntervalString, 1024) > 0) {
		timerIntervalValue = _tcstoul(timerIntervalString, nullptr, 10);
		ATLTRACE(_T("    ::Initialize(...) => sendVisitPointsTimerInterval = %d"), timerIntervalValue);
	}
	return timerIntervalValue;
}
