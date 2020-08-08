using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Models.Plugins
{
    public class PluginModel
    {
        public string Group { get; set; }
        public string SystemName { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string AssemblyFileName { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public string RestartState { get; set; }
    }
}
