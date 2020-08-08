using Agile.Models.Plugins;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Services.Plugins
{
    public partial interface IPluginService
    {
        PluginDescriptor GetPluginDescriptorBySystemName(string systemName);
        IEnumerable<PluginDescriptor> GetPluginDescriptors();
        void InstallPlugins();
        void UninstallPlugins();
        bool IsRestartRequired();
        bool ChangePlugin(string systemName);
        void PreparePluginToInstall(string systemName);
        void PreparePluginToUninstall(string systemName);
        void PreparePluginToDelete(string systemName);
        void DeletePlugins();
        void ClearPlugins(string systemName);
        List<PluginModel> GetPluginModels(PluginSearchModel search, int pageIndex, int pageSize, out int total);
    }
}
