using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// The entity can be skipped from coverage but needs to supply a reason
    /// </summary>
    public abstract class SkippedEntity
    {
        private SkippedMethod? _skippedDueTo;

        /// <summary>
        /// If this class has been skipped then this value will describe why
        /// </summary>
        [XmlAttribute("skippedDueTo")]
        public SkippedMethod SkippedDueTo
        {
            get { return _skippedDueTo.GetValueOrDefault(); }
            set { _skippedDueTo = value; }
        }

        /// <summary>
        /// If this class has been skipped then this value will allow the data to be serialized
        /// </summary>
        public bool ShouldSerializeSkippedDueTo() { return _skippedDueTo.HasValue; }

        /// <summary>
        /// Mark an entity as skipped
        /// </summary>
        /// <param name="reason">Provide a reason</param>
        public abstract void MarkAsSkipped(SkippedMethod reason);
    }
}