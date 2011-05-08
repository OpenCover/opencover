#pragma once

/// <summary>The <c>MethodBuffer</c> encapsulates all the buffer handling</summary>
class MethodBuffer
{
protected:
    void SetBuffer(BYTE* pBuffer) {
        m_bufferBase = pBuffer;
        m_bufferCurrent = pBuffer;
        m_position = 0;
    }

    unsigned long GetPosition() { return m_position; }

    template<typename value_type> value_type Read() {
        _ASSERTE(m_bufferCurrent != NULL);
        value_type value = *(value_type*)(m_bufferCurrent);
        Advance(sizeof(value_type));
        return value;
    }

    template<typename value_type> void Write(value_type value) {
        _ASSERTE(m_bufferCurrent != NULL);
        *(value_type*)(m_bufferCurrent) = value;
        Advance(sizeof(value_type));
    }

    template<typename value_type> void Align() {
        _ASSERTE(m_bufferCurrent != NULL);
        long i = sizeof(value_type) - 1;
        long incr = ((m_position + i) & ~i) - m_position;
        Advance(incr);
    }

    void Advance(long num) {
        _ASSERTE(m_bufferCurrent != NULL);
        m_bufferCurrent += num;
        m_position += num;
    }

private:
    BYTE * m_bufferBase;
    BYTE * m_bufferCurrent;
    unsigned long m_position;
};
