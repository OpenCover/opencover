using NUnit.Framework;
using OpenCover.Framework.Symbols;
using OpenCover.Test.MoqFramework;

namespace OpenCover.Test.Framework.Symbols
{
    [TestFixture]
    public class CecilSymbolManagerFactoryTests :
        UnityAutoMockContainerBase<ISymbolManagerFactory, CecilSymbolManagerFactory>
    {
        [Test]
        public void Factory_Creates_SymbolManager()
        {
            // arrange
            var modulePath = GetType().Assembly.Location;
            var moduleName = GetType().Assembly.FullName;

            // act
            var manager = Instance.CreateSymbolManager(modulePath, moduleName);

            // assert
            Assert.IsNotNull(manager);
            Assert.AreEqual(modulePath, manager.ModulePath);
            Assert.AreEqual(moduleName, manager.ModuleName);

        }
    }
}