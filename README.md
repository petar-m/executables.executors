## Executables.Executors.Simple  

[![NuGet](https://img.shields.io/nuget/v/M.Executables.Executors.Simple.svg)](https://www.nuget.org/packages/M.Executables.Executors.Simple)

A very simple implementation of IExecutor and IExecutorAsync ([M.Executables](https://github.com/petar-m/executables)).  
  
This is not a "real world" implementation, rather a starting point for something useful.  

## Executables.Executors.SimpleInjector  
  
[![NuGet](https://img.shields.io/nuget/v/M.Executables.Executors.SimpleInjector.svg)](https://www.nuget.org/packages/M.Executables.Executors.SimpleInjector)

An implementation of IExecutor and IExecutorAsync ([M.Executables](https://github.com/petar-m/executables)) utilizing [SimpleInjector](https://simpleinjector.org) as IoC container.  
  
Additional interfaces:  

`IScopeEndHandler` - an action can be plugged in when scope ends, e.g. UnitOfWork.Commit/Rollback  
  
`IErrorHandler` - an action the executor will call on exception, e.g. logging.  
  
*NOTE: SimpleInjectorExecutor propagates the exception after calling IErrorHandler.Handle leaving the environment to decide what to do.*
  
`IExecutorScope` - **obsolete** 

`IScopedContext` - an alternative scope provider when the environment does not provide a way to start/end scope, e.g. service executing recurring tasks in background.  

Example:  

error handler:

    public class ExecutorErrorHandler : IErrorHandler
    {
        // called from the executor
        public void Handle(Exception exception, Scope scope)
        {
            // log...

            // indicate there was error
            scope.SetItem("HasError", true);

            // force IScopeEndHandler to be called
            scope.Dispose();
        }
    }  
  
scope end handler:   

    public class ScopeEndHandler : IScopeEndHandler
    {
        public void Handle(Scope scope)
        {
            object hasErrorItem = scope.GetItem("HasError");
            var hasError = hasErrorItem == null ? false : (bool)hasErrorItem;
            if (hasError)
            {
                // clean up
            }
            else
            {
                // go on with the happy path
            }
        }
    }  

setup:  

	container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
    
    container.Register<IExecutor, SimpleInjectorExecutor>(Lifestyle.Scoped);
    container.Register<IExecutorAsync, SimpleInjectorExecutor>(Lifestyle.Scoped);	
	
	container.Register<IErrorHandler, ExecutorErrorHandler>(Lifestyle.Singleton);
	container.Register<IScopeEndHandler, ScopeEndHandler>(Lifestyle.Scoped);
  
	container.Register<IScopedContext, ScopedContext>(Lifestyle.Singleton);  

usage:

	// with OWIN pipeline
    appBuilder.Use(async (context, next) =>
    {
        using (var scope = AsyncScopedLifestyle.BeginScope(container))
        {
            scope.WhenScopeEnds(() => scope.Container.GetInstance<IScopeEndHandler>().Handle(scope));
            await next();
        }
    });
	
	// controller example	
	public class SomeController : ApiController
    {
        private readonly IExecutorAsync executor;

        public SomeController(IExecutorAsync executor)
        {
            this.executor = executor;
        }

        [HttpGet]
        public async Task<IHttpActionResult> Something()
        {
            var result = await executor.ExecuteAsync<SomeExecutable, Result>();
            return Ok(result);
        }
    }
	
	...

	// outside of OWIN pipeline
	// ScopedContext will create new scope with IScopeEndHandler set, 
	// execute the action passing arguments resolved from the scope and dispose the scope
    var scope = container.GetInstance<IScopedContext>());
	scope.Execute<IExecutor>(executor => executor.Execute<SomeExecutable>());
	
      


 

 
  