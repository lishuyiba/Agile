using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Web.Framework.Mvc.Routing
{
    public interface IRouteProvider
    {
        void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder);

        int Priority { get; }
    }
}
