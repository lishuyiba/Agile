using Agile.Core;
using Agile.Core.ComponentModel;
using Agile.Core.Configuration;
using Agile.Core.Infrastructure;
using Agile.Services.Plugins;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Agile.Web.Framework.Infrastructure.Extensions
{
    public static class ApplicationPartManagerExtensions
    {
        private static readonly IAgileFileProvider _fileProvider;
        private static readonly List<string> _baseAppLibraries;
        private static readonly Dictionary<string, PluginLoadedAssemblyInfo> _loadedAssemblies = new Dictionary<string, PluginLoadedAssemblyInfo>();
        private static readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        static ApplicationPartManagerExtensions()
        {
            _fileProvider = CommonHelper.DefaultFileProvider;

            _baseAppLibraries = new List<string>();

            _baseAppLibraries.AddRange(_fileProvider.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll")
                .Select(fileName => _fileProvider.GetFileName(fileName)));

            if (!AppDomain.CurrentDomain.BaseDirectory.Equals(Environment.CurrentDirectory, StringComparison.InvariantCultureIgnoreCase))
            {
                _baseAppLibraries.AddRange(_fileProvider.GetFiles(Environment.CurrentDirectory, "*.dll")
                    .Select(fileName => _fileProvider.GetFileName(fileName)));
            }

            var refsPathName = _fileProvider.Combine(Environment.CurrentDirectory, AgilePluginDefaults.RefsPathName);
            if (_fileProvider.DirectoryExists(refsPathName))
            {
                _baseAppLibraries.AddRange(_fileProvider.GetFiles(refsPathName, "*.dll")
                    .Select(fileName => _fileProvider.GetFileName(fileName)));
            }
        }

        public static void InitializePlugins(this ApplicationPartManager applicationPartManager, AgileConfig config)
        {
            if (applicationPartManager == null)
                throw new ArgumentNullException(nameof(applicationPartManager));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            PluginsInfo = new PluginsInfo(_fileProvider);

            PluginsInfo.LoadPluginInfo();

            using (new ReaderWriteLockDisposable(_locker))
            {
                var pluginDescriptors = new List<PluginDescriptor>();
                try
                {
                    var pluginsDirectory = _fileProvider.MapPath(AgilePluginDefaults.Path);
                    _fileProvider.CreateDirectory(pluginsDirectory);

                    var shadowCopyDirectory = _fileProvider.MapPath(AgilePluginDefaults.ShadowCopyPath);
                    _fileProvider.CreateDirectory(shadowCopyDirectory);

                    var binFiles = _fileProvider.GetFiles(shadowCopyDirectory, "*", false);

                    foreach (var item in GetDescriptionFilesAndDescriptors(pluginsDirectory))
                    {
                        var descriptionFile = item.DescriptionFile;
                        var pluginDescriptor = item.PluginDescriptor;

                        //判断插件是否已被安装
                        var installedPugins = PluginsInfo.InstalledPlugins.Select(pd => pd.SystemName);
                        pluginDescriptor.Installed = installedPugins.Any(pluginName => pluginName.Equals(pluginDescriptor.SystemName, StringComparison.InvariantCultureIgnoreCase));

                        try
                        {
                            //获取插件目录
                            var pluginDirectory = _fileProvider.GetDirectoryName(descriptionFile);
                            if (string.IsNullOrEmpty(pluginDirectory))
                            {
                                throw new Exception($"插件目录'{_fileProvider.GetFileName(descriptionFile)}'不存在！");
                            }

                            //获取插件目录下所有程序集
                            var pluginFiles = _fileProvider.GetFiles(pluginDirectory, "*.dll", false)
                                .Where(file => !binFiles.Contains(file) && IsPluginDirectory(_fileProvider.GetDirectoryName(file)))
                                .ToList();

                            //读取插件程序集
                            var mainPluginFile = pluginFiles.FirstOrDefault(file =>
                            {
                                var fileName = _fileProvider.GetFileName(file);
                                return fileName.Equals(pluginDescriptor.AssemblyFileName, StringComparison.InvariantCultureIgnoreCase);
                            });

                            var pluginName = pluginDescriptor.SystemName;

                            pluginDescriptor.OriginalAssemblyFile = mainPluginFile;

                            var needToDeploy = PluginsInfo.InstalledPlugins.Select(pd => pd.SystemName).Contains(pluginName);

                            needToDeploy = needToDeploy || PluginsInfo.PluginNamesToInstall.Any(pluginInfo => pluginInfo.Equals(pluginName));

                            needToDeploy = needToDeploy && !PluginsInfo.PluginNamesToDelete.Contains(pluginName);

                            needToDeploy = needToDeploy && !PluginsInfo.PluginNamesToUninstall.Contains(pluginName);

                            if (needToDeploy)
                            {
                                pluginDescriptor.ReferencedAssembly = applicationPartManager.PerformFileDeploy(mainPluginFile, shadowCopyDirectory, config, _fileProvider);

                                var pluginType = pluginDescriptor.ReferencedAssembly.GetTypes().FirstOrDefault();

                                if (pluginType != default)
                                {
                                    pluginDescriptor.PluginType = pluginType;
                                }

                                pluginDescriptor.IsRestartActivate = false;
                            }
                            pluginDescriptors.Add(pluginDescriptor);
                        }
                        catch (ReflectionTypeLoadException exception)
                        {
                            var error = exception.LoaderExceptions.Aggregate($"Plugin '{pluginDescriptor.FriendlyName}'. ",
                                (message, nextMessage) => $"{message}{nextMessage.Message}{Environment.NewLine}");

                            throw new Exception(error, exception);
                        }
                        catch (Exception exception)
                        {
                            throw new Exception($"Plugin '{pluginDescriptor.FriendlyName}'. {exception.Message}", exception);
                        }
                    }
                }
                catch (Exception exception)
                {
                    var message = string.Empty;
                    for (var inner = exception; inner != null; inner = inner.InnerException)
                        message = $"{message}{inner.Message}{Environment.NewLine}";

                    throw new Exception(message, exception);
                }

                PluginsInfo.PluginDescriptors = pluginDescriptors;
            }
        }

        private static bool IsAlreadyLoaded(string filePath, string pluginName)
        {
            var fileName = _fileProvider.GetFileName(filePath);
            if (_baseAppLibraries.Any(library => library.Equals(fileName, StringComparison.InvariantCultureIgnoreCase)))
                return true;

            try
            {
                var fileNameWithoutExtension = _fileProvider.GetFileNameWithoutExtension(filePath);
                if (string.IsNullOrEmpty(fileNameWithoutExtension))
                    throw new Exception($"Cannot get file extension for {fileName}");

                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    var assemblyName = assembly.FullName.Split(',').FirstOrDefault();
                    if (!fileNameWithoutExtension.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    if (!_loadedAssemblies.ContainsKey(assemblyName))
                    {
                        _loadedAssemblies.Add(assemblyName, new PluginLoadedAssemblyInfo(assemblyName, assembly.FullName));
                    }

                    _loadedAssemblies[assemblyName].References.Add((pluginName, AssemblyName.GetAssemblyName(filePath).FullName));

                    return true;
                }
            }
            catch
            {
            }
            return false;
        }

        private static Assembly PerformFileDeploy(this ApplicationPartManager applicationPartManager,
            string assemblyFile, string shadowCopyDirectory, AgileConfig config, IAgileFileProvider fileProvider)
        {
            if (string.IsNullOrEmpty(assemblyFile) || string.IsNullOrEmpty(fileProvider.GetParentDirectory(assemblyFile)))
            {
                throw new InvalidOperationException($"The plugin directory for the {fileProvider.GetFileName(assemblyFile)} file exists in a directory outside of the allowed nopCommerce directory hierarchy");
            }

            fileProvider.CreateDirectory(shadowCopyDirectory);
            var shadowCopiedFile = ShadowCopyFile(fileProvider, assemblyFile, shadowCopyDirectory);

            Assembly shadowCopiedAssembly = null;
            try
            {
                shadowCopiedAssembly = AddApplicationParts(applicationPartManager, shadowCopiedFile);
            }
            catch (UnauthorizedAccessException)
            {

            }
            catch (FileLoadException)
            {

            }

            if (shadowCopiedAssembly != null)
                return shadowCopiedAssembly;

            var reserveDirectory = fileProvider.Combine(fileProvider.MapPath(AgilePluginDefaults.ShadowCopyPath),
                $"{AgilePluginDefaults.ReserveShadowCopyPathName}{DateTime.Now.ToFileTimeUtc()}");

            return PerformFileDeploy(applicationPartManager, assemblyFile, reserveDirectory, config, fileProvider);
        }

        private static Assembly AddApplicationParts(ApplicationPartManager applicationPartManager, string assemblyFile)
        {
            Assembly assembly = null;
            try
            {
                assembly = Assembly.LoadFrom(assemblyFile);
            }
            catch (FileLoadException)
            {
            }

            applicationPartManager.ApplicationParts.Add(new AssemblyPart(assembly));

            return assembly;
        }

        private static string ShadowCopyFile(IAgileFileProvider fileProvider, string assemblyFile, string shadowCopyDirectory)
        {
            var shadowCopiedFile = fileProvider.Combine(shadowCopyDirectory, fileProvider.GetFileName(assemblyFile));
            if (fileProvider.FileExists(shadowCopiedFile))
            {
                var areFilesIdentical = fileProvider.GetCreationTime(shadowCopiedFile).ToUniversalTime().Ticks >=
                    fileProvider.GetCreationTime(assemblyFile).ToUniversalTime().Ticks;
                if (areFilesIdentical)
                {
                    return shadowCopiedFile;
                }

                fileProvider.DeleteFile(shadowCopiedFile);
            }

            try
            {
                fileProvider.FileCopy(assemblyFile, shadowCopiedFile, true);
            }
            catch (IOException)
            {
                try
                {
                    var oldFile = $"{shadowCopiedFile}{Guid.NewGuid():N}.old";
                    fileProvider.FileMove(shadowCopiedFile, oldFile);
                }
                catch (IOException exception)
                {
                    throw new IOException($"{shadowCopiedFile} rename failed, cannot initialize plugin", exception);
                }

                fileProvider.FileCopy(assemblyFile, shadowCopiedFile, true);
            }

            return shadowCopiedFile;
        }

        private static IList<(string DescriptionFile, PluginDescriptor PluginDescriptor)> GetDescriptionFilesAndDescriptors(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
                throw new ArgumentNullException(nameof(directoryName));

            var result = new List<(string DescriptionFile, PluginDescriptor PluginDescriptor)>();

            var files = _fileProvider.GetFiles(directoryName, AgilePluginDefaults.DescriptionFileName, false);

            foreach (var descriptionFile in files)
            {
                if (!IsPluginDirectory(_fileProvider.GetDirectoryName(descriptionFile)))
                    continue;

                var text = _fileProvider.ReadAllText(descriptionFile, Encoding.UTF8);
                var pluginDescriptor = PluginDescriptor.GetPluginDescriptorFromText(text);

                result.Add((descriptionFile, pluginDescriptor));
            }

            result = result.OrderBy(item => item.PluginDescriptor.DisplayOrder).ToList();

            return result;
        }

        private static bool IsPluginDirectory(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
                return false;

            var parent = _fileProvider.GetParentDirectory(directoryName);
            if (string.IsNullOrEmpty(parent))
                return false;

            if (!_fileProvider.GetDirectoryNameOnly(parent).Equals(AgilePluginDefaults.PathName, StringComparison.InvariantCultureIgnoreCase))
                return false;

            return true;
        }

        private static IPluginsInfo PluginsInfo
        {
            get => Singleton<IPluginsInfo>.Instance;
            set => Singleton<IPluginsInfo>.Instance = value;
        }
    }
}
