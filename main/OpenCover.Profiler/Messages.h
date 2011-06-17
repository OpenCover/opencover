//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
#pragma once

enum VisitType : int
{
    VT_SeqPt = 1,
};

#pragma pack(push)
#pragma pack(1)

typedef struct SequencePoint
{
    ULONG UniqueId;
    long Offset;
};

typedef struct VisitPoint
{
    ULONG UniqueId;
    VisitType VisitType;
};

#pragma pack(pop)

enum MSG_Type : int
{
    MSG_Unknown = 0,
    MSG_TrackAssembly = 1,
    MSG_GetSequencePoints = 2,
};

#pragma pack(push)
#pragma pack(1)

typedef struct _MSG_TrackAssembly_Request
{
    MSG_Type type;
    WCHAR szModuleName[512];
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
    WCHAR szModuleName[512];
} MSG_GetSequencePoints_Request;

typedef struct _MSG_GetSequencePoints_Response
{
    BOOL hasMore;
    int count;
    SequencePoint points[8000];
} MSG_GetSequencePoints_Response;

typedef struct _MSG_SendVisitPoints_Request
{
    int count;
    VisitPoint points[8000];
} MSG_SendVisitPoints_Request;

#pragma pack(pop)

typedef union _MSG_Union
{
    MSG_Type type;
    MSG_TrackAssembly_Request trackRequest;
    MSG_TrackAssembly_Response trackResponse;
    MSG_GetSequencePoints_Request getSequencePointsRequest;
    MSG_GetSequencePoints_Response getSequencePointsResponse;
} MSG_Union;
