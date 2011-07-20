//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
namespace OpenCover.Framework
{
    public interface ICommandLine
    {
        string TargetDir { get; }
        bool MergeByHash { get; }
    }
}