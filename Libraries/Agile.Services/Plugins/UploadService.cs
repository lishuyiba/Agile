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
using System.Reflection.Metadata;
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

        public virtual (PluginDescriptor descriptor, bool IsUpdate) UploadPlugins(IFormFile archivefile)
        {
            if (archivefile == null)
            {
                throw new ArgumentNullException(nameof(archivefile));
            }
            var zipFilePath = string.Empty;
            PluginDescriptor descriptor = null;
            bool isUpdate = false;
            try
            {
                if (!_fileProvider.GetFileExtension(archivefile.FileName)?.Equals(".zip", StringComparison.InvariantCultureIgnoreCase) ?? true)
                {
                    throw new Exception("只支持zip压缩文件！");
                }
                var tempDirectory = _fileProvider.MapPath(AgilePluginDefaults.UploadsTempPath);
                _fileProvider.CreateDirectory(tempDirectory);
                zipFilePath = _fileProvider.Combine(tempDirectory, archivefile.FileName);
                using (var fileStream = new FileStream(zipFilePath, FileMode.Create))
                {
                    archivefile.CopyTo(fileStream);
                }
                var uploadResult = UploadPlugin(zipFilePath);
                descriptor = uploadResult.descriptor;
                isUpdate = uploadResult.IsUpdate;
            }
            finally
            {
                if (!string.IsNullOrEmpty(zipFilePath))
                {
                    _fileProvider.DeleteFile(zipFilePath);
                }
            }
            return (descriptor, isUpdate);
        }

        protected virtual (PluginDescriptor descriptor, bool IsUpdate) UploadPlugin(string archivePath)
        {
            var pluginsDirectory = _fileProvider.MapPath(AgilePluginDefaults.Path);
            PluginDescriptor descriptor = null;
            bool isUpdate = false;
            using (var archive = ZipFile.OpenRead(archivePath))
            {
                var pluginDescriptor = archive.Entries.FirstOrDefault(s => s.FullName.Equals(AgilePluginDefaults.DescriptionFileName));
                if (pluginDescriptor == null)
                {
                    throw new Exception($"插件{archivePath}内未找到描述！");
                }
                using var unzippedEntryStream = pluginDescriptor.Open();
                using var reader = new StreamReader(unzippedEntryStream);
                descriptor = PluginDescriptor.GetPluginDescriptorFromText(reader.ReadToEnd());
            }
            if (descriptor == null)
            {
                throw new Exception("没有找到插件描述文件！");
            }
            var uploadedItemDirectoryName = descriptor.SystemName;
            var directoryPath = _fileProvider.Combine(pluginsDirectory, uploadedItemDirectoryName);
            var pathToUpload = _fileProvider.Combine(pluginsDirectory, descriptor.SystemName);
            if (_fileProvider.DirectoryExists(pathToUpload))
            {
                isUpdate = true;
                _fileProvider.DeleteDirectory(pathToUpload);
            }
            ZipFile.ExtractToDirectory(archivePath, pathToUpload);
            return (descriptor, isUpdate);
        }
    }
}
