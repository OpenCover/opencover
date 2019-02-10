#include "stdafx.h"

#include "SharedMemory.h"

CSharedMemory::~CSharedMemory() {
    CloseMapping();
}

void CSharedMemory::CloseMapping() {
    if (m_hMemory != nullptr) {
        for (auto it = m_viewMap.begin(); it != m_viewMap.end(); ++it) {
            ::UnmapViewOfFile((*it).first);
        }
        m_viewMap.clear();
        CloseHandle(m_hMemory);
        m_hMemory = nullptr;
    }
}

void CSharedMemory::OpenFileMapping(const TCHAR* pName) {
    CloseMapping();
    m_hMemory = ::OpenFileMapping(FILE_MAP_WRITE, false, pName);
}

void* CSharedMemory::MapViewOfFile(DWORD dwFileOffsetHigh, DWORD dwFileOffsetLow, SIZE_T dwNumberOfBytesToMap) {
    if (!IsValid()) {
        return nullptr;
    }
    void* pMappedData = ::MapViewOfFile(
        m_hMemory,
        SECTION_MAP_WRITE | SECTION_MAP_READ,
        dwFileOffsetHigh,
        dwFileOffsetLow,
        dwNumberOfBytesToMap
        ); 

     if (pMappedData != nullptr) {
         m_viewMap.push_back(std::pair<void*, SIZE_T>(pMappedData, dwNumberOfBytesToMap));
     }
     return pMappedData;
}

void CSharedMemory::FlushViewOfFile() {
    for (auto it = m_viewMap.begin(); it != m_viewMap.end(); ++it) {
        ::FlushViewOfFile((*it).first, (*it).second);
    }
}

DWORD CSharedMemory::GetAllocationGranularity() {
    SYSTEM_INFO info;
    ::ZeroMemory(&info, sizeof(SYSTEM_INFO));
    ::GetSystemInfo(&info);
    return info.dwAllocationGranularity;
}
