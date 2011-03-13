using System.Collections.Generic;

namespace OpenCover.Framework.Model
{
    public class Method
    {
        public int MetadataToken { get; set; }
        public string Name { get; set; }
        public IList<SequencePoint> SequencePoints { get; set; }
    }
}
