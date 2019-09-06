## Executables.Executors.NetCoreExecutor  

[![NuGet](https://img.shields.io/nuget/v/M.Executables.Executors.NetCore.svg)](https://www.nuget.org/packages/M.Executables.Executors.NetCore)

An implementation of IExecutor and IExecutorAsync ([M.Executables](https://github.com/petar-m/executables)) utilizing .NET Core dependecy injection.  

### Changes  

#### 2.0  

- **(breaking)** `IServiceCollection` extension methods `AddScopedNetCoreExecutor ` and `AddTransientExecutables` are removed. Executor and executables registration is straight-forward and these methods were not covering all possible scenarios.  
- **(breaking)** Constructor dependency on `IHttpContextAccessor` is removed. Now every executable is resolved and executed in new scope. This makes the executor ignorant of hosting environment. This also removes scoped dependencies which can create hidden coupling of executables.  
#### 2.1 
  - `NetCoreExecutor` now implements the new `IExecutors` interface.
  - `NetCoreExecutor` supports the interceptors via the new `IExecutionInterceptor` interface.


#### 3.0

Sync and async implementations are now separated to be independent of each other.  
Interceptors handling is changed to align to this separation and support more scenarios. 

- **(breaking)** `NetCoreExecutor` does not implement `IExecutors` any more.  
Implementations are now separated:
`NetCoreExecutor` implements `IExecutor`
new `NetCoreExecutorAsync` implements `IExecutorAsync`
There is a new class `NetCoreExecutors` implementing `IExecutors` depending on the new implementations.
- **(breaking)** `IExecutionInterceptor` is not called by both `IExecutor` and `IExecutorAsync` any more.
`IExecutorAsync` will use `IExecutionInterceptorAsync` and `IExecutionInterceptorAsync<TExecutable, TInput, TResult>`
`IExecutor` will use `IExecutionInterceptor` and `IExecutionInterceptor<TExecutable, TInput, TResult>`  
- **(breaking)** Async implementations are using `.ConfigureAwait(false)` for all `await` statements


Interceptors handling

Generic and non-generic interceptors are ordered by their `OrderIndex` and executed in ascending order (interceptors with lower indexes are executed first). Note that if two or more interceptors have the same `OrderIndex` ordering of their execution is arbitrary.

1. If there is an interceptor implemeting `IDiscardOtherInterceptors` it will be the only one executed.
2. If there is more than one interceptor implemeting `IDiscardOtherInterceptors`:
    - if the generic one with the lowest `OrderIndex` also implements  `IDiscardNonGenericInterceptors` it will be executed
    - if generic and non-generic have the same lowest `OrderIndex` the generic one will be executed
    - the one with the lowest `OrderIndex` will be executed
3. If any of the generic interceptors implements `IDiscardNonGenericInterceptors` only the generic interceptors will be executed (`IDiscardNonGenericInterceptors` has no effect if non-generic interceptor implements it)
4. All interceptors will be executed









