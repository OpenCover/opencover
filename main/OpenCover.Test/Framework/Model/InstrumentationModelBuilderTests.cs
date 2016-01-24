using System;
using System.Linq;
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
                .Setup(x => x.UseAssembly(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var module = Instance.BuildModuleModel(false);

            // assert
            Assert.IsNotNull(module);
            Assert.AreEqual("ModulePath", module.ModulePath);
            Assert.AreEqual("ModulePath", module.Aliases[0]);
        }

        [Test]
        public void BuildModuleModel_Gets_ModuleName_From_SymbolReader()
        {
            // arrange
            Container.GetMock<ISymbolManager>()
                .SetupGet(x => x.ModuleName)
                .Returns("ModuleName");

            Container.GetMock<IFilter>()
                .Setup(x => x.UseAssembly(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var module = Instance.BuildModuleModel(false);

            // assert
            Assert.IsNotNull(module);
            Assert.AreEqual("ModuleName", module.ModuleName);
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
                .Setup(x => x.UseAssembly(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var module = Instance.BuildModuleModel(true);

            // assert
            Assert.AreEqual(1, module.Classes.GetLength(0));
            Assert.AreSame(@class, module.Classes[0]);
            Container.GetMock<ISymbolManager>()
                .Verify(x => x.GetMethodsForType(It.IsAny<Class>(), It.IsAny<File[]>()), Times.Once());
        }

        [Test]
        public void BuildModuleModel_DoesNotGetMethods_For_SkippedClasses()
        {
            // arrange
            var @class = new Class();
            @class.MarkAsSkipped(SkippedMethod.File);
            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetInstrumentableTypes())
                .Returns(new[] { @class });

            Container.GetMock<IFilter>()
                .Setup(x => x.UseAssembly(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var module = Instance.BuildModuleModel(true);

            // assert
            Assert.AreEqual(1, module.Classes.GetLength(0));
            Container.GetMock<ISymbolManager>()
                .Verify(x => x.GetMethodsForType(It.IsAny<Class>(), It.IsAny<File[]>()), Times.Never());
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
                .Setup(x => x.UseAssembly(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var module = Instance.BuildModuleModel(true);

            // assert
            Assert.AreEqual(1, module.Classes[0].Methods.GetLength(0));
            Assert.AreSame(@method, module.Classes[0].Methods[0]);
        }

        [Test]
        public void BuildModuleModel_Gets_SequencePoints_From_SymbolReader()
        {
            // arrange
            var @class = new Class();
            var method = new Method{MetadataToken = 101};
            var seqPoint = new SequencePoint();
            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetInstrumentableTypes())
                .Returns(new[] { @class });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetMethodsForType(@class, It.IsAny<File[]>()))
                .Returns(new[] { method });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetSequencePointsForToken(101))
                .Returns(new[] { seqPoint });

            Container.GetMock<IFilter>()
                .Setup(x => x.UseAssembly(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var module = Instance.BuildModuleModel(true);

            // assert
            Assert.AreEqual(1, module.Classes[0].Methods[0].SequencePoints.GetLength(0));
            Assert.AreSame(seqPoint, module.Classes[0].Methods[0].SequencePoints[0]);
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
        public void CanGetDefinition_If_AssemblyFound()
        {
            // arrange 
            var mockDef = AssemblyDefinition.CreateAssembly(
                new AssemblyNameDefinition("temp", new Version()), "temp", ModuleKind.Dll);
            Container.GetMock<ISymbolManager>()
                .SetupGet(x => x.SourceAssembly)
                .Returns(mockDef);

            // act
            var definition = Instance.GetAssemblyDefinition;

            // assert
            Assert.AreSame(mockDef, definition);
        }

        [Test]
        public void CanBuildModelOf_RealAssembly()
        {
            // arrange
            Container.GetMock<ISymbolManager>()
                .SetupGet(x => x.ModulePath)
                .Returns(System.IO.Path.Combine(Environment.CurrentDirectory, "OpenCover.Test.dll"));
            
            Container.GetMock<IFilter>()
               .Setup(x => x.UseAssembly(It.IsAny<string>(), It.IsAny<string>()))
               .Returns(true);

            // act
            var module = Instance.BuildModuleModel(true);

            // assert
            Assert.IsNotNullOrEmpty(module.ModuleHash);

        }

        [Test]
        public void BuildModuleTestModel_GetsTrackedMethods_From_SymbolReader()
        {
            // arrange
            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetTrackedMethods())
                .Returns(new TrackedMethod[0]);

            // act
            var module = Instance.BuildModuleTestModel(null, true);

            // assert
            Assert.NotNull(module);
            Assert.AreEqual(0, module.TrackedMethods.GetLength(0));
            Container.GetMock<ISymbolManager>().Verify(x=>x.GetTrackedMethods(), Times.Once());

        }

        [Test]
        public void BuildModuleTestModel_GetsTrackedMethods_From_SymbolReader_UpdatesSuppliedModel()
        {
            // arrange
            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetTrackedMethods())
                .Returns(new TrackedMethod[0]);

            // act
            var origModule = new Module();
            var module = Instance.BuildModuleTestModel(origModule, true);

            // assert
            Assert.NotNull(module);
            Assert.AreSame(origModule, module);
            Assert.AreEqual(0, module.TrackedMethods.GetLength(0));
            Container.GetMock<ISymbolManager>().Verify(x => x.GetTrackedMethods(), Times.Once());

        }

        [Test]
        public void BuildModuleModel_MethodPoint_WhenOffsetZero()
        {
            // arrange
            var @class = new Class();
            var method = new Method { MetadataToken = 101 };
            var seqPoint = new SequencePoint();
            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetInstrumentableTypes())
                .Returns(new[] { @class });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetMethodsForType(@class, It.IsAny<File[]>()))
                .Returns(new[] { method });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetSequencePointsForToken(101))
                .Returns(new[] { seqPoint });

            Container.GetMock<IFilter>()
                .Setup(x => x.UseAssembly(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var module = Instance.BuildModuleModel(true);

            // assert
            Assert.AreSame(seqPoint, module.Classes[0].Methods[0].SequencePoints[0]);
            Assert.AreSame(seqPoint, module.Classes[0].Methods[0].MethodPoint);
        }

        [Test]
        public void BuildModuleModel_MethodPoint_WhenOffsetGreaterThanZero()
        {
            // arrange
            var @class = new Class();
            var method = new Method { MetadataToken = 101 };
            var seqPoint = new SequencePoint {Offset = 1};
            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetInstrumentableTypes())
                .Returns(new[] { @class });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetMethodsForType(@class, It.IsAny<File[]>()))
                .Returns(new[] { method });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetSequencePointsForToken(101))
                .Returns(new[] { seqPoint });

            Container.GetMock<IFilter>()
                .Setup(x => x.UseAssembly(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var module = Instance.BuildModuleModel(true);

            // assert
            Assert.AreSame(seqPoint, module.Classes[0].Methods[0].SequencePoints[0]);
            Assert.AreNotSame(seqPoint, module.Classes[0].Methods[0].MethodPoint);
        }

        [Test]
        public void BuildModuleModel_Gets_BranchPoints_WhenHaveSequencePoints()
        {
            // arrange
            var @class = new Class();
            var @method = new Method { MetadataToken = 101 };
            var seqPoint = new SequencePoint();
            var brPoint = new BranchPoint();
            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetInstrumentableTypes())
                .Returns(new[] { @class });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetMethodsForType(@class, It.IsAny<File[]>()))
                .Returns(new[] { @method });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetSequencePointsForToken(101))
                .Returns(new[] { seqPoint });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetBranchPointsForToken(101))
                .Returns(new[] { brPoint });

            Container.GetMock<IFilter>()
                .Setup(x => x.UseAssembly(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var module = Instance.BuildModuleModel(true);

            // assert
            Assert.AreSame(seqPoint, module.Classes[0].Methods[0].SequencePoints[0]);
            Assert.AreSame(brPoint, module.Classes[0].Methods[0].BranchPoints[0]);
        }

        [Test]
        public void BuildModuleModel_Gets_NoBranchPoints_WhenNoSequencePoints()
        {
            // arrange
            var @class = new Class();
            var @method = new Method { MetadataToken = 101 };
            var brPoint = new BranchPoint();
            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetInstrumentableTypes())
                .Returns(new[] { @class });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetMethodsForType(@class, It.IsAny<File[]>()))
                .Returns(new[] { @method });

            Container.GetMock<ISymbolManager>()
                .Setup(x => x.GetBranchPointsForToken(101))
                .Returns(new[] { brPoint });

            Container.GetMock<IFilter>()
                .Setup(x => x.UseAssembly(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            // act
            var module = Instance.BuildModuleModel(true);

            // assert
            Assert.IsFalse(module.Classes[0].Methods[0].SequencePoints.Any());
            Assert.IsFalse(module.Classes[0].Methods[0].BranchPoints.Any());
        }

    }
}
