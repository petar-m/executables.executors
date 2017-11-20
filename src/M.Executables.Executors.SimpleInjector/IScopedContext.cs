using System;
using System.Threading.Tasks;

namespace M.Executables.Executors.SimpleInjector
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Provides a context for executing an action in a scope.
    /// </summary>
    public interface IScopedContext
    {
        void Execute<T>(Action<T> action)
            where T : class;

        void Execute<T1, T2>(Action<T1, T2> action)
            where T1 : class
            where T2 : class;

        void Execute<T1, T2, T3>(Action<T1, T2, T3> action)
            where T1 : class
            where T2 : class
            where T3 : class;

        void Execute<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class;

        void Execute<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class;

        Task ExecuteAsync<T>(Func<T, Task> action)
            where T : class;

        Task ExecuteAsync<T1, T2>(Func<T1, T2, Task> action)
            where T1 : class
            where T2 : class;

        Task ExecuteAsync<T1, T2, T3>(Func<T1, T2, T3, Task> action)
            where T1 : class
            where T2 : class
            where T3 : class;

        Task ExecuteAsync<T1, T2, T3, T4>(Func<T1, T2, T3, T4, Task> action)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class;

        Task ExecuteAsync<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, Task> action)
            where T1 : class
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class;
    }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
