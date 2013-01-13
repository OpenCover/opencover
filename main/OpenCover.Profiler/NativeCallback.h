#pragma once

#ifdef __cplusplus
extern "C" {
#endif

void __stdcall _FunctionEnter2(
    FunctionID funcID, 
    UINT_PTR clientData, 
    COR_PRF_FRAME_INFO func, 
    COR_PRF_FUNCTION_ARGUMENT_INFO *argumentInfo);

void __stdcall _FunctionLeave2(
    FunctionID funcID, 
    UINT_PTR clientData, 
    COR_PRF_FRAME_INFO func, 
    COR_PRF_FUNCTION_ARGUMENT_RANGE *retvalRange);

void __stdcall _FunctionTailcall2(
    FunctionID funcID, 
    UINT_PTR clientData, 
    COR_PRF_FRAME_INFO func);

#ifdef __cplusplus
}
#endif