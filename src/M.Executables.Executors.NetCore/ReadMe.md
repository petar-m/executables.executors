## Executables.Executors.NetCoreExecutor  

[![NuGet](https://img.shields.io/nuget/v/M.Executables.Executors.NetCore.svg)](https://www.nuget.org/packages/M.Executables.Executors.NetCore)

An implementation of IExecutor and IExecutorAsync ([M.Executables](https://github.com/petar-m/executables)) utilizing .NET Core dependecy injection.  

### Changes  

#### 2.0  

- **(breaking)** `IServiceCollection` extension methods `AddScopedNetCoreExecutor ` and `AddTransientExecutables` are removed. Executor and executables registration is straight-forward and these methods were not covering all possible scenarios.  
- **(breaking)** Constructor dependency on `IHttpContextAccessor` is removed. Now every executable is resolved and executed in new scope. This makes the executor ignorant of hosting environment. This also removes scoped dependencies which can create hidden coupling of executables.