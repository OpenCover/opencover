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
            var builder = _instrumentationModelBuilderFactory.CreateModelBuilder(modulePath, assemblyName);
            if (!_filter.UseAssembly(assemblyName))
            {
                var module = builder.BuildModuleModel(false);
                module.SkippedDueTo = SkippedMethod.Filter;
                _persistance.PersistModule(module);
                return false;
            }
            if (!builder.CanInstrument)
            {
                var module = builder.BuildModuleModel(false);
                module.SkippedDueTo = SkippedMethod.MissingPdb;
                _persistance.PersistModule(module);
                return false;
            }
            _persistance.PersistModule(builder.BuildModuleModel(true));
            return true;
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
    }
}