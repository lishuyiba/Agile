using Agile.Core.Configuration;
using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agile.Core.Infrastructure.DependencyManagement
{
    public interface IDependencyRegistrar
    {
        void Register(ContainerBuilder builder, ITypeFinder typeFinder, AgileConfig config);

        int Order { get; }
    }
}
