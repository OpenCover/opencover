namespace OpenCover.Framework.Model
{
    /// <summary>
    /// Describes how a method or class was skipped
    /// </summary>
    public enum SkippedMethod
    {
        /// <summary>
        /// Entity was skipped due to a matching exclusion attribute filter
        /// </summary>
        Attribute,

        /// <summary>
        /// Entity was skipped due to a matching exclusion file filter
        /// </summary>
        File,

        /// <summary>
        /// Entity was skipped due to a matching exclusion module/class filter 
        /// </summary>
        Filter,

        /// <summary>
        /// Entity was skipped due to a missing PDB
        /// </summary>
        MissingPdb,
    }
}