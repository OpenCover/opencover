//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#pragma once

#define SEQ_BUFFER_SIZE 8000
#define BRANCH_BUFFER_SIZE 2000
#define VP_BUFFER_SIZE 16000
#define MAX_MSG_SIZE 65536

#pragma pack(push)
#pragma pack(1)

struct SequencePoint
{
    SequencePoint() : UniqueId(0), Offset(0) {}
    ULONG UniqueId;
    long Offset;
};

struct BranchPoint
{
    BranchPoint() : UniqueId(0), Offset(0), Path(0) {}
    ULONG UniqueId;
    long Offset;
    long Path; // for a branch 0 == false, 1 == true ; for a switch it is ...
};

struct VisitPoint
{
    VisitPoint() : UniqueId(0) {}
    ULONG UniqueId;
};

#pragma pack(pop)

enum MSG_Type : int
{
    MSG_Unknown = 0,
    MSG_TrackAssembly = 1,
    MSG_GetSequencePoints = 2,
    MSG_GetBranchPoints = 3,
    MSG_TrackMethod = 4,
    MSG_AllocateMemoryBuffer = 5,
    MSG_CloseChannel = 6,
    MSG_TrackProcess = 7,
};

enum MSG_IdType : ULONG
{
    IT_VisitPoint = 0x00000000,
    IT_MethodEnter = 0x40000000,
    IT_MethodLeave = 0x80000000,
    IT_MethodTailcall = 0xC0000000,
};

#pragma pack(push)
#pragma pack(1)

typedef struct _MSG_TrackAssembly_Request
{
    MSG_Type type;
    WCHAR szProcessName[512];
    WCHAR szModulePath[512];
    WCHAR szAssemblyName[512];
} MSG_TrackAssembly_Request;

typedef struct _MSG_TrackAssembly_Response
{
    BOOL bResponse;
} MSG_TrackAssembly_Response;

typedef struct _MSG_GetSequencePoints_Request
{
    MSG_Type type;
    int functionToken;
    WCHAR szProcessName[512];
    WCHAR szModulePath[512];
    WCHAR szAssemblyName[512];
} MSG_GetSequencePoints_Request;

typedef struct _MSG_GetSequencePoints_Response
{
    BOOL hasMore;
    int count;
    SequencePoint points[SEQ_BUFFER_SIZE];
} MSG_GetSequencePoints_Response;

typedef struct _MSG_GetBranchPoints_Request
{
    MSG_Type type;
    int functionToken;
    WCHAR szProcessName[512];
    WCHAR szModulePath[512];
    WCHAR szAssemblyName[512];
} MSG_GetBranchPoints_Request;

typedef struct _MSG_GetBranchPoints_Response
{
    BOOL hasMore;
    int count;
    BranchPoint points[BRANCH_BUFFER_SIZE];
} MSG_GetBranchPoints_Response;

typedef struct _MSG_SendVisitPoints_Request
{
    int count;
    VisitPoint points[VP_BUFFER_SIZE];
} MSG_SendVisitPoints_Request;

typedef struct _MSG_TrackMethod_Request
{
    MSG_Type type;
    int functionToken;
    WCHAR szModulePath[512];
    WCHAR szAssemblyName[512];
} MSG_TrackMethod_Request;

typedef struct _MSG_TrackMethod_Response
{
    BOOL bResponse;
    ULONG ulUniqueId;
} MSG_TrackMethod_Response;

typedef struct _MSG_AllocateBuffer_Request
{
    MSG_Type type;
    LONG lBufferSize;
} MSG_AllocateBuffer_Request;

typedef struct _MSG_AllocateBuffer_Response
{
    BOOL bResponse;
    ULONG ulBufferId;
} MSG_AllocateBuffer_Response;

typedef struct _MSG_CloseChannel_Request
{
    MSG_Type type;
    ULONG ulBufferId;
} MSG_CloseChannel_Request;

typedef struct _MSG_CloseChannel_Response
{
    BOOL bResponse;
} MSG_CloseChannel_Response;

typedef struct _MSG_TrackProcess_Request
{
    MSG_Type type;
    WCHAR szProcessName[512];
} MSG_TrackProcess_Request;

typedef struct _MSG_TrackProcess_Response
{
    BOOL bResponse;
} MSG_TrackProcess_Response;

#pragma pack(pop)

typedef union _MSG_Union
{
    MSG_Type type;
    MSG_TrackAssembly_Request trackAssemblyRequest;
    MSG_TrackAssembly_Response trackAssemblyResponse;
    MSG_GetSequencePoints_Request getSequencePointsRequest;
    MSG_GetSequencePoints_Response getSequencePointsResponse;
    MSG_GetBranchPoints_Request getBranchPointsRequest;
    MSG_GetBranchPoints_Response getBranchPointsResponse;
    MSG_TrackMethod_Request trackMethodRequest;
    MSG_TrackMethod_Response trackMethodResponse;
    MSG_AllocateBuffer_Request allocateBufferRequest;
    MSG_AllocateBuffer_Response allocateBufferResponse;
    MSG_CloseChannel_Request closeChannelRequest;
    MSG_CloseChannel_Response closeChannelResponse;
    MSG_TrackProcess_Request trackProcessRequest;
    MSG_TrackProcess_Response trackProcessResponse;
} MSG_Union;

