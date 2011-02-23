// dllmain.h : Declaration of module class.

class COpenCoverProfilerModule : public ATL::CAtlDllModuleT< COpenCoverProfilerModule >
{
public :
	DECLARE_LIBID(LIBID_OpenCoverProfilerLib)
	DECLARE_REGISTRY_APPID_RESOURCEID(IDR_OPENCOVERPROFILER, "{0A09C7B0-D778-49CF-8EE7-5C7145885ABF}")
};

extern class COpenCoverProfilerModule _AtlModule;
