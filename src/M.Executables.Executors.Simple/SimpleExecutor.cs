using System.Threading.Tasks;

namespace M.Executables.Executors.Simple
{
    /// <summary>
    /// Resolves an executable instance and executes it.
    /// </summary>
    public class SimpleExecutor : IExecutor, IExecutorAsync
    {
        private readonly IInstanceProvider instanceProvider;

        /// <summary>
        /// Creates a new instance of SimpleExecutor.
        /// </summary>
        /// <param name="instanceProvider">An instance of IInstanceProvider for creaing instances of executables.</param>
        public SimpleExecutor(IInstanceProvider instanceProvider)
        {
            this.instanceProvider = instanceProvider;
        }

        /// <summary>
        /// Resolves an instance of TExecutableVoid and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoid">The type if the executable to resolve.</typeparam>
        public void Execute<TExecutableVoid>() where TExecutableVoid : class, IExecutableVoid
        {
            var executable = ResolveExecutable<TExecutableVoid>();
            executable.Execute();
        }

        /// <summary>
        /// Resolves an instance of TExecutableVoid and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoid">The type if the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        public void Execute<TExecutableVoid, TInput>(TInput input) where TExecutableVoid : class, IExecutableVoid<TInput>
        {
            var executable = ResolveExecutable<TExecutableVoid>();
            executable.Execute(input);
        }

        /// <summary>
        /// Resolves an instance of TExecutable and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutable">The type if the executable to resolve.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <returns>An instance if TResult.</returns>
        public TResult Execute<TExecutable, TResult>() where TExecutable : class, IExecutable<TResult>
        {
            var executable = ResolveExecutable<TExecutable>();
            return executable.Execute();
        }

        /// <summary>
        /// Resolves an instance of TExecutable and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutable">The type if the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        /// <returns>An instance if TResult.</returns>
        public TResult Execute<TExecutable, TInput, TResult>(TInput input) where TExecutable : class, IExecutable<TInput, TResult>
        {
            var executable = ResolveExecutable<TExecutable>();
            return executable.Execute(input);
        }

        /// <summary>
        /// Resolves an instance of TExecutableVoidAsync and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoidAsync">The type if the executable to resolve.</typeparam>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task ExecuteAsync<TExecutableVoidAsync>() where TExecutableVoidAsync : class, IExecutableVoidAsync
        {
            var executable = ResolveExecutable<TExecutableVoidAsync>();
            await executable.ExecuteAsync();
        }

        /// <summary>
        /// Resolves an instance of TExecutableVoidAsync and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoidAsync">The type if the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task ExecuteAsync<TExecutableVoidAsync, TInput>(TInput input) where TExecutableVoidAsync : class, IExecutableVoidAsync<TInput>
        {
            var executable = ResolveExecutable<TExecutableVoidAsync>();
            await executable.ExecuteAsync(input);
        }

        /// <summary>
        /// Resolves an instance of TExecutableAsync and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableAsync">The type if the executable to resolve.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task<TResult> ExecuteAsync<TExecutableAsync, TResult>() where TExecutableAsync : class, IExecutableAsync<TResult>
        {
            var executable = ResolveExecutable<TExecutableAsync>();
            TResult result = await executable.ExecuteAsync();
            return result;
        }

        /// <summary>
        /// Resolves an instance of TExecutableAsync and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableAsync">The type if the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task<TResult> ExecuteAsync<TExecutableAsync, TInput, TResult>(TInput input) where TExecutableAsync : class, IExecutableAsync<TInput, TResult>
        {
            var executable = ResolveExecutable<TExecutableAsync>();
            TResult result = await executable.ExecuteAsync(input);
            return result;
        }

        private T ResolveExecutable<T>()
        {
            return (T)instanceProvider.GetInstance(typeof(T));
        }
    }
}
