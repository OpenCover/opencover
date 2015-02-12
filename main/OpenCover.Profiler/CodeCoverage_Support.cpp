#include "stdafx.h"
#include "CodeCoverage.h"

#include "dllmain.h"

#include "Method.h"
#include "ProfilerInfo.h"

#define TESTPLATFORM_UTILITIES_ASSEMBLY L"Microsoft.VisualStudio.TestPlatform.Utilities"
#define DEFAULTTESTEXECUTOR_LAUNCHPROCESS L"Microsoft.VisualStudio.TestPlatform.Utilities.DefaultTestExecutorLauncher::LaunchProcess"
#define DEFAULTTESTEXECUTOR_CTOR L"Microsoft.VisualStudio.TestPlatform.Utilities.DefaultTestExecutorLauncher::.ctor"

#define TESTPLATFORM_TESTEXECUTOR_CORE_ASSEMBLY L"Microsoft.VisualStudio.TestPlatform.TestExecutor.Core"
#define TESTEXECUTORMAIN_RUN L"Microsoft.VisualStudio.TestPlatform.TestExecutor.TestExecutorMain::Run"
#define TESTEXECUTORMAIN_CTOR L"Microsoft.VisualStudio.TestPlatform.TestExecutor.TestExecutorMain::.ctor"

#import <mscorlib.tlb> raw_interfaces_only
using namespace mscorlib;

extern COpenCoverProfilerModule _AtlModule;

namespace {
	struct __declspec(uuid("2180EC45-CF11-456E-9A76-389A4521A4BE"))
	IDomainHelper : IUnknown
	{
		virtual HRESULT __stdcall AddResolveEventHandler() = 0;
	};
}

LPSAFEARRAY GetInjectedDllAsSafeArray()
{
	HINSTANCE hInst = _AtlModule.m_hModule;
	HRSRC hClrHookDllRes = FindResource(hInst, MAKEINTRESOURCE(IDR_SUPPORT), L"ASSEMBLY");
	ATLASSERT(hClrHookDllRes != NULL);

	HGLOBAL hClrHookDllHGlb = LoadResource(hInst, hClrHookDllRes);
	ATLASSERT(hClrHookDllHGlb != NULL);

	DWORD dllMemorySize = SizeofResource(hInst, hClrHookDllRes);

	LPBYTE lpDllData = (LPBYTE)LockResource(hClrHookDllHGlb);
	ATLASSERT(lpDllData != NULL);

	SAFEARRAYBOUND bound = { 0 };
	bound.cElements = dllMemorySize;
	bound.lLbound = 0;

	LPBYTE lpArrayData;
	LPSAFEARRAY lpAsmblyData = SafeArrayCreate(VT_UI1, 1, &bound);
	ATLASSERT(lpAsmblyData != NULL);

	SafeArrayAccessData(lpAsmblyData, (void**)&lpArrayData);
	memcpy(lpArrayData, lpDllData, dllMemorySize);
	SafeArrayUnaccessData(lpAsmblyData);

	return lpAsmblyData;
}

EXTERN_C HRESULT STDAPICALLTYPE LoadOpenCoverSupportAssembly(IUnknown *pUnk)
{
	ATLTRACE(_T("****LoadInjectorAssembly - Start****"));

	CComPtr<_AppDomain> pAppDomain;
	HRESULT hr = pUnk->QueryInterface(__uuidof(_AppDomain), (void**)&pAppDomain);
	ATLASSERT(hr == S_OK);
	LPSAFEARRAY lpAsmblyData = GetInjectedDllAsSafeArray();
	ATLASSERT(lpAsmblyData != NULL);

	CComPtr<_Assembly> pAssembly;
	hr = pAppDomain->Load_3(lpAsmblyData, &pAssembly);
	ATLASSERT(hr == S_OK);

	SafeArrayDestroy(lpAsmblyData);

	CComVariant variant;
	hr = pAssembly->CreateInstance(W2BSTR(L"OpenCover.Support.DomainHelper"), &variant);
	ATLASSERT(hr == S_OK);

	CComPtr<IDomainHelper> pDomainHelper;
    hr = variant.punkVal->QueryInterface(__uuidof(IDomainHelper), (void**)&pDomainHelper);
	ATLASSERT(hr == S_OK);

    hr = pDomainHelper->AddResolveEventHandler();
	ATLASSERT(hr == S_OK);
	ATLTRACE(_T("****LoadInjectorAssembly - End****"));

	return S_OK;
}

HRESULT CCodeCoverage::OpenCoverSupportInitialize(
	/* [in] */ IUnknown *pICorProfilerInfoUnk)
{
	TCHAR ext[1024] = { 0 };
	::GetEnvironmentVariable(_T("CHAIN_EXTERNAL_PROFILER"), ext, 1024);
	if (ext[0] != 0)
	{
		ATLTRACE(_T("::OpenCoverSupportInitialize"));

		ATLTRACE(_T("    ::Initialize(...) => ext = %s"), ext);

		TCHAR loc[1024] = { 0 };
		::GetEnvironmentVariable(_T("CHAIN_EXTERNAL_PROFILER_LOCATION"), loc, 1024);
		ATLTRACE(_T("    ::Initialize(...) => loc = %s"), loc);

		CLSID clsid;
		HRESULT hr = CLSIDFromString(T2OLE(ext), &clsid);
		ATLASSERT(hr == S_OK);

		HMODULE hmodule = LoadLibrary(loc);
		ATLASSERT(hmodule != NULL);

		BOOL(WINAPI*DllGetClassObject)(REFCLSID, REFIID, LPVOID) =
			(BOOL(WINAPI*)(REFCLSID, REFIID, LPVOID))GetProcAddress(hmodule, "DllGetClassObject");
		ATLASSERT(DllGetClassObject != NULL);

		CComPtr<IClassFactory> pClassFactory;
		hr = DllGetClassObject(clsid, IID_IClassFactory, &pClassFactory);
		ATLASSERT(hr == S_OK);


		hr = pClassFactory->CreateInstance(NULL, __uuidof(ICorProfilerCallback4), (void**)&m_chainedProfiler);
		ATLASSERT(hr == S_OK);

		HRESULT hr2 = CComObject<CProfilerInfo>::CreateInstance(&m_infoHook);
		ULONG count = m_infoHook->AddRef();

		m_infoHook->m_pProfilerHook = this;

		m_infoHook->SetProfilerInfo(pICorProfilerInfoUnk);

		hr = m_chainedProfiler->Initialize(m_infoHook);

		ATLTRACE(_T("  ::OpenCoverSupportInitialize => fakes = 0x%X"), hr);
	}
	
	return S_OK;
}

HRESULT CCodeCoverage::GetOpenCoverSupportRef(ModuleID moduleId, mdModuleRef &supportRef)
{
	// get interfaces
	CComPtr<IMetaDataEmit2> metaDataEmit;
	HRESULT hr = m_profilerInfo->GetModuleMetaData(moduleId,
		ofRead | ofWrite, IID_IMetaDataEmit2, (IUnknown**)&metaDataEmit);
	ATLASSERT(hr == S_OK);

	CComPtr<IMetaDataAssemblyEmit> metaDataAssemblyEmit;
	hr = metaDataEmit->QueryInterface(
		IID_IMetaDataAssemblyEmit, (void**)&metaDataAssemblyEmit);
	ATLASSERT(hr == S_OK);

	// find injected
	ASSEMBLYMETADATA assembly;
	ZeroMemory(&assembly, sizeof(assembly));
	assembly.usMajorVersion = 1;
	assembly.usMinorVersion = 0;
	assembly.usBuildNumber = 0;
	assembly.usRevisionNumber = 0;
	const BYTE pubToken[] = { 0xe1, 0x91, 0x8c, 0xac, 0x69, 0xeb, 0x73, 0xf4 };
	hr = metaDataAssemblyEmit->DefineAssemblyRef(pubToken,
		sizeof(pubToken), L"OpenCover.Support", &assembly, NULL, 0, 0,
		&supportRef);
	ATLASSERT(hr == S_OK);
	return hr;
}

HRESULT CCodeCoverage::OpenCoverSupportModulesAttachedToAssembly(
	/* [in] */ ModuleID moduleId,
	/* [in] */ AssemblyID assemblyId)
{

	std::wstring assemblyName = GetAssemblyName(assemblyId);

	if (TESTPLATFORM_UTILITIES_ASSEMBLY == GetAssemblyName(assemblyId))
	{
		std::wstring modulePath = GetModulePath(moduleId);
		ATLTRACE(_T("::ModuleAttachedToAssembly(...) => (%X => %s, %X => %s)"),
			moduleId, W2CT(modulePath.c_str()),
			assemblyId, W2CT(assemblyName.c_str()));

		m_targetLoadOpenCoverProfilerInsteadRef = 0;
		// get reference to injected
		mdModuleRef injectedRef;
		HRESULT hr = GetOpenCoverSupportRef(moduleId, injectedRef);
		ATLASSERT(hr == S_OK);

		// get interfaces
		CComPtr<IMetaDataEmit> metaDataEmit;
		hr = m_profilerInfo->GetModuleMetaData(moduleId,
			ofRead | ofWrite, IID_IMetaDataEmit, (IUnknown**)&metaDataEmit);
		ATLASSERT(hr == S_OK);

		static COR_SIGNATURE methodCallSignature[] =
		{
			IMAGE_CEE_CS_CALLCONV_DEFAULT,
			0x01,
			ELEMENT_TYPE_VOID,
			ELEMENT_TYPE_OBJECT
		};

		// get method to call
		mdTypeRef classTypeRef;
		hr = metaDataEmit->DefineTypeRefByName(injectedRef,
			L"OpenCover.Support.Fakes.FakesHelper", &classTypeRef);
		ATLASSERT(hr == S_OK);

		hr = metaDataEmit->DefineMemberRef(classTypeRef,
			L"LoadOpenCoverProfilerInstead", methodCallSignature,
			sizeof(methodCallSignature), &m_targetLoadOpenCoverProfilerInsteadRef);
		ATLASSERT(hr == S_OK);

		// get object ref
		mdModuleRef mscorlibRef;
		hr = GetModuleRef(moduleId, MSCORLIB_NAME, mscorlibRef);
		ATLASSERT(hr == S_OK);

		hr = metaDataEmit->DefineTypeRefByName(mscorlibRef,
			L"System.Object", &m_objectTypeRef);
		ATLASSERT(hr == S_OK);

		m_pinvokeAttach = CreatePInvokeHook(metaDataEmit);
	}

	if (TESTPLATFORM_TESTEXECUTOR_CORE_ASSEMBLY == GetAssemblyName(assemblyId))
	{
		std::wstring modulePath = GetModulePath(moduleId);
		ATLTRACE(_T("::ModuleAttachedToAssembly(...) => (%X => %s, %X => %s)"),
			moduleId, W2CT(modulePath.c_str()),
			assemblyId, W2CT(assemblyName.c_str()));

		m_targetPretendWeLoadedFakesProfilerRef = 0;
		// get reference to injected
		mdModuleRef injectedRef;
		HRESULT hr = GetOpenCoverSupportRef(moduleId, injectedRef);
		ATLASSERT(hr == S_OK);

		// get interfaces
		CComPtr<IMetaDataEmit> metaDataEmit;
		hr = m_profilerInfo->GetModuleMetaData(moduleId,
			ofRead | ofWrite, IID_IMetaDataEmit, (IUnknown**)&metaDataEmit);
		ATLASSERT(hr == S_OK);

		static COR_SIGNATURE methodCallSignature[] =
		{
			IMAGE_CEE_CS_CALLCONV_DEFAULT,
			0x01,
			ELEMENT_TYPE_VOID,
			ELEMENT_TYPE_OBJECT
		};

		// get method to call
		mdTypeRef classTypeRef;
		hr = metaDataEmit->DefineTypeRefByName(injectedRef,
			L"OpenCover.Support.Fakes.FakesHelper", &classTypeRef);
		ATLASSERT(hr == S_OK);

		hr = metaDataEmit->DefineMemberRef(classTypeRef,
			L"PretendWeLoadedFakesProfiler", methodCallSignature,
			sizeof(methodCallSignature), &m_targetPretendWeLoadedFakesProfilerRef);
		ATLASSERT(hr == S_OK);

		// get object ref
		mdModuleRef mscorlibRef;
		hr = GetModuleRef(moduleId, MSCORLIB_NAME, mscorlibRef);
		ATLASSERT(hr == S_OK);

		hr = metaDataEmit->DefineTypeRefByName(mscorlibRef,
			L"System.Object", &m_objectTypeRef);
		ATLASSERT(hr == S_OK);

		m_pinvokeAttach = CreatePInvokeHook(metaDataEmit);

	}

	return S_OK;
}

mdMethodDef CCodeCoverage::CreatePInvokeHook(IMetaDataEmit* pMetaDataEmit){

	mdTypeDef	tkInjClass;

	HRESULT hr = pMetaDataEmit->DefineTypeDef(L"__ClrProbeInjection_", tdAbstract | tdSealed, m_objectTypeRef, NULL, &tkInjClass);
	ATLASSERT(hr == S_OK);

	static BYTE sg_sigPLoadInjectorAssembly[] = {
		0, // IMAGE_CEE_CS_CALLCONV_DEFAULT
		1, // argument count
		ELEMENT_TYPE_VOID, // ret = ELEMENT_TYPE_VOID
		ELEMENT_TYPE_OBJECT
	};

	mdModuleRef	tkRefClrProbe;
	hr = pMetaDataEmit->DefineModuleRef(L"OPENCOVER.PROFILER.DLL", &tkRefClrProbe);
	ATLASSERT(hr == S_OK);

	mdMethodDef	tkAttachDomain;
	pMetaDataEmit->DefineMethod(tkInjClass, L"LoadOpenCoverSupportAssembly",
		mdStatic | mdPinvokeImpl,
		sg_sigPLoadInjectorAssembly, sizeof(sg_sigPLoadInjectorAssembly),
		0, 0, &tkAttachDomain
		);
	ATLASSERT(hr == S_OK);

	BYTE tiunk = NATIVE_TYPE_IUNKNOWN;
	mdParamDef paramDef;
	hr = pMetaDataEmit->DefinePinvokeMap(tkAttachDomain, 0, L"LoadOpenCoverSupportAssembly", tkRefClrProbe);
	ATLASSERT(hr == S_OK);

	hr = pMetaDataEmit->DefineParam(tkAttachDomain, 1, L"appDomain",
		pdIn | pdHasFieldMarshal, 0, NULL, 0, &paramDef);
	ATLASSERT(hr == S_OK);

	hr = pMetaDataEmit->SetFieldMarshal(paramDef, &tiunk, 1);
	ATLASSERT(hr == S_OK);

	return tkAttachDomain;
}

HRESULT CCodeCoverage::OpenCoverSupportCompilation(FunctionID functionId, mdToken functionToken, ModuleID moduleId, AssemblyID assemblyId, std::wstring &modulePath)
{
	if (TESTPLATFORM_UTILITIES_ASSEMBLY == GetAssemblyName(assemblyId))
	{
		std::wstring typeMethodName = GetTypeAndMethodName(functionId);

		if (DEFAULTTESTEXECUTOR_CTOR == typeMethodName)
		{
			ATLTRACE(_T("::JITCompilationStarted(%X, ...) => %d, %X => %s"), functionId, functionToken, moduleId, W2CT(typeMethodName.c_str()));

			InstructionList instructions; 

			mdMethodDef getCurrentDomain = Get_CurrentDomainMethod(moduleId);
			instructions.push_back(new Instruction(CEE_CALL, getCurrentDomain));
			instructions.push_back(new Instruction(CEE_CALL, m_pinvokeAttach));

			InstrumentMethodWith(moduleId, functionToken, instructions);
		}

		if (DEFAULTTESTEXECUTOR_LAUNCHPROCESS == typeMethodName)
		{
			ATLTRACE(_T("::JITCompilationStarted(%X, ...) => %d, %X => %s"), functionId, functionToken, moduleId, W2CT(typeMethodName.c_str()));

			InstructionList instructions; 

			instructions.push_back(new Instruction(CEE_LDARG_S, 4));
			instructions.push_back(new Instruction(CEE_CALL, m_targetLoadOpenCoverProfilerInsteadRef));

			InstrumentMethodWith(moduleId, functionToken, instructions);
		}

		return S_OK;
	}

	if (TESTPLATFORM_TESTEXECUTOR_CORE_ASSEMBLY == GetAssemblyName(assemblyId))
	{
		std::wstring typeMethodName = GetTypeAndMethodName(functionId);

		if (TESTEXECUTORMAIN_CTOR == typeMethodName)
		{
			ATLTRACE(_T("::JITCompilationStarted(%X, ...) => %d, %X => %s"), functionId, functionToken, moduleId, W2CT(typeMethodName.c_str()));

			InstructionList instructions;

			mdMethodDef getCurrentDomain = Get_CurrentDomainMethod(moduleId);
			instructions.push_back(new Instruction(CEE_CALL, getCurrentDomain));
			instructions.push_back(new Instruction(CEE_CALL, m_pinvokeAttach));

			InstrumentMethodWith(moduleId, functionToken, instructions);
		}

		if (TESTEXECUTORMAIN_RUN == typeMethodName)
		{
			ATLTRACE(_T("::JITCompilationStarted(%X, ...) => %d, %X => %s"), functionId, functionToken, moduleId, W2CT(typeMethodName.c_str()));

			InstructionList instructions; // NOTE: this IL will be different for an instance method or if the local vars signature is different

			instructions.push_back(new Instruction(CEE_LDARG, 0));
			instructions.push_back(new Instruction(CEE_CALL, m_targetPretendWeLoadedFakesProfilerRef));

			InstrumentMethodWith(moduleId, functionToken, instructions);
		}
	}
}

mdMethodDef CCodeCoverage::Get_CurrentDomainMethod(ModuleID moduleID)
{
	CComPtr<IMetaDataEmit> metaDataEmit;
	COM_FAIL_MSG_RETURN_OTHER(m_profilerInfo->GetModuleMetaData(moduleID, ofWrite, IID_IMetaDataEmit, (IUnknown**)&metaDataEmit), 0,
		_T("    ::Get_CurrentDomainMethod(ModuleID) => GetModuleMetaData => 0x%X"));

	mdModuleRef mscorlibRef;
	HRESULT hr = GetModuleRef(moduleID, MSCORLIB_NAME, mscorlibRef);
	ATLASSERT(hr == S_OK);

	mdMethodDef getCurrentDomain;
	mdTypeDef tkAppDomain;
	hr = metaDataEmit->DefineTypeRefByName(mscorlibRef, L"System.AppDomain", &tkAppDomain);
	ATLASSERT(hr == S_OK);

	BYTE importSig[] = { IMAGE_CEE_CS_CALLCONV_DEFAULT, 0 /*<no args*/, 0x12 /*< ret class*/, 0, 0, 0, 0, 0 };
	ULONG l = CorSigCompressToken(tkAppDomain, importSig + 3);	//< Add the System.AppDomain token ref
	hr = metaDataEmit->DefineMemberRef(tkAppDomain, L"get_CurrentDomain", importSig, 3 + l, &getCurrentDomain);
	ATLASSERT(hr == S_OK);
	return getCurrentDomain;
}



