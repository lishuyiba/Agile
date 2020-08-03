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
        private readonly IAgileFileProvider _fileProvider;

        public PluginService(IAgileFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
            _pluginsInfo = Singleton<IPluginsInfo>.Instance;
        }

        public void InstallPlugins()
        {
            var pluginDescriptors = _pluginsInfo.PluginDescriptors.Where(descriptor => !descriptor.Installed).ToList();
            pluginDescriptors = pluginDescriptors.Where(descriptor => _pluginsInfo.PluginNamesToInstall.Any(item => item.Equals(descriptor.SystemName))).ToList();
            if (!pluginDescriptors.Any())
            {
                return;
            }
            foreach (var descriptor in pluginDescriptors.OrderBy(pluginDescriptor => pluginDescriptor.DisplayOrder))
            {
                try
                {
                    var pluginToInstall = _pluginsInfo.PluginNamesToInstall.FirstOrDefault(plugin => plugin.Equals(descriptor.SystemName));

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
            {
                return;
            }
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
            {
                return;
            }
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

        public virtual void DeletePlugins()
        {
            var pluginDescriptors = _pluginsInfo.PluginDescriptors.Where(descriptor => !descriptor.Installed).ToList();

            pluginDescriptors = pluginDescriptors.Where(descriptor => _pluginsInfo.PluginNamesToDelete.Contains(descriptor.SystemName)).ToList();
            if (!pluginDescriptors.Any())
                return;

            foreach (var descriptor in pluginDescriptors)
            {
                try
                {
                    var pluginDirectory = _fileProvider.GetDirectoryName(descriptor.OriginalAssemblyFile);
                    if (_fileProvider.DirectoryExists(pluginDirectory))
                    {
                        _fileProvider.DeleteDirectory(pluginDirectory);
                    }

                    _pluginsInfo.PluginDescriptors.Remove(descriptor);

                    _pluginsInfo.PluginNamesToDelete.Remove(descriptor.SystemName);
                }
                catch (Exception exception)
                {
                }
            }
            _pluginsInfo.Save();
        }

        public void ClearPlugins(string systemName)
        {
            var pluginDescriptors = _pluginsInfo.PluginNamesToDelete.Where(s => s.Contains(systemName)).Any();
            if (pluginDescriptors)
            {
                _pluginsInfo.PluginNamesToDelete.Remove(systemName);
            }
            pluginDescriptors = _pluginsInfo.PluginNamesToUninstall.Where(s => s.Contains(systemName)).Any();
            if (pluginDescriptors)
            {
                _pluginsInfo.PluginNamesToUninstall.Remove(systemName);
            }
            _pluginsInfo.Save();
        }
    }
}
