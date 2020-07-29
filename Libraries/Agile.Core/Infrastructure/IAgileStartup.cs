using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Core.Infrastructure
{
    public interface IAgileStartup
    {
        void ConfigureServices(IServiceCollection services, IConfiguration configuration);

        void Configure(IApplicationBuilder application);

        int Order { get; }
    }
}
