using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;
using OpenCover.Framework.Model;

namespace OpenCover.Test.Framework.Model
{
    [TestFixture]
    public class SummarySkippedEntityTests
    {
        public class FakeEntity : SummarySkippedEntity
        {
            public override void MarkAsSkipped(SkippedMethod reason)
            {
                SkippedDueTo = reason;
            }
        }

        private FakeEntity _entity;

        [SetUp]
        public void SetUp()
        {
            _entity = new FakeEntity();
        }

        [Test]
        public void When_Entity_Is_NOT_Skipped_Summary_IS_Serialized()
        {
            // arrange
            
            // act
            var serializer = new XmlSerializer(_entity.GetType());
            var writer = new StringWriter();
            serializer.Serialize(writer, _entity);
            var doc = new XmlDocument();
            doc.LoadXml(writer.ToString());

            // assert
            Assert.IsNotNull(doc.SelectSingleNode("//FakeEntity/Summary"));

        }
        
        [Test]
        public void When_Entity_IS_Skipped_Summary_Is_NOT_Serialized()
        {
            // arrange
            _entity.MarkAsSkipped(SkippedMethod.MissingPdb);

            // act
            var serializer = new XmlSerializer(_entity.GetType());
            var writer = new StringWriter();
            serializer.Serialize(writer, _entity);
            var doc = new XmlDocument();
            doc.LoadXml(writer.ToString());

            // assert
            Assert.IsNull(doc.SelectSingleNode("//FakeEntity/Summary"));
        }
    }
}
