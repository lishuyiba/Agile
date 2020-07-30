using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Services.Plugins
{
    public interface IDescriptor
    {
        string SystemName { get; set; }

        string FriendlyName { get; set; }
    }
}
