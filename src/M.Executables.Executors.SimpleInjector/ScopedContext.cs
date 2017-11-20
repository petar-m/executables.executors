using System;
using System.Threading.Tasks;
using SimpleInjector;
using SimpleInjector.Lifestyles;

namespace M.Executables.Executors.SimpleInjector
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    
    /// <summary>
    /// Provides a context for executing an action in a scope. Every method uses a new child scope to resolve arguments and disposes it after execution.
    /// </summary>
    public class ScopedContext : IScopedContext
    {
        private readonly Container container;

        public ScopedContext(Container container)
        {
            this.container = container;
        }

        public void Execute<T>(Action<T> action)
            where T : class
        {
            using (var scope = CreateScope(container))
            {
                var arg = scope.Container.GetInstance<T>();
                action(arg);
            }
        }

        public void Execute<T1, T2>(Action<T1, T2> action)
            where T1 : class
            where T2 : class
        {
            using (var scope = CreateScope(container))
            {
                var arg1 = scope.Container.GetInstance<T1>();
                var arg2 = scope.Container.GetInstance<T2>();
                action(arg1, arg2);
            }
        }

        public void Execute<T1, T2, T3>(Action<T1, T2, T3> action)
            where T1 : class
            where T2 : class
            where T3 : class
        {
            using (var scope = CreateScope(container))
            {
                var arg1 = scope.Container.GetInstance<T1>();
                var arg2 = scope.Container.GetInstance<T2>();
                var arg3 = scope.Container.GetInstance<T3>();
                action(arg1, arg2, arg3);
            }
        }

        public void Execute<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            using (var scope = CreateScope(container))
            {
                var arg1 = scope.Container.GetInstance<T1>();
                var arg2 = scope.Container.GetInstance<T2>();
                var arg3 = scope.Container.GetInstance<T3>();
                var arg4 = scope.Container.GetInstance<T4>();
                action(arg1, arg2, arg3, arg4);
            }
        }

        public void Execute<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            using (var scope = CreateScope(container))
            {
                var arg1 = scope.Container.GetInstance<T1>();
                var arg2 = scope.Container.GetInstance<T2>();
                var arg3 = scope.Container.GetInstance<T3>();
                var arg4 = scope.Container.GetInstance<T4>();
                var arg5 = scope.Container.GetInstance<T5>();
                action(arg1, arg2, arg3, arg4, arg5);
            }
        }

        public async Task ExecuteAsync<T>(Func<T, Task> action)
            where T : class
        {
            using (var scope = CreateScope(container))
            {
                var arg = scope.Container.GetInstance<T>();
                await action(arg);
            }
        }

        public async Task ExecuteAsync<T1, T2>(Func<T1, T2, Task> action)
            where T1 : class
            where T2 : class
        {
            using (var scope = CreateScope(container))
            {
                var arg1 = scope.Container.GetInstance<T1>();
                var arg2 = scope.Container.GetInstance<T2>();
                await action(arg1, arg2);
            }
        }

        public async Task ExecuteAsync<T1, T2, T3>(Func<T1, T2, T3, Task> action)
            where T1 : class
            where T2 : class
            where T3 : class
        {
            using (var scope = CreateScope(container))
            {
                var arg1 = scope.Container.GetInstance<T1>();
                var arg2 = scope.Container.GetInstance<T2>();
                var arg3 = scope.Container.GetInstance<T3>();
                await action(arg1, arg2, arg3);
            }
        }


        public async Task ExecuteAsync<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> action)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
        {
            using (var scope = CreateScope(container))
            {
                var arg1 = scope.Container.GetInstance<T1>();
                var arg2 = scope.Container.GetInstance<T2>();
                var arg3 = scope.Container.GetInstance<T3>();
                var arg4 = scope.Container.GetInstance<T4>();
                await action(arg1, arg2, arg3, arg4);
            }
        }

        public async Task ExecuteAsync<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> action)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            using (var scope = CreateScope(container))
            {
                var arg1 = scope.Container.GetInstance<T1>();
                var arg2 = scope.Container.GetInstance<T2>();
                var arg3 = scope.Container.GetInstance<T3>();
                var arg4 = scope.Container.GetInstance<T4>();
                var arg5 = scope.Container.GetInstance<T5>();
                await action(arg1, arg2, arg3, arg4, arg5);
            }
        }

        private static Scope CreateScope(Container container)
        {
            var scope = AsyncScopedLifestyle.BeginScope(container);
            scope.WhenScopeEnds(() => scope.Container.GetInstance<IScopeEndHandler>().Handle(scope));
            return scope;
        }
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
}
