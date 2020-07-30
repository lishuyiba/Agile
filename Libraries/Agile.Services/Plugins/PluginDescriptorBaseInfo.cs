using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Services.Plugins
{
    public partial class PluginDescriptorBaseInfo : IComparable<PluginDescriptorBaseInfo>
    {
        [JsonProperty(PropertyName = "SystemName")]
        public virtual string SystemName { get; set; }

        [JsonProperty(PropertyName = "Version")]
        public virtual string Version { get; set; }

        public int CompareTo(PluginDescriptorBaseInfo other)
        {
            return string.Compare(SystemName, other.SystemName, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object value)
        {
            return SystemName?.Equals((value as PluginDescriptorBaseInfo)?.SystemName) ?? false;
        }

        public override int GetHashCode()
        {
            return SystemName.GetHashCode();
        }

        [JsonIgnore]
        public virtual PluginDescriptorBaseInfo GetBaseInfoCopy =>
            new PluginDescriptorBaseInfo { SystemName = SystemName, Version = Version };
    }
}
