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

        public void Execute<TExecutableVoid>() where TExecutableVoid : IExecutableVoid
        {
            var executable = ResolveExecutable<TExecutableVoid>();
            executable.Execute();
        }

        public void Execute<TExecutableVoid, TInput>(TInput input) where TExecutableVoid : IExecutableVoid<TInput>
        {
            var executable = ResolveExecutable<TExecutableVoid>();
            executable.Execute(input);
        }

        public TResult Execute<TExecutable, TResult>() where TExecutable : IExecutable<TResult>
        {
            var executable = ResolveExecutable<TExecutable>();
            return executable.Execute();
        }

        public TResult Execute<TExecutable, TInput, TResult>(TInput input) where TExecutable : IExecutable<TInput, TResult>
        {
            var executable = ResolveExecutable<TExecutable>();
            return executable.Execute(input);
        }

        public async Task ExecuteAsync<TExecutableVoid>() where TExecutableVoid : IExecutableVoid
        {
            var executable = ResolveExecutable<TExecutableVoid>();
            await Task.Run(() => executable.Execute());
        }

        public async Task ExecuteAsync<TExecutableVoid, TInput>(TInput input) where TExecutableVoid : IExecutableVoid<TInput>
        {
            var executable = ResolveExecutable<TExecutableVoid>();
            await Task.Run(() => executable.Execute(input));
        }

        public async Task<TResult> ExecuteAsync<TExecutable, TResult>() where TExecutable : IExecutable<TResult>
        {
            var executable = ResolveExecutable<TExecutable>();
            TResult result = await Task.Run(() => executable.Execute());
            return result;
        }

        public async Task<TResult> ExecuteAsync<TExecutable, TInput, TResult>(TInput input) where TExecutable : IExecutable<TInput, TResult>
        {
            var executable = ResolveExecutable<TExecutable>();
            TResult result = await Task.Run(() => executable.Execute(input));
            return result;
        }

        private T ResolveExecutable<T>()
        {
            return (T)instanceProvider.GetInstance(typeof(T));
        }
    }
}
