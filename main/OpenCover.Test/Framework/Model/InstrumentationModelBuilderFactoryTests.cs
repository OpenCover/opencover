using System.IO;
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
                .Setup(x => x.GetSymbolFolders(It.IsAny<string>(), It.IsAny<ICommandLine>()))
                .Returns(new[]
                    {new SymbolFile($"{Path.Combine(assemblyPath, "OpenCover.Test.pdb")}", new PdbReaderProvider())});

            // act
            var model = Instance.CreateModelBuilder(Path.Combine(assemblyPath, "OpenCover.Test.dll"), "OpenCover.Test");

            // assert
            Assert.IsNotNull(model);
            Assert.IsTrue(model.CanInstrument);
        }

    }
}
