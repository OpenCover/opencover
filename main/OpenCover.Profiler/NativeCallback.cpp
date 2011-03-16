#include "StdAfx.h"
#include "NativeCallback.h"
#include "CodeCoverage.h"

// http://msdn.microsoft.com/en-us/library/aa964981.aspx
void __stdcall FunctionEnter2Global(
    /*[in]*/FunctionID                          funcID, 
    /*[in]*/UINT_PTR                            clientData, 
    /*[in]*/COR_PRF_FRAME_INFO                  func, 
    /*[in]*/COR_PRF_FUNCTION_ARGUMENT_INFO      *argumentInfo)
{
    std::wstring fullMethodName = CCodeCoverage::g_pProfiler->GetFullMethodName(funcID);
    ATLTRACE(_T("FunctionEnter2Global(%x => %s)"), funcID, W2CT(fullMethodName.c_str()));

}

// http://msdn.microsoft.com/en-us/library/aa964942.aspx
void __stdcall FunctionLeave2Global(
    /*[in]*/FunctionID                          funcID, 
    /*[in]*/UINT_PTR                            clientData, 
    /*[in]*/COR_PRF_FRAME_INFO                  func, 
    /*[in]*/COR_PRF_FUNCTION_ARGUMENT_RANGE     *retvalRange)
{
    std::wstring fullMethodName = CCodeCoverage::g_pProfiler->GetFullMethodName(funcID);
    ATLTRACE(_T("FunctionLeave2Global(%x => %s)"), funcID, W2CT(fullMethodName.c_str()));
}

// http://msdn.microsoft.com/en-us/library/aa964754.aspx
void __stdcall FunctionTailcall2Global(
    /*[in]*/FunctionID                          funcID, 
    /*[in]*/UINT_PTR                            clientData, 
    /*[in]*/COR_PRF_FRAME_INFO                  func)
{
    std::wstring fullMethodName = CCodeCoverage::g_pProfiler->GetFullMethodName(funcID);
    ATLTRACE(_T("FunctionTailcall2Global2(%x => %s)"), funcID, W2CT(fullMethodName.c_str()));
}

#if defined(_WIN64)
void _FunctionEnter2(
    FunctionID funcID, 
    UINT_PTR clientData, 
    COR_PRF_FRAME_INFO func, 
    COR_PRF_FUNCTION_ARGUMENT_INFO *argumentInfo)
{
	FunctionEnter2Global(funcID, clientData, func, argumentInfo);
}

void _FunctionLeave2(
    FunctionID funcID, 
    UINT_PTR clientData, 
    COR_PRF_FRAME_INFO func, 
    COR_PRF_FUNCTION_ARGUMENT_RANGE *retvalRange)
{
	FunctionLeave2Global(funcID, clientData, func, retvalRange);
}

void _FunctionTailcall2(
    FunctionID funcID, 
    UINT_PTR clientData, 
    COR_PRF_FRAME_INFO func)
{
	FunctionTailcall2Global(funcID, clientData, func);
}
#else
void _declspec(naked) _FunctionEnter2(
    FunctionID funcID, 
    UINT_PTR clientData, 
    COR_PRF_FRAME_INFO func, 
    COR_PRF_FUNCTION_ARGUMENT_INFO *argumentInfo)
{
    __asm
    {
        push    ebp                 
        mov     ebp,esp
        pushad

        mov     eax,[ebp+0x14]      //argumentInfo
        push    eax
        mov     ecx,[ebp+0x10]      //func
        push    ecx
        mov     edx,[ebp+0x0C]      //clientData
        push    edx
        mov     eax,[ebp+0x08]      //funcID
        push    eax
        call    FunctionEnter2Global

        popad
        pop     ebp
        ret     16
    }
}

void _declspec(naked) _FunctionLeave2(
    FunctionID funcID, 
    UINT_PTR clientData, 
    COR_PRF_FRAME_INFO func, 
    COR_PRF_FUNCTION_ARGUMENT_RANGE *retvalRange)
{
    __asm
    {
        push    ebp
        mov     ebp,esp
        pushad

        mov     eax,[ebp+0x14]      //argumentInfo
        push    eax
        mov     ecx,[ebp+0x10]      //func
        push    ecx
        mov     edx,[ebp+0x0C]      //clientData
        push    edx
        mov     eax,[ebp+0x08]      //funcID
        push    eax
        call    FunctionLeave2Global

        popad
        pop     ebp
        ret     16
    }
}

void _declspec(naked) _FunctionTailcall2(
    FunctionID funcID, 
    UINT_PTR clientData, 
    COR_PRF_FRAME_INFO func)
{
    __asm
    {
        push    ebp
        mov     ebp,esp
        pushad

        mov     ecx,[ebp+0x10]      //func
        push    ecx
        mov     edx,[ebp+0x0C]      //clientData
        push    edx
        mov     eax,[ebp+0x08]      //funcID
        push    eax
        call    FunctionTailcall2Global

        popad
        pop     ebp
        ret     12
    }
}
#endif
