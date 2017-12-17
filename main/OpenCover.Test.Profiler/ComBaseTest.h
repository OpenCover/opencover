#pragma once

class ComBaseTest : public ::testing::Test {
protected:
	template<class T>
	void CreateComObject(CComObject<T> **profilerInfo) const
	{
		*profilerInfo = new CComObject<T>();
		HRESULT hr = CComObject<T>::CreateInstance(profilerInfo);
		ASSERT_EQ(S_OK, hr);
		(*profilerInfo)->AddRef();
	}
};
