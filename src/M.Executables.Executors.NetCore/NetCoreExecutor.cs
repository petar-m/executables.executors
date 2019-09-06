using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace M.Executables.Executors.NetCore
{
    /// <summary>
    /// Resolves an executable instance in dedicated scope and executes it.
    /// </summary>
    public class NetCoreExecutor : IExecutor
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Creates a new instance of NetCoreExecutor class.
        /// </summary>
        /// <param name="serviceScopeFactory">IServiceScopeFactory used to create scope for each execution.</param>
        public NetCoreExecutor(IServiceScopeFactory serviceScopeFactory) => _serviceScopeFactory = serviceScopeFactory;

        /// <summary>
        /// Resolves an instance of TExecutableVoid using dedicated scope and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoid">The type of the executable to resolve.</typeparam>
        public void Execute<TExecutableVoid>() where TExecutableVoid : class, IExecutableVoid =>
            Execute<TExecutableVoid, IEmpty, IEmpty>((x, _) => { x.Execute(); return null; }, null);

        /// <summary>
        /// Resolves an instance of TExecutableVoid using dedicated scope and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoid">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        public void Execute<TExecutableVoid, TInput>(TInput input) where TExecutableVoid : class, IExecutableVoid<TInput> =>
            Execute<TExecutableVoid, TInput, IEmpty>((x, i) => { x.Execute(i); return null; }, input);

        /// <summary>
        /// Resolves an instance of TExecutable using dedicated scope and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutable">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <returns>An instance of TResult.</returns>
        public TResult Execute<TExecutable, TResult>() where TExecutable : class, IExecutable<TResult> =>
            Execute<TExecutable, IEmpty, TResult>((x, _) => x.Execute(), null);

        /// <summary>
        /// Resolves an instance of TExecutable using dedicated scope and executes it synchronously.
        /// </summary>
        /// <typeparam name="TExecutable">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        /// <returns>An instance of TResult.</returns>
        public TResult Execute<TExecutable, TInput, TResult>(TInput input) where TExecutable : class, IExecutable<TInput, TResult> =>
            Execute<TExecutable, TInput, TResult>((x, i) => x.Execute(i), input);

        private TResult Execute<T, TInput, TResult>(Func<T, TInput, TResult> execute, TInput input)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var executable = scope.ServiceProvider.GetRequiredService<T>();
                var interceptors = GetInterceptors<T, TInput, TResult>(scope);

                interceptors.Before(executable, input);

                TResult result = default;
                try
                {
                    result = execute(executable, input);
                }
                catch (Exception exception)
                {
                    interceptors.After(executable, input, result, exception);
                    throw;
                }

                interceptors.After(executable, input, result, null);
                return result;
            }
        }

        private static IInterceptors<TExecutable, TInput, TResult> GetInterceptors<TExecutable, TInput, TResult>(IServiceScope scope)
        {
            var generalInterceptors = scope.ServiceProvider.GetServices<IExecutionInterceptor>();
            var general = generalInterceptors as IExecutionInterceptor[] ?? generalInterceptors.ToArray();
            var specificInterceptors = scope.ServiceProvider.GetServices<IExecutionInterceptor<TExecutable, TInput, TResult>>();
            var specific = specificInterceptors as IExecutionInterceptor<TExecutable, TInput, TResult>[] ?? specificInterceptors.ToArray();

            if (specific.Length > 0 || general.Length > 0)
            {
                var firstSpecific = specific.Where(x => x is IDiscardOtherInterceptors).OrderBy(x => x.OrderingIndex).FirstOrDefault();
                var firstGeneral = general.Where(x => x is IDiscardOtherInterceptors).OrderBy(x => x.OrderingIndex).FirstOrDefault();
                if (firstSpecific != null && firstGeneral != null)
                {
                    if (firstSpecific.OrderingIndex <= firstGeneral.OrderingIndex || firstSpecific is IDiscardNonGenericInterceptors)
                    {
                        return new Interceptors<TExecutable, TInput, TResult>(Array.Empty<IExecutionInterceptor>(), new[] { firstSpecific });
                    }
                    else
                    {
                        return new Interceptors<TExecutable, TInput, TResult>(new[] { firstGeneral }, Array.Empty<IExecutionInterceptor<TExecutable, TInput, TResult>>());
                    }
                }

                if (firstSpecific != null)
                {
                    return new Interceptors<TExecutable, TInput, TResult>(Array.Empty<IExecutionInterceptor>(), new[] { firstSpecific });
                }

                if (firstGeneral != null)
                {
                    return new Interceptors<TExecutable, TInput, TResult>(new[] { firstGeneral }, Array.Empty<IExecutionInterceptor<TExecutable, TInput, TResult>>());
                }

                if (specificInterceptors.Any(x => x is IDiscardNonGenericInterceptors))
                {
                    return new Interceptors<TExecutable, TInput, TResult>(Array.Empty<IExecutionInterceptor>(), specific);
                }

                return new Interceptors<TExecutable, TInput, TResult>(general, specific);
            }

            return EmptyInterceptors<TExecutable, TInput, TResult>.Instance;
        }

        private interface IInterceptors<TExecutable, TInput, TResult>
        {
            void After(TExecutable executable, TInput input, TResult result, Exception exception);

            void Before(TExecutable executable, TInput input);
        }

        private class EmptyInterceptors<TExecutable, TInput, TResult> : IInterceptors<TExecutable, TInput, TResult>
        {
            private EmptyInterceptors()
            {
            }

            public static readonly EmptyInterceptors<TExecutable, TInput, TResult> Instance = new EmptyInterceptors<TExecutable, TInput, TResult>();

            public void After(TExecutable executable, TInput input, TResult result, Exception exception)
            {
            }

            public void Before(TExecutable executable, TInput input)
            {
            }
        }

        private class Interceptors<TExecutable, TInput, TResult> : IInterceptors<TExecutable, TInput, TResult>
        {
            private readonly IExecutionInterceptor<TExecutable, TInput, TResult>[] _specificInterceptors;
            private readonly IExecutionInterceptor[] _generalInterceptors;
            private readonly (bool isGeneral, int ordering, int index)[] _interceptors;

            public Interceptors(
                IExecutionInterceptor[] generalInterceptors,
                IExecutionInterceptor<TExecutable, TInput, TResult>[] specificInterceptors)
            {
                _specificInterceptors = specificInterceptors;
                _generalInterceptors = generalInterceptors;
                _interceptors = _specificInterceptors.Select((x, i) => (isGeneral: false, ordering: x.OrderingIndex, index: i))
                                     .Concat(_generalInterceptors.Select((x, i) => (isGeneral: true, ordering: x.OrderingIndex, index: i)))
                                     .OrderBy(x => x.ordering)
                                     .ToArray();
            }

            public  void After(TExecutable executable, TInput input, TResult result, Exception exception)
            {
                for (int i = _interceptors.Length - 1; i >= 0; i--)
                {
                    var (isGeneral, _, index) = _interceptors[i];
                    if (isGeneral)
                    {
                        _generalInterceptors[index].After(executable, input, result, exception);
                    }
                    else
                    {
                        _specificInterceptors[index].After(executable, input, result, exception);
                    }
                }
            }

            public  void Before(TExecutable executable, TInput input)
            {
                for (int i = 0; i < _interceptors.Length; i++)
                {
                    var (isGeneral, _, index) = _interceptors[i];
                    if (isGeneral)
                    {
                        _generalInterceptors[index].Before(executable, input);
                    }
                    else
                    {
                        _specificInterceptors[index].Before(executable, input);
                    }
                }
            }
        }
    }
}
