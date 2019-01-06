using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace M.Executables.Executors.NetCore
{
    /// <summary>
    /// Resolves an executable instance using IServiceProvider for current request if available or in dedicated scope and executes it.
    /// </summary>
    public class NetCoreExecutor : IExecutors
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Creates a new instance of NetCoreExecutor.
        /// </summary>
        /// <param name="serviceScopeFactory">IServiceScopeFactory used to create scope in case the executor is invoked out of request scope.</param>
        public NetCoreExecutor(IServiceScopeFactory serviceScopeFactory) => _serviceScopeFactory = serviceScopeFactory;

        /// <summary>
        /// Resolves an instance of TExecutableVoid using IServiceProvider for current request if available or using dedicated scope and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoid">The type of the executable to resolve.</typeparam>
        public void Execute<TExecutableVoid>() where TExecutableVoid : class, IExecutableVoid =>
            Execute<TExecutableVoid, IEmpty, IEmpty>((x, _) => { x.Execute(); return null; }, null);

        /// <summary>
        /// Resolves an instance of TExecutableVoid using IServiceProvider for current request if available or using dedicated scope and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoid">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        public void Execute<TExecutableVoid, TInput>(TInput input) where TExecutableVoid : class, IExecutableVoid<TInput> =>
            Execute<TExecutableVoid, TInput, IEmpty>((x, i) => { x.Execute(i); return null; }, input);

        /// <summary>
        /// Resolves an instance of TExecutable using IServiceProvider for current request if available or using dedicated scope and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutable">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <returns>An instance of TResult.</returns>
        public TResult Execute<TExecutable, TResult>() where TExecutable : class, IExecutable<TResult> =>
            Execute<TExecutable, IEmpty, TResult>((x, _) => x.Execute(), null);

        /// <summary>
        /// Resolves an instance of TExecutable using IServiceProvider for current request if available or using dedicated scope and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutable">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        /// <returns></returns>
        public TResult Execute<TExecutable, TInput, TResult>(TInput input) where TExecutable : class, IExecutable<TInput, TResult> =>
            Execute<TExecutable, TInput, TResult>((x, i) => x.Execute(i), input);

        /// <summary>
        /// Resolves an instance of TExecutableVoidAsync using IServiceProvider for current request if available or using dedicated scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoidAsync">The type of the executable to resolve.</typeparam>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task ExecuteAsync<TExecutableVoidAsync>() where TExecutableVoidAsync : class, IExecutableVoidAsync =>
            await ExecuteAsync<TExecutableVoidAsync, IEmpty, IEmpty>(async (x, _) => { await x.ExecuteAsync(); return null; }, null);

        /// <summary>
        /// Resolves an instance of TExecutableVoidAsync using IServiceProvider for current request if available or using dedicated scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoidAsync">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task ExecuteAsync<TExecutableVoidAsync, TInput>(TInput input) where TExecutableVoidAsync : class, IExecutableVoidAsync<TInput> =>
            await ExecuteAsync<TExecutableVoidAsync, TInput, IEmpty>(async (x, i) => { await x.ExecuteAsync(i); return null; }, input);

        /// <summary>
        /// Resolves an instance of TExecutableAsync using IServiceProvider for current request if available or using dedicated scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableAsync">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task<TResult> ExecuteAsync<TExecutableAsync, TResult>() where TExecutableAsync : class, IExecutableAsync<TResult> =>
            await ExecuteAsync<TExecutableAsync, IEmpty, TResult>(async (x, _) => await x.ExecuteAsync(), null);

        /// <summary>
        /// Resolves an instance of TExecutableAsync using IServiceProvider for current request if available or using dedicated scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableAsync">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task<TResult> ExecuteAsync<TExecutableAsync, TInput, TResult>(TInput input) where TExecutableAsync : class, IExecutableAsync<TInput, TResult> =>
            await ExecuteAsync<TExecutableAsync, TInput, TResult>(async (x, i) => await x.ExecuteAsync(i), input);

        private TResult Execute<T, TInput, TResult>(Func<T, TInput, TResult> execute, TInput input)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var executable = scope.ServiceProvider.GetRequiredService<T>();
                IExecutionInterceptor[] interceptors = GetInterceptors(scope);

                InterceptBefore(executable, input, interceptors);

                TResult result = default(TResult);
                try
                {
                    result = execute(executable, input);
                }
                catch (Exception exception)
                {
                    InterceptAfter(executable, input, result, interceptors, exception);
                    throw;
                }

                InterceptAfter(executable, input, result, interceptors);
                return result;
            }
        }

        private async Task<TResult> ExecuteAsync<T, TInput, TResult>(Func<T, TInput, Task<TResult>> execute, TInput input)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var executable = scope.ServiceProvider.GetRequiredService<T>();
                IExecutionInterceptor[] interceptors = GetInterceptors(scope);

                InterceptBefore(executable, input, interceptors);

                TResult result = default(TResult);
                try
                {
                    result = await execute(executable, input);
                }
                catch (Exception exception)
                {
                    InterceptAfter(executable, input, result, interceptors, exception);
                    throw;
                }

                InterceptAfter(executable, input, result, interceptors);
                return result;
            }
        }

        private static IExecutionInterceptor[] GetInterceptors(IServiceScope scope)
        {
            return scope.ServiceProvider
                        .GetServices<IExecutionInterceptor>()
                        .OrderBy(x => x.OrderingIndex)
                        .ToArray();
        }

        private static void InterceptBefore<T, TInput>(T executable, TInput input, IExecutionInterceptor[] interceptors)
        {
            for(int i = 0; i < interceptors.Length; i++)
            {
                interceptors[i].Before(executable, input);
            }
        }

        private static void InterceptAfter<T, TInput, TResult>(T executable, TInput input, TResult result, IExecutionInterceptor[] interceptors, Exception exception = null)
        {
            for (int i = interceptors.Length - 1; i >= 0; i--)
            {
                interceptors[i].After(executable, input, result, exception);
            }
        }
    }
}
