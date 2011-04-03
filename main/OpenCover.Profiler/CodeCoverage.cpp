// CodeCoverage.cpp : Implementation of CCodeCoverage

#include "stdafx.h"
#include "CodeCoverage.h"
#include "NativeCallback.h"
#include "Method.h"

CCodeCoverage* CCodeCoverage::g_pProfiler = NULL;
// CCodeCoverage

HRESULT STDMETHODCALLTYPE CCodeCoverage::Initialize( 
    /* [in] */ IUnknown *pICorProfilerInfoUnk) 
{
    ATLTRACE(_T("::Initialize"));

    m_profilerInfo = pICorProfilerInfoUnk;
    if (m_profilerInfo != NULL) ATLTRACE(_T("    ::Initialize (m_profilerInfo OK)"));
    if (m_profilerInfo == NULL) return E_FAIL;
    m_profilerInfo2 = pICorProfilerInfoUnk;
    if (m_profilerInfo2 != NULL) ATLTRACE(_T("    ::Initialize (m_profilerInfo2 OK)"));
    if (m_profilerInfo2 == NULL) return E_FAIL;
    m_profilerInfo3 = pICorProfilerInfoUnk;
    if (m_profilerInfo3 != NULL) ATLTRACE(_T("    ::Initialize (m_profilerInfo3 OK)"));

    WCHAR pszPortNumber[10];
    ::GetEnvironmentVariableW(L"OpenCover_Port", pszPortNumber, 10);
    int portNumber = _wtoi(pszPortNumber);
    ATLTRACE(_T("->Port Number %d"), portNumber);

    m_host = new ProfilerCommunication(portNumber);

    m_host->Start();

    DWORD dwMask = 0;
    dwMask |= COR_PRF_MONITOR_MODULE_LOADS;			// Controls the ModuleLoad, ModuleUnload, and ModuleAttachedToAssembly callbacks.
    dwMask |= COR_PRF_MONITOR_JIT_COMPILATION;	    // Controls the JITCompilation, JITFunctionPitched, and JITInlining callbacks.
    dwMask |= COR_PRF_DISABLE_INLINING;				// Disables all inlining.
    dwMask |= COR_PRF_DISABLE_OPTIMIZATIONS;		// Disables all code optimizations.

    m_profilerInfo2->SetEventMask(dwMask);

    g_pProfiler = this;

    return S_OK; 
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::Shutdown( void) 
{ 
    ATLTRACE(_T("::Shutdown"));
    m_host->Stop();
    delete m_host;
    return S_OK; 
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::ModuleAttachedToAssembly( 
    /* [in] */ ModuleID moduleId,
    /* [in] */ AssemblyID assemblyId)
{
    ATLTRACE(_T("::ModuleAttachedToAssembly(%X, %X)"), moduleId, assemblyId);

    std::wstring moduleName = GetModuleName(moduleId);
    std::wstring assemblyName = GetAssemblyName(assemblyId);
    ATLTRACE(_T("    ::ModuleAttachedToAssembly(%X => %s, %X => %s)"), 
        moduleId, W2CT(moduleName.c_str()), 
        assemblyId, W2CT(assemblyName.c_str()));
    m_host->TrackAssembly((LPWSTR)moduleName.c_str(), (LPWSTR)assemblyName.c_str());
    return S_OK; 
}

static void __fastcall UnmanagedCall(void)
{
    ATLTRACE(_T("Hello From Unmanaged Call"));
}

HRESULT STDMETHODCALLTYPE CCodeCoverage::JITCompilationStarted( 
        /* [in] */ FunctionID functionId,
        /* [in] */ BOOL fIsSafeToBlock)
{
    std::wstring moduleName;
    mdToken functionToken;
    ModuleID moduleId;

    if (GetTokenAndModule(functionId, functionToken, moduleId, moduleName))
    {
        ATLTRACE(_T("::JITCompilationStarted(%X, %d, %s)"), functionId, functionToken, W2CT(moduleName.c_str()));
        unsigned int points;
        InstrumentPoint ** ppPoints = NULL;
        
        if (m_host->GetSequencePoints(functionToken, (LPWSTR)moduleName.c_str(), &points, &ppPoints))
        {
            ATLTRACE(_T("    points %d"), points);
            for (unsigned int i=0; i < points; i++)
            {
                ATLTRACE(_T("    %d %X"), ppPoints[i]->Ordinal, ppPoints[i]->Offset);
            }

            CComPtr<IMetaDataEmit> metaDataEmit;
            m_profilerInfo2->GetModuleMetaData(moduleId, ofWrite, IID_IMetaDataEmit, (IUnknown**) &metaDataEmit);

            static COR_SIGNATURE unmanagedCallSignature[] = 
            {
                IMAGE_CEE_CS_CALLCONV_DEFAULT,          // Default CallKind!
                0x00,                                   // Parameter count
                ELEMENT_TYPE_VOID,                      // Return type
                ELEMENT_TYPE_VOID                       // Parameter type (void )
            };

            void (__fastcall *pt)(void) = &UnmanagedCall ;

            mdSignature pmsig ;
            COM_FAIL_RETURN(metaDataEmit->GetTokenFromSig(unmanagedCallSignature, sizeof(unmanagedCallSignature), &pmsig), S_OK);

            ATLTRACE(_T("%d %d"), pmsig, sizeof(pt));

            BYTE ilCode[10];
            ilCode[0] = 0x20 ;                                      // ldc.i4
            memcpy(ilCode + 1, (void*)&pt, sizeof(pt));             // ftn pointer                                     
            ilCode[5]= 0x29;                                        // calli
            memcpy(ilCode + 6, (void*)&pmsig, sizeof(pmsig));       // call site descr

            LPCBYTE pMethodHeader = NULL;
	        ULONG iMethodSize = 0;
            m_profilerInfo2->GetILFunctionBody(moduleId, functionToken, &pMethodHeader, &iMethodSize);

            IMAGE_COR_ILMETHOD* pMethod = (IMAGE_COR_ILMETHOD*)pMethodHeader;
 	        COR_ILMETHOD_FAT* fatImage = (COR_ILMETHOD_FAT*)&pMethod->Fat;

            CComPtr<IMethodMalloc> methodMalloc;
            m_profilerInfo2->GetILFunctionBodyAllocator(moduleId, &methodMalloc);

            BYTE * pBody = NULL;
            ULONG bodyLength = 0;
/*
 	        if(!fatImage->IsFat())
            {
                COR_ILMETHOD_TINY* tinyImage = (COR_ILMETHOD_TINY*)&pMethod->Tiny;
                ATLTRACE(_T("IsTiny(%d, %d, %d)"), sizeof(IMAGE_COR_ILMETHOD), iMethodSize, tinyImage->GetCodeSize());
                IMAGE_COR_ILMETHOD* pNewMethod = (IMAGE_COR_ILMETHOD*)methodMalloc->Alloc(sizeof(COR_ILMETHOD_FAT) + tinyImage->GetCodeSize() + 10);
                COR_ILMETHOD_FAT* fatNewImage = (COR_ILMETHOD_FAT*)&pNewMethod->Fat;

                fatNewImage->SetSize(3);
                fatNewImage->SetFlags(CorILMethod_FatFormat);
                fatNewImage->MaxStack = 2;

                // ############
                memcpy(pNewMethod, pMethod, fatImage->Size * sizeof(DWORD));

                memcpy(fatNewImage->GetCode(), ilCode, 10);

                memcpy(fatNewImage->GetCode() + 10, tinyImage->GetCode(), tinyImage->GetCodeSize());

                fatNewImage->SetCodeSize(tinyImage->GetCodeSize()+10);

                m_profilerInfo2->SetILFunctionBody(moduleId, functionToken, (LPCBYTE) pNewMethod);
            }
            else
            {
                ATLTRACE(_T("IsFat (%d, %d, %d)"), sizeof(IMAGE_COR_ILMETHOD), iMethodSize, fatImage->GetCodeSize());
                IMAGE_COR_ILMETHOD* pNewMethod = (IMAGE_COR_ILMETHOD*)methodMalloc->Alloc(iMethodSize + 10);
                COR_ILMETHOD_FAT* fatNewImage = (COR_ILMETHOD_FAT*)&pNewMethod->Fat;

                // ############
                memcpy(pNewMethod, pMethod, fatImage->Size * sizeof(DWORD));

                memcpy(fatNewImage->GetCode(), ilCode, 10);

                memcpy(fatNewImage->GetCode() + 10, fatImage->GetCode(), fatImage->GetCodeSize());

                fatNewImage->SetCodeSize(fatImage->GetCodeSize()+10);

                m_profilerInfo2->SetILFunctionBody(moduleId, functionToken, (LPCBYTE) pNewMethod);
            }
*/
            /*if(!fatImage->IsFat())
            {
                COR_ILMETHOD_TINY* tinyImage = (COR_ILMETHOD_TINY*)&pMethod->Tiny;
                for (int i = 0 ; i < tinyImage->GetCodeSize(); i++)
                {
                    ATLTRACE(_T("0x%2X"), tinyImage->GetCode()[i]);
                }
            }
            else
            {
                for (int i = 0 ; i < fatImage->GetCodeSize(); i++)
                {
                    ATLTRACE(_T("0x%2X"), fatImage->GetCode()[i]);
                }
            }*/

            Method x;
            x.ReadMethod(pMethod);
        }
    }
    
    return S_OK; 
}

        