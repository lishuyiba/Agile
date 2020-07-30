using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Services.Plugins
{
    public interface IPlugin
    {
        string GetConfigurationPageUrl();

        PluginDescriptor PluginDescriptor { get; set; }
        void Install();

        void Uninstall();

        void Update(string currentVersion, string targetVersion);

        void PreparePluginToUninstall();
    }
}
