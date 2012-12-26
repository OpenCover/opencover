//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Diagnostics;
using System.Linq;
using OpenCover.Framework.Model;
using OpenCover.Framework.Persistance;
using OpenCover.Framework.Symbols;

namespace OpenCover.Framework.Service
{
    internal class ProfilerCommunication : IProfilerCommunication
    {
        private readonly IFilter _filter;
        private readonly IPersistance _persistance;
        private readonly IInstrumentationModelBuilderFactory _instrumentationModelBuilderFactory;

        public ProfilerCommunication(IFilter filter,
            IPersistance persistance,
            IInstrumentationModelBuilderFactory instrumentationModelBuilderFactory)
        {
            _filter = filter;
            _persistance = persistance;
            _instrumentationModelBuilderFactory = instrumentationModelBuilderFactory;
        }

        public bool TrackAssembly(string modulePath, string assemblyName)
        {
            if (_persistance.IsTracking(modulePath)) return true;
            Module module = null;
            var builder = _instrumentationModelBuilderFactory.CreateModelBuilder(modulePath, assemblyName);
            if (!_filter.UseAssembly(assemblyName))
            {
                module = builder.BuildModuleModel(false);
                module.MarkAsSkipped(SkippedMethod.Filter);
            }
            else if (!builder.CanInstrument)
            {
                module = builder.BuildModuleModel(false);
                module.MarkAsSkipped(SkippedMethod.MissingPdb);
            }

            module = module ?? builder.BuildModuleModel(true);
            
            if (_filter.UseTestAssembly(modulePath))
            {
                module = builder.BuildModuleTestModel(module, false);
            }
            _persistance.PersistModule(module);
            return !module.ShouldSerializeSkippedDueTo();
        }

        public bool GetBranchPoints(string modulePath, string assemblyName, int functionToken, out BranchPoint[] instrumentPoints)
        {
            BranchPoint[] points = null;

            var ret = GetPoints(() => _persistance.GetBranchPointsForFunction(modulePath, functionToken, out points),
                    modulePath, assemblyName, functionToken, out instrumentPoints);

            instrumentPoints = points;
            return ret;
        }

        public bool GetSequencePoints(string modulePath, string assemblyName, int functionToken, out InstrumentationPoint[] instrumentPoints)
        {
            InstrumentationPoint[] points = null;

            var ret = GetPoints(() => _persistance.GetSequencePointsForFunction(modulePath, functionToken, out points),
                                modulePath, assemblyName, functionToken, out instrumentPoints);

            instrumentPoints = points;
            return ret;
        }

        private bool GetPoints<T>(Func<bool> getPointsFunc, string modulePath, string assemblyName, int functionToken, out T[] points)
        {
            points = new T[0];
            return CanReturnPoints(modulePath, assemblyName, functionToken) && getPointsFunc();
        }

        private bool CanReturnPoints(string modulePath, string assemblyName, int functionToken)
        {
            var className = _persistance.GetClassFullName(modulePath, functionToken);
            return _filter.InstrumentClass(assemblyName, className);
        }

        public void Stopping()
        {
            _persistance.Commit();
        }

        public bool TrackMethod(string modulePath, string assemblyName, int functionToken, out uint uniqueId)
        {
            return _persistance.GetTrackingMethod(modulePath, assemblyName,functionToken, out uniqueId);
        }
    }
}