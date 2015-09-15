using System;

namespace OpenCover.Console.CrashReporter
{
    /// <summary>
    /// http://crashreporterdotnet.codeplex.com/SourceControl/latest#CrashReporter.NET/trunk/CrashReporter.NET/DrDump/AnonymousData.cs
    /// </summary>
    internal class AnonymousData
    {
        public Exception Exception { get; set; }
        public string ToEmail { get; set; }
        public Guid? ApplicationGuid { get; set; }
    }
}