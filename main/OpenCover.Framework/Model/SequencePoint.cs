namespace OpenCover.Framework.Model
{
    public class SequencePoint
    {
        public int Ordinal { get; set; }
        public int Offset { get; set; }
        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }
        public int VisitCount { get; set; }
    }
}
