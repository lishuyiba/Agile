using Agile.Core.Infrastructure;
using Agile.Services.Plugins;
using Agile.Web.Framework.Mvc.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Agile.Web.Framework.Infrastructure.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static void ConfigureRequestPipeline(this IApplicationBuilder application)
        {
            EngineContext.Current.ConfigureRequestPipeline(application);
        }

        public static void StartEngine(this IApplicationBuilder application)
        {
            var engine = EngineContext.Current;

            var pluginService = engine.Resolve<IPluginService>();

            pluginService.InstallPlugins();
            pluginService.UninstallPlugins();
            pluginService.DeletePlugins();
        }

        public static void UseAgileEndpoints(this IApplicationBuilder application)
        {
            application.UseRouting();

            application.UseEndpoints(endpoints =>
            {
                EngineContext.Current.Resolve<IRoutePublisher>().RegisterRoutes(endpoints);
            });
        }

        public static void UseAgileStaticFiles(this IApplicationBuilder application)
        {
            application.UseStaticFiles();
        }

        public static void UseNopExceptionHandler(this IApplicationBuilder application)
        {
            application.UseExceptionHandler(handler =>
            {
                handler.Run(context =>
                {
                    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    if (exception == null)
                    {
                        return Task.CompletedTask;
                    }

                    ExceptionDispatchInfo.Throw(exception);

                    return Task.CompletedTask;
                });
            });
        }

        public static void UseBadRequestResult(this IApplicationBuilder application)
        {
            application.UseStatusCodePages(context =>
            {
                if (context.HttpContext.Response.StatusCode == StatusCodes.Status400BadRequest)
                {

                }
                return Task.CompletedTask;
            });
        }

        public static void UsePageNotFound(this IApplicationBuilder application)
        {
            application.UseStatusCodePages(async context =>
            {
                if (context.HttpContext.Response.StatusCode == StatusCodes.Status404NotFound)
                {
                    var originalPath = context.HttpContext.Request.Path;
                    var originalQueryString = context.HttpContext.Request.QueryString;
                    try
                    {
                        var pageNotFoundPath = "/Admin/Common/PageNotFound";
                        context.HttpContext.Response.Redirect(context.HttpContext.Request.PathBase + pageNotFoundPath);
                    }
                    finally
                    {
                        context.HttpContext.Request.QueryString = originalQueryString;
                        context.HttpContext.Request.Path = originalPath;
                    }
                    await Task.CompletedTask;
                }
            });
        }

    }
}
