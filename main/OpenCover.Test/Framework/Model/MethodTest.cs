/*
 * Created by SharpDevelop.
 * User: ddur
 * Date: 8.1.2016.
 * Time: 7:52
 * 
 */
using System;
using NUnit.Framework;
using OpenCover.Framework.Model;

namespace OpenCover.Test.Framework.Model
{
    [TestFixture]
    public class MethodTest
    {
        [Test]
        public void MethodIsGenerated()
        {
            // arrange
            var method = new Method
            {
                FullName = "System.Boolean DD.Collections.BitSetArray::BitSetArray_<_SetItems>b__b_0(System.Int32)"
            };

            // act
            var result = method.IsGenerated;

            // assert
            Assert.True (result);
        }

        [Test]
        public void MethodIsGenerated2()
        {
            // arrange
            var method = new Method
            {
                FullName = "System.Boolean DD.Collections.BitSetArray::BitSetArray_<_SetItems>b__b_0(System.Int32)"
            };

            // act twice to cover cached result
            var result = method.IsGenerated;
            result = method.IsGenerated;

            // assert
            Assert.True (result);
        }

        [Test]
        public void MethodIsNotGenerated()
        {
            // arrange
            var method = new Method
            {
                FullName = "System.Void DD.Collections.BitSetArray::_SetItems(System.Collections.Generic.IEnumerable`1<System.Int32>)"
            };

            // act
            var result = method.IsGenerated;

            // assert
            Assert.False (result);
        }

        [Test]
        public void MethodIsNotGeneratedFullNameIsNull()
        {
            // arrange
            var method = new Method
            {
                FullName = null
            };

            // act
            var result = method.IsGenerated;

            // assert
            Assert.False (result);
        }

        [Test]
        public void MethodIsNotGeneratedFullNameIsEmpty()
        {
            // arrange
            var method = new Method
            {
                FullName = string.Empty
            };

            // act
            var result = method.IsGenerated;

            // assert
            Assert.False (result);
        }

        [Test]
        public void MethodCallNameGenerated()
        {
            // arrange
            var method = new Method
            {
                FullName = "System.Boolean DD.Collections.BitSetArray::BitSetArray_<_SetItems>b__b_0(System.Int32)"
            };

            // act
            var result = method.CallName;

            // assert
            Assert.True (result == "BitSetArray_<_SetItems>b__b_0");
        }

        [Test]
        public void MethodCallnameStandard()
        {
            // arrange
            var method = new Method
            {
                FullName = "System.Void DD.Collections.BitSetArray::_SetItems(System.Collections.Generic.IEnumerable`1<System.Int32>)"
            };

            // act
            var result = method.CallName;

            // assert
            Assert.True (result == "_SetItems");
        }

        [Test]
        public void MethodCallNameTwice()
        {
            // arrange
            var method = new Method
            {
                FullName = "System.Boolean DD.Collections.BitSetArray::BitSetArray_<_SetItems>b__b_0(System.Int32)"
            };

            // act twice to cover cached result
            var result = method.CallName;
            result = method.CallName;

            // assert
            Assert.True (result == "BitSetArray_<_SetItems>b__b_0");
        }

        [Test]
        public void MethodCallNameWhenFullNameIsNull()
        {
            // arrange
            var method = new Method
            {
                FullName = null
            };

            // act
            var result = method.CallName;

            // assert
            Assert.True (result == "");
        }

        [Test]
        public void MethodCallNameWhenFullNameIsEmpty()
        {
            // arrange
            var method = new Method
            {
                FullName = string.Empty
            };

            // act
            var result = method.CallName;

            // assert
            Assert.True (result == "");
        }

        [Test]
        public void MethodCallNameWhenFullNameIsInvalid()
        {
            // arrange
            var method = new Method
            {
                FullName = "a::c"
            };

            // act
            var result = method.CallName; // now covers all branches

            // assert
            Assert.True (result == "");
        }
    }
}
