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
    COM_FAIL(m_profilerInfo->GetModuleInfo(moduleId, NULL, dwNameSize, &dwNameSize, szModulePath, NULL), std::wstring());
    return std::wstring(szModulePath);
}

std::wstring CCodeCoverage::GetModulePath(ModuleID moduleId, AssemblyID *pAssemblyID)
{
    ULONG dwNameSize = 512;
    WCHAR szModulePath[512] = {};
    COM_FAIL(m_profilerInfo->GetModuleInfo(moduleId, NULL, dwNameSize, &dwNameSize, szModulePath, pAssemblyID), std::wstring());
    return std::wstring(szModulePath);
}

/// <summary>
/// Get the assembly name from an AssemblyID
/// </summary>
std::wstring CCodeCoverage::GetAssemblyName(AssemblyID assemblyId)
{
    ULONG dwNameSize = 512; 
    WCHAR szAssemblyName[512] = {};
    COM_FAIL(m_profilerInfo->GetAssemblyInfo(assemblyId, dwNameSize, &dwNameSize, szAssemblyName, NULL, NULL), std::wstring());
    return std::wstring(szAssemblyName);
}

/// <summary>
/// Get the function token, module ID and module name for a supplied FunctionID
/// </summary>
BOOL CCodeCoverage::GetTokenAndModule(FunctionID funcId, mdToken& functionToken, ModuleID& moduleId, std::wstring &modulePath, AssemblyID *pAssemblyId)
{
    COM_FAIL(m_profilerInfo2->GetFunctionInfo2(funcId, NULL, NULL, &moduleId, &functionToken, 0, NULL, NULL), FALSE);
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
    COM_FAIL(m_profilerInfo2->GetModuleMetaData(moduleID, ofWrite, IID_IMetaDataEmit, (IUnknown**) &metaDataEmit), 0);

    mdSignature pmsig ;
    COM_FAIL(metaDataEmit->GetTokenFromSig(unmanagedCallSignature, sizeof(unmanagedCallSignature), &pmsig), 0);
    return pmsig;
}


HRESULT CCodeCoverage::GetModuleRef(ModuleID moduleId, WCHAR*moduleName, mdModuleRef &mscorlibRef)
{
    CComPtr<IMetaDataEmit> metaDataEmit;
    COM_FAIL_RETURNMSG(m_profilerInfo->GetModuleMetaData(moduleId, 
        ofRead | ofWrite, IID_IMetaDataEmit, (IUnknown**)&metaDataEmit), 
        _T("GetModuleRef => GetModuleMetaData(0x%x)"));      
    
    CComPtr<IMetaDataAssemblyEmit> metaDataAssemblyEmit;
    COM_FAIL_RETURNMSG(metaDataEmit->QueryInterface(
        IID_IMetaDataAssemblyEmit, (void**)&metaDataAssemblyEmit), 
        _T("GetModuleRef => QueryInterface(0x%x)"));

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

HRESULT CCodeCoverage::GetModuleRef4000(IMetaDataAssemblyEmit *metaDataAssemblyEmit, WCHAR*moduleName, mdModuleRef &mscorlibRef)
{
    ASSEMBLYMETADATA assembly;
    ZeroMemory(&assembly, sizeof(assembly));
    assembly.usMajorVersion = 4;
    assembly.usMinorVersion = 0;
    assembly.usBuildNumber = 0; 
    assembly.usRevisionNumber = 0;
    BYTE publicKey[] = { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89 };
    COM_FAIL_RETURNMSG(metaDataAssemblyEmit->DefineAssemblyRef(publicKey, 
        sizeof(publicKey), moduleName, &assembly, NULL, 0, 0, 
        &mscorlibRef), _T("GetModuleRef4000 => DefineAssemblyRef(0x%x)"));

    return S_OK;
}

HRESULT CCodeCoverage::GetModuleRef2000(IMetaDataAssemblyEmit *metaDataAssemblyEmit, WCHAR*moduleName, mdModuleRef &mscorlibRef)
{
    ASSEMBLYMETADATA assembly;
    ZeroMemory(&assembly, sizeof(assembly));
    assembly.usMajorVersion = 2;
    assembly.usMinorVersion = 0;
    assembly.usBuildNumber = 0; 
    assembly.usRevisionNumber = 0;
    BYTE publicKey[] = { 0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89 };
    COM_FAIL_RETURNMSG(metaDataAssemblyEmit->DefineAssemblyRef(publicKey, 
        sizeof(publicKey), moduleName, &assembly, NULL, 0, 0, &mscorlibRef), 
        _T("GetModuleRef2000 => DefineAssemblyRef(0x%x)"));

    return S_OK;
}

HRESULT CCodeCoverage::GetModuleRef2050(IMetaDataAssemblyEmit *metaDataAssemblyEmit, WCHAR*moduleName, mdModuleRef &mscorlibRef)
{
    ASSEMBLYMETADATA assembly;
    ZeroMemory(&assembly, sizeof(assembly));
    assembly.usMajorVersion = 2;
    assembly.usMinorVersion = 0;
    assembly.usBuildNumber = 5; 
    assembly.usRevisionNumber = 0;

    BYTE publicKey[] = { 0x7C, 0xEC, 0x85, 0xD7, 0xBE, 0xA7, 0x79, 0x8E };
    COM_FAIL_RETURNMSG(metaDataAssemblyEmit->DefineAssemblyRef(publicKey, 
        sizeof(publicKey), moduleName, &assembly, NULL, 0, 0, &mscorlibRef), 
        _T("GetModuleRef2050 => DefineAssemblyRef(0x%x)"));

    return S_OK;
}

