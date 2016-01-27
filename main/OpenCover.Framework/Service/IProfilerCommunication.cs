//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//

using OpenCover.Framework.Model;

namespace OpenCover.Framework.Service
{
    /// <summary>
    /// Defines the tye of operations the profiler will make
    /// </summary>
    public interface IProfilerCommunication
    {
        /// <summary>
        /// Should we track this assembly
        /// </summary>
        /// <param name="processPath"></param>
        /// <param name="modulePath"></param>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        bool TrackAssembly(string processPath, string modulePath, string assemblyName);

        /// <summary>
        /// Get sequence points
        /// </summary>
        /// <param name="processPath"></param>
        /// <param name="modulePath"></param>
        /// <param name="assemblyName"></param>
        /// <param name="functionToken"></param>
        /// <param name="sequencePoints"></param>
        /// <returns></returns>
        bool GetSequencePoints(string processPath, string modulePath, string assemblyName, int functionToken, out InstrumentationPoint[] sequencePoints);

        /// <summary>
        /// Get branch points
        /// </summary>
        /// <param name="processPath"></param>
        /// <param name="modulePath"></param>
        /// <param name="assemblyName"></param>
        /// <param name="functionToken"></param>
        /// <param name="branchPoints"></param>
        /// <returns></returns>
        bool GetBranchPoints(string processPath, string modulePath, string assemblyName, int functionToken, out BranchPoint[] branchPoints);

        /// <summary>
        /// We are stopping...
        /// </summary>
        void Stopping();

        /// <summary>
        /// Should we track this method
        /// </summary>
        /// <param name="modulePath"></param>
        /// <param name="assemblyName"></param>
        /// <param name="functionToken"></param>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        bool TrackMethod(string modulePath, string assemblyName, int functionToken, out uint uniqueId);

        /// <summary>
        /// Should we track this process
        /// </summary>
        /// <param name="processPath"></param>
        /// <returns></returns>
        bool TrackProcess(string processPath);
    }
}
