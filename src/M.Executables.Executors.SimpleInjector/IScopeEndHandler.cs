using SimpleInjector;

namespace M.Executables.Executors.SimpleInjector
{
    /// <summary>
    /// Provides a action to be performed when SimpleInjector scope ends.
    /// </summary>
    public interface IScopeEndHandler
    {
        /// <summary>
        /// An action to be performed when SimpleInjector scope ends.
        /// </summary>
        /// <param name="scope">The scope that is ending.</param>
        void Handle(Scope scope);
    }
}
