// CodeCoverage.cpp : Implementation of CCodeCoverage

#include "stdafx.h"
#include "CodeCoverage.h"


// CCodeCoverage

HRESULT STDMETHODCALLTYPE CCodeCoverage::Initialize( 
            /* [in] */ IUnknown *pICorProfilerInfoUnk) 
{
	ATLTRACE(_T("::Initialize"));
	m_profilerInfo = pICorProfilerInfoUnk;
	m_profilerInfo2 = pICorProfilerInfoUnk;
	m_profilerInfo3 = pICorProfilerInfoUnk;

	return S_OK; 
}
        
HRESULT STDMETHODCALLTYPE CCodeCoverage::Shutdown( void) 
{ 
	ATLTRACE(_T("::Shutdown"));
	return S_OK; 
}