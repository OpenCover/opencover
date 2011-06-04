#include "StdAfx.h"
#include "ProfilerCommunication.h"

#define ONERROR_GOEXIT(hr) if (FAILED(hr)) goto Exit

ProfilerCommunication::ProfilerCommunication(int port)
{
}

ProfilerCommunication::~ProfilerCommunication(void)
{
}

void ProfilerCommunication::Start()
{
}

void ProfilerCommunication::Stop()
{
}

BOOL ProfilerCommunication::TrackAssembly(WCHAR* pModuleName, WCHAR* pAssemblyName)
{
    return false;
}

BOOL ProfilerCommunication::GetSequencePoints(mdToken functionToken, WCHAR* pModuleName, unsigned int* pNumPoints, SequencePoint*** pppInstrumentPoints)
{
    return false;
}


void ProfilerCommunication::SendVisitPoints(unsigned int numPoints, VisitPoint **ppPoints)
{
    return;
}


