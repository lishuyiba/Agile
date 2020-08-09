using Agile.Core.Infrastructure;
using Agile.Models.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
            _pluginsInfo.Save();
        }

        public virtual void UninstallPlugins()
        {
            var pluginDescriptors = _pluginsInfo.PluginDescriptors.Where(descriptor => descriptor.Installed).ToList();
            pluginDescriptors = pluginDescriptors.Where(descriptor => _pluginsInfo.PluginNamesToUninstall.Contains(descriptor.SystemName)).ToList();
            if (!pluginDescriptors.Any())
            {
                return;
            }

            foreach (var descriptor in pluginDescriptors.OrderByDescending(pluginDescriptor => pluginDescriptor.DisplayOrder))
            {
                try
                {
                    _pluginsInfo.InstalledPlugins.Remove(descriptor);
                    _pluginsInfo.PluginNamesToUninstall.Remove(descriptor.SystemName);
                    descriptor.Installed = false;
                }
                catch (Exception exception)
                {
                    throw exception;
                }
            }
            _pluginsInfo.Save();
        }

        public virtual IEnumerable<PluginDescriptor> GetPluginDescriptors()
        {
            var pluginDescriptors = _pluginsInfo.PluginDescriptors;
            pluginDescriptors = pluginDescriptors.OrderBy(descriptor => descriptor.Group).ThenBy(descriptor => descriptor.DisplayOrder).ToList();
            return pluginDescriptors;
        }

        public virtual bool CheckIsRestartRequired()
        {
            return _pluginsInfo.PluginNamesToInstall.Any() || _pluginsInfo.PluginNamesToUninstall.Any() || _pluginsInfo.PluginNamesToDelete.Any() || _pluginsInfo.PluginNamesToUpdate.Any();
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

        public void PreparePluginToUpdate(string systemName)
        {
            if (_pluginsInfo.PluginNamesToUpdate.Any(item => item == systemName))
            {
                return;
            }
            _pluginsInfo.PluginNamesToUpdate.Add(systemName);
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
            {
                return;
            }
            _pluginsInfo.PluginNamesToDelete.Add(systemName);
            _pluginsInfo.Save();
        }

        public virtual void DeletePlugins()
        {
            var pluginDescriptors = _pluginsInfo.PluginDescriptors.Where(descriptor => !descriptor.Installed).ToList();
            pluginDescriptors = pluginDescriptors.Where(descriptor => _pluginsInfo.PluginNamesToDelete.Contains(descriptor.SystemName)).ToList();
            if (!pluginDescriptors.Any())
            {
                return;
            }
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
                    throw exception;
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

        public string ChangePlugin(string systemName)
        {
            string changePluginMessage = "";
            if (_pluginsInfo.PluginNamesToInstall.Any(s => s.Equals(systemName)))
            {
                changePluginMessage = $"插件“{systemName}”重启状态已存在（安装），请重启系统以便于插件生效！";
            }
            if (_pluginsInfo.PluginNamesToUninstall.Any(s => s.Equals(systemName)))
            {
                changePluginMessage = $"插件“{systemName}”重启状态已存在（卸载），请重启系统以便于插件生效！";
            }
            if (_pluginsInfo.PluginNamesToDelete.Any(s => s.Equals(systemName)))
            {
                changePluginMessage = $"插件“{systemName}”重启状态已存在（删除），请重启系统以便于插件生效！";
            }
            if (_pluginsInfo.PluginNamesToUpdate.Any(s => s.Equals(systemName)))
            {
                changePluginMessage = $"插件“{systemName}”重启状态已存在（更新），请重启系统以便于插件生效！";
            }
            return changePluginMessage;
        }

        public List<PluginModel> GetPluginModels(PluginSearchModel search, int pageIndex, int pageSize, out int total)
        {
            total = 0;
            var plugins = new List<PluginModel>();
            var pluginDescriptors = GetPluginDescriptors(search);
            if (pluginDescriptors != null)
            {
                total = pluginDescriptors.Count();
                foreach (var pluginDescriptor in pluginDescriptors)
                {
                    var pluginModel = new PluginModel();
                    pluginModel.Group = pluginDescriptor.Group;
                    pluginModel.SystemName = pluginDescriptor.SystemName;
                    pluginModel.Author = pluginDescriptor.Author;
                    pluginModel.Version = pluginDescriptor.Version;
                    pluginModel.AssemblyFileName = pluginDescriptor.AssemblyFileName;
                    pluginModel.Description = pluginDescriptor.Description;
                    pluginModel.State = ParsePluginStateToString(pluginDescriptor.Installed);
                    pluginModel.RestartState = GetPluginRestartState(pluginDescriptor);
                    plugins.Add(pluginModel);
                }
            }
            return plugins;
        }

        private string GetPluginRestartState(PluginDescriptor pluginDescriptor)
        {
            string restartState = ParsePluginStateToString(pluginDescriptor.Installed);
            if (_pluginsInfo.PluginNamesToDelete.Any(s => s.Equals(pluginDescriptor.SystemName)))
            {
                restartState = "删除";
            }
            else if (_pluginsInfo.PluginNamesToInstall.Any(s => s.Equals(pluginDescriptor.SystemName)))
            {
                restartState = "安装";
            }
            else if (_pluginsInfo.PluginNamesToUninstall.Any(s => s.Equals(pluginDescriptor.SystemName)))
            {
                restartState = "卸载";
            }
            else if (_pluginsInfo.PluginNamesToUpdate.Any(s => s.Equals(pluginDescriptor.SystemName)))
            {
                restartState = "更新";
            }
            return restartState;
        }

        public virtual IEnumerable<PluginDescriptor> GetPluginDescriptors(PluginSearchModel search)
        {
            var pluginDescriptors = _pluginsInfo.PluginDescriptors;

            var filters = ListFilters(search);

            pluginDescriptors = pluginDescriptors.Where(filters).ToList();

            return pluginDescriptors;
        }

        private Func<PluginDescriptor, bool> ListFilters(PluginSearchModel search)
        {
            if (search == null)
            {
                return descriptor => true;
            }
            if (search.State == StateType.UnInstall || search.State == StateType.Installed)
            {
                return descriptor => FilterByPluginSystemName(descriptor, search.SystemName) && FilterByPluginAuthor(descriptor, search.Author) && FilterByPluginState(descriptor, search.State);
            }
            return descriptor => FilterByPluginSystemName(descriptor, search.SystemName) && FilterByPluginAuthor(descriptor, search.Author);
        }

        protected virtual bool FilterByPluginSystemName(PluginDescriptor pluginDescriptor, string systemName)
        {
            if (pluginDescriptor == null)
            {
                throw new ArgumentNullException(nameof(pluginDescriptor));
            }

            if (string.IsNullOrEmpty(systemName))
            {
                return true;
            }

            return pluginDescriptor.SystemName.Contains(systemName, StringComparison.InvariantCultureIgnoreCase);
        }

        protected virtual bool FilterByPluginAuthor(PluginDescriptor pluginDescriptor, string author)
        {
            if (pluginDescriptor == null)
            {
                throw new ArgumentNullException(nameof(pluginDescriptor));
            }

            if (string.IsNullOrEmpty(author))
            {
                return true;
            }

            return pluginDescriptor.Author.Contains(author, StringComparison.InvariantCultureIgnoreCase);
        }

        protected virtual bool FilterByPluginState(PluginDescriptor pluginDescriptor, StateType state)
        {
            if (pluginDescriptor == null)
            {
                throw new ArgumentNullException(nameof(pluginDescriptor));
            }
            var installed = true;
            if (state == StateType.UnInstall)
            {
                installed = false;
            }
            else if (state == StateType.Installed)
            {
                installed = true;
            }
            return pluginDescriptor.Installed.Equals(installed);
        }

        public bool ParsePluginStateToBool(string state)
        {
            return state == "已安装" ? true : false;
        }

        public string ParsePluginStateToString(bool state)
        {
            return state == true ? "已安装" : "未安装";
        }

        public void UpdatePlugins()
        {
            _pluginsInfo.PluginNamesToUpdate.Clear();
            _pluginsInfo.Save();
        }
    }
}
