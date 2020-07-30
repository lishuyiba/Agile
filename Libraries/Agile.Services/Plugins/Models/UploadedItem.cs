using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Services.Plugins.Models
{
    public class UploadedItem
    {
        [JsonProperty(PropertyName = "SystemName")]
        public string SystemName { get; set; }

        [JsonProperty(PropertyName = "SupportedVersion")]
        public string SupportedVersions { get; set; }

        [JsonProperty(PropertyName = "DirectoryPath")]
        public string DirectoryPath { get; set; }

        [JsonProperty(PropertyName = "SourceDirectoryPath")]
        public string SourceDirectoryPath { get; set; }
    }

}
