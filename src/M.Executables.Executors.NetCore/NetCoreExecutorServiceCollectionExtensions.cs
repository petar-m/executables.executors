using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace M.Executables.Executors.NetCore
{
    /// <summary>
    /// Extension methods for setting up NetCoreExecutor in an IServiceCollection.
    /// </summary>
    public static class NetCoreExecutorServiceCollectionExtensions
    {
        private const string ExecutableInterfaceName = "M.Executables.IExecutable";

        /// <summary>
        /// Adds IExecutor and IExecutorAsync as scoped to the specified IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <returns>The specified IServiceCollection that can be used to further configuration.</returns>
        public static IServiceCollection AddScopedNetCoreExecutor(this IServiceCollection services)
        {
            services.AddScoped<IExecutor, NetCoreExecutor>()
                    .AddScoped<IExecutorAsync, NetCoreExecutor>();
            return services;
        }

        /// <summary>
        /// Adds all IExecutable and IExecutableAsync direct implemetations exported from the specified assemblies as transient to the specified IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <param name="assemblies">Assemblies to scan for IExecutable and IExecutableAsync direct implemetations.</param>
        /// <returns></returns>
        public static IServiceCollection AddTransientExecutables(this IServiceCollection services, params Assembly[] assemblies)
        {
            foreach (var executable in assemblies.SelectMany(IExecutableImplementations))
            {
                services.AddTransient(executable);
            }

            return services;
        }

        private static IEnumerable<Type> IExecutableImplementations(Assembly assembly) =>
            assembly.GetExportedTypes()
                    .Where(x => !x.IsAbstract && !x.IsGenericType && !x.IsInterface
                                && x.GetInterfaces().Any(i => i.FullName.StartsWith(ExecutableInterfaceName)));
    }
}
