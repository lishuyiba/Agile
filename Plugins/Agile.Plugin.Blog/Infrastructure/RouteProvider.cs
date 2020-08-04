using Agile.Web.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Plugin.Blog.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapAreaControllerRoute(name: "areaRoute", "blog", pattern: "{area:exists}/{controller=Test}/{action=Index}/{id?}");
        }

        public int Priority => 0;
    }
}
