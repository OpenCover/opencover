//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// A file reference within the coverage session and is used to point to an existing File entity
    /// </summary>
    public class FileRef
    {
        /// <summary>
        /// The uniqueid of a file in a coverage session
        /// </summary>
        [XmlAttribute("uid")]
        public UInt32 UniqueId { get; set; }
    }

    /// <summary>
    /// File details of a source file
    /// </summary>
    public class File : FileRef
    {
        private static int _uId;

        static readonly List<File> Files = new List<File>();

        internal static void ResetAfterLoading()
        {
            _uId = (int)Files.Max(x => x.UniqueId);
        }

        /// <summary>
        /// A standard constructor
        /// </summary>
        public File()
        {
            UniqueId = (UInt32)Interlocked.Increment(ref _uId);
            Files.Add(this);
        }

        /// <summary>
        /// The path to file
        /// </summary>
        [XmlAttribute("fullPath")]
        public string FullPath { get; set; }
    }

    internal class FileEqualityComparer : IEqualityComparer<File>
    {
        public bool Equals(File x, File y)
        {
            return x.FullPath == y.FullPath;
        }

        public int GetHashCode(File obj)
        {
            return 0;
        }
    }
}
