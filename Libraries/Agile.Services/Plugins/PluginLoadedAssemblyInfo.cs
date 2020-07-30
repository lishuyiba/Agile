using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agile.Services.Plugins
{
    public partial class PluginLoadedAssemblyInfo
    {
        public PluginLoadedAssemblyInfo(string shortName, string assemblyInMemory)
        {
            ShortName = shortName;
            References = new List<(string PluginName, string AssemblyName)>();
            AssemblyFullNameInMemory = assemblyInMemory;
        }

        public string ShortName { get; }

        public string AssemblyFullNameInMemory { get; }

        public List<(string PluginName, string AssemblyName)> References { get; }

        public IList<(string PluginName, string AssemblyName)> Collisions =>
            References.Where(reference => !reference.AssemblyName.Equals(AssemblyFullNameInMemory, StringComparison.CurrentCultureIgnoreCase)).ToList();
    }
}
