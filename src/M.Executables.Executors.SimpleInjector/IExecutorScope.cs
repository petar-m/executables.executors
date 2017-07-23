using System;

namespace M.Executables.Executors.SimpleInjector
{
    public interface IExecutorScope : IDisposable
    {
        IExecutor Executor { get; }
        IExecutorAsync ExecutorAsync { get; }
    }
}
