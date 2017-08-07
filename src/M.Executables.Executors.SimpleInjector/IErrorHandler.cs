using System;
using SimpleInjector;

namespace M.Executables.Executors.SimpleInjector
{
    /// <summary>
    /// Handles an exception is thrown from SimpleInjectorExecutor when executing an executable.
    /// </summary>
    public interface IErrorHandler
    {
        /// <summary>
        /// Called when an exception is thrown from SimpleInjectorExecutor when executing an executable.
        /// </summary>
        /// <param name="exception">The exception being thrown.</param>
        /// <param name="scope">The SimpleInjector scope used by the instance of the SimpleInjectorExecutor.</param>
        void Handle(Exception exception, Scope scope);
    }
}