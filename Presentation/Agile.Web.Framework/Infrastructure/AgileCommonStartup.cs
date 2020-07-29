using Agile.Core.Infrastructure;
using Agile.Web.Framework.Infrastructure.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Web.Framework.Infrastructure
{
    public class AgileCommonStartup : IAgileStartup
    {
        public int Order => 100;

        public void Configure(IApplicationBuilder application)
        {
            application.UseAgileStaticFiles();
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {

        }
    }
}
