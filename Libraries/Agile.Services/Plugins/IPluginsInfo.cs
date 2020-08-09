using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Services.Plugins
{
    public interface IPluginsInfo
    {
        void Save();

        bool LoadPluginInfo();

        void CopyFrom(IPluginsInfo pluginsInfo);

        IList<PluginDescriptorBaseInfo> InstalledPlugins { get; set; }

        IList<string> PluginNamesToUninstall { get; set; }

        IList<string> PluginNamesToDelete { get; set; }

        IList<string> PluginNamesToInstall { get; set; }
        IList<string> PluginNamesToUpdate { get; set; }

        IList<PluginLoadedAssemblyInfo> AssemblyLoadedCollision { get; set; }

        IList<PluginDescriptor> PluginDescriptors { get; set; }

        IList<string> IncompatiblePlugins { get; set; }
    }
}
