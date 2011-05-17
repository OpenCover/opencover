#include "Instruction.h"

#pragma once

class ExceptionHandler;
class Method;

typedef std::vector<ExceptionHandler *> ExceptionHandlerList;
typedef ExceptionHandlerList::iterator ExceptionHandlerListIter;
typedef ExceptionHandlerList::const_iterator ExceptionHandlerListConstIter;


/// <summary>A representation of a try/catch section handler</summary>
class ExceptionHandler
{
public:
    ExceptionHandler(void);
    ~ExceptionHandler(void);

private:
    CorExceptionFlag m_handlerType;
    Instruction * m_tryStart;
    Instruction * m_tryEnd;
    Instruction * m_handlerStart;
    Instruction * m_handlerEnd;
    Instruction * m_filterStart;

    ULONG m_token;

public:
    friend class Method;
};

