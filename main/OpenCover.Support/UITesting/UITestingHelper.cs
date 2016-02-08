using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OpenCover.Support.UITesting
{
// ReSharper disable once InconsistentNaming
    public static class UITestingHelper
    {
        public static void PropagateRequiredEnvironmentVariables(object data)
        {
            var pi = data as ProcessStartInfo;
            if (pi == null) 
                return;
            foreach (var ev in from DictionaryEntry ev in Environment.GetEnvironmentVariables()
                where (ev.Key.ToString().StartsWith("COR", StringComparison.InvariantCultureIgnoreCase) ||
                      ev.Key.ToString().StartsWith("OPEN", StringComparison.InvariantCultureIgnoreCase) ||
                      ev.Key.ToString().StartsWith("CHAIN", StringComparison.InvariantCultureIgnoreCase))
                where !pi.EnvironmentVariables.Cast<DictionaryEntry>()
                    .Any(e => e.Key.ToString().Equals(ev.Key.ToString(), StringComparison.InvariantCultureIgnoreCase))
                select ev)
            {
                pi.EnvironmentVariables[ev.Key.ToString()] = ev.Value.ToString();
            }
        }
    }
}
