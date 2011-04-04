#include "StdAfx.h"
#include "ExceptionHandler.h"


ExceptionHandler::ExceptionHandler(void)
{
    m_tryStart = NULL;
    m_tryEnd = NULL;
    m_handlerStart = NULL;
    m_handlerEnd = NULL;
    m_filterStart = NULL;
    m_token = 0;
}


ExceptionHandler::~ExceptionHandler(void)
{
}
