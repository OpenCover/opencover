﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenCover.Framework.Model;
using OpenCover.Test.MoqFramework;

namespace OpenCover.Test.Framework.Model
{
    [TestFixture]
    public class InstrumentationModelBuilderFactoryTests
        : UnityAutoMockContainerBase<IInstrumentationModelBuilderFactory, InstrumentationModelBuilderFactory>
    {
        [Test]
        public void CreateModelBuilder_Creates_Model()
        {
            // arrange
            var assemblyPath = Path.GetDirectoryName(GetType().Assembly.Location);

            // act
            var model = Instance.CreateModelBuilder(Path.Combine(assemblyPath, "OpenCover.Test.dll"), "OpenCover.Test");

            // assert
            Assert.IsNotNull(model);
            Assert.IsTrue(model.CanInstrument);
        }

    }
}
