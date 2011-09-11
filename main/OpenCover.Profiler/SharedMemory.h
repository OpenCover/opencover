//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#pragma once

class CSharedMemory
{
public:
    CSharedMemory() : m_hMemory(NULL) { }
    ~CSharedMemory();

public:
    void OpenFileMapping(const TCHAR *pName);  
    void* MapViewOfFile(DWORD dwFileOffsetHigh, DWORD dwFileOffsetLow, SIZE_T dwNumberOfBytesToMap);
    static DWORD GetAllocationGranularity();
    bool IsValid() {return m_hMemory!=NULL; }

private:
    HANDLE m_hMemory;
    std::list<void*> m_viewMap;
};