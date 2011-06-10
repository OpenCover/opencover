#include "stdafx.h"

#include "SharedMemory.h"

CSharedMemory::~CSharedMemory()
{
    if (m_hMemory!=NULL) 
    {
        for (std::list<void*>::iterator it = m_viewMap.begin(); it != m_viewMap.end(); it++)
        {
            ::UnmapViewOfFile(*it);
        }
        m_viewMap.clear();
        CloseHandle(m_hMemory); 
        m_hMemory = NULL;
    }
}

void CSharedMemory::OpenFileMapping(const TCHAR* pName)
{
    m_hMemory = ::OpenFileMapping(FILE_MAP_ALL_ACCESS, false, pName);
}

void* CSharedMemory::MapViewOfFile(DWORD dwFileOffsetHigh, DWORD dwFileOffsetLow, SIZE_T dwNumberOfBytesToMap)
{
     void* pMappedData = ::MapViewOfFile(
        m_hMemory,
        FILE_MAP_ALL_ACCESS,
        dwFileOffsetHigh,
        dwFileOffsetLow,
        dwNumberOfBytesToMap
        ); 

     if (pMappedData != NULL)
     {
         m_viewMap.push_back(pMappedData);
     }
     return pMappedData;
}

DWORD CSharedMemory::GetAllocationGranularity()
{
    SYSTEM_INFO info;
    ::ZeroMemory(&info, sizeof(SYSTEM_INFO));
    ::GetSystemInfo(&info);
    return info.dwAllocationGranularity;
}

