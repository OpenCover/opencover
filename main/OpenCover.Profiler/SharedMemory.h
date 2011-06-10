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

private:
    HANDLE m_hMemory;
    std::list<void*> m_viewMap;
};