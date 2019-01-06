using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace M.Executables.Executors.NetCore
{
    /// <summary>
    /// Resolves an executable instance using IServiceProvider for current request if available or in dedicated scope and executes it.
    /// </summary>
    public class NetCoreExecutor : IExecutor, IExecutorAsync
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
            Execute<TExecutableVoid, int>(x => { x.Execute(); return 0; });

        /// <summary>
        /// Resolves an instance of TExecutableVoid using IServiceProvider for current request if available or using dedicated scope and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoid">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        public void Execute<TExecutableVoid, TInput>(TInput input) where TExecutableVoid : class, IExecutableVoid<TInput> =>
            Execute<TExecutableVoid, int>(x => { x.Execute(input); return 0; });

        /// <summary>
        /// Resolves an instance of TExecutable using IServiceProvider for current request if available or using dedicated scope and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutable">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <returns>An instance of TResult.</returns>
        public TResult Execute<TExecutable, TResult>() where TExecutable : class, IExecutable<TResult> =>
            Execute<TExecutable, TResult>(x => x.Execute());

        /// <summary>
        /// Resolves an instance of TExecutable using IServiceProvider for current request if available or using dedicated scope and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutable">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        /// <returns></returns>
        public TResult Execute<TExecutable, TInput, TResult>(TInput input) where TExecutable : class, IExecutable<TInput, TResult> =>
            Execute<TExecutable, TResult>(x => x.Execute(input));

        /// <summary>
        /// Resolves an instance of TExecutableVoidAsync using IServiceProvider for current request if available or using dedicated scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoidAsync">The type of the executable to resolve.</typeparam>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task ExecuteAsync<TExecutableVoidAsync>() where TExecutableVoidAsync : class, IExecutableVoidAsync =>
            await ExecuteAsync<TExecutableVoidAsync, int>(async x => { await x.ExecuteAsync(); return 0; });

        /// <summary>
        /// Resolves an instance of TExecutableVoidAsync using IServiceProvider for current request if available or using dedicated scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoidAsync">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task ExecuteAsync<TExecutableVoidAsync, TInput>(TInput input) where TExecutableVoidAsync : class, IExecutableVoidAsync<TInput> =>
            await ExecuteAsync<TExecutableVoidAsync, int>(async x => { await x.ExecuteAsync(input); return 0; });

        /// <summary>
        /// Resolves an instance of TExecutableAsync using IServiceProvider for current request if available or using dedicated scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableAsync">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task<TResult> ExecuteAsync<TExecutableAsync, TResult>() where TExecutableAsync : class, IExecutableAsync<TResult> =>
            await ExecuteAsync<TExecutableAsync, TResult>(async x => await x.ExecuteAsync());

        /// <summary>
        /// Resolves an instance of TExecutableAsync using IServiceProvider for current request if available or using dedicated scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableAsync">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task<TResult> ExecuteAsync<TExecutableAsync, TInput, TResult>(TInput input) where TExecutableAsync : class, IExecutableAsync<TInput, TResult> =>
            await ExecuteAsync<TExecutableAsync, TResult>(async x => await x.ExecuteAsync(input));

        private TResult Execute<T, TResult>(Func<T, TResult> execute)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var executable = scope.ServiceProvider.GetRequiredService<T>();
                return execute(executable);
            }
        }

        private async Task<TResult> ExecuteAsync<T, TResult>(Func<T, Task<TResult>> execute)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var executable = scope.ServiceProvider.GetRequiredService<T>();
                return await execute(executable);
            }
        }
    }
}
