using Agile.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Agile.Services.Plugins
{
    public partial class PluginService : IPluginService
    {

        private readonly IPluginsInfo _pluginsInfo;

        public PluginService()
        {
            _pluginsInfo = Singleton<IPluginsInfo>.Instance;
        }

        public void InstallPlugins()
        {
            var pluginDescriptors = _pluginsInfo.PluginDescriptors.Where(descriptor => !descriptor.Installed).ToList();

            pluginDescriptors = pluginDescriptors.Where(descriptor => _pluginsInfo.PluginNamesToInstall
                .Any(item => item.Equals(descriptor.SystemName))).ToList();
            if (!pluginDescriptors.Any())
                return;

            foreach (var descriptor in pluginDescriptors.OrderBy(pluginDescriptor => pluginDescriptor.DisplayOrder))
            {
                try
                {
                    var pluginToInstall = _pluginsInfo.PluginNamesToInstall
                        .FirstOrDefault(plugin => plugin.Equals(descriptor.SystemName));
                    _pluginsInfo.InstalledPlugins.Add(descriptor.GetBaseInfoCopy);
                    _pluginsInfo.PluginNamesToInstall.Remove(pluginToInstall);

                    descriptor.Installed = true;
                    descriptor.ShowInPluginsList = true;
                }
                catch (Exception exception)
                {

                }
            }
            _pluginsInfo.Save();
        }

        public virtual void UninstallPlugins()
        {
            var pluginDescriptors = _pluginsInfo.PluginDescriptors.Where(descriptor => descriptor.Installed).ToList();

            pluginDescriptors = pluginDescriptors
                .Where(descriptor => _pluginsInfo.PluginNamesToUninstall.Contains(descriptor.SystemName)).ToList();
            if (!pluginDescriptors.Any())
                return;

            foreach (var descriptor in pluginDescriptors.OrderByDescending(pluginDescriptor => pluginDescriptor.DisplayOrder))
            {
                try
                {
                    _pluginsInfo.InstalledPlugins.Remove(descriptor);
                    _pluginsInfo.PluginNamesToUninstall.Remove(descriptor.SystemName);

                    descriptor.Installed = false;
                    descriptor.ShowInPluginsList = true;
                }
                catch (Exception exception)
                {

                }
            }

            _pluginsInfo.Save();
        }

        public virtual IEnumerable<PluginDescriptor> GetPluginDescriptors()
        {
            var pluginDescriptors = _pluginsInfo.PluginDescriptors;

            pluginDescriptors = pluginDescriptors.OrderBy(descriptor => descriptor.Group)
                .ThenBy(descriptor => descriptor.DisplayOrder).ToList();

            return pluginDescriptors;
        }

        public virtual bool IsRestartRequired()
        {
            return _pluginsInfo.PluginNamesToInstall.Any()
                || _pluginsInfo.PluginNamesToUninstall.Any()
                || _pluginsInfo.PluginNamesToDelete.Any();
        }

        public PluginDescriptor GetPluginDescriptorBySystemName(string systemName)
        {
            return GetPluginDescriptors().FirstOrDefault(descriptor => descriptor.SystemName.Equals(systemName));
        }
        public virtual void PreparePluginToInstall(string systemName)
        {
            if (_pluginsInfo.PluginNamesToInstall.Any(item => item == systemName))
                return;

            var pluginsAfterRestart = _pluginsInfo.InstalledPlugins.Select(pd => pd.SystemName).Where(installedSystemName => !_pluginsInfo.PluginNamesToUninstall.Contains(installedSystemName)).ToList();

            pluginsAfterRestart.AddRange(_pluginsInfo.PluginNamesToInstall.Select(item => item));

            _pluginsInfo.PluginNamesToInstall.Add(systemName);
            _pluginsInfo.Save();
        }

        public IEnumerable<PluginDescriptor> GetPluginDescriptors(string systemName)
        {
            return GetPluginDescriptors().Where(s => s.SystemName.Equals(systemName));
        }

        public virtual PluginDescriptor GetPluginDescriptorBySystemName<TPlugin>(string systemName)
        {
            return GetPluginDescriptors().FirstOrDefault(descriptor => descriptor.SystemName.Equals(systemName));
        }

        public virtual void PreparePluginToUninstall(string systemName)
        {
            if (_pluginsInfo.PluginNamesToUninstall.Contains(systemName))
                return;
            var dependentPlugins = GetPluginDescriptors(systemName).ToList();
            var descriptor = GetPluginDescriptorBySystemName<IPlugin>(systemName);

            if (dependentPlugins.Any())
            {
                var dependsOn = new List<string>();

                foreach (var dependentPlugin in dependentPlugins)
                {
                    if (!_pluginsInfo.InstalledPlugins.Select(pd => pd.SystemName).Contains(dependentPlugin.SystemName))
                        continue;
                    if (_pluginsInfo.PluginNamesToUninstall.Contains(dependentPlugin.SystemName))
                        continue;

                    dependsOn.Add(string.IsNullOrEmpty(dependentPlugin.FriendlyName)
                        ? dependentPlugin.SystemName
                        : dependentPlugin.FriendlyName);
                }
            }

            var plugin = descriptor?.Instance<IPlugin>();
            plugin?.PreparePluginToUninstall();

            _pluginsInfo.PluginNamesToUninstall.Add(systemName);
            _pluginsInfo.Save();
        }

        public virtual void PreparePluginToDelete(string systemName)
        {
            if (_pluginsInfo.PluginNamesToDelete.Contains(systemName))
                return;

            _pluginsInfo.PluginNamesToDelete.Add(systemName);
            _pluginsInfo.Save();
        }
    }
}
