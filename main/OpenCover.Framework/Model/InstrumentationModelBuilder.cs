//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using OpenCover.Framework.Symbols;

namespace OpenCover.Framework.Model
{
    internal class InstrumentationModelBuilder : IInstrumentationModelBuilder
    {
        private readonly ISymbolManager _symbolManager;

        /// <summary>
        /// Standard constructor
        /// </summary>
        /// <param name="symbolManager">the symbol manager that will provide the data</param>
        /// <param name="filter">A filter to decide whether to include or exclude an assembly or its classes</param>
        public InstrumentationModelBuilder(ISymbolManager symbolManager)
        {
            _symbolManager = symbolManager;
        }

        public Module BuildModuleModel(bool full)
        {
            var hash = string.Empty;
            if (System.IO.File.Exists(_symbolManager.ModulePath))
            {
                hash = HashFile(_symbolManager.ModulePath);
            }
            var module = new Module
                             {
                                 ModuleName = _symbolManager.ModuleName,
                                 FullName = _symbolManager.ModulePath,
                                 ModuleHash = hash
                             };
            if (full)
            {
                module.Files = _symbolManager.GetFiles();
                module.Aliases.Add(_symbolManager.ModulePath);
                module.Classes = _symbolManager.GetInstrumentableTypes();
                foreach (var @class in module.Classes)
                {
                    BuildClassModel(@class, module.Files);
                }
            }
            return module;
        }

        private string HashFile(string sPath)
        {
            using (var sr = new StreamReader(sPath))
            using (var prov = new SHA1CryptoServiceProvider())
            {
                return BitConverter.ToString(prov.ComputeHash(sr.BaseStream));
            }
        }

        public bool CanInstrument
        {
            get { return _symbolManager.SourceAssembly != null; }
        }

        private void BuildClassModel(Class @class, File[] files)
        {
            if (@class.ShouldSerializeSkippedDueTo()) return;
            var methods = _symbolManager.GetMethodsForType(@class, files);

            foreach (var method in methods)
            {
                if (!method.ShouldSerializeSkippedDueTo())
                {
                    method.SequencePoints = _symbolManager.GetSequencePointsForToken(method.MetadataToken);
                    method.MethodPoint = (method.SequencePoints != null)
                                             ? method.SequencePoints.FirstOrDefault(pt => pt.Offset == 0)
                                             : null;
                    method.MethodPoint = method.MethodPoint ?? new InstrumentationPoint();
                    method.BranchPoints = _symbolManager.GetBranchPointsForToken(method.MetadataToken);
                }
                method.CyclomaticComplexity = _symbolManager.GetCyclomaticComplexityForToken(method.MetadataToken);
            }

            @class.Methods = methods;
        }
    }
}
