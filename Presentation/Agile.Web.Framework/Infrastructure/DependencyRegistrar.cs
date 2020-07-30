using Agile.Core;
using Agile.Core.Configuration;
using Agile.Core.Infrastructure;
using Agile.Core.Infrastructure.DependencyManagement;
using Agile.Services.Plugins;
using Agile.Web.Framework.Mvc.Routing;
using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Web.Framework.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 0;

        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, AgileConfig config)
        {

            builder.RegisterType<AgileFileProvider>().As<IAgileFileProvider>().InstancePerLifetimeScope();

            builder.RegisterType<WebHelper>().As<IWebHelper>().InstancePerLifetimeScope();

            builder.RegisterType<RoutePublisher>().As<IRoutePublisher>().SingleInstance();

            builder.RegisterType<UploadService>().As<IUploadService>().InstancePerLifetimeScope();

        }
    }
}
