using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace M.Executables.Executors.NetCore
{
    /// <summary>
    /// Resolves an executable instance using IServiceProvider in dedicated scope and executes it.
    /// </summary>
    public class NetCoreExecutorAsync : IExecutorAsync
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Creates a new instance of NetCoreAsyncExecutor.
        /// </summary>
        /// <param name="serviceScopeFactory">IServiceScopeFactory used to create scope.</param>
        public NetCoreExecutorAsync(IServiceScopeFactory serviceScopeFactory) => _serviceScopeFactory = serviceScopeFactory;

        /// <summary>
        /// Resolves an instance of TExecutableVoidAsync using dedicated scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoidAsync">The type of the executable to resolve.</typeparam>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task ExecuteAsync<TExecutableVoidAsync>() where TExecutableVoidAsync : class, IExecutableVoidAsync =>
            await ExecuteAsync<TExecutableVoidAsync, IEmpty, IEmpty>(async (x, _) => { await x.ExecuteAsync().ConfigureAwait(false); return null; }, null).ConfigureAwait(false);

        /// <summary>
        /// Resolves an instance of TExecutableVoidAsync using dedicated scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableVoidAsync">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task ExecuteAsync<TExecutableVoidAsync, TInput>(TInput input) where TExecutableVoidAsync : class, IExecutableVoidAsync<TInput> =>
            await ExecuteAsync<TExecutableVoidAsync, TInput, IEmpty>(async (x, i) => { await x.ExecuteAsync(i).ConfigureAwait(false); return null; }, input).ConfigureAwait(false);

        /// <summary>
        /// Resolves an instance of TExecutableAsync using dedicated scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableAsync">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task<TResult> ExecuteAsync<TExecutableAsync, TResult>() where TExecutableAsync : class, IExecutableAsync<TResult> =>
            await ExecuteAsync<TExecutableAsync, IEmpty, TResult>(async (x, _) => await x.ExecuteAsync().ConfigureAwait(false), null).ConfigureAwait(false);

        /// <summary>
        /// Resolves an instance of TExecutableAsync using dedicated scope and executes it asynchronously.
        /// </summary>
        /// <typeparam name="TExecutableAsync">The type of the executable to resolve.</typeparam>
        /// <typeparam name="TInput">The type of the parameter to pass to the executable.</typeparam>
        /// <typeparam name="TResult">The type of the result returned from the executable.</typeparam>
        /// <param name="input">An instance of TInput to pass to the executable.</param>
        /// <returns>A Task representing the asynchronous execution result.</returns>
        public async Task<TResult> ExecuteAsync<TExecutableAsync, TInput, TResult>(TInput input) where TExecutableAsync : class, IExecutableAsync<TInput, TResult> =>
            await ExecuteAsync<TExecutableAsync, TInput, TResult>(async (x, i) => await x.ExecuteAsync(i).ConfigureAwait(false), input).ConfigureAwait(false);

        private async Task<TResult> ExecuteAsync<T, TInput, TResult>(Func<T, TInput, Task<TResult>> execute, TInput input)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var executable = scope.ServiceProvider.GetRequiredService<T>();
                var interceptors = GetAsyncInterceptors<T, TInput, TResult>(scope);

                await interceptors.BeforeAsync(executable, input).ConfigureAwait(false);

                TResult result = default;
                try
                {
                    result = await execute(executable, input).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    await interceptors.AfterAsync(executable, input, result, exception).ConfigureAwait(false);
                    throw;
                }

                await interceptors.AfterAsync(executable, input, result, null).ConfigureAwait(false);
                return result;
            }
        }

        private static IAsyncInterceptors<TExecutable, TInput, TResult> GetAsyncInterceptors<TExecutable, TInput, TResult>(IServiceScope scope)
        {
            var generalInterceptors = scope.ServiceProvider.GetServices<IExecutionInterceptorAsync>();
            var general = generalInterceptors as IExecutionInterceptorAsync[] ?? generalInterceptors.ToArray();
            var specificInterceptors = scope.ServiceProvider.GetServices<IExecutionInterceptorAsync<TExecutable, TInput, TResult>>();
            var specific = specificInterceptors as IExecutionInterceptorAsync<TExecutable, TInput, TResult>[] ?? specificInterceptors.ToArray();

            if (specific.Length > 0 || general.Length > 0)
            {
                var firstSpecific = specific.Where(x => x is IDiscardOtherInterceptors).OrderBy(x => x.OrderingIndex).FirstOrDefault();
                var firstGeneral = general.Where(x => x is IDiscardOtherInterceptors).OrderBy(x => x.OrderingIndex).FirstOrDefault();
                if (firstSpecific != null && firstGeneral != null)
                {
                    if (firstSpecific.OrderingIndex <= firstGeneral.OrderingIndex || firstSpecific is IDiscardNonGenericInterceptors)
                    {
                        return new AsyncInterceptors<TExecutable, TInput, TResult>(Array.Empty<IExecutionInterceptorAsync>(), new[] { firstSpecific });
                    }
                    else
                    {
                        return new AsyncInterceptors<TExecutable, TInput, TResult>(new[] { firstGeneral }, Array.Empty<IExecutionInterceptorAsync<TExecutable, TInput, TResult>>());
                    }
                }

                if (firstSpecific != null)
                {
                    return new AsyncInterceptors<TExecutable, TInput, TResult>(Array.Empty<IExecutionInterceptorAsync>(), new[] { firstSpecific });
                }

                if (firstGeneral != null)
                {
                    return new AsyncInterceptors<TExecutable, TInput, TResult>(new[] { firstGeneral }, Array.Empty<IExecutionInterceptorAsync<TExecutable, TInput, TResult>>());
                }

                if(specificInterceptors.Any(x => x is IDiscardNonGenericInterceptors))
                {
                    return new AsyncInterceptors<TExecutable, TInput, TResult>(Array.Empty<IExecutionInterceptorAsync>(), specific);
                }

                return new AsyncInterceptors<TExecutable, TInput, TResult>(general, specific);
            }

            return EmptyAsyncInterceptors<TExecutable, TInput, TResult>.Instance;
        }

        private interface IAsyncInterceptors<TExecutable, TInput, TResult>
        {
            Task AfterAsync(TExecutable executable, TInput input, TResult result, Exception exception);

            Task BeforeAsync(TExecutable executable, TInput input);
        }

        private class EmptyAsyncInterceptors<TExecutable, TInput, TResult> : IAsyncInterceptors<TExecutable, TInput, TResult>
        {
            private EmptyAsyncInterceptors()
            {
            }

            public static readonly EmptyAsyncInterceptors<TExecutable, TInput, TResult> Instance = new EmptyAsyncInterceptors<TExecutable, TInput, TResult>();

            public Task AfterAsync(TExecutable executable, TInput input, TResult result, Exception exception) => Task.CompletedTask;

            public Task BeforeAsync(TExecutable executable, TInput input) => Task.CompletedTask;
        }

        private class AsyncInterceptors<TExecutable, TInput, TResult> : IAsyncInterceptors<TExecutable, TInput, TResult>
        {
            private readonly IExecutionInterceptorAsync<TExecutable, TInput, TResult>[] _specificInterceptors;
            private readonly IExecutionInterceptorAsync[] _generalInterceptors;
            private readonly (bool isGeneral, int ordering, int index)[] _interceptors;

            public AsyncInterceptors(
                IExecutionInterceptorAsync[] generalInterceptors,
                IExecutionInterceptorAsync<TExecutable, TInput, TResult>[] specificInterceptors)
            {
                _specificInterceptors = specificInterceptors;
                _generalInterceptors = generalInterceptors;
                _interceptors = _specificInterceptors.Select((x, i) => (isGeneral: false, ordering: x.OrderingIndex, index: i))
                                     .Concat(_generalInterceptors.Select((x, i) => (isGeneral: true, ordering: x.OrderingIndex, index: i)))
                                     .OrderBy(x => x.ordering)
                                     .ToArray();
            }

            public async Task AfterAsync(TExecutable executable, TInput input, TResult result, Exception exception)
            {
                for (int i = _interceptors.Length - 1; i >= 0; i--)
                {
                    var (isGeneral, _, index) = _interceptors[i];
                    if (isGeneral)
                    {
                        await _generalInterceptors[index].AfterAsync(executable, input, result, exception).ConfigureAwait(false);
                    }
                    else
                    {
                        await _specificInterceptors[index].AfterAsync(executable, input, result, exception).ConfigureAwait(false);
                    }
                }
            }

            public async Task BeforeAsync(TExecutable executable, TInput input)
            {
                for (int i = 0; i < _interceptors.Length; i++)
                {
                    var (isGeneral, _, index) = _interceptors[i];
                    if (isGeneral)
                    {
                        await _generalInterceptors[index].BeforeAsync(executable, input).ConfigureAwait(false);
                    }
                    else
                    {
                        await _specificInterceptors[index].BeforeAsync(executable, input).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
