//
// OpenCover - S Wilde
//
// This source code is released under the MIT License; see the accompanying license file.
//

using System.Collections.Generic;
using OpenCover.Framework.Strategy;

namespace OpenCover.Framework.Model
{
    public interface IInstrumentationModelBuilderFactory
    {
        IInstrumentationModelBuilder CreateModelBuilder(string modulePath, string moduleName);
        IEnumerable<ITrackedMethodStrategy> MethodStrategies { get; }
    }
}