using System;
using System.Threading.Tasks;
using SimpleInjector;

namespace M.Executables.Executors.SimpleInjector
{
    /// <summary>
    /// Resolves an executable instance using current SimpleInjector scope and executes it.
    /// </summary>
    public class SimpleInjectorExecutor : IExecutor, IExecutorAsync
    {
        private readonly Container container;
        private readonly IErrorHandler errorHandler;
        private Scope scope;

        /// <summary>
        /// Creates a new instance of SimpleInjectorExecutor.
        /// </summary>
        /// <param name="container">An instance of SimpleInjector container.</param>
        /// <param name="errorHandler">An instance of IErrorHandler.</param>
        public SimpleInjectorExecutor(Container container, IErrorHandler errorHandler)
        {
            this.container = container;
            this.errorHandler = errorHandler;
            scope = Lifestyle.Scoped.GetCurrentScope(container);
        }

        /// <summary>
        /// Resolves an instance of TExecutableVoid using current SimpleInjector scope and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoid">The type of the executable to resolve.</typeparam>
        public void Execute<TExecutableVoid>() where TExecutableVoid : class, IExecutableVoid
        {
            var executable = ResolveExecutable<TExecutableVoid>();
            Execute(() => { executable.Execute(); return 0; });
        }

        /// <summary>
        /// Resolves an instance of TExecutableVoid using current SimpleInjector scope and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoid">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        public void Execute<TExecutableVoid, TInput>(TInput input) where TExecutableVoid : class, IExecutableVoid<TInput>
        {
            var executable = ResolveExecutable<TExecutableVoid>();
            Execute(() => { executable.Execute(input); return 0; });
        }

        /// <summary>
        /// Resolves an instance of TExecutable using current SimpleInjector scope and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutable">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <returns>An instance of TResult.</returns>
        public TResult Execute<TExecutable, TResult>() where TExecutable : class, IExecutable<TResult>
        {
            var executable = ResolveExecutable<TExecutable>();
            return Execute(() => executable.Execute());
        }

        /// <summary>
        /// Resolves an instance of TExecutable using current SimpleInjector scope and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutable">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        /// <returns>An instance of TResult.</returns>
        public TResult Execute<TExecutable, TInput, TResult>(TInput input) where TExecutable : class, IExecutable<TInput, TResult>
        {
            var executable = ResolveExecutable<TExecutable>();
            return Execute(() => executable.Execute(input));
        }

        /// <summary>
        /// Resolves an instance of TExecutableVoidAsync using current SimpleInjector scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoidAsync">The type of the executable to resolve.</typeparam>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task ExecuteAsync<TExecutableVoidAsync>() where TExecutableVoidAsync : class, IExecutableVoidAsync
        {
            var executable = ResolveExecutable<TExecutableVoidAsync>();
            await ExecuteAsync(async () => { await executable.ExecuteAsync(); return 0; });
        }

        /// <summary>
        /// Resolves an instance of TExecutableVoidAsync using current SimpleInjector scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoidAsync">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task ExecuteAsync<TExecutableVoidAsync, TInput>(TInput input) where TExecutableVoidAsync : class, IExecutableVoidAsync<TInput>
        {
            var executable = ResolveExecutable<TExecutableVoidAsync>();
            await ExecuteAsync(async () => { await executable.ExecuteAsync(input); return 0; });
        }

        /// <summary>
        /// Resolves an instance of TExecutableAsync using current SimpleInjector scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableAsync">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task<TResult> ExecuteAsync<TExecutableAsync, TResult>() where TExecutableAsync : class, IExecutableAsync<TResult>
        {
            var executable = ResolveExecutable<TExecutableAsync>();
            TResult result = await ExecuteAsync(async () => await executable.ExecuteAsync());
            return result;
        }

        /// <summary>
        /// Resolves an instance of TExecutableAsync using current SimpleInjector scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableAsync">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task<TResult> ExecuteAsync<TExecutableAsync, TInput, TResult>(TInput input) where TExecutableAsync : class, IExecutableAsync<TInput, TResult>
        {
            var executable = ResolveExecutable<TExecutableAsync>();
            TResult result = await ExecuteAsync(async () => await executable.ExecuteAsync(input));
            return result;
        }

        private TResult Execute<TResult>(Func<TResult> func)
        {
            try
            {
                return func();
            }
            catch (Exception x)
            {
                errorHandler.Handle(x, scope);
                throw;
            }
        }

        private async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> func)
        {
            try
            {
                return await func();
            }
            catch (Exception x)
            {
                errorHandler.Handle(x, scope);
                throw;
            }
        }

        private T ResolveExecutable<T>() where T : class
        {
            return scope.Container.GetInstance<T>();
        }
    }
}
