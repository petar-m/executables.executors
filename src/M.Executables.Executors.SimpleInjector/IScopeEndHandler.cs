using SimpleInjector;

namespace M.Executables.Executors.SimpleInjector
{
    public interface IScopeEndHandler
    {
        void Handle(Scope scope);
    }
}
