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
    public class BasePersistanceStub : BasePersistance
    {
        public BasePersistanceStub(ICommandLine commandLine)
            : base(commandLine)
        {
        }

        public override void Commit()
        {
            //throw new NotImplementedException();
        }
    }

    [TestFixture]
    public class BasePersistenceTests :
        UnityAutoMockContainerBase<IPersistance, BasePersistanceStub>
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

        [Test]
        public void IsTracking_True_IfModuleKnown()
        {
            // arrange
            var module = new Module() {FullName = "ModulePath"};
            module.Aliases.Add("ModulePath");
            Instance.PersistModule(module);

            // act
            var tracking = Instance.IsTracking("ModulePath");

            // assert
            Assert.IsTrue(tracking);
        }

        [Test]
        public void Can_GetSequencePoints_Of_MethodByToken()
        {
            // arrange
            var methodPoint = new InstrumentationPoint();
            var target = new SequencePoint();
            InstrumentationPoint[] pts;
            var module = new Module() { FullName = "ModulePath", Classes = new[]
                {
                    new Class() { FullName = "namespace.class", Methods = new[] { new Method() { MethodPoint = methodPoint, MetadataToken = 1001, 
                        SequencePoints = new[] { target } } } }
                }};

            module.Aliases.Add("ModulePath");
            Instance.PersistModule(module);

            // act
            Instance.GetSequencePointsForFunction("ModulePath", 1001, out pts);

            // assert
            Assert.AreEqual(methodPoint.UniqueSequencePoint, pts[0].UniqueSequencePoint);
            Assert.AreEqual(target.UniqueSequencePoint, pts[1].UniqueSequencePoint);
        }

        [Test]
        public void Can_GetBranchPoints_Of_MethodByToken()
        {
            // arrange
            var target = new BranchPoint();
            BranchPoint[] pts;
            var module = new Module() {
                    FullName = "ModulePath",
                    Classes = new[]
                    {
                        new Class() { FullName = "namespace.class", Methods = new[] { new Method() { MetadataToken = 1001, 
                            BranchPoints = new[] { target } } } }
                    }};

            module.Aliases.Add("ModulePath");
            Instance.PersistModule(module);

            // act
            Instance.GetBranchPointsForFunction("ModulePath", 1001, out pts);

            // assert
            Assert.AreEqual(target.UniqueSequencePoint, pts[0].UniqueSequencePoint);
        }

        [Test]
        public void Can_GetFullClassName_Of_MethodByToken()
        {
            // arrange
            var target = new SequencePoint();
            SequencePoint[] pts;
            var module = new Module() {
                FullName = "ModulePath",
                Classes = new[]
                {
                    new Class() { FullName = "namespace.class", Methods = new[] { new Method() { MetadataToken = 1001 } } }
                }};

            module.Aliases.Add("ModulePath");
            Instance.PersistModule(module);

            // act
            var name = Instance.GetClassFullName("ModulePath", 1001);

            // assert
            Assert.AreEqual("namespace.class", name);
        }

        [Test]
        public void SaveVisitPoints_Aggregates_Visits()
        {
            // arrange
            var pt1 = new SequencePoint();
            var pt2 = new SequencePoint();

            var data = new List<byte>();
            data.AddRange(BitConverter.GetBytes((UInt32)4));
            data.AddRange(BitConverter.GetBytes(pt1.UniqueSequencePoint));
            data.AddRange(BitConverter.GetBytes(pt2.UniqueSequencePoint));
            data.AddRange(BitConverter.GetBytes(pt2.UniqueSequencePoint));
            data.AddRange(BitConverter.GetBytes(pt2.UniqueSequencePoint));

            // act
            Instance.SaveVisitData(data.ToArray());

            // assert
            Assert.AreEqual(1, SequencePoint.GetCount(pt1.UniqueSequencePoint));
            Assert.AreEqual(3, SequencePoint.GetCount(pt2.UniqueSequencePoint));
        }


        [Test]
        public void GetSequencePoints_GetsPoints_When_ModuleAndFunctionKnown()
        {
            // arrange
            var methodPoint = new InstrumentationPoint();
            var module = new Module() { FullName = "ModulePath", Classes = new[] { new Class() { Methods = new[] { new Method() { MetadataToken = 1, SequencePoints = new[] { new SequencePoint() { VisitCount = 1000 } } } } } } };

            module.Aliases.Add("ModulePath");
            Instance.PersistModule(module);

            // act
            InstrumentationPoint[] points;
            Instance.GetSequencePointsForFunction("ModulePath", 1, out points);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(2, points.Count());
            Assert.AreEqual(1000, points[1].VisitCount);
        }

        [Test]
        public void GetSequencePoints_GetsZeroPoints_When_ModuleNotKnown()
        {
            // arrange
            Instance.PersistModule(new Module() { FullName = "ModuleName", Classes = new[] { new Class() { Methods = new[] { new Method() { MetadataToken = 1, SequencePoints = new[] { new SequencePoint() { VisitCount = 1000 } } } } } } });

            // act
            InstrumentationPoint[] points;
            Instance.GetSequencePointsForFunction("ModuleName1", 1, out points);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(0, points.Count());
        }


        [Test]
        public void GetSequencePoints_GetsZeroPoints_When_FunctionNotKnown()
        {
            // arrange
            var module = new Module()
            {
                FullName = "ModuleName",
                Classes = new[] { new Class() { Methods = new[] 
                { new Method() { MetadataToken = 1, SequencePoints = new[] { new SequencePoint() { VisitCount = 1000 } } } } } }
            };
            module.Aliases.Add("ModuleName");
            Instance.PersistModule(module);

            // act
            InstrumentationPoint[] points;
            Instance.GetSequencePointsForFunction("ModuleName", 2, out points);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(0, points.Count());
        }

        [Test]
        public void GeBranchPoints_GetsZeroPoints_When_FunctionNotKnown()
        {
            // arrange
            var module = new Module()
            {
                FullName = "ModuleName",
                Classes = new[] { new Class() { Methods = new[] 
                { new Method() { MetadataToken = 1, BranchPoints = new[] { new BranchPoint() { VisitCount = 1000 } } } } } }
            };
            module.Aliases.Add("ModuleName");
            Instance.PersistModule(module);

            // act
            BranchPoint[] points;
            Instance.GetBranchPointsForFunction("ModuleName", 2, out points);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(0, points.Count());
        }

    }
}
