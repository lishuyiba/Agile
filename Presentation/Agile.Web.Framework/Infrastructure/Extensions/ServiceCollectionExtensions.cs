using System;
using System.Linq;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Agile.Core;
using Agile.Core.Configuration;
using Agile.Core.Infrastructure;
using Agile.Web.Framework.ViewLocationExpanders;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;

namespace Agile.Web.Framework.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static (IEngine, AgileConfig) ConfigureApplicationServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            var nopConfig = services.ConfigureStartupConfig<AgileConfig>(configuration.GetSection("Agile"));

            CommonHelper.DefaultFileProvider = new AgileFileProvider(webHostEnvironment);

            var mvcCoreBuilder = services.AddMvcCore();
            mvcCoreBuilder.PartManager.InitializePlugins(nopConfig);

            var engine = EngineContext.Create();
            engine.ConfigureServices(services, configuration, nopConfig);

            return (engine, nopConfig);
        }

        public static TConfig ConfigureStartupConfig<TConfig>(this IServiceCollection services, IConfiguration configuration) where TConfig : class, new()
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var config = new TConfig();

            configuration.Bind(config);

            services.AddSingleton(config);

            return config;
        }

        public static IMvcBuilder AddAgileMvc(this IServiceCollection services)
        {
            var mvcBuilder = services.AddControllersWithViews();

            //使用JsonResult返回时中文被编码
            mvcBuilder.AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            });

            mvcBuilder.AddRazorRuntimeCompilation();

            services.AddRazorPages();

            return mvcBuilder;
        }

        public static void AddThemes(this IServiceCollection services)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new DefaultViewLocationExpander());
            });
        }

    }
}
