#include "stdafx.h"
#include "CodeCoverage.h"

#define CUCKOO_SAFE_METHOD_NAME L"SafeVisited"
#define CUCKOO_CRITICAL_METHOD_NAME L"VisitedCritical"
#define CUCKOO_NEST_TYPE_NAME L"System.CannotUnloadAppDomainException"

static COR_SIGNATURE visitedMethodCallSignature[] =
{
	IMAGE_CEE_CS_CALLCONV_DEFAULT,
	0x01,
	ELEMENT_TYPE_VOID,
	ELEMENT_TYPE_I4
};

static COR_SIGNATURE ctorCallSignature[] =
{
	IMAGE_CEE_CS_CALLCONV_DEFAULT | IMAGE_CEE_CS_CALLCONV_HASTHIS,
	0x00,
	ELEMENT_TYPE_VOID
};

using namespace Instrumentation;

HRESULT CCodeCoverage::RegisterCuckoos(ModuleID moduleId){

	CComPtr<IMetaDataEmit> metaDataEmit;
	COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo->GetModuleMetaData(moduleId,
		ofRead | ofWrite, IID_IMetaDataEmit, (IUnknown**)&metaDataEmit),
		_T("    ::ModuleLoadFinished(...) => GetModuleMetaData => 0x%X"));
	if (metaDataEmit == NULL) return S_OK;

	CComPtr<IMetaDataImport> metaDataImport;
	COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo->GetModuleMetaData(moduleId,
		ofRead | ofWrite, IID_IMetaDataImport, (IUnknown**)&metaDataImport),
		_T("    ::ModuleLoadFinished(...) => GetModuleMetaData => 0x%X"));
	if (metaDataImport == NULL) return S_OK;

	mdTypeDef systemObject = mdTokenNil;
	if (S_OK == metaDataImport->FindTypeDefByName(L"System.Object", mdTokenNil, &systemObject))
	{
		RELTRACE(_T("::ModuleLoadFinished(...) => Adding methods to mscorlib..."));
		mdMethodDef systemObjectCtor;
		COM_FAIL_MSG_RETURN_ERROR(metaDataImport->FindMethod(systemObject, L".ctor",
			ctorCallSignature, sizeof(ctorCallSignature), &systemObjectCtor),
			_T("    ::ModuleLoadFinished(...) => FindMethod => 0x%X)"));

		ULONG ulCodeRVA = 0;
		COM_FAIL_MSG_RETURN_ERROR(metaDataImport->GetMethodProps(systemObjectCtor, NULL, NULL,
			0, NULL, NULL, NULL, NULL, &ulCodeRVA, NULL),
			_T("    ::ModuleLoadFinished(...) => GetMethodProps => 0x%X"));

		mdCustomAttribute customAttr;
		mdToken attributeCtor;
		mdTypeDef attributeTypeDef;
		mdTypeDef nestToken;

		COM_FAIL_MSG_RETURN_ERROR(metaDataImport->FindTypeDefByName(CUCKOO_NEST_TYPE_NAME, mdTokenNil, &nestToken),
			_T("    ::ModuleLoadFinished(...) => FindTypeDefByName => 0x%X"));

		// create a method that we will mark up with the SecurityCriticalAttribute
		COM_FAIL_MSG_RETURN_ERROR(metaDataEmit->DefineMethod(nestToken, CUCKOO_CRITICAL_METHOD_NAME,
			mdPublic | mdStatic | mdHideBySig, visitedMethodCallSignature, sizeof(visitedMethodCallSignature),
			ulCodeRVA, miIL | miManaged | miPreserveSig | miNoInlining, &m_cuckooCriticalToken),
			_T("    ::ModuleLoadFinished(...) => DefineMethod => 0x%X"));

		COM_FAIL_MSG_RETURN_ERROR(metaDataImport->FindTypeDefByName(L"System.Security.SecurityCriticalAttribute",
			NULL, &attributeTypeDef), _T("    :ModuleLoadFinished(...) => FindTypeDefByName => 0x%X"));

		if (m_runtimeType == COR_PRF_DESKTOP_CLR)
		{
			// for desktop we use the .ctor that takes a SecurityCriticalScope argument as the 
			// default (no arguments) constructor fails with "0x801311C2 - known custom attribute value is bad" 
			// when we try to attach it in .NET2 - .NET4 version doesn't care which one we use
			mdTypeDef scopeToken;
			COM_FAIL_MSG_RETURN_ERROR(metaDataImport->FindTypeDefByName(L"System.Security.SecurityCriticalScope", mdTokenNil, &scopeToken),
				_T("    ::ModuleLoadFinished(...) => FindTypeDefByName => 0x%X"));

			ULONG sigLength = 4;
			COR_SIGNATURE ctorCallSignatureEnum[] =
			{
				IMAGE_CEE_CS_CALLCONV_DEFAULT | IMAGE_CEE_CS_CALLCONV_HASTHIS,
				0x01,
				ELEMENT_TYPE_VOID,
				ELEMENT_TYPE_VALUETYPE,
				0x00, 0x00, 0x00, 0x00 // make room for our compressed token - should always be 2 but...
			};

			sigLength += CorSigCompressToken(scopeToken, &ctorCallSignatureEnum[4]);

			COM_FAIL_MSG_RETURN_ERROR(metaDataImport->FindMember(attributeTypeDef,
				L".ctor", ctorCallSignatureEnum, sigLength, &attributeCtor),
				_T("    ::ModuleLoadFinished(...) => FindMember => 0x%X"));

			unsigned char blob[] = { 0x01, 0x00, 0x01, 0x00, 0x00, 0x00 }; // prolog U2 plus an enum of I4 (little-endian)
			COM_FAIL_MSG_RETURN_ERROR(metaDataEmit->DefineCustomAttribute(m_cuckooCriticalToken, attributeCtor, blob, sizeof(blob), &customAttr),
				_T("    ::ModuleLoadFinished(...) => DefineCustomAttribute => 0x%X"));
		}
		else
		{
			// silverlight only has one .ctor for this type
			COM_FAIL_MSG_RETURN_ERROR(metaDataImport->FindMember(attributeTypeDef,
				L".ctor", ctorCallSignature, sizeof(ctorCallSignature), &attributeCtor),
				_T("    ::ModuleLoadFinished(...) => FindMember => 0x%X"));

			COM_FAIL_MSG_RETURN_ERROR(metaDataEmit->DefineCustomAttribute(m_cuckooCriticalToken, attributeCtor, NULL, 0, &customAttr),
				_T("    ::ModuleLoadFinished(...) => DefineCustomAttribute => 0x%X"));
		}

		// create a method that we will mark up with the SecuritySafeCriticalAttribute
		COM_FAIL_MSG_RETURN_ERROR(metaDataEmit->DefineMethod(nestToken, CUCKOO_SAFE_METHOD_NAME,
			mdPublic | mdStatic | mdHideBySig, visitedMethodCallSignature, sizeof(visitedMethodCallSignature),
			ulCodeRVA, miIL | miManaged | miPreserveSig | miNoInlining, &m_cuckooSafeToken),
			_T("    ::ModuleLoadFinished(...) => DefineMethod => 0x%X"));

		COM_FAIL_MSG_RETURN_ERROR(metaDataImport->FindTypeDefByName(L"System.Security.SecuritySafeCriticalAttribute",
			NULL, &attributeTypeDef),
			_T("    ::ModuleLoadFinished(...) => FindTypeDefByName => 0x%X"));

		COM_FAIL_MSG_RETURN_ERROR(metaDataImport->FindMember(attributeTypeDef,
			L".ctor", ctorCallSignature, sizeof(ctorCallSignature), &attributeCtor),
			_T("    ::ModuleLoadFinished(...) => FindMember => 0x%X"));

		COM_FAIL_MSG_RETURN_ERROR(metaDataEmit->DefineCustomAttribute(m_cuckooSafeToken, attributeCtor, NULL, 0, &customAttr),
			_T("    ::ModuleLoadFinished(...) => DefineCustomAttribute => 0x%X"));

		RELTRACE(_T("::ModuleLoadFinished(...) => Added methods to mscorlib"));
	}

	return S_OK;
}

mdMemberRef CCodeCoverage::RegisterSafeCuckooMethod(ModuleID moduleId, const WCHAR* moduleName)
{
	ATLTRACE(_T("::RegisterSafeCuckooMethod(%X) => %s"), moduleId, CUCKOO_SAFE_METHOD_NAME);

	// for modules we are going to instrument add our reference to the method marked 
	// with the SecuritySafeCriticalAttribute
	CComPtr<IMetaDataEmit> metaDataEmit;
	COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo->GetModuleMetaData(moduleId,
		ofRead | ofWrite, IID_IMetaDataEmit, (IUnknown**)&metaDataEmit),
		_T("    ::RegisterSafeCuckooMethod(...) => GetModuleMetaData => 0x%X"));

	mdModuleRef mscorlibRef;
	COM_FAIL_MSG_RETURN_ERROR(GetModuleRef(moduleId, moduleName, mscorlibRef),
		_T("    ::RegisterSafeCuckooMethod(...) => GetModuleRef => 0x%X"));

	mdTypeDef nestToken;
	COM_FAIL_MSG_RETURN_ERROR(metaDataEmit->DefineTypeRefByName(mscorlibRef, CUCKOO_NEST_TYPE_NAME, &nestToken),
		_T("    ::RegisterSafeCuckooMethod(...) => DefineTypeRefByName => 0x%X"));

	mdMemberRef cuckooSafeToken;
	COM_FAIL_MSG_RETURN_ERROR(metaDataEmit->DefineMemberRef(nestToken, CUCKOO_SAFE_METHOD_NAME,
		visitedMethodCallSignature, sizeof(visitedMethodCallSignature), &cuckooSafeToken),
		_T("    ::RegisterSafeCuckooMethod(...) => DefineMemberRef => 0x%X"));

	return cuckooSafeToken;
}

/// <summary>This is the method marked with the SecurityCriticalAttribute</summary>
/// <remarks>This method makes the call into the profiler</remarks>
HRESULT CCodeCoverage::AddCriticalCuckooBody(ModuleID moduleId)
{
	ATLTRACE(_T("::AddCriticalCuckooBody => Adding VisitedCritical..."));

	mdSignature pvsig = GetMethodSignatureToken_I4(moduleId);
	void(__fastcall *pt)(ULONG) = GetInstrumentPointVisit();

	BYTE data[] = { (0x01 << 2) | CorILMethod_TinyFormat, CEE_RET };
	Instrumentation::Method criticalMethod((IMAGE_COR_ILMETHOD*)data);
	InstructionList instructions;
	instructions.push_back(new Instruction(CEE_LDARG_0));
#ifdef _WIN64
	instructions.push_back(new Instruction(CEE_LDC_I8, (ULONGLONG)pt));
#else
	instructions.push_back(new Instruction(CEE_LDC_I4, (ULONG)pt));
#endif
	instructions.push_back(new Instruction(CEE_CALLI, pvsig));

	criticalMethod.InsertInstructionsAtOffset(0, instructions);

	InstrumentMethodWith(moduleId, m_cuckooCriticalToken, instructions);

	ATLTRACE(_T("::AddCriticalCuckooBody => Adding VisitedCritical - Done!"));

	return S_OK;
}

/// <summary>This is the body of our method marked with the SecuritySafeCriticalAttribute</summary>
/// <remarks>Calls the method that is marked with the SecurityCriticalAttribute</remarks>
HRESULT CCodeCoverage::AddSafeCuckooBody(ModuleID moduleId)
{
	ATLTRACE(_T("::AddSafeCuckooBody => Adding SafeVisited..."));

	BYTE data[] = { (0x01 << 2) | CorILMethod_TinyFormat, CEE_RET };
	Instrumentation::Method criticalMethod((IMAGE_COR_ILMETHOD*)data);
	InstructionList instructions;
	instructions.push_back(new Instruction(CEE_LDARG_0));
	instructions.push_back(new Instruction(CEE_CALL, m_cuckooCriticalToken));

	criticalMethod.InsertInstructionsAtOffset(0, instructions);

	InstrumentMethodWith(moduleId, m_cuckooSafeToken, instructions);

	ATLTRACE(_T("::AddSafeCuckooBody => Adding SafeVisited - Done!"));

	return S_OK;
}

HRESULT CCodeCoverage::CuckooSupportCompilation(
	AssemblyID assemblyId,
	mdToken functionToken,
	ModuleID moduleId)
{
    // early escape if token is not one we want
    if ((m_cuckooCriticalToken != functionToken) && (m_cuckooSafeToken != functionToken))
        return S_OK;

	auto assemblyName = GetAssemblyName(assemblyId);
	// check that we have the right module
	if (MSCORLIB_NAME == assemblyName || DNCORLIB_NAME == assemblyName) 
	{
		if (m_cuckooCriticalToken == functionToken)
		{
			COM_FAIL_MSG_RETURN_ERROR(AddCriticalCuckooBody(moduleId),
				_T("    ::JITCompilationStarted(...) => AddCriticalCuckooBody => 0x%X"));
		}

		if (m_cuckooSafeToken == functionToken)
		{
			COM_FAIL_MSG_RETURN_ERROR(AddSafeCuckooBody(moduleId),
				_T("    ::JITCompilationStarted(...) => AddSafeCuckooBody => 0x%X"));
		}
	}
	return S_OK;
}