using System.Threading.Tasks;

namespace M.Executables.Executors.Simple
{
    public class SimpleExecutor : IExecutor, IExecutorAsync
    {
        private readonly IInstanceProvider instanceProvider;

        public SimpleExecutor(IInstanceProvider instanceProvider)
        {
            this.instanceProvider = instanceProvider;
        }

        public void Execute<TExecutableVoid>() where TExecutableVoid : class, IExecutableVoid
        {
            var executable = ResolveExecutable<TExecutableVoid>();
            executable.Execute();
        }

        public void Execute<TExecutableVoid, TInput>(TInput input) where TExecutableVoid : class, IExecutableVoid<TInput>
        {
            var executable = ResolveExecutable<TExecutableVoid>();
            executable.Execute(input);
        }

        public TResult Execute<TExecutable, TResult>() where TExecutable : class, IExecutable<TResult>
        {
            var executable = ResolveExecutable<TExecutable>();
            return executable.Execute();
        }

        public TResult Execute<TExecutable, TInput, TResult>(TInput input) where TExecutable : class, IExecutable<TInput, TResult>
        {
            var executable = ResolveExecutable<TExecutable>();
            return executable.Execute(input);
        }

        public async Task ExecuteAsync<TExecutableVoidAsync>() where TExecutableVoidAsync : class, IExecutableVoidAsync
        {
            var executable = ResolveExecutable<TExecutableVoidAsync>();
            await executable.ExecuteAsync();
        }

        public async Task ExecuteAsync<TExecutableVoidAsync, TInput>(TInput input) where TExecutableVoidAsync : class, IExecutableVoidAsync<TInput>
        {
            var executable = ResolveExecutable<TExecutableVoidAsync>();
            await executable.ExecuteAsync(input);
        }

        public async Task<TResult> ExecuteAsync<TExecutableAsync, TResult>() where TExecutableAsync : class, IExecutableAsync<TResult>
        {
            var executable = ResolveExecutable<TExecutableAsync>();
            TResult result = await executable.ExecuteAsync();
            return result;
        }

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
