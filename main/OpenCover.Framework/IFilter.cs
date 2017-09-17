using Mono.Cecil;

namespace OpenCover.Framework
{
    /// <summary>
    /// A filter that is used to decide whether an assembly/class pair is instrumented
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Add a filter
        /// </summary>
        /// <param name="assemblyClassName">A filter is of the format (+ or -)[assemblyName]className, wildcards are allowed. <br/>
        /// i.e. -[mscorlib], -[System.*]*, +[App.*]*, +[*]*
        /// </param>
        void AddFilter(string assemblyClassName);

        /// <summary>
        /// Decides whether an assembly should be included in the instrumentation
        /// </summary>
        /// <param name="processName">The name of the process being profiled</param>
        /// <param name="assemblyName">the name of the assembly under profile</param>
        /// <remarks>All assemblies matching either the inclusion or exclusion filter should be included 
        /// as it is the class that is being filtered within these unless the class filter is *</remarks>
        bool UseAssembly(string processName, string assemblyName);

        /// <summary>
        /// Decides whether an assembly should be analysed for test methods
        /// </summary>
        /// <param name="assemblyName">the name of the assembly under profile</param>
        /// <returns>true - if the assembly matches the test assembly filter</returns>
        bool UseTestAssembly(string assemblyName);

        /// <summary>
        /// Add file exclusion filters
        /// </summary>
        /// <param name="exclusionFilters"></param>
        void AddFileExclusionFilters(string[] exclusionFilters);

        /// <summary>
        /// Determine if an [assemblyname]classname pair matches the current Exclusion or Inclusion filters  
        /// </summary>
        /// <param name="assemblyName">the name of the assembly under profile</param>
        /// <param name="className">the name of the class under profile</param>
        /// <returns>false - if pair matches the exclusion filter or matches no filters, true - if pair matches in the inclusion filter</returns>
        bool InstrumentClass(string assemblyName, string className);

        /// <summary>
        /// Determine if an [assemblyname]classname pair matches the current Exclusion or Inclusion filters  
        /// </summary>
        /// <param name="processName">The name of the process</param>
        /// <param name="assemblyName">The name of the assembly under profile</param>
        /// <param name="className">the name of the class under profile</param>
        /// <returns>false - if pair matches the exclusion filter or matches no filters, true - if pair matches in the inclusion filter</returns>
        bool InstrumentClass(string processName, string assemblyName, string className);


        /// <summary>
        /// Add attribute exclusion filters
        /// </summary>
        /// <param name="exclusionFilters">An array of filters that are used to wildcard match an attribute</param>
        void AddAttributeExclusionFilters(string[] exclusionFilters);

        /// <summary>
        /// Is this entity (method/type) excluded due to an attributeFilter
        /// </summary>
        /// <param name="entity">The entity to test</param>
        /// <returns></returns>
        bool ExcludeByAttribute(IMemberDefinition entity);

        /// <summary>
        /// Is this entity excluded due to an attributeFilter
        /// </summary>
        /// <param name="entity">The entity to test</param>
        /// <returns></returns>
        bool ExcludeByAttribute(AssemblyDefinition entity);

        /// <summary>
        /// Is this file excluded
        /// </summary>
        /// <param name="fileName">The name of the file to test</param>
        /// <returns></returns>
        bool ExcludeByFile(string fileName);

        /// <summary>
        /// Add test file filters
        /// </summary>
        /// <param name="testFilters"></param>
        void AddTestFileFilters(string[] testFilters);

        /// <summary>
        /// Is the method an auto-implemented property get/set
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        bool IsAutoImplementedProperty(MethodDefinition method);

        /// <summary>
        /// Is the method an F# implementation detail
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        bool IsFSharpInternal(MethodDefinition method);

        /// <summary>
        /// filters should be treated as regular expressions rather than wildcard
        /// </summary>
        bool RegExFilters { get; }

        /// <summary>
        /// Should we instrument this process
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        bool InstrumentProcess(string processName);

        /// <summary>
        /// Add a folder to the list that modules in these folders (and their children) should be excluded
        /// </summary>
        /// <param name="excludedPath"></param>
        void AddExcludedFolder(string excludedPath);
        
        /// <summary>
        /// Should we use this module based on it's path
        /// </summary>
        /// <param name="modulePath"></param>
        /// <returns></returns>
        bool UseModule(string modulePath);
    }

}
