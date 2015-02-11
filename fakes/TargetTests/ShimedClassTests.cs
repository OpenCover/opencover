using System;
using Microsoft.QualityTools.Testing.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Target;
using TargetFakes;
using TargetFakes.Fakes;

namespace TargetTests
{
    // samples from https://msdn.microsoft.com/en-us/library/hh549176.asp
    [TestClass]
    public class ShimedClassTests
    {
        // ReSharper disable InconsistentNaming
        private const int FUTURAMA = 1729;
        private const int HHGTTG = 42;
        // ReSharper restore InconsistentNaming

        [TestMethod]
        public void ShimedStaticMethod()
        {
            Assert.AreEqual(HHGTTG, MyStaticClassCaller.MyStaticMethod());
            using (ShimsContext.Create())
            {
                ShimMyStaticClass.MyStaticMethod = () => FUTURAMA;
                Assert.AreEqual(FUTURAMA, MyStaticClassCaller.MyStaticMethod());
            }
        }

        [TestMethod]
        public void ShimedAllInstanceMethod()
        {
            var instance = new MyInstanceClass();
            Assert.AreEqual(HHGTTG, MyInstanceClassCaller.MyInstanceMethod(instance));
            using (ShimsContext.Create())
            {
                ShimMyInstanceClass.AllInstances.MyInstanceMethod = @class => FUTURAMA;
                Assert.AreEqual(FUTURAMA, MyInstanceClassCaller.MyInstanceMethod(instance));
            }
        }

        [TestMethod]
        public void ShimedSingleInstanceMethod()
        {
            using (ShimsContext.Create())
            {
                var instance = new ShimMyInstanceClass()
                {
                    MyInstanceMethod = () => HHGTTG
                };
                Assert.AreEqual(HHGTTG, MyInstanceClassCaller.MyInstanceMethod(instance));

                instance = new ShimMyInstanceClass()
                {
                    MyInstanceMethod = () => FUTURAMA
                };
                Assert.AreEqual(FUTURAMA, MyInstanceClassCaller.MyInstanceMethod(instance));
            }
        }

        [TestMethod]
        public void ShimedConstructor()
        {
            var instance = new MyInstanceClass(8);
            Assert.AreEqual(8, MyInstanceClassCaller.MyInstanceMethod(instance));
            using (ShimsContext.Create())
            {
                ShimMyInstanceClass.ConstructorInt32 = (@class, i) =>
                {
                    var shim = new ShimMyInstanceClass(@class)
                    {
                        ValueGet = () => -5
                    };
                };

                Assert.AreEqual(8, instance.Value, "Instance was created before the shim and so should have original value");

                instance = new MyInstanceClass(8);
                Assert.AreEqual(-5, instance.Value, "Instance was created after the shim was put in place and so will have the original value");
            }
        }
    }
}