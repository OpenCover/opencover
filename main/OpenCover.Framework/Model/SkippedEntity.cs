using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// The entity can be skipped from coverage but needs to supply a reason
    /// </summary>
    public abstract class SkippedEntity
    {
        private SkippedMethod? skippedDueTo;

        /// <summary>
        /// If this class has been skipped then this value will describe why
        /// </summary>
        [XmlAttribute("skippedDueTo")]
        public SkippedMethod SkippedDueTo
        {
            get { return skippedDueTo.GetValueOrDefault(); }
            set { skippedDueTo = value; }
        }

        /// <summary>
        /// If this class has been skipped then this value will allow the data to be serialized
        /// </summary>
        public bool ShouldSerializeSkippedDueTo() { return skippedDueTo.HasValue; }
    }
}