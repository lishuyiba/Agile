using Agile.Core.Configuration;
using Agile.Core.Infrastructure;
using Agile.Core.Infrastructure.DependencyManagement;
using Agile.Services.Plugins;
using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Services.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 0;

        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, AgileConfig config)
        {
            builder.RegisterType<PluginService>().As<IPluginService>().SingleInstance();
        }
    }
}
