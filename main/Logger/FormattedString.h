#pragma once

#include <stdarg.h>
#include <assert.h>
#include <memory>

struct FormatArgs
{
	va_list mArgs;
	const char* mFormat;

	FormatArgs (va_list args, const char* format) :
		mArgs (args),
		mFormat (format)
	{
	}
};

class FormattedString
{
public:

	FormattedString() :
		mPtr (new char[1]),
		mLength (1)
	{
		mPtr[0] = 0;
	}

	FormattedString (const char* format, ...)
	{
		va_list args;
		va_start (args, format);

		SetFormat (FormatArgs (args, format));

		va_end(args);
	}

	FormattedString (const FormatArgs& formatArgs)
	{
		SetFormat (formatArgs);
	}

	FormattedString (const FormattedString& other)
	{
		CopyFrom (other);
	}

	FormattedString& operator = (const FormattedString& other)
	{
		Free ();
		CopyFrom (other);
		return *this;
	}

	FormattedString& operator += (const FormattedString& other)
	{
		Append (other);
		return *this;
	}

	FormattedString& Append (const char* format, ...)
	{
		va_list args;
		va_start (args, format);

		Append (FormatArgs (args, format));

		va_end(args);

		return *this;
	}

	FormattedString& Append (const FormattedString& other)
	{
		size_t actualLength = strlen (mPtr) + strlen (other.mPtr) + 1;
		if (actualLength > mLength)
		{
			char* ptr = new char [actualLength];
			memcpy (ptr, mPtr, strlen(mPtr) + 1);
			delete[] mPtr;
			mPtr = ptr;
			mLength = actualLength;
		}
		memcpy (mPtr+strlen(mPtr), other.mPtr, strlen(other.mPtr)+1);
		return *this;
	}

	void SetFormat (const FormatArgs& formatArgs)
	{
		const char* format = formatArgs.mFormat;
		va_list args = formatArgs.mArgs;

		int lengthWithoutNull = _vscprintf (
			format, args);
		
		mLength = 1 + lengthWithoutNull;
		mPtr = new char [mLength];
		
		int written = vsnprintf_s(
			mPtr, mLength, lengthWithoutNull, 
			format, args);
		
		assert (written == lengthWithoutNull);
	}

	~FormattedString ()
	{
		Free ();
	}

	operator const char* () const { return mPtr; }

private:

	void Free ()
	{
		if (mPtr)
		{
			delete[] mPtr;
			mPtr = 0;
			mLength = 0;
		}
	}

	void CopyFrom (const FormattedString& other)
	{
		mLength = other.mLength;
		mPtr = new char [mLength];
		memcpy (mPtr, other.mPtr, mLength);
	}

	char* mPtr;
	size_t mLength;
};
