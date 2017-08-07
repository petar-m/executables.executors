using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace M.Executables.Executors.SimpleInjector
{
    /// <summary>
    /// Represents a SimpleInjector scope for executing executables.
    /// </summary>
    public class ExecutorScope : IExecutorScope
    {
        private readonly Container container;
        private Scope scope;

        /// <summary>
        /// Creates a new instance of ExecutorScope.
        /// </summary>
        /// <param name="container">An instance of SimpleInjector container.</param>
        public ExecutorScope(Container container)
        {
            this.container = container;
        }

        /// <summary>
        /// Gets an instance of IExecutor. All executables are executed in the same scope.
        /// </summary>
        public IExecutor Executor
        {
            get
            {
                EnsureScope();
                return scope.Container.GetInstance<IExecutor>();
            }
        }

        /// <summary>
        /// Gets an instance of IExecutorAsync. All executables are executed in the same scope.
        /// </summary>
        public IExecutorAsync ExecutorAsync
        {
            get
            {
                EnsureScope();
                return scope.Container.GetInstance<IExecutorAsync>();
            }
        }

        /// <summary>
        /// Releases all resources.
        /// </summary>
        public void Dispose()
        {
            if (scope != null)
            {
                scope.Dispose();
                scope = null;
            }
        }

        private void EnsureScope()
        {
            if (scope == null)
            {
                scope = AsyncScopedLifestyle.BeginScope(container);
                scope.WhenScopeEnds(() => scope.Container.GetInstance<IScopeEndHandler>().Handle(scope));
            }
        }
    }
}
