using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agile.Web.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Agile.Web.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(name: "areaRoute", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            endpointRouteBuilder.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

            endpointRouteBuilder.MapControllerRoute("defaultRoute", $"/Default/", new { controller = "Default", action = "Index" });
        }

        public int Priority => 0;
    }
}
