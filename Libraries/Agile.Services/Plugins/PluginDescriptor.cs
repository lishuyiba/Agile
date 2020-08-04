using Agile.Core.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Agile.Services.Plugins
{
    public partial class PluginDescriptor : PluginDescriptorBaseInfo, IDescriptor, IComparable<PluginDescriptor>
    {
        public PluginDescriptor()
        {
            LimitedToStores = new List<int>();
            LimitedToCustomerRoles = new List<int>();
            DependsOn = new List<string>();
        }

        public PluginDescriptor(Assembly referencedAssembly) : this()
        {
            ReferencedAssembly = referencedAssembly;
        }
        public static PluginDescriptor GetPluginDescriptorFromText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new PluginDescriptor();

            var descriptor = JsonConvert.DeserializeObject<PluginDescriptor>(text);

            return descriptor;
        }

        public virtual TPlugin Instance<TPlugin>() where TPlugin : class, IPlugin
        {
            var instance = EngineContext.Current.ResolveUnregistered(PluginType);
            var typedInstance = instance as TPlugin;
            if (typedInstance != null)
                typedInstance.PluginDescriptor = this;

            return typedInstance;
        }

        public int CompareTo(PluginDescriptor other)
        {
            if (DisplayOrder != other.DisplayOrder)
                return DisplayOrder.CompareTo(other.DisplayOrder);

            return string.Compare(SystemName, other.SystemName, StringComparison.InvariantCultureIgnoreCase);
        }

        public virtual void Save()
        {
            var fileProvider = EngineContext.Current.Resolve<IAgileFileProvider>();

            if (OriginalAssemblyFile == null)
                throw new Exception($"Cannot load original assembly path for {SystemName} plugin.");

            var filePath = fileProvider.Combine(fileProvider.GetDirectoryName(OriginalAssemblyFile), AgilePluginDefaults.DescriptionFileName);
            if (!fileProvider.FileExists(filePath))
                throw new Exception($"Description file for {SystemName} plugin does not exist. {filePath}");

            var text = JsonConvert.SerializeObject(this, Formatting.Indented);
            fileProvider.WriteAllText(filePath, text, Encoding.UTF8);
        }

        [JsonProperty(PropertyName = "Group")]
        public virtual string Group { get; set; }

        [JsonProperty(PropertyName = "Author")]
        public virtual string Author { get; set; }

        [JsonProperty(PropertyName = "DisplayOrder")]
        public virtual int DisplayOrder { get; set; }

        [JsonProperty(PropertyName = "FileName")]
        public virtual string AssemblyFileName { get; set; }

        [JsonProperty(PropertyName = "Description")]
        public virtual string Description { get; set; }

        [JsonProperty(PropertyName = "LimitedToStores")]
        public virtual IList<int> LimitedToStores { get; set; }

        [JsonProperty(PropertyName = "LimitedToCustomerRoles")]
        public virtual IList<int> LimitedToCustomerRoles { get; set; }

        [JsonProperty(PropertyName = "DependsOnSystemNames")]
        public virtual IList<string> DependsOn { get; set; }

        [JsonIgnore]
        public virtual bool Installed { get; set; }

        [JsonIgnore]
        public virtual Type PluginType { get; set; }

        [JsonIgnore]
        public virtual string OriginalAssemblyFile { get; set; }

        [JsonIgnore]
        public virtual Assembly ReferencedAssembly { get; set; }
    }
}
