﻿using System.Collections.Generic;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// An entity that can be instrumented
    /// </summary>
    public class Method
    {
        public int MetadataToken { get; set; }
        public string Name { get; set; }
        public File File { get; set; }
        public SequencePoint[] SequencePoints { get; set; }
    }
}