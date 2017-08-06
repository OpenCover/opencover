//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#include "stdafx.h"
#include "CodeCoverage.h"

// reference http://www.ecma-international.org/publications/standards/Ecma-335.htm


/// <summary>
/// Gets the module name from a ModuleID
/// </summary>
std::wstring CCodeCoverage::GetModulePath(ModuleID moduleId)
{
    ULONG dwNameSize = 512;
    WCHAR szModulePath[512] = {};
    COM_FAIL_MSG_RETURN_OTHER(m_profilerInfo->GetModuleInfo(moduleId, NULL, dwNameSize, &dwNameSize, szModulePath, NULL), std::wstring(),
        _T("    ::GetModulePath(ModuleID) => GetModuleInfo => 0x%X"));
    return std::wstring(szModulePath);
}

std::wstring CCodeCoverage::GetModulePath(ModuleID moduleId, AssemblyID *pAssemblyID)
{
    ULONG dwNameSize = 512;
    WCHAR szModulePath[512] = {};
    COM_FAIL_MSG_RETURN_OTHER(m_profilerInfo->GetModuleInfo(moduleId, NULL, dwNameSize, &dwNameSize, szModulePath, pAssemblyID), std::wstring(),
        _T("    ::GetModulePath(ModuleID,AssemblyID*) => GetModuleInfo => 0x%X"));
    return std::wstring(szModulePath);
}

/// <summary>
/// Get the assembly name from an AssemblyID
/// </summary>
std::wstring CCodeCoverage::GetAssemblyName(AssemblyID assemblyId)
{
    ULONG dwNameSize = 512; 
    WCHAR szAssemblyName[512] = {};
    COM_FAIL_MSG_RETURN_OTHER(m_profilerInfo->GetAssemblyInfo(assemblyId, dwNameSize, &dwNameSize, szAssemblyName, NULL, NULL), std::wstring(),
        _T("    ::GetAssemblyName(AssemblyID) => GetAssemblyInfo => 0x%X"));
    return std::wstring(szAssemblyName);
}

/// <summary>
/// Get the function token, module ID and module name for a supplied FunctionID
/// </summary>
BOOL CCodeCoverage::GetTokenAndModule(FunctionID funcId, mdToken& functionToken, ModuleID& moduleId, std::wstring &modulePath, AssemblyID *pAssemblyId)
{
    COM_FAIL_MSG_RETURN_OTHER(m_profilerInfo2->GetFunctionInfo2(funcId, NULL, NULL, &moduleId, &functionToken, 0, NULL, NULL), FALSE,
         _T("    ::GetTokenAndModule(...) => GetFunctionInfo2 => 0x%X"));
    modulePath = GetModulePath(moduleId, pAssemblyId);
    return TRUE;
}

/// <summary>Get the token for a method having a single I4 parameter</summary>
mdSignature CCodeCoverage::GetMethodSignatureToken_I4(ModuleID moduleID)
{
    static COR_SIGNATURE unmanagedCallSignature[] = 
    {
        IMAGE_CEE_CS_CALLCONV_DEFAULT,          // Default CallKind!
        0x01,                                   // Parameter count
        ELEMENT_TYPE_VOID,                      // Return type
        ELEMENT_TYPE_I4                         // Parameter type (I4)
    };

    CComPtr<IMetaDataEmit> metaDataEmit;
    COM_FAIL_MSG_RETURN_OTHER(m_profilerInfo2->GetModuleMetaData(moduleID, ofWrite, IID_IMetaDataEmit, (IUnknown**) &metaDataEmit), 0, 
        _T("    ::GetMethodSignatureToken_I4(ModuleID) => GetModuleMetaData => 0x%X"));

    mdSignature pmsig ;
    COM_FAIL_MSG_RETURN_OTHER(metaDataEmit->GetTokenFromSig(unmanagedCallSignature, sizeof(unmanagedCallSignature), &pmsig), 0,
        _T("    ::GetMethodSignatureToken_I4(ModuleID) => GetTokenFromSig => 0x%X"));
    return pmsig;
}


HRESULT CCodeCoverage::GetModuleRef(ModuleID moduleId, const WCHAR*moduleName, mdModuleRef &mscorlibRef)
{
    CComPtr<IMetaDataEmit> metaDataEmit;
    COM_FAIL_MSG_RETURN_ERROR(m_profilerInfo->GetModuleMetaData(moduleId, 
        ofRead | ofWrite, IID_IMetaDataEmit, (IUnknown**)&metaDataEmit), 
        _T("GetModuleRef(...) => GetModuleMetaData => 0x%X"));      
    
    CComPtr<IMetaDataAssemblyEmit> metaDataAssemblyEmit;
    COM_FAIL_MSG_RETURN_ERROR(metaDataEmit->QueryInterface(
        IID_IMetaDataAssemblyEmit, (void**)&metaDataAssemblyEmit), 
        _T("GetModuleRef(...) => QueryInterface => 0x%X"));

    if (m_profilerInfo3 != NULL) 
    {
        if (m_runtimeType == COR_PRF_DESKTOP_CLR)
            return GetModuleRef4000(metaDataAssemblyEmit, moduleName, mscorlibRef);
        if (m_runtimeType == COR_PRF_CORE_CLR)
            return GetModuleRef2050(metaDataAssemblyEmit, moduleName, mscorlibRef);
    }
    else
    {
        return GetModuleRef2000(metaDataAssemblyEmit, moduleName, mscorlibRef);
    }

    return S_OK;
}

HRESULT CCodeCoverage::GetModuleRef4000(IMetaDataAssemblyEmit *metaDataAssemblyEmit, const WCHAR*moduleName, mdModuleRef &mscorlibRef)
{
    ASSEMBLYMETADATA assembly;
    ZeroMemory(&assembly, sizeof(assembly));
    assembly.usMajorVersion = 4;
    assembly.usMinorVersion = 0;
    assembly.usBuildNumber = 0; 
    assembly.usRevisionNumber = 0;
    BYTE publicKey[] = { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89 };
    COM_FAIL_MSG_RETURN_ERROR(metaDataAssemblyEmit->DefineAssemblyRef(publicKey, 
        sizeof(publicKey), moduleName, &assembly, NULL, 0, 0, 
        &mscorlibRef), _T("GetModuleRef4000(...) => DefineAssemblyRef => 0x%X"));

    return S_OK;
}

HRESULT CCodeCoverage::GetModuleRef2000(IMetaDataAssemblyEmit *metaDataAssemblyEmit, const WCHAR*moduleName, mdModuleRef &mscorlibRef)
{
    ASSEMBLYMETADATA assembly;
    ZeroMemory(&assembly, sizeof(assembly));
    assembly.usMajorVersion = 2;
    assembly.usMinorVersion = 0;
    assembly.usBuildNumber = 0; 
    assembly.usRevisionNumber = 0;
    BYTE publicKey[] = { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89 };
    COM_FAIL_MSG_RETURN_ERROR(metaDataAssemblyEmit->DefineAssemblyRef(publicKey, 
        sizeof(publicKey), moduleName, &assembly, NULL, 0, 0, &mscorlibRef), 
        _T("GetModuleRef2000(...) => DefineAssemblyRef => 0x%X"));

    return S_OK;
}

HRESULT CCodeCoverage::GetModuleRef2050(IMetaDataAssemblyEmit *metaDataAssemblyEmit, const WCHAR*moduleName, mdModuleRef &mscorlibRef)
{
    ASSEMBLYMETADATA assembly;
    ZeroMemory(&assembly, sizeof(assembly));
    assembly.usMajorVersion = 2;
    assembly.usMinorVersion = 0;
    assembly.usBuildNumber = 5; 
    assembly.usRevisionNumber = 0;

    BYTE publicKey[] = { 0x7C, 0xEC, 0x85, 0xD7, 0xBE, 0xA7, 0x79, 0x8E };
    COM_FAIL_MSG_RETURN_ERROR(metaDataAssemblyEmit->DefineAssemblyRef(publicKey, 
        sizeof(publicKey), moduleName, &assembly, NULL, 0, 0, &mscorlibRef), 
        _T("GetModuleRef2050(...) => DefineAssemblyRef => 0x%X"));

    return S_OK;
}

std::wstring CCodeCoverage::GetTypeAndMethodName(FunctionID functionId)
{
	std::wstring empty = L"";
	CComPtr<IMetaDataImport2> metaDataImport2;
	mdMethodDef functionToken;
	COM_FAIL_MSG_RETURN_OTHER(m_profilerInfo->GetTokenAndMetaDataFromFunction(functionId, IID_IMetaDataImport, (IUnknown **)&metaDataImport2, &functionToken),
		empty, _T("GetTokenAndMetaDataFromFunction"));

	mdTypeDef classId;
	WCHAR szMethodName[512] = {};
	COM_FAIL_MSG_RETURN_OTHER(metaDataImport2->GetMethodProps(functionToken, &classId, szMethodName, 512, NULL, NULL, NULL, NULL, NULL, NULL),
		empty, _T("GetMethodProps"));

	WCHAR szTypeName[512] = {};
	COM_FAIL_MSG_RETURN_OTHER(metaDataImport2->GetTypeDefProps(classId, szTypeName, 512, NULL, NULL, NULL),
		empty, _T("GetTypeDefProps"));

	std::wstring methodName = szTypeName;
	methodName += L"::";
	methodName += szMethodName;

	//ATLTRACE(_T("::GetTypeAndMethodName(%s)"), W2CT(methodName.c_str()));

	return methodName;
}

