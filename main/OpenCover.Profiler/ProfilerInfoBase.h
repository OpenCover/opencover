#pragma once
#include "resource.h"       // main symbols

#include "OpenCoverProfiler_i.h"

using namespace ATL;

class CProfilerInfoBase : public ICorProfilerInfo8
{
public:
    virtual ~CProfilerInfoBase()
    {
    }

    void ChainProfilerInfo(IUnknown *pICorProfilerInfoUnk){
		m_pProfilerInfo = pICorProfilerInfoUnk;
		m_pProfilerInfo2 = pICorProfilerInfoUnk;
		m_pProfilerInfo3 = pICorProfilerInfoUnk;
		m_pProfilerInfo4 = pICorProfilerInfoUnk;
        m_pProfilerInfo5 = pICorProfilerInfoUnk;
        m_pProfilerInfo6 = pICorProfilerInfoUnk;
		m_pProfilerInfo7 = pICorProfilerInfoUnk;
		m_pProfilerInfo8 = pICorProfilerInfoUnk;
	}

private:
	CComQIPtr<ICorProfilerInfo> m_pProfilerInfo;
	CComQIPtr<ICorProfilerInfo2> m_pProfilerInfo2;
	CComQIPtr<ICorProfilerInfo3> m_pProfilerInfo3;
	CComQIPtr<ICorProfilerInfo4> m_pProfilerInfo4;
    CComQIPtr<ICorProfilerInfo5> m_pProfilerInfo5;
    CComQIPtr<ICorProfilerInfo6> m_pProfilerInfo6;
	CComQIPtr<ICorProfilerInfo7> m_pProfilerInfo7;
	CComQIPtr<ICorProfilerInfo8> m_pProfilerInfo8;

public: // ICorProfilerInfo
	virtual HRESULT STDMETHODCALLTYPE GetClassFromObject(
		/* [in] */ ObjectID objectId,
		/* [out] */ ClassID *pClassId) override
	{
		//ATLTRACE(_T("GetClassFromObject"));
		return m_pProfilerInfo->GetClassFromObject(objectId, pClassId);
	}

	virtual HRESULT STDMETHODCALLTYPE GetClassFromToken(
		/* [in] */ ModuleID moduleId,
		/* [in] */ mdTypeDef typeDef,
		/* [out] */ ClassID *pClassId) override
	{
		//ATLTRACE(_T("GetClassFromToken"));
		return m_pProfilerInfo->GetClassFromToken(moduleId, typeDef, pClassId);
	}

	virtual HRESULT STDMETHODCALLTYPE GetCodeInfo(
		/* [in] */ FunctionID functionId,
		/* [out] */ LPCBYTE *pStart,
		/* [out] */ ULONG *pcSize) override
	{
		//ATLTRACE(_T("GetCodeInfo"));
		return m_pProfilerInfo->GetCodeInfo(functionId, pStart, pcSize);
	}

	virtual HRESULT STDMETHODCALLTYPE GetEventMask(
		/* [out] */ DWORD *pdwEvents) override
	{
		//ATLTRACE(_T("GetEventMask"));
		return m_pProfilerInfo->GetEventMask(pdwEvents);
	}

	virtual HRESULT STDMETHODCALLTYPE GetFunctionFromIP(
		/* [in] */ LPCBYTE ip,
		/* [out] */ FunctionID *pFunctionId) override
	{
		//ATLTRACE(_T("GetFunctionFromIP"));
		return m_pProfilerInfo->GetFunctionFromIP(ip, pFunctionId);
	}

	virtual HRESULT STDMETHODCALLTYPE GetFunctionFromToken(
		/* [in] */ ModuleID moduleId,
		/* [in] */ mdToken token,
		/* [out] */ FunctionID *pFunctionId) override
	{
		//ATLTRACE(_T("GetFunctionFromToken"));
		return m_pProfilerInfo->GetFunctionFromToken(moduleId, token, pFunctionId);
	}

	virtual HRESULT STDMETHODCALLTYPE GetHandleFromThread(
		/* [in] */ ThreadID threadId,
		/* [out] */ HANDLE *phThread) override
	{
		//ATLTRACE(_T("GetHandleFromThread"));
		return m_pProfilerInfo->GetHandleFromThread(threadId, phThread);
	}

	virtual HRESULT STDMETHODCALLTYPE GetObjectSize(
		/* [in] */ ObjectID objectId,
		/* [out] */ ULONG *pcSize) override
	{
		//ATLTRACE(_T("GetObjectSize"));
		return m_pProfilerInfo->GetObjectSize(objectId, pcSize);
	}

	virtual HRESULT STDMETHODCALLTYPE IsArrayClass(
		/* [in] */ ClassID classId,
		/* [out] */ CorElementType *pBaseElemType,
		/* [out] */ ClassID *pBaseClassId,
		/* [out] */ ULONG *pcRank) override
	{
		//ATLTRACE(_T("IsArrayClass"));
		return m_pProfilerInfo->IsArrayClass(classId, pBaseElemType, pBaseClassId, pcRank);
	}

	virtual HRESULT STDMETHODCALLTYPE GetThreadInfo(
		/* [in] */ ThreadID threadId,
		/* [out] */ DWORD *pdwWin32ThreadId) override
	{
		//ATLTRACE(_T("GetThreadInfo"));
		return m_pProfilerInfo->GetThreadInfo(threadId, pdwWin32ThreadId);
	}

	virtual HRESULT STDMETHODCALLTYPE GetCurrentThreadID(
		/* [out] */ ThreadID *pThreadId) override
	{
		//ATLTRACE(_T("GetCurrentThreadID"));
		return m_pProfilerInfo->GetCurrentThreadID(pThreadId);
	}

	virtual HRESULT STDMETHODCALLTYPE GetClassIDInfo(
		/* [in] */ ClassID classId,
		/* [out] */ ModuleID *pModuleId,
		/* [out] */ mdTypeDef *pTypeDefToken) override
	{
		//ATLTRACE(_T("GetClassIDInfo"));
		return m_pProfilerInfo->GetClassIDInfo(classId, pModuleId, pTypeDefToken);
	}

	virtual HRESULT STDMETHODCALLTYPE GetFunctionInfo(
		/* [in] */ FunctionID functionId,
		/* [out] */ ClassID *pClassId,
		/* [out] */ ModuleID *pModuleId,
		/* [out] */ mdToken *pToken) override
	{
		//ATLTRACE(_T("GetFunctionInfo"));
		return m_pProfilerInfo->GetFunctionInfo(functionId, pClassId, pModuleId, pToken);
	}

	virtual HRESULT STDMETHODCALLTYPE SetEventMask(
		/* [in] */ DWORD dwEvents) override
	{
		ATLTRACE(_T("CProfilerInfoBase::SetEventMask(0x%X)"), dwEvents);
		return m_pProfilerInfo->SetEventMask(dwEvents);
	}

	virtual HRESULT STDMETHODCALLTYPE SetEnterLeaveFunctionHooks(
		/* [in] */ FunctionEnter *pFuncEnter,
		/* [in] */ FunctionLeave *pFuncLeave,
		/* [in] */ FunctionTailcall *pFuncTailcall) override
	{
		//ATLTRACE(_T("SetEnterLeaveFunctionHooks"));
		return m_pProfilerInfo->SetEnterLeaveFunctionHooks(pFuncEnter, pFuncLeave, pFuncTailcall);
	}

	virtual HRESULT STDMETHODCALLTYPE SetFunctionIDMapper(
		/* [in] */ FunctionIDMapper *pFunc) override
	{
		//ATLTRACE(_T("SetFunctionIDMapper"));
		return m_pProfilerInfo->SetFunctionIDMapper(pFunc);
	}

	virtual HRESULT STDMETHODCALLTYPE GetTokenAndMetaDataFromFunction(
		/* [in] */ FunctionID functionId,
		/* [in] */ REFIID riid,
		/* [out] */ IUnknown **ppImport,
		/* [out] */ mdToken *pToken) override
	{
		//ATLTRACE(_T("GetTokenAndMetaDataFromFunction"));
		return m_pProfilerInfo->GetTokenAndMetaDataFromFunction(functionId, riid, ppImport, pToken);
	}

	virtual HRESULT STDMETHODCALLTYPE GetModuleInfo(
		/* [in] */ ModuleID moduleId,
		/* [out] */ LPCBYTE *ppBaseLoadAddress,
		/* [in] */ ULONG cchName,
		/* [out] */ ULONG *pcchName,
		/* [annotation][out] */
		_Out_writes_to_(cchName, *pcchName)  WCHAR szName[],
		/* [out] */ AssemblyID *pAssemblyId) override
	{
		//ATLTRACE(_T("GetModuleInfo"));
		return m_pProfilerInfo->GetModuleInfo(moduleId, ppBaseLoadAddress, cchName, pcchName, szName, pAssemblyId);
	}

	virtual HRESULT STDMETHODCALLTYPE GetModuleMetaData(
		/* [in] */ ModuleID moduleId,
		/* [in] */ DWORD dwOpenFlags,
		/* [in] */ REFIID riid,
		/* [out] */ IUnknown **ppOut) override
	{
		//ATLTRACE(_T("GetModuleMetaData"));
		return m_pProfilerInfo->GetModuleMetaData(moduleId, dwOpenFlags, riid, ppOut);
	}

	virtual HRESULT STDMETHODCALLTYPE GetILFunctionBody(
		/* [in] */ ModuleID moduleId,
		/* [in] */ mdMethodDef methodId,
		/* [out] */ LPCBYTE *ppMethodHeader,
		/* [out] */ ULONG *pcbMethodSize) override
	{
		//ATLTRACE(_T("GetILFunctionBody"));
		return m_pProfilerInfo->GetILFunctionBody(moduleId, methodId, ppMethodHeader, pcbMethodSize);
	}

	virtual HRESULT STDMETHODCALLTYPE GetILFunctionBodyAllocator(
		/* [in] */ ModuleID moduleId,
		/* [out] */ IMethodMalloc **ppMalloc) override
	{
		//ATLTRACE(_T("GetILFunctionBodyAllocator"));
		return m_pProfilerInfo->GetILFunctionBodyAllocator(moduleId, ppMalloc);
	}

	virtual HRESULT STDMETHODCALLTYPE SetILFunctionBody(
		/* [in] */ ModuleID moduleId,
		/* [in] */ mdMethodDef methodid,
		/* [in] */ LPCBYTE pbNewILMethodHeader) override
	{
		//ATLTRACE(_T("SetILFunctionBody"));
		return m_pProfilerInfo->SetILFunctionBody(moduleId, methodid, pbNewILMethodHeader);
	}

	virtual HRESULT STDMETHODCALLTYPE GetAppDomainInfo(
		/* [in] */ AppDomainID appDomainId,
		/* [in] */ ULONG cchName,
		/* [out] */ ULONG *pcchName,
		/* [annotation][out] */
		_Out_writes_to_(cchName, *pcchName)  WCHAR szName[],
		/* [out] */ ProcessID *pProcessId) override
	{
		//ATLTRACE(_T("GetAppDomainInfo"));
		return m_pProfilerInfo->GetAppDomainInfo(appDomainId, cchName, pcchName, szName, pProcessId);
	}

	virtual HRESULT STDMETHODCALLTYPE GetAssemblyInfo(
		/* [in] */ AssemblyID assemblyId,
		/* [in] */ ULONG cchName,
		/* [out] */ ULONG *pcchName,
		/* [annotation][out] */
		_Out_writes_to_(cchName, *pcchName)  WCHAR szName[],
		/* [out] */ AppDomainID *pAppDomainId,
		/* [out] */ ModuleID *pModuleId) override
	{
		//ATLTRACE(_T("GetAssemblyInfo"));
		return m_pProfilerInfo->GetAssemblyInfo(assemblyId, cchName, pcchName, szName, pAppDomainId, pModuleId);
	}

	virtual HRESULT STDMETHODCALLTYPE SetFunctionReJIT(
		/* [in] */ FunctionID functionId) override
	{
		//ATLTRACE(_T("SetFunctionReJIT"));
		return m_pProfilerInfo->SetFunctionReJIT(functionId);
	}

	virtual HRESULT STDMETHODCALLTYPE ForceGC(void) override
	{
		//ATLTRACE(_T("GetClassFromObject"));
		return m_pProfilerInfo->ForceGC();
	}

	virtual HRESULT STDMETHODCALLTYPE SetILInstrumentedCodeMap(
		/* [in] */ FunctionID functionId,
		/* [in] */ BOOL fStartJit,
		/* [in] */ ULONG cILMapEntries,
		/* [size_is][in] */ COR_IL_MAP rgILMapEntries[]) override
	{
		//ATLTRACE(_T("SetILInstrumentedCodeMap"));
		return m_pProfilerInfo->SetILInstrumentedCodeMap(functionId, fStartJit, cILMapEntries, rgILMapEntries);
	}

	virtual HRESULT STDMETHODCALLTYPE GetInprocInspectionInterface(
		/* [out] */ IUnknown **ppicd) override
	{
		//ATLTRACE(_T("GetInprocInspectionInterface"));
		return m_pProfilerInfo->GetInprocInspectionInterface(ppicd);
	}

	virtual HRESULT STDMETHODCALLTYPE GetInprocInspectionIThisThread(
		/* [out] */ IUnknown **ppicd) override
	{
		//ATLTRACE(_T("GetInprocInspectionIThisThread"));
		return m_pProfilerInfo->GetInprocInspectionIThisThread(ppicd);
	}

	virtual HRESULT STDMETHODCALLTYPE GetThreadContext(
		/* [in] */ ThreadID threadId,
		/* [out] */ ContextID *pContextId) override
	{
		//ATLTRACE(_T("GetThreadContext"));
		return m_pProfilerInfo->GetThreadContext(threadId, pContextId);
	}

	virtual HRESULT STDMETHODCALLTYPE BeginInprocDebugging(
		/* [in] */ BOOL fThisThreadOnly,
		/* [out] */ DWORD *pdwProfilerContext) override
	{
		//ATLTRACE(_T("BeginInprocDebugging"));
		return m_pProfilerInfo->BeginInprocDebugging(fThisThreadOnly, pdwProfilerContext);
	}

	virtual HRESULT STDMETHODCALLTYPE EndInprocDebugging(
		/* [in] */ DWORD dwProfilerContext) override
	{
		//ATLTRACE(_T("EndInprocDebugging"));
		return m_pProfilerInfo->EndInprocDebugging(dwProfilerContext);
	}

	virtual HRESULT STDMETHODCALLTYPE GetILToNativeMapping(
		/* [in] */ FunctionID functionId,
		/* [in] */ ULONG32 cMap,
		/* [out] */ ULONG32 *pcMap,
		/* [length_is][size_is][out] */ COR_DEBUG_IL_TO_NATIVE_MAP map[]) override
	{
		//ATLTRACE(_T("GetILToNativeMapping"));
		return m_pProfilerInfo->GetILToNativeMapping(functionId, cMap, pcMap, map);
	}

public: //ICorProfilerInfo2
	virtual HRESULT STDMETHODCALLTYPE DoStackSnapshot(
		/* [in] */ ThreadID thread,
		/* [in] */ StackSnapshotCallback *callback,
		/* [in] */ ULONG32 infoFlags,
		/* [in] */ void *clientData,
		/* [size_is][in] */ BYTE context[],
		/* [in] */ ULONG32 contextSize) override
	{
		//ATLTRACE(_T("DoStackSnapshot"));
		return m_pProfilerInfo2->DoStackSnapshot(thread, callback, infoFlags, clientData, context, contextSize);
	}

	virtual HRESULT STDMETHODCALLTYPE SetEnterLeaveFunctionHooks2(
		/* [in] */ FunctionEnter2 *pFuncEnter,
		/* [in] */ FunctionLeave2 *pFuncLeave,
		/* [in] */ FunctionTailcall2 *pFuncTailcall) override
	{
		//ATLTRACE(_T("SetEnterLeaveFunctionHooks2"));
		return m_pProfilerInfo2->SetEnterLeaveFunctionHooks2(pFuncEnter, pFuncLeave, pFuncTailcall);
	}

	virtual HRESULT STDMETHODCALLTYPE GetFunctionInfo2(
		/* [in] */ FunctionID funcId,
		/* [in] */ COR_PRF_FRAME_INFO frameInfo,
		/* [out] */ ClassID *pClassId,
		/* [out] */ ModuleID *pModuleId,
		/* [out] */ mdToken *pToken,
		/* [in] */ ULONG32 cTypeArgs,
		/* [out] */ ULONG32 *pcTypeArgs,
		/* [out] */ ClassID typeArgs[]) override
	{
		//ATLTRACE(_T("GetFunctionInfo2"));
		return m_pProfilerInfo2->GetFunctionInfo2(funcId, frameInfo, pClassId, pModuleId, pToken, cTypeArgs, pcTypeArgs, typeArgs);
	}

	virtual HRESULT STDMETHODCALLTYPE GetStringLayout(
		/* [out] */ ULONG *pBufferLengthOffset,
		/* [out] */ ULONG *pStringLengthOffset,
		/* [out] */ ULONG *pBufferOffset) override
	{
		//ATLTRACE(_T("GetStringLayout"));
		return m_pProfilerInfo2->GetStringLayout(pBufferLengthOffset, pStringLengthOffset, pBufferOffset);
	}

	virtual HRESULT STDMETHODCALLTYPE GetClassLayout(
		/* [in] */ ClassID classID,
		/* [out][in] */ COR_FIELD_OFFSET rFieldOffset[],
		/* [in] */ ULONG cFieldOffset,
		/* [out] */ ULONG *pcFieldOffset,
		/* [out] */ ULONG *pulClassSize) override
	{
		//ATLTRACE(_T("GetClassLayout"));
		return m_pProfilerInfo2->GetClassLayout(classID, rFieldOffset, cFieldOffset, pcFieldOffset, pulClassSize);
	}

	virtual HRESULT STDMETHODCALLTYPE GetClassIDInfo2(
		/* [in] */ ClassID classId,
		/* [out] */ ModuleID *pModuleId,
		/* [out] */ mdTypeDef *pTypeDefToken,
		/* [out] */ ClassID *pParentClassId,
		/* [in] */ ULONG32 cNumTypeArgs,
		/* [out] */ ULONG32 *pcNumTypeArgs,
		/* [out] */ ClassID typeArgs[]) override
	{
		//ATLTRACE(_T("GetClassIDInfo2"));
		return m_pProfilerInfo2->GetClassIDInfo2(classId, pModuleId, pTypeDefToken, pParentClassId, cNumTypeArgs, pcNumTypeArgs, typeArgs);
	}

	virtual HRESULT STDMETHODCALLTYPE GetCodeInfo2(
		/* [in] */ FunctionID functionID,
		/* [in] */ ULONG32 cCodeInfos,
		/* [out] */ ULONG32 *pcCodeInfos,
		/* [length_is][size_is][out] */ COR_PRF_CODE_INFO codeInfos[]) override
	{
		//ATLTRACE(_T("GetCodeInfo2"));
		return m_pProfilerInfo2->GetCodeInfo2(functionID, cCodeInfos, pcCodeInfos, codeInfos);
	}

	virtual HRESULT STDMETHODCALLTYPE GetClassFromTokenAndTypeArgs(
		/* [in] */ ModuleID moduleID,
		/* [in] */ mdTypeDef typeDef,
		/* [in] */ ULONG32 cTypeArgs,
		/* [size_is][in] */ ClassID typeArgs[],
		/* [out] */ ClassID *pClassID) override
	{
		//ATLTRACE(_T("GetClassFromTokenAndTypeArgs"));
		return m_pProfilerInfo2->GetClassFromTokenAndTypeArgs(moduleID, typeDef, cTypeArgs, typeArgs, pClassID);
	}

	virtual HRESULT STDMETHODCALLTYPE GetFunctionFromTokenAndTypeArgs(
		/* [in] */ ModuleID moduleID,
		/* [in] */ mdMethodDef funcDef,
		/* [in] */ ClassID classId,
		/* [in] */ ULONG32 cTypeArgs,
		/* [size_is][in] */ ClassID typeArgs[],
		/* [out] */ FunctionID *pFunctionID) override
	{
		//ATLTRACE(_T("GetFunctionFromTokenAndTypeArgs"));
		return m_pProfilerInfo2->GetFunctionFromTokenAndTypeArgs(moduleID, funcDef, classId, cTypeArgs, typeArgs, pFunctionID);
	}

	virtual HRESULT STDMETHODCALLTYPE EnumModuleFrozenObjects(
		/* [in] */ ModuleID moduleID,
		/* [out] */ ICorProfilerObjectEnum **ppEnum) override
	{
		//ATLTRACE(_T("GetClassFromObject"));
		return m_pProfilerInfo2->EnumModuleFrozenObjects(moduleID, ppEnum);
	}

	virtual HRESULT STDMETHODCALLTYPE GetArrayObjectInfo(
		/* [in] */ ObjectID objectId,
		/* [in] */ ULONG32 cDimensions,
		/* [size_is][out] */ ULONG32 pDimensionSizes[],
		/* [size_is][out] */ int pDimensionLowerBounds[],
		/* [out] */ BYTE **ppData) override
	{
		//ATLTRACE(_T("GetArrayObjectInfo"));
		return m_pProfilerInfo2->GetArrayObjectInfo(objectId, cDimensions, pDimensionSizes, pDimensionLowerBounds, ppData);
	}

	virtual HRESULT STDMETHODCALLTYPE GetBoxClassLayout(
		/* [in] */ ClassID classId,
		/* [out] */ ULONG32 *pBufferOffset) override
	{
		//ATLTRACE(_T("GetBoxClassLayout"));
		return m_pProfilerInfo2->GetBoxClassLayout(classId, pBufferOffset);
	}

	virtual HRESULT STDMETHODCALLTYPE GetThreadAppDomain(
		/* [in] */ ThreadID threadId,
		/* [out] */ AppDomainID *pAppDomainId) override
	{
		//ATLTRACE(_T("GetThreadAppDomain"));
		return m_pProfilerInfo2->GetThreadAppDomain(threadId, pAppDomainId);
	}

	virtual HRESULT STDMETHODCALLTYPE GetRVAStaticAddress(
		/* [in] */ ClassID classId,
		/* [in] */ mdFieldDef fieldToken,
		/* [out] */ void **ppAddress) override
	{
		//ATLTRACE(_T("GetRVAStaticAddress"));
		return m_pProfilerInfo2->GetRVAStaticAddress(classId, fieldToken, ppAddress);
	}

	virtual HRESULT STDMETHODCALLTYPE GetAppDomainStaticAddress(
		/* [in] */ ClassID classId,
		/* [in] */ mdFieldDef fieldToken,
		/* [in] */ AppDomainID appDomainId,
		/* [out] */ void **ppAddress) override
	{
		//ATLTRACE(_T("GetAppDomainStaticAddress"));
		return m_pProfilerInfo2->GetAppDomainStaticAddress(classId, fieldToken, appDomainId, ppAddress);
	}

	virtual HRESULT STDMETHODCALLTYPE GetThreadStaticAddress(
		/* [in] */ ClassID classId,
		/* [in] */ mdFieldDef fieldToken,
		/* [in] */ ThreadID threadId,
		/* [out] */ void **ppAddress) override
	{
		//ATLTRACE(_T("GetThreadStaticAddress"));
		return m_pProfilerInfo2->GetThreadStaticAddress(classId, fieldToken, threadId, ppAddress);
	}

	virtual HRESULT STDMETHODCALLTYPE GetContextStaticAddress(
		/* [in] */ ClassID classId,
		/* [in] */ mdFieldDef fieldToken,
		/* [in] */ ContextID contextId,
		/* [out] */ void **ppAddress) override
	{
		//ATLTRACE(_T("GetContextStaticAddress"));
		return m_pProfilerInfo2->GetContextStaticAddress(classId, fieldToken, contextId, ppAddress);
	}

	virtual HRESULT STDMETHODCALLTYPE GetStaticFieldInfo(
		/* [in] */ ClassID classId,
		/* [in] */ mdFieldDef fieldToken,
		/* [out] */ COR_PRF_STATIC_TYPE *pFieldInfo) override
	{
		//ATLTRACE(_T("GetStaticFieldInfo"));
		return m_pProfilerInfo2->GetStaticFieldInfo(classId, fieldToken, pFieldInfo);
	}

	virtual HRESULT STDMETHODCALLTYPE GetGenerationBounds(
		/* [in] */ ULONG cObjectRanges,
		/* [out] */ ULONG *pcObjectRanges,
		/* [length_is][size_is][out] */ COR_PRF_GC_GENERATION_RANGE ranges[]) override
	{
		//ATLTRACE(_T("GetGenerationBounds"));
		return m_pProfilerInfo2->GetGenerationBounds(cObjectRanges, pcObjectRanges, ranges);
	}

	virtual HRESULT STDMETHODCALLTYPE GetObjectGeneration(
		/* [in] */ ObjectID objectId,
		/* [out] */ COR_PRF_GC_GENERATION_RANGE *range) override
	{
		//ATLTRACE(_T("GetObjectGeneration"));
		return m_pProfilerInfo2->GetObjectGeneration(objectId, range);
	}

	virtual HRESULT STDMETHODCALLTYPE GetNotifiedExceptionClauseInfo(
		/* [out] */ COR_PRF_EX_CLAUSE_INFO *pinfo) override
	{
		//ATLTRACE(_T("GetNotifiedExceptionClauseInfo"));
		return m_pProfilerInfo2->GetNotifiedExceptionClauseInfo(pinfo);
	}

public: // ICorProfilerInfo3
	virtual HRESULT STDMETHODCALLTYPE EnumJITedFunctions(
		/* [out] */ ICorProfilerFunctionEnum **ppEnum) override
	{
		//ATLTRACE(_T("EnumJITedFunctions"));
		return m_pProfilerInfo3->EnumJITedFunctions(ppEnum);
	}

	virtual HRESULT STDMETHODCALLTYPE RequestProfilerDetach(
		/* [in] */ DWORD dwExpectedCompletionMilliseconds) override
	{
		//ATLTRACE(_T("RequestProfilerDetach"));
		return m_pProfilerInfo3->RequestProfilerDetach(dwExpectedCompletionMilliseconds);
	}

	virtual HRESULT STDMETHODCALLTYPE SetFunctionIDMapper2(
		/* [in] */ FunctionIDMapper2 *pFunc,
		/* [in] */ void *clientData) override
	{
		//ATLTRACE(_T("SetFunctionIDMapper2"));
		return m_pProfilerInfo3->SetFunctionIDMapper2(pFunc, clientData);
	}

	virtual HRESULT STDMETHODCALLTYPE GetStringLayout2(
		/* [out] */ ULONG *pStringLengthOffset,
		/* [out] */ ULONG *pBufferOffset) override
	{
		//ATLTRACE(_T("GetStringLayout2"));
		return m_pProfilerInfo3->GetStringLayout2(pStringLengthOffset, pBufferOffset);
	}

	virtual HRESULT STDMETHODCALLTYPE SetEnterLeaveFunctionHooks3(
		/* [in] */ FunctionEnter3 *pFuncEnter3,
		/* [in] */ FunctionLeave3 *pFuncLeave3,
		/* [in] */ FunctionTailcall3 *pFuncTailcall3) override
	{
		//ATLTRACE(_T("SetEnterLeaveFunctionHooks3"));
		return m_pProfilerInfo3->SetEnterLeaveFunctionHooks3(pFuncEnter3, pFuncLeave3, pFuncTailcall3);
	}

	virtual HRESULT STDMETHODCALLTYPE SetEnterLeaveFunctionHooks3WithInfo(
		/* [in] */ FunctionEnter3WithInfo *pFuncEnter3WithInfo,
		/* [in] */ FunctionLeave3WithInfo *pFuncLeave3WithInfo,
		/* [in] */ FunctionTailcall3WithInfo *pFuncTailcall3WithInfo) override
	{
		//ATLTRACE(_T("SetEnterLeaveFunctionHooks3WithInfo"));
		return m_pProfilerInfo3->SetEnterLeaveFunctionHooks3WithInfo(pFuncEnter3WithInfo, pFuncLeave3WithInfo, pFuncTailcall3WithInfo);
	}

	virtual HRESULT STDMETHODCALLTYPE GetFunctionEnter3Info(
		/* [in] */ FunctionID functionId,
		/* [in] */ COR_PRF_ELT_INFO eltInfo,
		/* [out] */ COR_PRF_FRAME_INFO *pFrameInfo,
		/* [out][in] */ ULONG *pcbArgumentInfo,
		/* [size_is][out] */ COR_PRF_FUNCTION_ARGUMENT_INFO *pArgumentInfo) override
	{
		//ATLTRACE(_T("GetFunctionEnter3Info"));
		return m_pProfilerInfo3->GetFunctionEnter3Info(functionId, eltInfo, pFrameInfo, pcbArgumentInfo, pArgumentInfo);
	}

	virtual HRESULT STDMETHODCALLTYPE GetFunctionLeave3Info(
		/* [in] */ FunctionID functionId,
		/* [in] */ COR_PRF_ELT_INFO eltInfo,
		/* [out] */ COR_PRF_FRAME_INFO *pFrameInfo,
		/* [out] */ COR_PRF_FUNCTION_ARGUMENT_RANGE *pRetvalRange) override
	{
		//ATLTRACE(_T("GetFunctionLeave3Info"));
		return m_pProfilerInfo3->GetFunctionLeave3Info(functionId, eltInfo, pFrameInfo, pRetvalRange);
	}

	virtual HRESULT STDMETHODCALLTYPE GetFunctionTailcall3Info(
		/* [in] */ FunctionID functionId,
		/* [in] */ COR_PRF_ELT_INFO eltInfo,
		/* [out] */ COR_PRF_FRAME_INFO *pFrameInfo) override
	{
		//ATLTRACE(_T("GetFunctionTailcall3Info"));
		return m_pProfilerInfo3->GetFunctionTailcall3Info(functionId, eltInfo, pFrameInfo);
	}

	virtual HRESULT STDMETHODCALLTYPE EnumModules(
		/* [out] */ ICorProfilerModuleEnum **ppEnum) override
	{
		//ATLTRACE(_T("EnumModules"));
		return m_pProfilerInfo3->EnumModules(ppEnum);
	}

	virtual HRESULT STDMETHODCALLTYPE GetRuntimeInformation(
		/* [out] */ USHORT *pClrInstanceId,
		/* [out] */ COR_PRF_RUNTIME_TYPE *pRuntimeType,
		/* [out] */ USHORT *pMajorVersion,
		/* [out] */ USHORT *pMinorVersion,
		/* [out] */ USHORT *pBuildNumber,
		/* [out] */ USHORT *pQFEVersion,
		/* [in] */ ULONG cchVersionString,
		/* [out] */ ULONG *pcchVersionString,
		/* [annotation][out] */
		_Out_writes_to_(cchVersionString, *pcchVersionString)  WCHAR szVersionString[]) override
	{
		//ATLTRACE(_T("GetRuntimeInformation"));
		return m_pProfilerInfo3->GetRuntimeInformation(pClrInstanceId, pRuntimeType, pMajorVersion,
			pMinorVersion, pBuildNumber, pQFEVersion, cchVersionString, pcchVersionString, szVersionString);
	}

	virtual HRESULT STDMETHODCALLTYPE GetThreadStaticAddress2(
		/* [in] */ ClassID classId,
		/* [in] */ mdFieldDef fieldToken,
		/* [in] */ AppDomainID appDomainId,
		/* [in] */ ThreadID threadId,
		/* [out] */ void **ppAddress) override
	{
		//ATLTRACE(_T("GetThreadStaticAddress2"));
		return m_pProfilerInfo3->GetThreadStaticAddress2(classId,
			fieldToken, appDomainId, threadId, ppAddress);
	}

	virtual HRESULT STDMETHODCALLTYPE GetAppDomainsContainingModule(
		/* [in] */ ModuleID moduleId,
		/* [in] */ ULONG32 cAppDomainIds,
		/* [out] */ ULONG32 *pcAppDomainIds,
		/* [length_is][size_is][out] */ AppDomainID appDomainIds[]) override
	{
		//ATLTRACE(_T("GetAppDomainsContainingModule"));
		return m_pProfilerInfo3->GetAppDomainsContainingModule(moduleId,
			cAppDomainIds, pcAppDomainIds, appDomainIds);
	}

	virtual HRESULT STDMETHODCALLTYPE GetModuleInfo2(
		/* [in] */ ModuleID moduleId,
		/* [out] */ LPCBYTE *ppBaseLoadAddress,
		/* [in] */ ULONG cchName,
		/* [out] */ ULONG *pcchName,
		/* [annotation][out] */
		_Out_writes_to_(cchName, *pcchName)  WCHAR szName[],
		/* [out] */ AssemblyID *pAssemblyId,
		/* [out] */ DWORD *pdwModuleFlags) override
	{
		//ATLTRACE(_T("GetModuleInfo2"));
		return m_pProfilerInfo3->GetModuleInfo2(moduleId, ppBaseLoadAddress, cchName,
			pcchName, szName, pAssemblyId, pdwModuleFlags);
	}

public: // ICorProfilerInfo4
	virtual HRESULT STDMETHODCALLTYPE EnumThreads(
		/* [out] */ ICorProfilerThreadEnum **ppEnum) override
	{
		//ATLTRACE(_T("EnumThreads"));
		return m_pProfilerInfo4->EnumThreads(ppEnum);
	}

	virtual HRESULT STDMETHODCALLTYPE InitializeCurrentThread(void) override
	{
		//ATLTRACE(_T("InitializeCurrentThread"));
		return m_pProfilerInfo4->InitializeCurrentThread();
	}

	virtual HRESULT STDMETHODCALLTYPE RequestReJIT(
		/* [in] */ ULONG cFunctions,
		/* [size_is][in] */ ModuleID moduleIds[],
		/* [size_is][in] */ mdMethodDef methodIds[]) override
	{
		//ATLTRACE(_T("RequestReJIT"));
		return m_pProfilerInfo4->RequestReJIT(cFunctions, moduleIds, methodIds);
	}

	virtual HRESULT STDMETHODCALLTYPE RequestRevert(
		/* [in] */ ULONG cFunctions,
		/* [size_is][in] */ ModuleID moduleIds[],
		/* [size_is][in] */ mdMethodDef methodIds[],
		/* [size_is][out] */ HRESULT status[]) override
	{
		//ATLTRACE(_T("RequestRevert"));
		return m_pProfilerInfo4->RequestRevert(cFunctions, moduleIds, methodIds, status);
	}

	virtual HRESULT STDMETHODCALLTYPE GetCodeInfo3(
		/* [in] */ FunctionID functionID,
		/* [in] */ ReJITID reJitId,
		/* [in] */ ULONG32 cCodeInfos,
		/* [out] */ ULONG32 *pcCodeInfos,
		/* [length_is][size_is][out] */ COR_PRF_CODE_INFO codeInfos[]) override
	{
		//ATLTRACE(_T("GetCodeInfo3"));
		return m_pProfilerInfo4->GetCodeInfo3(functionID, reJitId, cCodeInfos, pcCodeInfos, codeInfos);
	}

	virtual HRESULT STDMETHODCALLTYPE GetFunctionFromIP2(
		/* [in] */ LPCBYTE ip,
		/* [out] */ FunctionID *pFunctionId,
		/* [out] */ ReJITID *pReJitId) override
	{
		//ATLTRACE(_T("GetFunctionFromIP2"));
		return m_pProfilerInfo4->GetFunctionFromIP2(ip, pFunctionId, pReJitId);
	}

	virtual HRESULT STDMETHODCALLTYPE GetReJITIDs(
		/* [in] */ FunctionID functionId,
		/* [in] */ ULONG cReJitIds,
		/* [out] */ ULONG *pcReJitIds,
		/* [length_is][size_is][out] */ ReJITID reJitIds[]) override
	{
		//ATLTRACE(_T("GetClassFromObject"));
		return m_pProfilerInfo4->GetReJITIDs(functionId, cReJitIds, pcReJitIds, reJitIds);
	}

	virtual HRESULT STDMETHODCALLTYPE GetILToNativeMapping2(
		/* [in] */ FunctionID functionId,
		/* [in] */ ReJITID reJitId,
		/* [in] */ ULONG32 cMap,
		/* [out] */ ULONG32 *pcMap,
		/* [length_is][size_is][out] */ COR_DEBUG_IL_TO_NATIVE_MAP map[]) override
	{
		//ATLTRACE(_T("GetILToNativeMapping2"));
		return m_pProfilerInfo4->GetILToNativeMapping2(functionId, reJitId, cMap, pcMap, map);
	}

	virtual HRESULT STDMETHODCALLTYPE EnumJITedFunctions2(
		/* [out] */ ICorProfilerFunctionEnum **ppEnum) override
	{
		//ATLTRACE(_T("EnumJITedFunctions2"));
		return m_pProfilerInfo4->EnumJITedFunctions2(ppEnum);
	}

	virtual HRESULT STDMETHODCALLTYPE GetObjectSize2(
		/* [in] */ ObjectID objectId,
		/* [out] */ SIZE_T *pcSize) override
	{
		//ATLTRACE(_T("GetObjectSize2"));
		return m_pProfilerInfo4->GetObjectSize2(objectId, pcSize);
	}

// ICorProfilerInfo5
public:
    virtual HRESULT STDMETHODCALLTYPE GetEventMask2(
        /* [out] */ DWORD *pdwEventsLow,
        /* [out] */ DWORD *pdwEventsHigh) override
    {
        //ATLTRACE(_T("GetEventMask2"));
        return m_pProfilerInfo5->GetEventMask2(pdwEventsLow, pdwEventsHigh);
    }

    virtual HRESULT STDMETHODCALLTYPE SetEventMask2(
        /* [in] */ DWORD dwEventsLow,
        /* [in] */ DWORD dwEventsHigh) override
    {
        //ATLTRACE(_T("SetEventMask2"));
        return m_pProfilerInfo5->SetEventMask2(dwEventsLow, dwEventsHigh);
    }

// ICorProfilerInfo6
public:
    virtual HRESULT STDMETHODCALLTYPE EnumNgenModuleMethodsInliningThisMethod(
        /* [in] */ ModuleID inlinersModuleId,
        /* [in] */ ModuleID inlineeModuleId,
        /* [in] */ mdMethodDef inlineeMethodId,
        /* [out] */ BOOL *incompleteData,
        /* [out] */ ICorProfilerMethodEnum **ppEnum) override
    {
        //ATLTRACE(_T("EnumNgenModuleMethodsInliningThisMethod"));
        return m_pProfilerInfo6->EnumNgenModuleMethodsInliningThisMethod(inlinersModuleId, inlineeModuleId, inlineeMethodId, incompleteData, ppEnum);
    }

// ICorProfilerInfo7
public:
    virtual HRESULT STDMETHODCALLTYPE ApplyMetaData(
        /* [in] */ ModuleID moduleId) override
    {
        //ATLTRACE(_T("ApplyMetaData"));
        return m_pProfilerInfo7->ApplyMetaData(moduleId);
    }

    virtual HRESULT STDMETHODCALLTYPE GetInMemorySymbolsLength(
        /* [in] */ ModuleID moduleId,
        /* [out] */ DWORD *pCountSymbolBytes) override
    {
        //ATLTRACE(_T("GetInMemorySymbolsLength"));
        return m_pProfilerInfo7->GetInMemorySymbolsLength(moduleId, pCountSymbolBytes);
    }

    virtual HRESULT STDMETHODCALLTYPE ReadInMemorySymbols(
        /* [in] */ ModuleID moduleId,
        /* [in] */ DWORD symbolsReadOffset,
        /* [out] */ BYTE *pSymbolBytes,
        /* [in] */ DWORD countSymbolBytes,
        /* [out] */ DWORD *pCountSymbolBytesRead) override
    {
        //ATLTRACE(_T("ReadInMemorySymbols"));
        return m_pProfilerInfo7->ReadInMemorySymbols(moduleId, symbolsReadOffset, pSymbolBytes, countSymbolBytes, pCountSymbolBytesRead);
    }

// ICorProfilerInfo8
public:
	virtual HRESULT STDMETHODCALLTYPE IsFunctionDynamic(
		/* [in] */ FunctionID functionId,
		/* [out] */ BOOL *isDynamic) override
	{
		//ATLTRACE(_T("IsFunctionDynamic"));
		return m_pProfilerInfo8->IsFunctionDynamic(functionId, isDynamic);
	}

	virtual HRESULT STDMETHODCALLTYPE GetFunctionFromIP3(
		/* [in] */ LPCBYTE ip,
		/* [out] */ FunctionID *functionId,
		/* [out] */ ReJITID *pReJitId) override
	{
		//ATLTRACE(_T("GetFunctionFromIP3"));
		return m_pProfilerInfo8->GetFunctionFromIP3(ip, functionId, pReJitId);
	}

	virtual HRESULT STDMETHODCALLTYPE GetDynamicFunctionInfo(
		/* [in] */ FunctionID functionId,
		/* [out] */ ModuleID *moduleId,
		/* [out] */ PCCOR_SIGNATURE *ppvSig,
		/* [out] */ ULONG *pbSig,
		/* [in] */ ULONG cchName,
		/* [out] */ ULONG *pcchName,
		/* [out] */ WCHAR wszName[]) override
	{
		//ATLTRACE(_T("GetDynamicFunctionInfo"));
		return m_pProfilerInfo8->GetDynamicFunctionInfo(functionId, moduleId, ppvSig, pbSig, cchName, pcchName, wszName);
	}
};