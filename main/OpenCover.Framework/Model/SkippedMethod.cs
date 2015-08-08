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
        Attribute = 3,

        /// <summary>
        /// Entity was skipped due to a matching exclusion file filter
        /// </summary>
        File = 4,

        /// <summary>
        /// Entity was skipped due to a matching exclusion module/class filter 
        /// </summary>
        Filter = 2,

        /// <summary>
        /// Entity was skipped due to a missing PDB
        /// </summary>
        MissingPdb = 1,

        /// <summary>
        /// Entity was skipped by inference (usually related to File filters)
        /// </summary>
        Inferred = 5,

        /// <summary>
        /// Entity (method) was skipped as it is an auto-implemented property.
        /// </summary>
        AutoImplementedProperty = 6,

        /// <summary>
        /// Entity (method) was skipped as it is native code.
        /// </summary>
        NativeCode = 7,

        /// <summary>
        /// Entity (method) was skipped for other reasons.
        /// </summary>
        Unknown = 8
    }
}