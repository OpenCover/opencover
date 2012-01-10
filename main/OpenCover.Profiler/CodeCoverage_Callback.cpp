//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
// CodeCoverage.cpp : Implementation of CCodeCoverage

#include "stdafx.h"
#include "CodeCoverage.h"

UINT_PTR CCodeCoverage::FunctionMapper2(FunctionID functionId, void* clientData, BOOL* pbHookFunction)
{
    CCodeCoverage* profiler = static_cast<CCodeCoverage*>(clientData);
    *pbHookFunction = FALSE;
    if(profiler == NULL)
        return 0;

    UINT_PTR retVal = 0;
    std::wstring modulePath;
    mdToken functionToken;
    ModuleID moduleId;
    AssemblyID assemblyId;

    if (profiler->GetTokenAndModule(functionId, functionToken, moduleId, modulePath, &assemblyId))
    {
        ULONG uniqueId;
        if (profiler->m_host.TrackMethod(functionToken, (LPWSTR)modulePath.c_str(), 
            (LPWSTR)profiler->m_allowModulesAssemblyMap[modulePath].c_str(), uniqueId))
        {
            *pbHookFunction = TRUE;
            retVal = uniqueId;
        }
    }
    
    return retVal;
}

UINT_PTR CCodeCoverage::FunctionMapper(FunctionID functionId, BOOL* pbHookFunction)
{
    return FunctionMapper2(functionId, g_pProfiler, pbHookFunction);
}

void CCodeCoverage::FunctionEnter2(
    /*[in]*/FunctionID                          funcID, 
    /*[in]*/UINT_PTR                            clientData, 
    /*[in]*/COR_PRF_FRAME_INFO                  func, 
    /*[in]*/COR_PRF_FUNCTION_ARGUMENT_INFO      *argumentInfo)
{
    m_host.AddTestEnterPoint((ULONG)clientData);
}

void CCodeCoverage::FunctionLeave2(
    /*[in]*/FunctionID                          funcID, 
    /*[in]*/UINT_PTR                            clientData, 
    /*[in]*/COR_PRF_FRAME_INFO                  func, 
    /*[in]*/COR_PRF_FUNCTION_ARGUMENT_RANGE     *retvalRange)
{
    m_host.AddTestLeavePoint((ULONG)clientData);
}

void CCodeCoverage::FunctionTailcall2(
    /*[in]*/FunctionID                          funcID, 
    /*[in]*/UINT_PTR                            clientData, 
    /*[in]*/COR_PRF_FRAME_INFO                  func)
{
    m_host.AddTestTailcallPoint((ULONG)clientData);
}
