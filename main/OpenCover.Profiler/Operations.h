#pragma once

enum CanonicalName {
        #define OPDEF(name, str, decs, incs, args, optp, stdlen, stdop1, stdop2, flow) name,
        #include "opcode.def"
        #undef OPDEF
    };

enum ControlFlow {
        NEXT,
        BREAK,
        CALL,
        RETURN,
        BRANCH,
        COND_BRANCH,
        THROW,
        META
    };

enum OperandParam {
    InlineNone,
    ShortInlineVar,
    ShortInlineI,
    InlineI,
    InlineI8,
    ShortInlineR,
    InlineR,
    InlineMethod,
    InlineSig,
    ShortInlineBrTarget,
    InlineBrTarget,
    InlineSwitch,
    InlineString,
    InlineType,
    InlineField,
    InlineTok,
    InlineVar
};

enum OperandSize {
    Null  = 0,
    Byte  = 1,
    Word  = 2,
    Dword = 4,
    Qword = 8
};

struct OperationDetails
{
    CanonicalName canonicalName;
    OperandParam operandParam;
    OperandSize operandSize;
    ControlFlow controlFlow;
    BYTE length;
    BYTE op1;
    BYTE op2;
    TCHAR *stringName;
};

typedef std::hash_map<CanonicalName, OperationDetails> MapCanonicalNameOperationDetails;
typedef std::hash_map<DWORD, OperationDetails> MapOpsOperationDetails;

class Operations
{
public:
    Operations();
    static MapCanonicalNameOperationDetails m_mapNameOperationDetails;
    static MapOpsOperationDetails m_mapOpsOperationDetails;
private:
    static Operations m_operations;

};
