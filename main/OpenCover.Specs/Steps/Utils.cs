using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NUnit.Framework;

namespace OpenCover.Specs.Steps
{
    internal class Utils
    {
        public static string GetTotalCoverage(string openCoverXml)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(openCoverXml);
            var coverage = xmlDoc.SelectSingleNode("/CoverageSession/Summary/@sequenceCoverage");
            Assert.NotNull(coverage);
            return coverage.Value;
        }
    }
}
