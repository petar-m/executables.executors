using System;
using System.Threading.Tasks;
using SimpleInjector;

namespace M.Executables.Executors.SimpleInjector
{
    public class SimpleInjectorExecutor : IExecutor, IExecutorAsync
    {
        private readonly Container container;
        private readonly IErrorHandler errorHandler;
        private Scope scope;

        public SimpleInjectorExecutor(Container container, IErrorHandler errorHandler)
        {
            this.container = container;
            this.errorHandler = errorHandler;
            scope = Lifestyle.Scoped.GetCurrentScope(container);
        }

        public void Execute<TExecutableVoid>() where TExecutableVoid : class, IExecutableVoid
        {
            var executable = ResolveExecutable<TExecutableVoid>();
            Execute(() => { executable.Execute(); return 0; });
        }

        public void Execute<TExecutableVoid, TInput>(TInput input) where TExecutableVoid : class, IExecutableVoid<TInput>
        {
            var executable = ResolveExecutable<TExecutableVoid>();
            Execute(() => { executable.Execute(input); return 0; });
        }

        public TResult Execute<TExecutable, TResult>() where TExecutable : class, IExecutable<TResult>
        {
            var executable = ResolveExecutable<TExecutable>();
            return Execute(() => executable.Execute());
        }

        public TResult Execute<TExecutable, TInput, TResult>(TInput input) where TExecutable : class, IExecutable<TInput, TResult>
        {
            var executable = ResolveExecutable<TExecutable>();
            return Execute(() => executable.Execute(input));
        }

        public async Task ExecuteAsync<TExecutableVoidAsync>() where TExecutableVoidAsync : class, IExecutableVoidAsync
        {
            var executable = ResolveExecutable<TExecutableVoidAsync>();
            await ExecuteAsync(async () => { await executable.ExecuteAsync(); return 0; } );
        }

        public async Task ExecuteAsync<TExecutableVoidAsync, TInput>(TInput input) where TExecutableVoidAsync : class, IExecutableVoidAsync<TInput>
        {
            var executable = ResolveExecutable<TExecutableVoidAsync>();
            await ExecuteAsync(async () => { await executable.ExecuteAsync(input); return 0; });
        }

        public async Task<TResult> ExecuteAsync<TExecutableAsync, TResult>() where TExecutableAsync : class, IExecutableAsync<TResult>
        {
            var executable = ResolveExecutable<TExecutableAsync>();
            TResult result = await ExecuteAsync(async () => await executable.ExecuteAsync());
            return result;
        }

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
