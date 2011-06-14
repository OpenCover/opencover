using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace OpenCover.Framework.Model
{
    public class FileRef
    {
        [XmlAttribute("uid")]
        public UInt32 UniqueId { get; set; }
    }

    /// <summary>
    /// File details
    /// </summary>
    public class File : FileRef
    {
        private static int _uId;
        
        public File()
        {
            UniqueId = (UInt32)Interlocked.Increment(ref _uId);
        }

        [XmlAttribute("fullPath")]
        public string FullPath { get; set; }
    }

    public class FileEqualityComparer : IEqualityComparer<File>
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
