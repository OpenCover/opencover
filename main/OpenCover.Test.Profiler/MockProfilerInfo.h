#pragma once
#include "TestProfilerInfo.h"

class MockProfilerInfo :
	public CTestProfilerInfo
{
public: // ICorProfilerInfo
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, GetClassFromObject, HRESULT(
		/* [in] */ ObjectID objectId,
		/* [out] */ ClassID *pClassId));

	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, GetClassFromToken, HRESULT(
		/* [in] */ ModuleID moduleId,
		/* [in] */ mdTypeDef typeDef,
		/* [out] */ ClassID *pClassId));

	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, GetCodeInfo, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [out] */ LPCBYTE *pStart,
		/* [out] */ ULONG *pcSize));

	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, GetEventMask, HRESULT(
		/* [out] */ DWORD *pdwEvents));

	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, GetFunctionFromIP, HRESULT(
		/* [in] */ LPCBYTE ip,
		/* [out] */ FunctionID *pFunctionId));

	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, GetFunctionFromToken, HRESULT(
		/* [in] */ ModuleID moduleId,
		/* [in] */ mdToken token,
		/* [out] */ FunctionID *pFunctionId));

	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, GetHandleFromThread, HRESULT(
		/* [in] */ ThreadID threadId,
		/* [out] */ HANDLE *phThread));

	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, GetObjectSize, HRESULT(
		/* [in] */ ObjectID objectId,
		/* [out] */ ULONG *pcSize));

	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, IsArrayClass, HRESULT(
		/* [in] */ ClassID classId,
		/* [out] */ CorElementType *pBaseElemType,
		/* [out] */ ClassID *pBaseClassId,
		/* [out] */ ULONG *pcRank));

	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, GetThreadInfo, HRESULT(
		/* [in] */ ThreadID threadId,
		/* [out] */ DWORD *pdwWin32ThreadId));

	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, GetCurrentThreadID, HRESULT(
		/* [out] */ ThreadID *pThreadId));

	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, GetClassIDInfo, HRESULT(
		/* [in] */ ClassID classId,
		/* [out] */ ModuleID *pModuleId,
		/* [out] */ mdTypeDef *pTypeDefToken));

	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, GetFunctionInfo, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [out] */ ClassID *pClassId,
		/* [out] */ ModuleID *pModuleId,
		/* [out] */ mdToken *pToken));

	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, SetEventMask, HRESULT(
		/* [in] */ DWORD dwEvents));

	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, SetEnterLeaveFunctionHooks, HRESULT(
		/* [in] */ FunctionEnter *pFuncEnter,
		/* [in] */ FunctionLeave *pFuncLeave,
		/* [in] */ FunctionTailcall *pFuncTailcall));

	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, SetFunctionIDMapper, HRESULT(
		/* [in] */ FunctionIDMapper *pFunc));

	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, GetTokenAndMetaDataFromFunction, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [in] */ REFIID riid,
		/* [out] */ IUnknown **ppImport,
		/* [out] */ mdToken *pToken));

	MOCK_METHOD6_WITH_CALLTYPE(STDMETHODCALLTYPE, GetModuleInfo, HRESULT(
		/* [in] */ ModuleID moduleId,
		/* [out] */ LPCBYTE *ppBaseLoadAddress,
		/* [in] */ ULONG cchName,
		/* [out] */ ULONG *pcchName,
		/* [annotation][out] */
		_Out_writes_to_(cchName, *pcchName)  WCHAR szName[],
		/* [out] */ AssemblyID *pAssemblyId));

	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, GetModuleMetaData, HRESULT(
		/* [in] */ ModuleID moduleId,
		/* [in] */ DWORD dwOpenFlags,
		/* [in] */ REFIID riid,
		/* [out] */ IUnknown **ppOut));

	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, GetILFunctionBody, HRESULT(
		/* [in] */ ModuleID moduleId,
		/* [in] */ mdMethodDef methodId,
		/* [out] */ LPCBYTE *ppMethodHeader,
		/* [out] */ ULONG *pcbMethodSize));

	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, GetILFunctionBodyAllocator, HRESULT(
		/* [in] */ ModuleID moduleId,
		/* [out] */ IMethodMalloc **ppMalloc));

	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, SetILFunctionBody, HRESULT(
		/* [in] */ ModuleID moduleId,
		/* [in] */ mdMethodDef methodid,
		/* [in] */ LPCBYTE pbNewILMethodHeader));

	MOCK_METHOD5_WITH_CALLTYPE(STDMETHODCALLTYPE, GetAppDomainInfo, HRESULT(
		/* [in] */ AppDomainID appDomainId,
		/* [in] */ ULONG cchName,
		/* [out] */ ULONG *pcchName,
		/* [annotation][out] */
		_Out_writes_to_(cchName, *pcchName)  WCHAR szName[],
		/* [out] */ ProcessID *pProcessId));

	MOCK_METHOD6_WITH_CALLTYPE(STDMETHODCALLTYPE, GetAssemblyInfo, HRESULT(
		/* [in] */ AssemblyID assemblyId,
		/* [in] */ ULONG cchName,
		/* [out] */ ULONG *pcchName,
		/* [annotation][out] */
		_Out_writes_to_(cchName, *pcchName)  WCHAR szName[],
		/* [out] */ AppDomainID *pAppDomainId,
		/* [out] */ ModuleID *pModuleId));

	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, SetFunctionReJIT, HRESULT(
		/* [in] */ FunctionID functionId));

	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, ForceGC, HRESULT(void));

	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, SetILInstrumentedCodeMap, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [in] */ BOOL fStartJit,
		/* [in] */ ULONG cILMapEntries,
		/* [size_is][in] */ COR_IL_MAP rgILMapEntries[]));

	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, GetInprocInspectionInterface, HRESULT(
		/* [out] */ IUnknown **ppicd));

	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, GetInprocInspectionIThisThread, HRESULT(
		/* [out] */ IUnknown **ppicd));

	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, GetThreadContext, HRESULT(
		/* [in] */ ThreadID threadId,
		/* [out] */ ContextID *pContextId));

	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, BeginInprocDebugging, HRESULT(
		/* [in] */ BOOL fThisThreadOnly,
		/* [out] */ DWORD *pdwProfilerContext));

	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, EndInprocDebugging, HRESULT(
		/* [in] */ DWORD dwProfilerContext));

	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, GetILToNativeMapping, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [in] */ ULONG32 cMap,
		/* [out] */ ULONG32 *pcMap,
		/* [length_is][size_is][out] */ COR_DEBUG_IL_TO_NATIVE_MAP map[]));

public: //ICorProfilerInfo2
	MOCK_METHOD6_WITH_CALLTYPE(STDMETHODCALLTYPE, DoStackSnapshot, HRESULT(
		/* [in] */ ThreadID thread,
		/* [in] */ StackSnapshotCallback *callback,
		/* [in] */ ULONG32 infoFlags,
		/* [in] */ void *clientData,
		/* [size_is][in] */ BYTE context[],
		/* [in] */ ULONG32 contextSize));

	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, SetEnterLeaveFunctionHooks2, HRESULT(
		/* [in] */ FunctionEnter2 *pFuncEnter,
		/* [in] */ FunctionLeave2 *pFuncLeave,
		/* [in] */ FunctionTailcall2 *pFuncTailcall));

	MOCK_METHOD8_WITH_CALLTYPE(STDMETHODCALLTYPE, GetFunctionInfo2, HRESULT(
		/* [in] */ FunctionID funcId,
		/* [in] */ COR_PRF_FRAME_INFO frameInfo,
		/* [out] */ ClassID *pClassId,
		/* [out] */ ModuleID *pModuleId,
		/* [out] */ mdToken *pToken,
		/* [in] */ ULONG32 cTypeArgs,
		/* [out] */ ULONG32 *pcTypeArgs,
		/* [out] */ ClassID typeArgs[]));

	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, GetStringLayout, HRESULT(
		/* [out] */ ULONG *pBufferLengthOffset,
		/* [out] */ ULONG *pStringLengthOffset,
		/* [out] */ ULONG *pBufferOffset));

	MOCK_METHOD5_WITH_CALLTYPE(STDMETHODCALLTYPE, GetClassLayout, HRESULT(
		/* [in] */ ClassID classID,
		/* [out][in] */ COR_FIELD_OFFSET rFieldOffset[],
		/* [in] */ ULONG cFieldOffset,
		/* [out] */ ULONG *pcFieldOffset,
		/* [out] */ ULONG *pulClassSize));

	MOCK_METHOD7_WITH_CALLTYPE(STDMETHODCALLTYPE, GetClassIDInfo2, HRESULT(
		/* [in] */ ClassID classId,
		/* [out] */ ModuleID *pModuleId,
		/* [out] */ mdTypeDef *pTypeDefToken,
		/* [out] */ ClassID *pParentClassId,
		/* [in] */ ULONG32 cNumTypeArgs,
		/* [out] */ ULONG32 *pcNumTypeArgs,
		/* [out] */ ClassID typeArgs[]));

	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, GetCodeInfo2, HRESULT(
		/* [in] */ FunctionID functionID,
		/* [in] */ ULONG32 cCodeInfos,
		/* [out] */ ULONG32 *pcCodeInfos,
		/* [length_is][size_is][out] */ COR_PRF_CODE_INFO codeInfos[]));

	MOCK_METHOD5_WITH_CALLTYPE(STDMETHODCALLTYPE, GetClassFromTokenAndTypeArgs, HRESULT(
		/* [in] */ ModuleID moduleID,
		/* [in] */ mdTypeDef typeDef,
		/* [in] */ ULONG32 cTypeArgs,
		/* [size_is][in] */ ClassID typeArgs[],
		/* [out] */ ClassID *pClassID));

	MOCK_METHOD6_WITH_CALLTYPE(STDMETHODCALLTYPE, GetFunctionFromTokenAndTypeArgs, HRESULT(
		/* [in] */ ModuleID moduleID,
		/* [in] */ mdMethodDef funcDef,
		/* [in] */ ClassID classId,
		/* [in] */ ULONG32 cTypeArgs,
		/* [size_is][in] */ ClassID typeArgs[],
		/* [out] */ FunctionID *pFunctionID));

	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, EnumModuleFrozenObjects, HRESULT(
		/* [in] */ ModuleID moduleID,
		/* [out] */ ICorProfilerObjectEnum **ppEnum));

	MOCK_METHOD5_WITH_CALLTYPE(STDMETHODCALLTYPE, GetArrayObjectInfo, HRESULT(
		/* [in] */ ObjectID objectId,
		/* [in] */ ULONG32 cDimensions,
		/* [size_is][out] */ ULONG32 pDimensionSizes[],
		/* [size_is][out] */ int pDimensionLowerBounds[],
		/* [out] */ BYTE **ppData));

	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, GetBoxClassLayout, HRESULT(
		/* [in] */ ClassID classId,
		/* [out] */ ULONG32 *pBufferOffset));

	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, GetThreadAppDomain, HRESULT(
		/* [in] */ ThreadID threadId,
		/* [out] */ AppDomainID *pAppDomainId));

	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, GetRVAStaticAddress, HRESULT(
		/* [in] */ ClassID classId,
		/* [in] */ mdFieldDef fieldToken,
		/* [out] */ void **ppAddress));

	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, GetAppDomainStaticAddress, HRESULT(
		/* [in] */ ClassID classId,
		/* [in] */ mdFieldDef fieldToken,
		/* [in] */ AppDomainID appDomainId,
		/* [out] */ void **ppAddress));

	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, GetThreadStaticAddress, HRESULT(
		/* [in] */ ClassID classId,
		/* [in] */ mdFieldDef fieldToken,
		/* [in] */ ThreadID threadId,
		/* [out] */ void **ppAddress));

	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, GetContextStaticAddress, HRESULT(
		/* [in] */ ClassID classId,
		/* [in] */ mdFieldDef fieldToken,
		/* [in] */ ContextID contextId,
		/* [out] */ void **ppAddress));

	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, GetStaticFieldInfo, HRESULT(
		/* [in] */ ClassID classId,
		/* [in] */ mdFieldDef fieldToken,
		/* [out] */ COR_PRF_STATIC_TYPE *pFieldInfo));

	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, GetGenerationBounds, HRESULT(
		/* [in] */ ULONG cObjectRanges,
		/* [out] */ ULONG *pcObjectRanges,
		/* [length_is][size_is][out] */ COR_PRF_GC_GENERATION_RANGE ranges[]));

	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, GetObjectGeneration, HRESULT(
		/* [in] */ ObjectID objectId,
		/* [out] */ COR_PRF_GC_GENERATION_RANGE *range));

	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, GetNotifiedExceptionClauseInfo, HRESULT(
		/* [out] */ COR_PRF_EX_CLAUSE_INFO *pinfo));

public: // ICorProfilerInfo3
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, EnumJITedFunctions, HRESULT(
		/* [out] */ ICorProfilerFunctionEnum **ppEnum));

	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, RequestProfilerDetach, HRESULT(
		/* [in] */ DWORD dwExpectedCompletionMilliseconds));

	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, SetFunctionIDMapper2, HRESULT(
		/* [in] */ FunctionIDMapper2 *pFunc,
		/* [in] */ void *clientData));

	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, GetStringLayout2, HRESULT(
		/* [out] */ ULONG *pStringLengthOffset,
		/* [out] */ ULONG *pBufferOffset));

	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, SetEnterLeaveFunctionHooks3, HRESULT(
		/* [in] */ FunctionEnter3 *pFuncEnter3,
		/* [in] */ FunctionLeave3 *pFuncLeave3,
		/* [in] */ FunctionTailcall3 *pFuncTailcall3));

	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, SetEnterLeaveFunctionHooks3WithInfo, HRESULT(
		/* [in] */ FunctionEnter3WithInfo *pFuncEnter3WithInfo,
		/* [in] */ FunctionLeave3WithInfo *pFuncLeave3WithInfo,
		/* [in] */ FunctionTailcall3WithInfo *pFuncTailcall3WithInfo));

	MOCK_METHOD5_WITH_CALLTYPE(STDMETHODCALLTYPE, GetFunctionEnter3Info, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [in] */ COR_PRF_ELT_INFO eltInfo,
		/* [out] */ COR_PRF_FRAME_INFO *pFrameInfo,
		/* [out][in] */ ULONG *pcbArgumentInfo,
		/* [size_is][out] */ COR_PRF_FUNCTION_ARGUMENT_INFO *pArgumentInfo));

	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, GetFunctionLeave3Info, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [in] */ COR_PRF_ELT_INFO eltInfo,
		/* [out] */ COR_PRF_FRAME_INFO *pFrameInfo,
		/* [out] */ COR_PRF_FUNCTION_ARGUMENT_RANGE *pRetvalRange));

	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, GetFunctionTailcall3Info, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [in] */ COR_PRF_ELT_INFO eltInfo,
		/* [out] */ COR_PRF_FRAME_INFO *pFrameInfo));

	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, EnumModules, HRESULT(
		/* [out] */ ICorProfilerModuleEnum **ppEnum));

	MOCK_METHOD9_WITH_CALLTYPE(STDMETHODCALLTYPE, GetRuntimeInformation, HRESULT(
		/* [out] */ USHORT *pClrInstanceId,
		/* [out] */ COR_PRF_RUNTIME_TYPE *pRuntimeType,
		/* [out] */ USHORT *pMajorVersion,
		/* [out] */ USHORT *pMinorVersion,
		/* [out] */ USHORT *pBuildNumber,
		/* [out] */ USHORT *pQFEVersion,
		/* [in] */ ULONG cchVersionString,
		/* [out] */ ULONG *pcchVersionString,
		/* [annotation][out] */
		_Out_writes_to_(cchVersionString, *pcchVersionString)  WCHAR szVersionString[]));

	MOCK_METHOD5_WITH_CALLTYPE(STDMETHODCALLTYPE, GetThreadStaticAddress2, HRESULT(
		/* [in] */ ClassID classId,
		/* [in] */ mdFieldDef fieldToken,
		/* [in] */ AppDomainID appDomainId,
		/* [in] */ ThreadID threadId,
		/* [out] */ void **ppAddress));

	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, GetAppDomainsContainingModule, HRESULT(
		/* [in] */ ModuleID moduleId,
		/* [in] */ ULONG32 cAppDomainIds,
		/* [out] */ ULONG32 *pcAppDomainIds,
		/* [length_is][size_is][out] */ AppDomainID appDomainIds[]));

	MOCK_METHOD7_WITH_CALLTYPE(STDMETHODCALLTYPE, GetModuleInfo2, HRESULT(
		/* [in] */ ModuleID moduleId,
		/* [out] */ LPCBYTE *ppBaseLoadAddress,
		/* [in] */ ULONG cchName,
		/* [out] */ ULONG *pcchName,
		/* [annotation][out] */
		_Out_writes_to_(cchName, *pcchName)  WCHAR szName[],
		/* [out] */ AssemblyID *pAssemblyId,
		/* [out] */ DWORD *pdwModuleFlags));

public: // ICorProfilerInfo4
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, EnumThreads, HRESULT(
		/* [out] */ ICorProfilerThreadEnum **ppEnum));

	MOCK_METHOD0_WITH_CALLTYPE(STDMETHODCALLTYPE, InitializeCurrentThread, HRESULT(void));

	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, RequestReJIT, HRESULT(
		/* [in] */ ULONG cFunctions,
		/* [size_is][in] */ ModuleID moduleIds[],
		/* [size_is][in] */ mdMethodDef methodIds[]));

	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, RequestRevert, HRESULT(
		/* [in] */ ULONG cFunctions,
		/* [size_is][in] */ ModuleID moduleIds[],
		/* [size_is][in] */ mdMethodDef methodIds[],
		/* [size_is][out] */ HRESULT status[]));

	MOCK_METHOD5_WITH_CALLTYPE(STDMETHODCALLTYPE, GetCodeInfo3, HRESULT(
		/* [in] */ FunctionID functionID,
		/* [in] */ ReJITID reJitId,
		/* [in] */ ULONG32 cCodeInfos,
		/* [out] */ ULONG32 *pcCodeInfos,
		/* [length_is][size_is][out] */ COR_PRF_CODE_INFO codeInfos[]));

	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, GetFunctionFromIP2, HRESULT(
		/* [in] */ LPCBYTE ip,
		/* [out] */ FunctionID *pFunctionId,
		/* [out] */ ReJITID *pReJitId));

	MOCK_METHOD4_WITH_CALLTYPE(STDMETHODCALLTYPE, GetReJITIDs, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [in] */ ULONG cReJitIds,
		/* [out] */ ULONG *pcReJitIds,
		/* [length_is][size_is][out] */ ReJITID reJitIds[]));

	MOCK_METHOD5_WITH_CALLTYPE(STDMETHODCALLTYPE, GetILToNativeMapping2, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [in] */ ReJITID reJitId,
		/* [in] */ ULONG32 cMap,
		/* [out] */ ULONG32 *pcMap,
		/* [length_is][size_is][out] */ COR_DEBUG_IL_TO_NATIVE_MAP map[]));

	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, EnumJITedFunctions2, HRESULT(
		/* [out] */ ICorProfilerFunctionEnum **ppEnum));

	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, GetObjectSize2, HRESULT(
		/* [in] */ ObjectID objectId,
		/* [out] */ SIZE_T *pcSize));

	// ICorProfilerInfo5
public:
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, GetEventMask2, HRESULT(
		/* [out] */ DWORD *pdwEventsLow,
		/* [out] */ DWORD *pdwEventsHigh));

	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, SetEventMask2, HRESULT(
		/* [in] */ DWORD dwEventsLow,
		/* [in] */ DWORD dwEventsHigh));

	// ICorProfilerInfo6
public:
	MOCK_METHOD5_WITH_CALLTYPE(STDMETHODCALLTYPE, EnumNgenModuleMethodsInliningThisMethod, HRESULT(
		/* [in] */ ModuleID inlinersModuleId,
		/* [in] */ ModuleID inlineeModuleId,
		/* [in] */ mdMethodDef inlineeMethodId,
		/* [out] */ BOOL *incompleteData,
		/* [out] */ ICorProfilerMethodEnum **ppEnum));

	// ICorProfilerInfo7
public:
	MOCK_METHOD1_WITH_CALLTYPE(STDMETHODCALLTYPE, ApplyMetaData, HRESULT(
		/* [in] */ ModuleID moduleId));

	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, GetInMemorySymbolsLength, HRESULT(
		/* [in] */ ModuleID moduleId,
		/* [out] */ DWORD *pCountSymbolBytes));

	MOCK_METHOD5_WITH_CALLTYPE(STDMETHODCALLTYPE, ReadInMemorySymbols, HRESULT(
		/* [in] */ ModuleID moduleId,
		/* [in] */ DWORD symbolsReadOffset,
		/* [out] */ BYTE *pSymbolBytes,
		/* [in] */ DWORD countSymbolBytes,
		/* [out] */ DWORD *pCountSymbolBytesRead));

	// ICorProfilerInfo8
public:
	MOCK_METHOD2_WITH_CALLTYPE(STDMETHODCALLTYPE, IsFunctionDynamic, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [out] */ BOOL *isDynamic));

	MOCK_METHOD3_WITH_CALLTYPE(STDMETHODCALLTYPE, GetFunctionFromIP3, HRESULT(
		/* [in] */ LPCBYTE ip,
		/* [out] */ FunctionID *functionId,
		/* [out] */ ReJITID *pReJitId));

	MOCK_METHOD7_WITH_CALLTYPE(STDMETHODCALLTYPE, GetDynamicFunctionInfo, HRESULT(
		/* [in] */ FunctionID functionId,
		/* [out] */ ModuleID *moduleId,
		/* [out] */ PCCOR_SIGNATURE *ppvSig,
		/* [out] */ ULONG *pbSig,
		/* [in] */ ULONG cchName,
		/* [out] */ ULONG *pcchName,
		/* [out] */ WCHAR wszName[]));
};
