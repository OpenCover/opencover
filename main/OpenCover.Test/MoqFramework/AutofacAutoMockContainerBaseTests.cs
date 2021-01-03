using NUnit.Framework;

namespace OpenCover.Test.MoqFramework
{
    public interface IStub { }

    public class Stub : IStub { }

    [TestFixture]
    public class AutofacAutoMockContainerBaseTests
         : AutofacAutoMockContainerBase<IStub, Stub>
    {
        [Test]
        public void Does_Instance_Return_Instantiated_Object()
        {
            // arrange

            // act
            var o = Instance;

            // assert
            Assert.IsInstanceOf(typeof(Stub), o);
        }
    }
}
