using System;
using SimpleInjector;

namespace M.Executables.Executors.SimpleInjector
{
    public interface IErrorHandler
    {
        void Handle(Exception exception, Scope scope);
    }
}