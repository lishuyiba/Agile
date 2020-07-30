using Agile.Core;
using Agile.Core.Infrastructure;
using Agile.Services.Plugins.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Agile.Services.Plugins
{
    public class UploadService : IUploadService
    {
        private readonly IAgileFileProvider _fileProvider;

        public UploadService(IAgileFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
         
        }

        public virtual IList<PluginDescriptor> UploadPlugins(IFormFile archivefile)
        {
            if (archivefile == null)
                throw new ArgumentNullException(nameof(archivefile));

            var zipFilePath = string.Empty;
            var descriptors = new List<PluginDescriptor>();
            try
            {
                if (!_fileProvider.GetFileExtension(archivefile.FileName)?.Equals(".zip", StringComparison.InvariantCultureIgnoreCase) ?? true)
                    throw new Exception("Only zip archives are supported");

                var tempDirectory = _fileProvider.MapPath(AgilePluginDefaults.UploadsTempPath);
                _fileProvider.CreateDirectory(tempDirectory);

                zipFilePath = _fileProvider.Combine(tempDirectory, archivefile.FileName);
                using (var fileStream = new FileStream(zipFilePath, FileMode.Create))
                    archivefile.CopyTo(fileStream);

                var uploadedItems = GetUploadedItems(zipFilePath);
                if (!uploadedItems?.Any() ?? true)
                {
                    descriptors.Add(UploadSingleItem(zipFilePath));
                }
                else
                    descriptors.AddRange(UploadMultipleItems(zipFilePath, uploadedItems));
            }
            finally
            {
                if (!string.IsNullOrEmpty(zipFilePath))
                    _fileProvider.DeleteFile(zipFilePath);
            }
            return descriptors;
        }

        protected virtual IList<PluginDescriptor> UploadMultipleItems(string archivePath, IList<UploadedItem> uploadedItems)
        {
            var pluginsDirectory = _fileProvider.MapPath(AgilePluginDefaults.Path);
            var descriptors = new List<PluginDescriptor>();
            using (var archive = ZipFile.OpenRead(archivePath))
            {
                foreach (var item in uploadedItems)
                {
                    if (!item.SupportedVersions?.Contains(AgileVersion.CurrentVersion) ?? true)
                        continue;

                    var itemPath = $"{item.DirectoryPath?.TrimEnd('/')}/";

                    var descriptorPath = string.Empty;
                    descriptorPath = $"{itemPath}{AgilePluginDefaults.DescriptionFileName}";

                    var descriptorEntry = archive.Entries.FirstOrDefault(entry => entry.FullName.Equals(descriptorPath, StringComparison.InvariantCultureIgnoreCase));
                    if (descriptorEntry == null)
                        continue;

                    PluginDescriptor descriptor = null;
                    using (var unzippedEntryStream = descriptorEntry.Open())
                    {
                        using var reader = new StreamReader(unzippedEntryStream);
                        descriptor = PluginDescriptor.GetPluginDescriptorFromText(reader.ReadToEnd());
                    }

                    if (descriptor == null)
                        continue;

                    if (descriptor is PluginDescriptor pluginDescriptor && !pluginDescriptor.SupportedVersions.Contains(AgileVersion.CurrentVersion))
                        continue;

                    var uploadedItemDirectoryName = _fileProvider.GetFileName(itemPath.TrimEnd('/'));
                    var pathToUpload = _fileProvider.Combine(pluginsDirectory, uploadedItemDirectoryName);

                    if (_fileProvider.DirectoryExists(pathToUpload))
                        _fileProvider.DeleteDirectory(pathToUpload);

                    var entries = archive.Entries.Where(entry => entry.FullName.StartsWith(itemPath, StringComparison.InvariantCultureIgnoreCase));
                    foreach (var entry in entries)
                    {
                        var fileName = entry.FullName.Substring(itemPath.Length);
                        if (string.IsNullOrEmpty(fileName))
                            continue;

                        var filePath = _fileProvider.Combine(pathToUpload, fileName);

                        if (string.IsNullOrEmpty(entry.Name) && !_fileProvider.DirectoryExists(filePath))
                        {
                            _fileProvider.CreateDirectory(filePath);
                            continue;
                        }

                        var directoryPath = _fileProvider.GetDirectoryName(filePath);

                        if (!_fileProvider.DirectoryExists(directoryPath))
                            _fileProvider.CreateDirectory(directoryPath);

                        if (!filePath.Equals($"{directoryPath}\\", StringComparison.InvariantCultureIgnoreCase))
                            entry.ExtractToFile(filePath);
                    }

                    descriptors.Add(descriptor);
                }
            }

            return descriptors;
        }

        protected virtual IList<UploadedItem> GetUploadedItems(string archivePath)
        {
            using var archive = ZipFile.OpenRead(archivePath);
            var uploadedItemsFileEntry = archive.Entries
                .FirstOrDefault(entry => entry.Name.Equals(AgilePluginDefaults.UploadedItemsFileName, StringComparison.InvariantCultureIgnoreCase)
                    && string.IsNullOrEmpty(_fileProvider.GetDirectoryName(entry.FullName)));
            if (uploadedItemsFileEntry == null)
                return null;

            using var unzippedEntryStream = uploadedItemsFileEntry.Open();
            using var reader = new StreamReader(unzippedEntryStream);
            return JsonConvert.DeserializeObject<IList<UploadedItem>>(reader.ReadToEnd());
        }

        protected virtual PluginDescriptor UploadSingleItem(string archivePath)
        {
            var pluginsDirectory = _fileProvider.MapPath(AgilePluginDefaults.Path);

            var themesDirectory = string.Empty;
            if (!string.IsNullOrEmpty(AgilePluginDefaults.ThemesPath))
                themesDirectory = _fileProvider.MapPath(AgilePluginDefaults.ThemesPath);

            PluginDescriptor descriptor = null;
            string uploadedItemDirectoryName;
            using (var archive = ZipFile.OpenRead(archivePath))
            {
                var rootDirectories = archive.Entries.Select(p => p.FullName.Split('/')[0]).Distinct().ToList();

                if (rootDirectories.Count != 1)
                {
                    throw new Exception("The archive should contain only one root plugin or theme directory. " +
                        "For example, Payments.PayPalDirect or DefaultClean. " +
                        $"To upload multiple items, the archive should have the '{AgilePluginDefaults.UploadedItemsFileName}' file in the root");
                }

                uploadedItemDirectoryName = rootDirectories.First();

                foreach (var entry in archive.Entries)
                {
                    var isPluginDescriptor = entry.FullName
                        .Equals($"{uploadedItemDirectoryName}/{AgilePluginDefaults.DescriptionFileName}", StringComparison.InvariantCultureIgnoreCase);

                    var isThemeDescriptor = entry.FullName
                        .Equals($"{uploadedItemDirectoryName}/{AgilePluginDefaults.ThemeDescriptionFileName}", StringComparison.InvariantCultureIgnoreCase);

                    if (!isPluginDescriptor && !isThemeDescriptor)
                        continue;

                    using var unzippedEntryStream = entry.Open();
                    using var reader = new StreamReader(unzippedEntryStream);
                    if (isPluginDescriptor)
                    {
                        descriptor = PluginDescriptor.GetPluginDescriptorFromText(reader.ReadToEnd());

                        if (!((PluginDescriptor)descriptor).SupportedVersions.Contains(AgileVersion.CurrentVersion))
                            throw new Exception($"This plugin doesn't support the current version - {AgileVersion.CurrentVersion}");
                    }

                    if (isThemeDescriptor)

                    break;
                }
            }

            if (descriptor == null)
                throw new Exception("No descriptor file is found. It should be in the root of the archive.");

            if (string.IsNullOrEmpty(uploadedItemDirectoryName))
                throw new Exception($"Cannot get the {(descriptor is PluginDescriptor ? "plugin" : "theme")} directory name");

            var directoryPath = descriptor is PluginDescriptor ? pluginsDirectory : themesDirectory;
            var pathToUpload = _fileProvider.Combine(directoryPath, uploadedItemDirectoryName);

            if (_fileProvider.DirectoryExists(pathToUpload))
                _fileProvider.DeleteDirectory(pathToUpload);

            ZipFile.ExtractToDirectory(archivePath, directoryPath);

            return descriptor;
        }
    }
}
