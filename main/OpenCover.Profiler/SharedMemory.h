//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#pragma once

class CSharedMemory
{
public:
    CSharedMemory() : m_hMemory(nullptr) { }
    ~CSharedMemory();

public:
    void OpenFileMapping(const TCHAR *pName);  
    void* MapViewOfFile(DWORD dwFileOffsetHigh, DWORD dwFileOffsetLow, SIZE_T dwNumberOfBytesToMap);
    static DWORD GetAllocationGranularity();
    bool IsValid() { return m_hMemory != nullptr; }
    void FlushViewOfFile();

private:
    HANDLE m_hMemory;
    std::list<std::pair<void*, SIZE_T>> m_viewMap;
    void CloseMapping();
};