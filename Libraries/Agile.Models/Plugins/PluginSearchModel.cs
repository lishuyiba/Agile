using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Models.Plugins
{
    public class PluginSearchModel
    {
        public string SystemName { get; set; }
        public string Author { get; set; }
        public StateType State { get; set; }
    }

    public enum StateType
    {
        All = 0,
        UnInstall = 1,
        Installed = 2
    }
}
