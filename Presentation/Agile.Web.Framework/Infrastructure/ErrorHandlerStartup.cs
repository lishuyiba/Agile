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
    public class ErrorHandlerStartup : IAgileStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
        }

        public void Configure(IApplicationBuilder application)
        {
            application.UseNopExceptionHandler();

            application.UseBadRequestResult();

            application.UsePageNotFound();
        }

        public int Order => 0;
    }
}
