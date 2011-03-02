#include "stdafx.h"
#include "CodeCoverage.h"

// reference http://www.ecma-international.org/publications/standards/Ecma-335.htm

#define COM_FAIL_RETURN(hr, ret) if (!SUCCEEDED(hr)) return ret
#define COM_FAIL(hr) if (!SUCCEEDED(hr)) return

/// <summary>
/// Gets the module name from a moduleId
/// </summary>
std::wstring CCodeCoverage::GetModuleName(ModuleID moduleId)
{
	ULONG dwNameSize = 512;
	WCHAR szModuleName[512];
	COM_FAIL_RETURN(m_profilerInfo->GetModuleInfo(moduleId, NULL, dwNameSize, &dwNameSize, szModuleName, NULL), std::wstring());
	return std::wstring(szModuleName);
}

std::wstring CCodeCoverage::GetAssemblyName(AssemblyID assemblyId)
{
	ULONG dwNameSize = 512; 
	WCHAR szAssemblyName[512];
	COM_FAIL_RETURN(m_profilerInfo->GetAssemblyInfo(assemblyId, dwNameSize, &dwNameSize, szAssemblyName, NULL, NULL), std::wstring());
	return std::wstring(szAssemblyName);
}

void CCodeCoverage::GetGenericSignature(mdTypeDef tokenTypeDef, IMetaDataImport2* metaDataImport2, std::wstring &className)
{
	HCORENUM hEnum = 0;
	mdGenericParam genericParams[128] = {0}; 
	ULONG genericParamCount = 128;
	
	HRESULT hr = S_OK;
	COM_FAIL(hr = metaDataImport2->EnumGenericParams(&hEnum, tokenTypeDef, genericParams, genericParamCount, &genericParamCount));
	if (hr==S_FALSE) return;

	if (genericParamCount > 0)
	{
		std::wstring genericSignature(L"<");
		for(ULONG g = 0; g < genericParamCount; ++g)
		{
			if (g > 0) genericSignature.append(L", ");
			
			ULONG genericNameLength = 512;
			WCHAR szGenericName[512];
			COM_FAIL(metaDataImport2->GetGenericParamProps(genericParams[g], NULL, NULL, NULL, NULL, szGenericName, genericNameLength, &genericNameLength));
			genericSignature.append(szGenericName);
		}
		genericSignature.append(L">");
		className.append(genericSignature);
	}
}


std::wstring CCodeCoverage::GetClassName(ModuleID moduleId, mdTypeDef tokenTypeDef)
{
 	CComPtr<IMetaDataImport2> metaDataImport;
	m_profilerInfo->GetModuleMetaData(moduleId, ofRead, IID_IMetaDataImport2, (IUnknown**) &metaDataImport);
	return GetClassName(tokenTypeDef, metaDataImport);
}

std::wstring CCodeCoverage::GetClassName(mdTypeDef tokenTypeDef, IMetaDataImport2* metaDataImport2)
{
	ULONG dwNameSize = 512;
	WCHAR szClassName[512];
	std::wstring className;
	DWORD typeDefFlags = 0;

	COM_FAIL_RETURN(metaDataImport2->GetTypeDefProps(tokenTypeDef, szClassName, dwNameSize, &dwNameSize, &typeDefFlags, NULL), className);
	className = szClassName;

	GetGenericSignature(tokenTypeDef, metaDataImport2, className);

	if (!IsTdNested(typeDefFlags)) 
		return className;
	
	mdTypeDef parentTypeDef;
	COM_FAIL_RETURN(metaDataImport2->GetNestedClassProps(tokenTypeDef, &parentTypeDef), className);

	return GetClassName(parentTypeDef, metaDataImport2) + L"+" + className;
}
