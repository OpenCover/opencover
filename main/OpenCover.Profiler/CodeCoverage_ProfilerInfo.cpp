#include "stdafx.h"
#include "CodeCoverage.h"

// reference http://www.ecma-international.org/publications/standards/Ecma-335.htm


/// <summary>
/// Gets the module name from a ModuleID
/// </summary>
std::wstring CCodeCoverage::GetModuleName(ModuleID moduleId)
{
    ULONG dwNameSize = 512;
    WCHAR szModuleName[512] = {};
    COM_FAIL_RETURN(m_profilerInfo->GetModuleInfo(moduleId, NULL, dwNameSize, &dwNameSize, szModuleName, NULL), std::wstring());
    return std::wstring(szModuleName);
}

/// <summary>
/// Get the assembly name from an AssemblyID
/// </summary>
std::wstring CCodeCoverage::GetAssemblyName(AssemblyID assemblyId)
{
    ULONG dwNameSize = 512; 
    WCHAR szAssemblyName[512] = {};
    COM_FAIL_RETURN(m_profilerInfo->GetAssemblyInfo(assemblyId, dwNameSize, &dwNameSize, szAssemblyName, NULL, NULL), std::wstring());
    return std::wstring(szAssemblyName);
}

/// <summary>
/// Get the function token, module ID and module name for a supplied FunctionID
/// </summary>
BOOL CCodeCoverage::GetTokenAndModule(FunctionID funcId, mdToken& functionToken, ModuleID& moduleId, std::wstring &moduleName)
{
    COM_FAIL_RETURN(m_profilerInfo2->GetFunctionInfo2(funcId, NULL, NULL, &moduleId, &functionToken, 0, NULL, NULL), FALSE);
    moduleName = GetModuleName(moduleId);
    return TRUE;
}

/// <summary>Get the token for an unmamaged method having a single I8 parameter</summary>
mdSignature CCodeCoverage::GetUnmanagedMethodSignatureToken_I8(ModuleID moduleID)
{
    static COR_SIGNATURE unmanagedCallSignature[] = 
    {
        IMAGE_CEE_CS_CALLCONV_DEFAULT,          // Default CallKind!
        0x01,                                   // Parameter count
        ELEMENT_TYPE_VOID,                      // Return type
        ELEMENT_TYPE_I8                         // Parameter type (I8)
    };

    CComPtr<IMetaDataEmit> metaDataEmit;
    COM_FAIL_RETURN(m_profilerInfo2->GetModuleMetaData(moduleID, ofWrite, IID_IMetaDataEmit, (IUnknown**) &metaDataEmit), 0);

    mdSignature pmsig ;
    COM_FAIL_RETURN(metaDataEmit->GetTokenFromSig(unmanagedCallSignature, sizeof(unmanagedCallSignature), &pmsig), 0);
    return pmsig;
}

/// <summary>Get the token for an unmamaged method having no parameters</summary>
mdSignature CCodeCoverage::GetUnmanagedMethodSignatureToken_(ModuleID moduleID)
{
    static COR_SIGNATURE unmanagedCallSignature[] = 
    {
        IMAGE_CEE_CS_CALLCONV_DEFAULT,          // Default CallKind!
        0x00,                                   // Parameter count
        ELEMENT_TYPE_VOID,                      // Return type
        ELEMENT_TYPE_VOID                       // Parameter type (void)
    };

    CComPtr<IMetaDataEmit> metaDataEmit;
    COM_FAIL_RETURN(m_profilerInfo2->GetModuleMetaData(moduleID, ofWrite, IID_IMetaDataEmit, (IUnknown**) &metaDataEmit), 0);

    mdSignature pmsig ;
    COM_FAIL_RETURN(metaDataEmit->GetTokenFromSig(unmanagedCallSignature, sizeof(unmanagedCallSignature), &pmsig), 0);
    return pmsig;
}


