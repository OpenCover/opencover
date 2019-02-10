using System.IO;
using Mono.Cecil;
using Mono.Cecil.Pdb;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Model;
using OpenCover.Framework.Symbols;
using OpenCover.Test.MoqFramework;

namespace OpenCover.Test.Framework.Model
{
    [TestFixture]
    internal class InstrumentationModelBuilderFactoryTests
        : UnityAutoMockContainerBase<IInstrumentationModelBuilderFactory, InstrumentationModelBuilderFactory>
    {
        [Test]
        public void CreateModelBuilder_Creates_Model()
        {
            // arrange
            var assemblyPath = Path.GetDirectoryName(GetType().Assembly.Location);
            Assert.IsNotNull(assemblyPath);
            Container.GetMock<ISymbolFileHelper>()
                .Setup(x => x.GetSymbolFileLocations(It.IsAny<string>(), It.IsAny<ICommandLine>()))
                .Returns(new[] { $"{Path.Combine(assemblyPath, "OpenCover.Test.pdb")}" });

            // act
            var model = Instance.CreateModelBuilder(Path.Combine(assemblyPath, "OpenCover.Test.dll"), "OpenCover.Test");

            // assert
            Assert.IsNotNull(model);
            Assert.IsTrue(model.CanInstrument);
        }

    }
}
