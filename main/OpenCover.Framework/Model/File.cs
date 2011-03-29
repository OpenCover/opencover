using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// File details
    /// </summary>
    public class File
    {
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
            return obj.GetHashCode();
        }
    }
}
