using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Services.Plugins
{
    public partial interface IUploadService
    {
        IList<PluginDescriptor> UploadPlugins(IFormFile archivefile);
    }
}
