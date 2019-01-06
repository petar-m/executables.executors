## Executables.Executors.NetCoreExecutor  

[![NuGet](https://img.shields.io/nuget/v/M.Executables.Executors.NetCore.svg)](https://www.nuget.org/packages/M.Executables.Executors.NetCore)

An implementation of IExecutor and IExecutorAsync ([M.Executables](https://github.com/petar-m/executables))  utilizing .NET Core dependecy injection.  

  
If `NetCoreExecutor` is used in ASP.NET Core context it will use `IHttpContextAccessor.HttpContext.RequestServices` to resolve executables.  
Otherwise it will use dedicated scope from `IServiceScopeFactory` for each call to Execute/ExecuteAsync.  