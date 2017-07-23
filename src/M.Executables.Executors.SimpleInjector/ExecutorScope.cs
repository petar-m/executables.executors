using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace M.Executables.Executors.SimpleInjector
{
    public class ExecutorScope : IExecutorScope
    {
        private readonly Container container;
        private Scope scope;

        public ExecutorScope(Container container)
        {
            this.container = container;
        }

        public IExecutor Executor
        {
            get
            {
                EnsureScope();
                return scope.Container.GetInstance<IExecutor>();
            }
        }

        public IExecutorAsync ExecutorAsync
        {
            get
            {
                EnsureScope();
                return scope.Container.GetInstance<IExecutorAsync>();
            }
        }

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
