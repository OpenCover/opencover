using System;
using Mono.Cecil;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Model;
using OpenCover.Framework.Symbols;
using OpenCover.Test.MoqFramework;

namespace OpenCover.Test.Framework.Model
{
    [TestFixture]
    internal class InstrumentationModelBuilderTests :
        UnityAutoMockContainerBase<IInstrumentationModelBuilder, InstrumentationModelBuilder>
    {
        [Test]
        public void BuildModuleModel_Gets_ModulePath_From_SymbolReader()
        {
            // arrange
            Container.GetMock<ISymbolManager>()
                .SetupGet(x => x.ModulePath)
                .Returns("ModulePath");

            Container.GetMock<IFilter>()
                .Setup(x => x.UseAssembly(It.IsAny<string>()))
                .Returns(true);

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

            Container.GetMock<IFilter>()
                .Setup(x => x.UseAssembly(It.IsAny<string>()))
                .Returns(true);

            // act
            var module = Instance.BuildModuleModel();

            // assert
            Assert.AreEqual(1, module.Classes.GetLength(0));
            Assert.AreSame(@class, module.Classes[0]);
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
                .Setup(x => x.GetMethodsForType(@class, It.IsAny<File[]>()))
                .Returns(new[] { @method });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetSequencePointsForToken(It.IsAny<int>()))
                .Returns(new[] { new SequencePoint() });

            Container.GetMock<IFilter>()
                .Setup(x => x.UseAssembly(It.IsAny<string>()))
                .Returns(true);

            // act
            var module = Instance.BuildModuleModel();

            // assert
            Assert.AreEqual(1, module.Classes[0].Methods.GetLength(0));
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
                .Setup(x => x.GetMethodsForType(@class, It.IsAny<File[]>()))
                .Returns(new[] { @method });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetSequencePointsForToken(101))
                .Returns(new[] { @seqPoint });

            Container.GetMock<IFilter>()
                .Setup(x => x.UseAssembly(It.IsAny<string>()))
                .Returns(true);

            // act
            var module = Instance.BuildModuleModel();

            // assert
            Assert.AreEqual(1, module.Classes[0].Methods[0].SequencePoints.GetLength(0));
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
                .Setup(x => x.GetMethodsForType(@class, It.IsAny<File[]>()))
                .Returns(new[] { @method });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetSequencePointsForToken(101))
                .Returns(default(SequencePoint[]));

            Container.GetMock<IFilter>()
                .Setup(x => x.UseAssembly(It.IsAny<string>()))
                .Returns(true);

            // act
            var module = Instance.BuildModuleModel();

            // assert
            Assert.AreEqual(0, module.Classes[0].Methods.GetLength(0));
        }

        [Test]
        public void CanInstrument_If_AssemblyFound()
        {
            // arrange 
            var mockDef = AssemblyDefinition.CreateAssembly(
                new AssemblyNameDefinition("temp", new Version()), "temp", ModuleKind.Dll);
            Container.GetMock<ISymbolManager>()
                .SetupGet(x => x.SourceAssembly)
                .Returns(mockDef);

            // act
            var canInstrument = Instance.CanInstrument;

            // assert
            Assert.IsTrue(canInstrument);
        }

        [Test]
        public void CanBuildModelOf_RealAssembly()
        {
            // arrange
            Container.GetMock<ISymbolManager>()
                .SetupGet(x => x.ModulePath)
                .Returns(System.IO.Path.Combine(Environment.CurrentDirectory, "OpenCover.Test.dll"));
            
            Container.GetMock<IFilter>()
               .Setup(x => x.UseAssembly(It.IsAny<string>()))
               .Returns(true);

            // act
            var module = Instance.BuildModuleModel();

            // assert
            Assert.IsNotNullOrEmpty(module.ModuleHash);

        }
    }
}
