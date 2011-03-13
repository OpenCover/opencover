using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Model;
using OpenCover.Framework.Symbols;
using OpenCover.Test.MoqFramework;

namespace OpenCover.Test.Framework
{
    [TestFixture]
    public class InstrumentationModelBuilderTests :
        UnityAutoMockContainerBase<IInstrumentationModelBuilder, InstrumentationModelBuilder>
    {
        [Test]
        public void BuildModuleModel_Gets_ModulePath_From_SymbolReader()
        {
            // arrange
            Container.GetMock<ISymbolManager>()
                .SetupGet(x => x.ModulePath)
                .Returns("ModulePath");

            // act
            var module = Instance.BuildModuleModel();

            // assert
            Assert.IsNotNull(module);
            Assert.AreEqual("ModulePath", module.FullName);
        }

        [Test]
        public void BuildModuleModel_GetsClasses_From_SymbolReader()
        {
            // arrange
            var @class = new Class();
            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetInstrumentableTypes())
                .Returns(new[] {@class});

            // act
            var module = Instance.BuildModuleModel();

            // assert
            Assert.AreEqual(1, module.Classes.Count);
            Assert.AreSame(@class, module.Classes[0]);
        }

        [Test]
        public void BuildModuleModel_Gets_ConstructorMethods_From_SymbolReader()
        {
            // arrange
            var @class = new Class();
            var @method = new Method();
            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetInstrumentableTypes())
                .Returns(new[] { @class });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetConstructorsForType(@class))
                .Returns(new [] {@method});

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetSequencePointsForToken(It.IsAny<int>()))
                .Returns(new[] {new SequencePoint()});

            // act
            var module = Instance.BuildModuleModel();

            // assert
            Assert.AreEqual(1, module.Classes[0].Methods.Count);
            Assert.AreSame(@method, module.Classes[0].Methods[0]);
        }

        [Test]
        public void BuildModuleModel_Gets_DeclaredMethods_From_SymbolReader()
        {
            // arrange
            var @class = new Class();
            var @method = new Method();
            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetInstrumentableTypes())
                .Returns(new[] { @class });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetMethodsForType(@class))
                .Returns(new[] { @method });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetSequencePointsForToken(It.IsAny<int>()))
                .Returns(new[] { new SequencePoint() });

            // act
            var module = Instance.BuildModuleModel();

            // assert
            Assert.AreEqual(1, module.Classes[0].Methods.Count);
            Assert.AreSame(@method, module.Classes[0].Methods[0]);
        }

        [Test]
        public void BuildModuleModel_Gets_SequencePoints_From_SymbolReader()
        {
            // arrange
            var @class = new Class();
            var @method = new Method(){MetadataToken = 101};
            var @seqPoint = new SequencePoint();
            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetInstrumentableTypes())
                .Returns(new[] { @class });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetMethodsForType(@class))
                .Returns(new[] { @method });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetSequencePointsForToken(101))
                .Returns(new[] { @seqPoint });

            // act
            var module = Instance.BuildModuleModel();

            // assert
            Assert.AreEqual(1, module.Classes[0].Methods[0].SequencePoints.Count);
            Assert.AreSame(@seqPoint, module.Classes[0].Methods[0].SequencePoints[0]);
        }

        [Test]
        public void BuildModule_IgnoresMethods_With_NoSequencePoints()
        {
            // arrange
            var @class = new Class();
            var @method = new Method() { MetadataToken = 101 };
            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetInstrumentableTypes())
                .Returns(new[] { @class });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetMethodsForType(@class))
                .Returns(new[] { @method });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetSequencePointsForToken(101))
                .Returns(default(SequencePoint[]));

            // act
            var module = Instance.BuildModuleModel();

            // assert
            Assert.AreEqual(0, module.Classes[0].Methods.Count);
        }
    }
}
