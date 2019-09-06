using Microsoft.Extensions.DependencyInjection;

namespace M.Executables.Executors.NetCore.UnitTests
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExecutor(this IServiceCollection services)
            => services.AddSingleton<IExecutorAsync, NetCoreExecutorAsync>()
                       .AddSingleton<IExecutor, NetCoreExecutor>();

        public static IServiceCollection AddExecutable<TExecutable>(this IServiceCollection services, TExecutable executable) => services.AddScoped(typeof(TExecutable), _ => executable);

        public static IServiceCollection AddGeneralInterceptors<TInterceptor>(this IServiceCollection services, params TInterceptor[] interceptors)
        {
            foreach(var interceptor in interceptors)
            {
                services.AddScoped(typeof(TInterceptor), _ => interceptor);
            }

            return services;
        }

        public static IServiceCollection AddSpecificInterceptors<TInterceptor>(this IServiceCollection services, params TInterceptor[] interceptors)
        {
            foreach (var interceptor in interceptors)
            {
                services.AddScoped(typeof(TInterceptor), _ => interceptor);
            }

            return services;
        }
    }
}
