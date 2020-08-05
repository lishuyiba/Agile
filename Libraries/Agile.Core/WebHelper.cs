using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Core
{
    public partial class WebHelper : IWebHelper
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public WebHelper(IHostApplicationLifetime hostApplicationLifetime)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        public virtual void RestartAppDomain()
        {
            _hostApplicationLifetime.StopApplication();
        }
    }
}
