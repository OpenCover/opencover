namespace OpenCover.Framework.Model
{
    /// <summary>
    /// A skipped entity that also carries a Summary object which is not 
    /// always serialized
    /// </summary>
    public abstract class SummarySkippedEntity : SkippedEntity
    {
        /// <summary>
        /// Initialise
        /// </summary>
        protected SummarySkippedEntity()
        {
            Summary = new Summary();
        } 

        /// <summary>
        /// A Summary of results for a entity
        /// </summary>
        public Summary Summary { get; set; }

        /// <summary>
        /// Control serialization of the Summary  object
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeSummary() { return !ShouldSerializeSkippedDueTo(); }
    }
}