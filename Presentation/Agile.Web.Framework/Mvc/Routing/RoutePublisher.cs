using Agile.Core.Infrastructure;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Agile.Web.Framework.Mvc.Routing
{
    public class RoutePublisher : IRoutePublisher
    {
        protected readonly ITypeFinder _typeFinder;
        public RoutePublisher(ITypeFinder typeFinder)
        {
            _typeFinder = typeFinder;
        }
        public virtual void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            var routeProviders = _typeFinder.FindClassesOfType<IRouteProvider>();
            var instances = routeProviders
                .Select(routeProvider => (IRouteProvider)Activator.CreateInstance(routeProvider))
                .OrderByDescending(routeProvider => routeProvider.Priority);

            foreach (var routeProvider in instances)
            {
                routeProvider.RegisterRoutes(endpointRouteBuilder);
            }
        }
    }
}
