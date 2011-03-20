using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenCover.Framework.Symbols;
using OpenCover.Test.MoqFramework;

namespace OpenCover.Test.Framework.Symbols
{
    [TestFixture]
    public class SymbolManagerFactoryTests :
        UnityAutoMockContainerBase<ISymbolManagerFactory, SymbolManagerFactory>
    {
        [Test]
        public void Factory_Creates_SymbolManager()
        {
            // arrange
            var modulePath = GetType().Assembly.Location;

            // act
            var manager = Instance.CreateSymbolManager(modulePath);

            // assert
            Assert.IsNotNull(manager);
            Assert.AreEqual(modulePath, manager.ModulePath);

        }
    }
}
