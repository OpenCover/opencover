using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Communication;
using OpenCover.Framework.Model;
using OpenCover.Framework.Persistance;
using OpenCover.Test.MoqFramework;

namespace OpenCover.Test.Framework.Persistance
{
    [TestFixture]
    public class BasePersistenceTests :
        UnityAutoMockContainerBase<IPersistance, BasePersistance>
    {
        [Test]
        public void Can_Add_Module_To_Session()
        {
            //arrange
            Assert.IsNull(Instance.CoverageSession.Modules);

            // act 
            Instance.PersistModule(new Module());

            // assert
            Assert.AreEqual(1, Instance.CoverageSession.Modules.Count());
        }

        [Test]
        public void Can_Add_SeveralModules_To_Session()
        {
            //arrange
            Assert.IsNull(Instance.CoverageSession.Modules);

            // act 
            var module1 = new Module() { ModuleHash = "123", FullName = "Path1" };
            module1.Aliases.Add("Path1");
            var module2 = new Module() { ModuleHash = "123", FullName = "Path2" };
            module2.Aliases.Add("Path2");
            Instance.PersistModule(module1);
            Instance.PersistModule(module2);

            // assert
            Assert.AreEqual(2, Instance.CoverageSession.Modules.Count());
        }

        [Test]
        public void Can_Merge_Modules_In_Session_When_HashMatched()
        {
            //arrange
            Container.GetMock<ICommandLine>()
                .SetupGet(x => x.MergeByHash)
                .Returns(true);

            Assert.IsNull(Instance.CoverageSession.Modules);

            // act 
            var module1 = new Module() { ModuleHash = "123", FullName = "Path1" };
            module1.Aliases.Add("Path1");
            var module2 = new Module() { ModuleHash = "123", FullName = "Path2" };
            module2.Aliases.Add("Path2");
            Instance.PersistModule(module1);
            Instance.PersistModule(module2);
            // assert
            Assert.AreEqual(1, Instance.CoverageSession.Modules.Count());
        }

    }
}
