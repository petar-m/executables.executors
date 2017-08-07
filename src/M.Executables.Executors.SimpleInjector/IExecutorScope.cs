using System;

namespace M.Executables.Executors.SimpleInjector
{
    /// <summary>
    /// Represents a scope for executing executables.
    /// </summary>
    public interface IExecutorScope : IDisposable
    {
        /// <summary>
        /// Gets an instance of IExecutor. All executables are executed in the same scope.
        /// </summary>
        IExecutor Executor { get; }

        /// <summary>
        /// Gets an instance of IExecutorAsync. All executables are executed in the same scope.
        /// </summary>
        IExecutorAsync ExecutorAsync { get; }
    }
}
