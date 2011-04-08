#pragma once

/// <summary>A list of opcodes</summary>
/// <remarks>Used, amongst other things, to build the <c>OperationDetails</c> structures 
/// when pulling in "opcode.def"</remarks>
enum CanonicalName {
        #define OPDEF(name, str, decs, incs, args, optp, stdlen, stdop1, stdop2, flow) name,
        #include "opcode.def"
        #undef OPDEF
    };

/// <summary>A list of control flow types</summary>
/// <remarks>Used, amongst other things, to build the <c>OperationDetails</c> structures 
/// when pulling in "opcode.def"</remarks>
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

/// <summary>A list of operand types/usage </summary>
/// <remarks>Used, amongst other things, to build the <c>OperationDetails</c> structures 
/// when pulling in "opcode.def"</remarks>
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

/// <summary>A list of opcode types</summary>
/// <remarks>Used, amongst other things, to build the <c>OperationDetails</c> structures 
/// when pulling in "opcode.def"</remarks>
enum OpcodeKind {
    IPrimitive,
    IMacro,
    IObjModel,
    IInternal,
    IPrefix
};

/// <summary>A list of operand sizes</summary>
/// <remarks>Used, amongst other things, to build the <c>OperationDetails</c> structures 
/// when pulling in "opcode.def"</remarks>
enum OperandSize {
    Null  = 0,
    Byte  = 1,
    Word  = 2,
    Dword = 4,
    Qword = 8
};

/// <summary>An operation structure see "opcode.def"</summary>
struct OperationDetails
{
    CanonicalName canonicalName;
    OperandParam operandParam;
    OperandSize operandSize;
    ControlFlow controlFlow;
    BYTE length;
    BYTE op1;
    BYTE op2;
    OpcodeKind opcodeKind;
    TCHAR *stringName;
};

typedef std::hash_map<CanonicalName, OperationDetails> MapCanonicalNameOperationDetails;
typedef std::hash_map<DWORD, OperationDetails> MapOpsOperationDetails;

/// <summary>The container of the static lists used for the <c>OperationDetails</c> lookups</summary>
/// <remarks>The lists are built when we instantiate our static instance m_operations</remarks>
class Operations
{
public:
    static MapCanonicalNameOperationDetails m_mapNameOperationDetails;
    static MapOpsOperationDetails m_mapOpsOperationDetails;
protected:
    Operations();
private:
    static Operations m_operations;

};
