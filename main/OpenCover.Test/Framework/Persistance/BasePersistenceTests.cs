using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using OpenCover.Framework;
using OpenCover.Framework.Communication;
using OpenCover.Framework.Model;
using OpenCover.Framework.Persistance;
using OpenCover.Test.MoqFramework;
using log4net;

namespace OpenCover.Test.Framework.Persistance
{
    public class BasePersistanceStub : BasePersistance
    {
        public BasePersistanceStub(ICommandLine commandLine, ILog logger)
            : base(commandLine, logger)
        {
        }
    }

    [TestFixture]
    public class BasePersistenceTests :
        UnityAutoMockContainerBase<IPersistance, BasePersistanceStub>
    {
        [Test]
        public void Can_Add_Valid_Module_To_Session()
        {
            //arrange

            // act 
            Instance.PersistModule(new Module() { Classes = new Class[0] });
            Instance.PersistModule(new Module() { TrackedMethods = new TrackedMethod[0] });

            // assert
            Assert.AreEqual(2, Instance.CoverageSession.Modules.Count());
        }

        [Test]
        public void CanNot_Add_Invalid_Module_To_Session()
        {
            //arrange

            // act 
            Instance.PersistModule(null);

            // assert
            Assert.AreEqual(0, Instance.CoverageSession.Modules.Count());
        }

        [Test]
        public void Can_Add_SeveralModules_To_Session()
        {
            //arrange

            // act 
            var module1 = new Module() { ModuleHash = "123", FullName = "Path1", Classes = new Class[0]};
            module1.Aliases.Add("Path1");
            var module2 = new Module() { ModuleHash = "123", FullName = "Path2", Classes = new Class[0]};
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

            // act 
            var module1 = new Module() { ModuleHash = "123", FullName = "Path1", Classes = new Class[0]};
            module1.Aliases.Add("Path1");
            var module2 = new Module() { ModuleHash = "123", FullName = "Path2", Classes = new Class[0]};
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
            var module = new Module() {FullName = "ModulePath", Classes = new Class[0]};
            module.Aliases.Add("ModulePath");
            Instance.PersistModule(module);

            // act
            var tracking = Instance.IsTracking("ModulePath");

            // assert
            Assert.IsTrue(tracking);
        }

        [Test]
        public void IsTracking_Fase_IfModuleSkipped()
        {
            // arrange
            var module = new Module() { FullName = "ModulePath"};
            module.MarkAsSkipped(SkippedMethod.Filter);

            module.Aliases.Add("ModulePath");
            Instance.PersistModule(module);

            // act
            var tracking = Instance.IsTracking("ModulePath");

            // assert
            Assert.IsFalse(tracking);
        }

        [Test]
        public void Can_GetSequencePoints_Of_MethodByToken()
        {
            // arrange
            var target = new SequencePoint();
            InstrumentationPoint[] pts;
            var module = new Module() { FullName = "ModulePath", Classes = new[]
                {
                    new Class() { FullName = "namespace.class", Methods = new[] { new Method() { MethodPoint = target, MetadataToken = 1001, 
                        SequencePoints = new[] { target } } } }
                }};

            module.Aliases.Add("ModulePath");
            Instance.PersistModule(module);

            // act
            Instance.GetSequencePointsForFunction("ModulePath", 1001, out pts);

            // assert
            Assert.AreEqual(target.UniqueSequencePoint, pts[0].UniqueSequencePoint);
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

            var points = new[] { pt1.UniqueSequencePoint, pt2.UniqueSequencePoint, pt2.UniqueSequencePoint, pt2.UniqueSequencePoint };
            data.AddRange(BitConverter.GetBytes((UInt32)points.Count()));
            foreach (var point in points)
                data.AddRange(BitConverter.GetBytes(point));

            // act
            Instance.SaveVisitData(data.ToArray());

            // assert
            Assert.AreEqual(1, InstrumentationPoint.GetVisitCount(pt1.UniqueSequencePoint));
            Assert.AreEqual(3, InstrumentationPoint.GetVisitCount(pt2.UniqueSequencePoint));
        }

        [Test]
        public void SaveVisitPoints_Warns_WhenPointID_IsOutOfRange_Low()
        {
            // arrange
            var data = new List<byte>();
            data.AddRange(BitConverter.GetBytes((UInt32)1));
            data.AddRange(BitConverter.GetBytes((UInt32)0));
            
            // act
            Instance.SaveVisitData(data.ToArray());

            //assert
            Container.GetMock<ILog>().Verify(x => x.DebugFormat(It.IsAny<string>(), 
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<object>()), Times.Once());
        }

        [Test]
        public void SaveVisitPoints_Warns_WhenPointID_IsOutOfRange_High()
        {
            // arrange
            var data = new List<byte>();
            data.AddRange(BitConverter.GetBytes((UInt32)1));
            data.AddRange(BitConverter.GetBytes((UInt32)1000000));
            
            // act
            Instance.SaveVisitData(data.ToArray());

            //assert
            Container.GetMock<ILog>().Verify(x => x.DebugFormat(It.IsAny<string>(), 
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<object>()), Times.Once());
        }


        [Test]
        public void SaveVisitPoints_Aggregates_Visits_ForTrackedMethods()
        {
            // arrange
            var pt1 = new SequencePoint();
            var pt2 = new SequencePoint();

            var data = new List<byte>();
            var points = new[] { 
                1 | (uint)MSG_IdType.IT_MethodEnter, pt1.UniqueSequencePoint, pt2.UniqueSequencePoint, 1 | (uint)MSG_IdType.IT_MethodLeave, 
                2 | (uint)MSG_IdType.IT_MethodEnter, pt2.UniqueSequencePoint, pt2.UniqueSequencePoint, 2 | (uint)MSG_IdType.IT_MethodLeave };
            data.AddRange(BitConverter.GetBytes((UInt32)points.Count()));
            foreach (var point in points)
                data.AddRange(BitConverter.GetBytes(point));

            // act
            Instance.SaveVisitData(data.ToArray());

            // assert
            Assert.AreEqual(1, InstrumentationPoint.GetVisitCount(pt1.UniqueSequencePoint));
            Assert.AreEqual(3, InstrumentationPoint.GetVisitCount(pt2.UniqueSequencePoint));
            Assert.AreEqual(1, pt1.TrackedMethodRefs.Count());
            Assert.AreEqual(1, pt1.TrackedMethodRefs[0].VisitCount);
            Assert.AreEqual(2, pt2.TrackedMethodRefs.Count());
            Assert.AreEqual(1, pt2.TrackedMethodRefs[0].VisitCount);
            Assert.AreEqual(2, pt2.TrackedMethodRefs[1].VisitCount);
        }

        [Test]
        public void GetSequencePoints_GetsPoints_When_ModuleAndFunctionKnown_FirstPointIsSequence()
        {
            // arrange
            var seqPoint = new SequencePoint() {VisitCount = 1000};
            var module = new Module() { FullName = "ModulePath", Classes = new[] { new Class() { Methods = new[] { new Method() { MethodPoint = seqPoint, MetadataToken = 1, SequencePoints = new[] { seqPoint } } } } } };

            module.Aliases.Add("ModulePath");
            Instance.PersistModule(module);

            // act
            InstrumentationPoint[] points;
            Instance.GetSequencePointsForFunction("ModulePath", 1, out points);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(1, points.Count());
            Assert.AreEqual(1000, points[0].VisitCount);
        }

        [Test]
        public void GetSequencePoints_GetsPoints_When_ModuleAndFunctionKnown_FirstPointIsNotSequence()
        {
            // arrange
            var methodPoint = new InstrumentationPoint() {VisitCount = 2000};
            var module = new Module() { FullName = "ModulePath", Classes = new[] { new Class() { Methods = new[] { new Method() { MethodPoint = methodPoint, MetadataToken = 1, SequencePoints = new[] { new SequencePoint() { VisitCount = 1000 } } } } } } };

            module.Aliases.Add("ModulePath");
            Instance.PersistModule(module);

            // act
            InstrumentationPoint[] points;
            Instance.GetSequencePointsForFunction("ModulePath", 1, out points);

            // assert
            Assert.IsNotNull(points);
            Assert.AreEqual(2, points.Count());
            Assert.AreEqual(2000, points[0].VisitCount);
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

        [Test]
        public void Commit_With_NoModules()
        {
            // arrange
            Instance.CoverageSession.Modules = null;

            // act
            Assert.DoesNotThrow(() => Instance.Commit());

            Assert.AreEqual(0, Instance.CoverageSession.Summary.NumSequencePoints);
            Assert.AreEqual(0, Instance.CoverageSession.Summary.VisitedSequencePoints);
            Assert.AreEqual(0, Instance.CoverageSession.Summary.NumBranchPoints);
            Assert.AreEqual(0, Instance.CoverageSession.Summary.VisitedBranchPoints);
            Assert.AreEqual(0, Instance.CoverageSession.Summary.MaxCyclomaticComplexity);
            Assert.AreEqual(0, Instance.CoverageSession.Summary.MinCyclomaticComplexity);

        }


        [Test]
        public void Commit_With_NoClasses()
        {
            // arrange
            Instance.CoverageSession.Modules = new [] {new Module(){Classes = null}};

            // act
            Assert.DoesNotThrow(() => Instance.Commit());

            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Summary.NumSequencePoints);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Summary.VisitedSequencePoints);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Summary.NumBranchPoints);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Summary.VisitedBranchPoints);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Summary.MaxCyclomaticComplexity);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Summary.MinCyclomaticComplexity);
        }

        [Test]
        public void Commit_With_NoMethods()
        {
            // arrange
            Instance.CoverageSession.Modules = new[] { new Module() { Classes = new []{new Class(), } } };

            // act
            Assert.DoesNotThrow(() => Instance.Commit());

            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Classes[0].Summary.NumSequencePoints);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Classes[0].Summary.VisitedSequencePoints);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Classes[0].Summary.NumBranchPoints);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Classes[0].Summary.VisitedBranchPoints);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Classes[0].Summary.MaxCyclomaticComplexity);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Classes[0].Summary.MinCyclomaticComplexity);
        }

        [Test]
        public void Commit_With_NoInstrumentedPoints()
        {
            // arrange
            Instance.CoverageSession.Modules = new[] { new Module() { Classes = new[] { new Class(){Methods = new []{new Method(), }}, } } };

            // act
            Assert.DoesNotThrow(() => Instance.Commit());

            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.NumSequencePoints);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.VisitedSequencePoints);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.NumBranchPoints);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.VisitedBranchPoints);
            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.MaxCyclomaticComplexity);
            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.MinCyclomaticComplexity);
        }

        [Test]
        public void Commit_With_WithMethodPointsOnly_GetsValue()
        {
            // arrange
            var point = new InstrumentationPoint();
            Instance.CoverageSession.Modules = new[] { new Module() { Classes = new[] { new Class() { Methods = new[] { new Method() { MethodPoint = point }, } }, } } };

            // act
            InstrumentationPoint.AddVisitCount(point.UniqueSequencePoint, 0, 25);
            Assert.DoesNotThrow(() => Instance.Commit());

            // assert
            Assert.AreEqual(25, point.VisitCount);

            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.NumSequencePoints);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.VisitedSequencePoints);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.NumBranchPoints);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.VisitedBranchPoints);
            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.MaxCyclomaticComplexity);
            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.MinCyclomaticComplexity);
        }

        [Test]
        public void Commit_With_WithSequencePointsOnly()
        {
            // arrange
            var point = new SequencePoint();
            Instance.CoverageSession.Modules = new[] { new Module() { Classes = new[] { new Class() { Methods = new[] { new Method() { SequencePoints = new [] { point }}}}}}};

            // act
            InstrumentationPoint.AddVisitCount(point.UniqueSequencePoint, 0, 37);
            Assert.DoesNotThrow(() => Instance.Commit());

            // assert
            Assert.AreEqual(37, point.VisitCount);

            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.NumSequencePoints);
            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.VisitedSequencePoints);
            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.NumBranchPoints);
            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.VisitedBranchPoints);
            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.MaxCyclomaticComplexity);
            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.MinCyclomaticComplexity);
        }

        [Test]
        public void Class_Summary_Aggregates_Methods()
        {
            // arrange
            Instance.CoverageSession.Modules = new[]
                {
                    new Module()
                        {
                            Classes =
                                new[]
                                    {
                                        new Class()
                                            {
                                                Methods =
                                                    new[]
                                                        {
                                                            new Method()
                                                                {
                                                                    SequencePoints = new[] {new SequencePoint() {VisitCount = 1}},
                                                                    CyclomaticComplexity = 1
                                                                },
                                                            new Method()
                                                                {
                                                                    SequencePoints = new[] {new SequencePoint() {VisitCount = 1}, new SequencePoint() {VisitCount = 0}},
                                                                    CyclomaticComplexity = 10
                                                                },
                                                            new Method()
                                                                {
                                                                    SequencePoints = new[] {new SequencePoint() {VisitCount = 0}},
                                                                    CyclomaticComplexity = 3                     
                                                                }
                                                        }
                                            }
                                    }
                        }
                };

            // act
            Assert.DoesNotThrow(() => Instance.Commit());

            // assert

            Assert.AreEqual(4, Instance.CoverageSession.Modules[0].Classes[0].Summary.NumSequencePoints);
            Assert.AreEqual(2, Instance.CoverageSession.Modules[0].Classes[0].Summary.VisitedSequencePoints);
            Assert.AreEqual(50, Instance.CoverageSession.Modules[0].Classes[0].Summary.SequenceCoverage);
            Assert.AreEqual(3, Instance.CoverageSession.Modules[0].Classes[0].Summary.NumBranchPoints);
            Assert.AreEqual(2, Instance.CoverageSession.Modules[0].Classes[0].Summary.VisitedBranchPoints);
            Assert.AreEqual(66.67m, Instance.CoverageSession.Modules[0].Classes[0].Summary.BranchCoverage);
            Assert.AreEqual(10, Instance.CoverageSession.Modules[0].Classes[0].Summary.MaxCyclomaticComplexity);
            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes[0].Summary.MinCyclomaticComplexity);
        }

        [Test]
        public void Module_Summary_Aggregates_Classes()
        {
            // arrange
            Instance.CoverageSession.Modules = new[]
                {
                    new Module()
                        {
                            Classes =
                                new[]
                                    {
                                        new Class()
                                            {
                                                Methods =
                                                    new[]
                                                        {
                                                            new Method()
                                                                {
                                                                    SequencePoints = new[] {new SequencePoint() {VisitCount = 1}},
                                                                    CyclomaticComplexity = 4
                                                                },
                                                        }
                                            },
                                        new Class()
                                            {
                                                Methods =
                                                    new[]
                                                        {
                                                            new Method()
                                                                {
                                                                    SequencePoints = new[] {new SequencePoint() {VisitCount = 1}, new SequencePoint() {VisitCount = 0}},
                                                                    CyclomaticComplexity = 17
                                                                },
                                                            new Method()
                                                                {
                                                                    SequencePoints = new[] {new SequencePoint() {VisitCount = 0}},
                                                                    CyclomaticComplexity = 6
                                                                }
                                                        }
                                            }
                                    }
                        }
                };

            // act
            Assert.DoesNotThrow(() => Instance.Commit());

            // assert

            Assert.AreEqual(4, Instance.CoverageSession.Modules[0].Summary.NumSequencePoints);
            Assert.AreEqual(2, Instance.CoverageSession.Modules[0].Summary.VisitedSequencePoints);
            Assert.AreEqual(50, Instance.CoverageSession.Modules[0].Summary.SequenceCoverage);
            Assert.AreEqual(3, Instance.CoverageSession.Modules[0].Summary.NumBranchPoints);
            Assert.AreEqual(2, Instance.CoverageSession.Modules[0].Summary.VisitedBranchPoints);
            Assert.AreEqual(66.67m, Instance.CoverageSession.Modules[0].Summary.BranchCoverage);
            Assert.AreEqual(4, Instance.CoverageSession.Modules[0].Classes[0].Summary.MaxCyclomaticComplexity);
            Assert.AreEqual(4, Instance.CoverageSession.Modules[0].Classes[0].Summary.MinCyclomaticComplexity);
            Assert.AreEqual(17, Instance.CoverageSession.Modules[0].Classes[1].Summary.MaxCyclomaticComplexity);
            Assert.AreEqual(6, Instance.CoverageSession.Modules[0].Classes[1].Summary.MinCyclomaticComplexity);
            Assert.AreEqual(17, Instance.CoverageSession.Modules[0].Summary.MaxCyclomaticComplexity);
            Assert.AreEqual(4, Instance.CoverageSession.Modules[0].Summary.MinCyclomaticComplexity);
        }

        [Test]
        public void Session_Summary_Aggregates_Modules()
        {
            // arrange
            Instance.CoverageSession.Modules = new[]
                {
                    new Module()
                        {
                            Classes =
                                new[]
                                    {
                                        new Class()
                                            {
                                                Methods =
                                                    new[]
                                                        {
                                                            new Method()
                                                                {
                                                                    SequencePoints = new[] {new SequencePoint() {VisitCount = 1}},
                                                                    CyclomaticComplexity = 7
                                                                },
                                                        }
                                            },
                                    }
                        },
                    new Module()
                        {
                            Classes =
                                new[]
                                    {
                                        new Class()
                                            {
                                                Methods =
                                                    new[]
                                                        {
                                                            new Method()
                                                                {
                                                                    SequencePoints = new[] {new SequencePoint() {VisitCount = 1}, new SequencePoint() {VisitCount = 0}},
                                                                    CyclomaticComplexity = 3
                                                                },
                                                            new Method()
                                                                {
                                                                    SequencePoints = new[] {new SequencePoint() {VisitCount = 0}},
                                                                    CyclomaticComplexity = 6

                                                                }
                                                        }
                                            }
                                    }
                        }
                };

            // act
            Assert.DoesNotThrow(() => Instance.Commit());

            // assert

            Assert.AreEqual(4, Instance.CoverageSession.Summary.NumSequencePoints);
            Assert.AreEqual(2, Instance.CoverageSession.Summary.VisitedSequencePoints);
            Assert.AreEqual(50, Instance.CoverageSession.Summary.SequenceCoverage);
            Assert.AreEqual(3, Instance.CoverageSession.Summary.NumBranchPoints);
            Assert.AreEqual(2, Instance.CoverageSession.Summary.VisitedBranchPoints);
            Assert.AreEqual(66.67m, Instance.CoverageSession.Summary.BranchCoverage);
            Assert.AreEqual(7, Instance.CoverageSession.Modules[0].Summary.MaxCyclomaticComplexity);
            Assert.AreEqual(7, Instance.CoverageSession.Modules[0].Summary.MinCyclomaticComplexity);
            Assert.AreEqual(6, Instance.CoverageSession.Modules[1].Summary.MaxCyclomaticComplexity);
            Assert.AreEqual(3, Instance.CoverageSession.Modules[1].Summary.MinCyclomaticComplexity);
            Assert.AreEqual(7, Instance.CoverageSession.Summary.MaxCyclomaticComplexity);
            Assert.AreEqual(3, Instance.CoverageSession.Summary.MinCyclomaticComplexity);
        }

        [Test]
        public void Commit_With_WithSequencePointsOnly_NoVisits()
        {
            // arrange
            var point = new SequencePoint();
            Instance.CoverageSession.Modules = new[] { new Module() { Classes = new[] { new Class() { Methods = new[] { new Method() { SequencePoints = new[] { point } } } } } } };

            // act
            Assert.DoesNotThrow(() => Instance.Commit());

            // assert
            Assert.AreEqual(0, point.VisitCount);

            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.NumSequencePoints);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.VisitedSequencePoints);
            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.NumBranchPoints);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.VisitedBranchPoints);
        }

        [Test]
        public void Commit_With_WithBranchPointsOnly()
        {
            // arrange
            var point = new BranchPoint();
            Instance.CoverageSession.Modules = new[] { new Module() { Classes = new[] { new Class() { Methods = new[] { new Method() { BranchPoints = new [] { point } } } } } } };

            // act
            InstrumentationPoint.AddVisitCount(point.UniqueSequencePoint, 0, 42);
            Assert.DoesNotThrow(() => Instance.Commit());

            // assert
            Assert.AreEqual(42, point.VisitCount);

            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.NumSequencePoints);
            Assert.AreEqual(0, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.VisitedSequencePoints);
            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.NumBranchPoints);
            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Summary.VisitedBranchPoints);

        }

        [Test]
        public void GetTrackingMethod_ReturnsFase_For_UnTrackedMethod()
        {
            // arrange
            var module = new Module()
                             {
                                 TrackedMethods =
                                     new[]
                                         {
                                             new TrackedMethod()
                                                 {MetadataToken = 1234, Name = "MethodName", UniqueId = 5678}
                                         }
                             };
            module.Aliases.Add("ModulePath");
            Instance.PersistModule(module);

            // act
            uint trackedId = 0;
            var result = Instance.GetTrackingMethod("ModulePath", "AssemblyName", 2222, out trackedId);

            // assert
            Assert.IsFalse(result);
            Assert.AreEqual(0, trackedId);
        }

        [Test]
        public void GetTrackingMethod_RetrievesId_For_TrackedMethod()
        {
            // arrange
            var module = new Module()
            {
                TrackedMethods =
                    new[]
                                         {
                                             new TrackedMethod()
                                                 {MetadataToken = 1234, Name = "MethodName", UniqueId = 5678}
                                         }
            };
            module.Aliases.Add("ModulePath");
            Instance.PersistModule(module);

            // act
            uint trackedId = 0;
            var result = Instance.GetTrackingMethod("ModulePath", "AssemblyName", 1234, out trackedId);

            // assert
            Assert.IsTrue(result);
            Assert.AreEqual(5678, trackedId);
        }

        readonly SkippedMethod[] _skippedReasonsModules = new[] { SkippedMethod.Filter, SkippedMethod.MissingPdb, };

        [Test]
        public void HideSkipped_With_X_Removes_SkippedModules([ValueSource("_skippedReasonsModules")]SkippedMethod reason)
        {
            // arrange
            Container.GetMock<ICommandLine>()
                .SetupGet(x => x.HideSkipped)
                .Returns(new List<SkippedMethod>(){reason});

            var module = new Module() {FullName = "Skipped"};
            module.MarkAsSkipped(reason);
            Instance.PersistModule(module);
            Instance.PersistModule(new Module() { FullName = "Keep" });

            // act
            Instance.Commit();

            // assert
            Assert.AreEqual(1, Instance.CoverageSession.Modules.Count());
            Assert.AreEqual("Keep", Instance.CoverageSession.Modules[0].FullName);
        }

        readonly SkippedMethod[] _skippedReasonsClasses = new[] { SkippedMethod.Filter, SkippedMethod.File, SkippedMethod.Attribute, };

        [Test]
        public void HideSkipped_With_X_Removes_SkippedClasses([ValueSource("_skippedReasonsClasses")]SkippedMethod reason)
        {
            // arrange
            Container.GetMock<ICommandLine>()
                .SetupGet(x => x.HideSkipped)
                .Returns(new List<SkippedMethod>() { reason });

            var @class = new Class() {FullName = "Skipped"};
            @class.MarkAsSkipped(reason);
            Instance.PersistModule(new Module()
            {
                FullName = "Keep",
                Classes = new[]
                        {
                            @class,
                            new Class() {FullName = "KeepClass", Methods = new[]{new Method()}},
                        }
            });

            // act
            Instance.Commit();

            // assert
            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes.Count());
            Assert.AreEqual("KeepClass", Instance.CoverageSession.Modules[0].Classes[0].FullName);
        }

        /// <summary>
        /// NOTE: A (compiler) generated method will not have any file references 
        /// </summary>
        [Test]
        public void InstrumentationPoints_Of_CompilerGeneratedMethods_Belonging_To_Classes_WhereAllOtherMethodsAreSkipped_AreRemoved()
        {
            // arrange
            var point = new InstrumentationPoint() { IsSkipped = false };
            var module = new Module()
                {
                    FullName = "Keep",
                    Classes = new[]
                        {
                            new Class() {Methods = new[] {new Method() {MethodPoint = point}, new Method()}},
                            new Class()
                                {
                                    Methods = new[]
                                        {
                                            new Method()
                                                {
                                                    MethodPoint = new InstrumentationPoint() {IsSkipped = false},
                                                    FileRef = new FileRef()
                                                },
                                            new Method()
                                        }
                                },
                        }
                };

            module.Classes[0].Methods[1].MarkAsSkipped(SkippedMethod.File);
            module.Classes[1].Methods[1].MarkAsSkipped(SkippedMethod.File);

            Instance.PersistModule(module);

            // act
            Instance.Commit();

            // assert
            Assert.IsNull(Instance.CoverageSession.Modules[0].Classes[0].Methods[0].MethodPoint);
            Assert.IsTrue(point.IsSkipped);
            Assert.IsFalse(Instance.CoverageSession.Modules[0].Classes[1].Methods[0].MethodPoint.IsSkipped);
        }

        readonly SkippedMethod[] _skippedReasonsMethods = new[] { SkippedMethod.File, SkippedMethod.Attribute, };

        [Test]
        public void HideSkipped_With_X_Removes_SkippedMethods([ValueSource("_skippedReasonsMethods")]SkippedMethod reason)
        {
            // arrange
            Container.GetMock<ICommandLine>()
                .SetupGet(x => x.HideSkipped)
                .Returns(new List<SkippedMethod>() { reason });

            var method = new Method() {Name = "SkippedMethod", FileRef = new FileRef()};
            method.MarkAsSkipped(reason);

            var module = new Module()
                {
                    FullName = "Keep",
                    Classes = new[]
                        {
                            new Class()
                                {
                                    FullName = "RemoveClassThoughSkippedAttribute",
                                    Methods = new[] { method }
                                },
                            new Class()
                                {
                                    FullName = "KeepClass",
                                    Methods = new[] { method, new Method() {Name = "KeepMethod", FileRef = new FileRef()}}
                                }
                        }
                };

            module.Classes[0].MarkAsSkipped(SkippedMethod.Attribute);

            Instance.PersistModule(module);

            // act
            Instance.Commit();

            // assert
            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes.Count());
            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes[0].Methods.Count());
            Assert.AreEqual("KeepMethod", Instance.CoverageSession.Modules[0].Classes[0].Methods[0].Name);
        }

        [Test]
        public void HideSkipped_With_File_Removes_EmptyClasses()
        {
            // arrange
            Container.GetMock<ICommandLine>()
                .SetupGet(x => x.HideSkipped)
                .Returns(new List<SkippedMethod>() { SkippedMethod.File });

            var module = new Module()
            {
                FullName = "Keep",
                Classes = new[]
                        {
                            new Class()
                                {
                                    FullName = "KeepClassThoughSkippedAttribute",
                                    Methods = new[] {new Method(){FileRef = new FileRef()}}
                                },
                            new Class()
                                {
                                    FullName = "RemoveClassThoughSkippedAttribute",
                                    Methods = new[] {new Method() {Name = "SkippedMethod", FileRef = new FileRef()}}
                                },
                            new Class()
                                {
                                    FullName = "KeepClass",
                                    Methods =
                                        new[]
                                            {
                                                new Method(){Name = "SkippedMethod", FileRef = new FileRef()},
                                                new Method() {Name = "KeepMethod", FileRef = new FileRef()}
                                            }
                                }
                        }
            };

            module.Classes[0].MarkAsSkipped(SkippedMethod.Attribute);
            module.Classes[1].MarkAsSkipped(SkippedMethod.Attribute);
            module.Classes[1].Methods[0].MarkAsSkipped(SkippedMethod.File);
            module.Classes[2].Methods[0].MarkAsSkipped(SkippedMethod.File);

            Instance.PersistModule(module);

            // act
            Instance.Commit();

            // assert
            Assert.AreEqual(2, Instance.CoverageSession.Modules[0].Classes.Count());
            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Classes[1].Methods.Count());
            Assert.AreEqual("KeepMethod", Instance.CoverageSession.Modules[0].Classes[1].Methods[0].Name);
        }

        [Test]
        public void HideSkipped_With_File_Removes_UnreferencedFiles()
        {
            // arrange
            Container.GetMock<ICommandLine>()
                .SetupGet(x => x.HideSkipped)
                .Returns(new List<SkippedMethod>() { SkippedMethod.File });

            var method = new Method() {Name = "SkippedMethod", FileRef = new FileRef() {UniqueId = 2}};
            method.MarkAsSkipped(SkippedMethod.File);

            Instance.PersistModule(new Module()
            {
                FullName = "Keep",
                Files = new []{new File(){UniqueId = 1, FullPath = "KeepFile"}, new File(){UniqueId = 2} },
                Classes = new[]
                        {
                            new Class()
                                {
                                    FullName = "KeepClass",
                                    Methods =
                                        new[]
                                            {
                                                method,
                                                new Method() {Name = "KeepMethod", FileRef = new FileRef(){UniqueId = 1}}
                                            }
                                }
                        }
            });

            Assert.AreEqual(2, Instance.CoverageSession.Modules[0].Files.Count());

            // act
            Instance.Commit();

            // assert
            Assert.AreEqual(1, Instance.CoverageSession.Modules[0].Files.Count());
            Assert.AreEqual("KeepFile", Instance.CoverageSession.Modules[0].Files[0].FullPath);
        }

    }
}
