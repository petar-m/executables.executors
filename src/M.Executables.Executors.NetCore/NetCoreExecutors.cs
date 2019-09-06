using System.Threading.Tasks;

namespace M.Executables.Executors.NetCore
{
    /// <summary>
    /// A utility class for both synchronous and asynchronous execution.
    /// </summary>
    public class NetCoreExecutors : IExecutors
    {
        private readonly IExecutor _executor;
        private readonly IExecutorAsync _executorAsync;

        /// <summary>
        /// Creates a new instance of NetCoreExecutors class.
        /// </summary>
        /// <param name="executor">An IExecutor implementation to handle synchronous execution.</param>
        /// <param name="executorAsync">An IExecutorAsync implementation to handle asynchronous execution.</param>
        public NetCoreExecutors(IExecutor executor, IExecutorAsync executorAsync)
        {
            _executor = executor;
            _executorAsync = executorAsync;
        }

        /// <summary>
        /// Resolves an instance of TExecutableVoid and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoid">The type of the executable to resolve.</typeparam>
        public void Execute<TExecutableVoid>() where TExecutableVoid : class, IExecutableVoid
            => _executor.Execute<TExecutableVoid>();

        /// <summary>
        /// Resolves an instance of TExecutableVoid and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoid">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        public void Execute<TExecutableVoid, TInput>(TInput input) where TExecutableVoid : class, IExecutableVoid<TInput>
            => _executor.Execute<TExecutableVoid, TInput>(input);

        /// <summary>
        /// Resolves an instance of TExecutable executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutable">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <returns>An instance of TResult.</returns>
        public TResult Execute<TExecutable, TResult>() where TExecutable : class, IExecutable<TResult>
            => _executor.Execute<TExecutable, TResult>();

        /// <summary>
        /// Resolves an instance of TExecutable using IServiceProvider for current request if available or using dedicated scope and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutable">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        /// <returns>An instance of TResult.</returns>
        public TResult Execute<TExecutable, TInput, TResult>(TInput input) where TExecutable : class, IExecutable<TInput, TResult>
            => _executor.Execute<TExecutable, TInput, TResult>(input);

        /// <summary>
        /// Resolves an instance of TExecutableVoidAsync using dedicated scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoidAsync">The type of the executable to resolve.</typeparam>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task ExecuteAsync<TExecutableVoidAsync>() where TExecutableVoidAsync : class, IExecutableVoidAsync
            => await _executorAsync.ExecuteAsync<TExecutableVoidAsync>();

        /// <summary>
        /// Resolves an instance of TExecutableVoidAsync using dedicated scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoidAsync">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task ExecuteAsync<TExecutableVoidAsync, TInput>(TInput input) where TExecutableVoidAsync : class, IExecutableVoidAsync<TInput>
            => await _executorAsync.ExecuteAsync<TExecutableVoidAsync, TInput>(input);

        /// <summary>
        /// Resolves an instance of TExecutableAsync using dedicated scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableAsync">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task<TResult> ExecuteAsync<TExecutableAsync, TResult>() where TExecutableAsync : class, IExecutableAsync<TResult>
            => await _executorAsync.ExecuteAsync<TExecutableAsync, TResult>();

        /// <summary>
        /// Resolves an instance of TExecutableAsync using dedicated scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableAsync">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task<TResult> ExecuteAsync<TExecutableAsync, TInput, TResult>(TInput input) where TExecutableAsync : class, IExecutableAsync<TInput, TResult>
            => await _executorAsync.ExecuteAsync<TExecutableAsync, TInput, TResult>(input);
    }
}