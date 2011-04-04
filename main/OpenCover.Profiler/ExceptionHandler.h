#include "Instruction.h"

#pragma once

class ExceptionHandler;

typedef std::vector<ExceptionHandler *> ExceptionHandlerList;
typedef ExceptionHandlerList::iterator ExceptionHandlerListIter;
typedef ExceptionHandlerList::const_iterator ExceptionHandlerListConstIter;

enum ExceptionHandlerType {
    CLAUSE_NONE = 0,
    CLAUSE_FILTER = 1,
    CLAUSE_FINALLY = 2,
    CLAUSE_FAULT = 4,
    CLAUSE_DUPLICATED = 8
};

class ExceptionHandler
{
public:
    ExceptionHandler(void);
    ~ExceptionHandler(void);

public:
    ExceptionHandlerType m_handlerType;
    Instruction * m_tryStart;
    Instruction * m_tryEnd;
    Instruction * m_handlerStart;
    Instruction * m_handlerEnd;
    Instruction * m_filterStart;

    ULONG m_token;
};

