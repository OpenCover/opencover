#include "stdafx.h"
#include "CodeCoverage.h"

// reference http://www.ecma-international.org/publications/standards/Ecma-335.htm


/// <summary>
/// Gets the module name from a moduleId
/// </summary>
std::wstring CCodeCoverage::GetModuleName(ModuleID moduleId)
{
    ULONG dwNameSize = 512;
    WCHAR szModuleName[512] = {};
    COM_FAIL_RETURN(m_profilerInfo->GetModuleInfo(moduleId, NULL, dwNameSize, &dwNameSize, szModuleName, NULL), std::wstring());
    return std::wstring(szModuleName);
}

std::wstring CCodeCoverage::GetAssemblyName(AssemblyID assemblyId)
{
    ULONG dwNameSize = 512; 
    WCHAR szAssemblyName[512] = {};
    COM_FAIL_RETURN(m_profilerInfo->GetAssemblyInfo(assemblyId, dwNameSize, &dwNameSize, szAssemblyName, NULL, NULL), std::wstring());
    return std::wstring(szAssemblyName);
}

BOOL CCodeCoverage::GetTokenAndModule(FunctionID funcId, mdToken& functionToken, ModuleID& moduleId, std::wstring &moduleName)
{
    COM_FAIL_RETURN(m_profilerInfo2->GetFunctionInfo2(funcId, NULL, NULL, &moduleId, &functionToken, 0, NULL, NULL), FALSE);
    moduleName = GetModuleName(moduleId);
    return TRUE;
}

