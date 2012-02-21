//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System.Collections.Generic;
using OpenCover.Framework.Model;

namespace OpenCover.Framework.Persistance
{
    /// <summary>
    /// A persistant entiry
    /// </summary>
    public interface IPersistance
    {
        /// <summary>
        /// A module that is to be persisted
        /// </summary>
        /// <param name="module"></param>
        void PersistModule(Module module);

        /// <summary>
        /// Save the instrumented data
        /// </summary>
        void Commit();

        /// <summary>
        /// Get the sequence points for a function
        /// </summary>
        /// <param name="modulePath">The identifying path to the module</param>
        /// <param name="functionToken">The token of the function</param>
        /// <param name="sequencePoints">The sequence points that make up that function</param>
        /// <returns>true - if sequence points exist</returns>
        bool GetSequencePointsForFunction(string modulePath, int functionToken, out InstrumentationPoint[] sequencePoints);

        /// <summary>
        /// Get the branch points for a function
        /// </summary>
        /// <param name="modulePath">The identifying path to the module</param>
        /// <param name="functionToken">The token of the function</param>
        /// <param name="branchPoints">The branch points that make up that function</param>
        /// <returns>true - if sequence points exist</returns>
        bool GetBranchPointsForFunction(string modulePath, int functionToken, out BranchPoint[] branchPoints);

        /// <summary>
        /// Check if the module is to be tracked i.e. instrumented
        /// </summary>
        /// <param name="modulePath"></param>
        /// <returns></returns>
        bool IsTracking(string modulePath);

        /// <summary>
        /// Get the full class name i.e. including namespace that the function is contained in
        /// </summary>
        /// <param name="modulePath"></param>
        /// <param name="functionToken"></param>
        /// <returns></returns>
        string GetClassFullName(string modulePath, int functionToken);

        /// <summary>
        /// The coverage session - this is the root entity of a persisted document
        /// </summary>
        CoverageSession CoverageSession { get; }

        /// <summary>
        /// extract and save the visit data
        /// </summary>
        /// <param name="data"></param>
        void SaveVisitData(byte[] data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modulePath"></param>
        /// <param name="assemblyName"></param>
        /// <param name="functionToken"></param>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        bool GetTrackingMethod(string modulePath, string assemblyName, int functionToken, out uint uniqueId);
    }
}