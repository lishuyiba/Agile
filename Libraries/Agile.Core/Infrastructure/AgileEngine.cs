using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Agile.Core.Configuration;
using Agile.Core.Infrastructure.DependencyManagement;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Agile.Core.Infrastructure
{
    public class AgileEngine : IEngine
    {
        private ITypeFinder _typeFinder;
        private IServiceProvider _serviceProvider { get; set; }

        protected IServiceProvider GetServiceProvider()
        {
            if (ServiceProvider == null)
            {
                return null;
            }
            var accessor = ServiceProvider?.GetService<IHttpContextAccessor>();
            var context = accessor?.HttpContext;
            return context?.RequestServices ?? ServiceProvider;
        }

        public virtual void RegisterDependencies(ContainerBuilder containerBuilder, AgileConfig agileConfig)
        {
            containerBuilder.RegisterInstance(this).As<IEngine>().SingleInstance();

            containerBuilder.RegisterInstance(_typeFinder).As<ITypeFinder>().SingleInstance();

            var dependencyRegistrars = _typeFinder.FindClassesOfType<IDependencyRegistrar>();

            var instances = dependencyRegistrars
                .Select(dependencyRegistrar => (IDependencyRegistrar)Activator.CreateInstance(dependencyRegistrar))
                .OrderBy(dependencyRegistrar => dependencyRegistrar.Order);

            foreach (var dependencyRegistrar in instances)
            {
                dependencyRegistrar.Register(containerBuilder, _typeFinder, agileConfig);
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if (assembly != null)
            {
                return assembly;
            }
            var tf = Resolve<ITypeFinder>();
            if (tf == null)
            {
                return null;
            }
            return tf.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration, AgileConfig agileConfig)
        {
            _typeFinder = new WebAppTypeFinder();
            var startupConfigurations = _typeFinder.FindClassesOfType<IAgileStartup>();

            var instances = startupConfigurations
                .Select(startup => (IAgileStartup)Activator.CreateInstance(startup))
                .OrderBy(startup => startup.Order);

            foreach (var instance in instances)
            {
                instance.ConfigureServices(services, configuration);
            }

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        public void ConfigureRequestPipeline(IApplicationBuilder application)
        {
            _serviceProvider = application.ApplicationServices;

            var typeFinder = Resolve<ITypeFinder>();
            var startupConfigurations = typeFinder.FindClassesOfType<IAgileStartup>();

            var instances = startupConfigurations
                .Select(startup => (IAgileStartup)Activator.CreateInstance(startup))
                .OrderBy(startup => startup.Order);

            foreach (var instance in instances)
            {
                instance.Configure(application);
            }
        }

        public T Resolve<T>() where T : class
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            var sp = GetServiceProvider();
            if (sp == null)
            {
                return null;
            }
            return sp.GetService(type);
        }

        public virtual IEnumerable<T> ResolveAll<T>()
        {
            return (IEnumerable<T>)GetServiceProvider().GetServices(typeof(T));
        }

        public virtual object ResolveUnregistered(Type type)
        {
            Exception innerException = null;
            foreach (var constructor in type.GetConstructors())
            {
                try
                {
                    var parameters = constructor.GetParameters().Select(parameter =>
                    {
                        var service = Resolve(parameter.ParameterType);
                        if (service == null)
                        {
                            throw new AgileException("未知依赖注入");
                        }
                        return service;
                    });
                    return Activator.CreateInstance(type, parameters.ToArray());
                }
                catch (Exception ex)
                {
                    innerException = ex;
                }
            }
            throw new AgileException("没有找到满足所有依赖项的构造函数！", innerException);
        }
        public virtual IServiceProvider ServiceProvider => _serviceProvider;
    }
}
