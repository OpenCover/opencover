using System.Diagnostics;
using System.Linq;
using OpenCover.Framework.Symbols;

namespace OpenCover.Framework.Model
{
    /// <summary>
    /// The purpose of this class is to be build the instumentation model for a target assembly
    /// </summary>
    public class InstrumentationModelBuilder : IInstrumentationModelBuilder
    {
        private readonly ISymbolManager _symbolManager;

        public InstrumentationModelBuilder(ISymbolManager symbolManager)
        {
            _symbolManager = symbolManager;
        }

        public Module BuildModuleModel()
        {
            var module = new Module {FullName = _symbolManager.ModulePath, Files = _symbolManager.GetFiles()};
            module.Classes = _symbolManager.GetInstrumentableTypes();
            foreach (var @class in module.Classes)
            {
                BuildClassModel(@class, module.Files);
            }

            return module;
        }

        private void BuildClassModel(Class @class, File[] files)
        {
            var methods = _symbolManager
                .GetConstructorsForType(@class, files)
                .Union(_symbolManager.GetMethodsForType(@class, files));

            foreach (var method in methods)
            {
                method.SequencePoints = _symbolManager.GetSequencePointsForToken(method.MetadataToken);
            }

            @class.Methods = methods.Where(method => method.SequencePoints != null).ToArray();
        }
    }
}
