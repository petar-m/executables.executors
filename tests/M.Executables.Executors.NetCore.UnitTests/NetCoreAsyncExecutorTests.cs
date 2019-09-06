using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace M.Executables.Executors.NetCore.UnitTests
{
    public class NetCoreExecutorAsyncTests
    {
        [Fact]
        public async Task ExecuteAsync_IsCalledOnVoidExecutable()
        {
            var executable = A.Fake<IExecutableVoidAsync>();
            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutorAsync>();

            await executor.ExecuteAsync<IExecutableVoidAsync>().ConfigureAwait(false);

            A.CallTo(() => executable.ExecuteAsync()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task ExecuteAsync_IsCalledOnVoidExecutableWithParameter()
        {
            var executable = A.Fake<IExecutableVoidAsync<string>>();
            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutorAsync>();

            await executor.ExecuteAsync<IExecutableVoidAsync<string>, string>("parameter hello").ConfigureAwait(false);

            A.CallTo(() => executable.ExecuteAsync("parameter hello")).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task ExecuteAsync_IsCalledOnExecutable()
        {
            var executable = A.Fake<IExecutableAsync<string>>();
            A.CallTo(() => executable.ExecuteAsync()).Returns("return value hello");
            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutorAsync>();

            var result = await executor.ExecuteAsync<IExecutableAsync<string>, string>().ConfigureAwait(false);

            A.CallTo(() => executable.ExecuteAsync()).MustHaveHappenedOnceExactly();
            Assert.Equal("return value hello", result);
        }

        [Fact]
        public async Task ExecuteAsync_IsCalledOnExecutableWithParameter()
        {
            var executable = A.Fake<IExecutableAsync<string, string>>();
            A.CallTo(() => executable.ExecuteAsync("parameter hello")).Returns("return value hello");
            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutorAsync>();

            var result = await executor.ExecuteAsync<IExecutableAsync<string, string>, string, string>("parameter hello").ConfigureAwait(false);

            A.CallTo(() => executable.ExecuteAsync("parameter hello")).MustHaveHappenedOnceExactly();
            Assert.Equal("return value hello", result);
        }

        [Fact]
        public async Task ExecuteAsync_InterceptorsAreCalledInOrder()
        {
            var executable = A.Fake<IExecutableAsync<string, string>>();
            A.CallTo(() => executable.ExecuteAsync("parameter hello")).Returns("return value hello");

            var generalInterceptor1 = A.Fake<IExecutionInterceptorAsync>();
            A.CallTo(() => generalInterceptor1.OrderingIndex).Returns(1);

            var generalInterceptor2 = A.Fake<IExecutionInterceptorAsync>();
            A.CallTo(() => generalInterceptor2.OrderingIndex).Returns(3);

            var specificInterceptor = A.Fake<IExecutionInterceptorAsync<IExecutableAsync<string, string>, string, string>>();
            A.CallTo(() => specificInterceptor.OrderingIndex).Returns(2);

            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).AddSpecificInterceptors(specificInterceptor).AddGeneralInterceptors(generalInterceptor1, generalInterceptor2).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutorAsync>();

            var result = await executor.ExecuteAsync<IExecutableAsync<string, string>, string, string>("parameter hello").ConfigureAwait(false);

            Assert.Equal("return value hello", result);
            // interceptors called in ascending OrderIndex order
            A.CallTo(() => generalInterceptor1.BeforeAsync(executable, "parameter hello")).MustHaveHappenedOnceExactly()
             .Then(A.CallTo(() => specificInterceptor.BeforeAsync(executable, "parameter hello")).MustHaveHappenedOnceExactly())
             .Then(A.CallTo(() => generalInterceptor2.BeforeAsync(executable, "parameter hello")).MustHaveHappenedOnceExactly())
             // executable called
             .Then(A.CallTo(() => executable.ExecuteAsync("parameter hello")).MustHaveHappenedOnceExactly())
             // interceptors called in descending OrderIndex order
             .Then(A.CallTo(() => generalInterceptor2.AfterAsync(executable, "parameter hello", "return value hello", null)).MustHaveHappenedOnceExactly())
             .Then(A.CallTo(() => specificInterceptor.AfterAsync(executable, "parameter hello", "return value hello", null)).MustHaveHappenedOnceExactly())
             .Then(A.CallTo(() => generalInterceptor1.AfterAsync(executable, "parameter hello", "return value hello", null)).MustHaveHappenedOnceExactly());
        }

        [Fact]
        public async Task ExecuteAsync_ExceptionIsPassedToInterceptors()
        {
            var executable = A.Fake<IExecutableAsync<string, string>>();
            var exception = new InvalidOperationException();
            A.CallTo(() => executable.ExecuteAsync("parameter hello")).ThrowsAsync(exception);

            var generalInterceptor1 = A.Fake<IExecutionInterceptorAsync>();
            A.CallTo(() => generalInterceptor1.OrderingIndex).Returns(1);

            var generalInterceptor2 = A.Fake<IExecutionInterceptorAsync>();
            A.CallTo(() => generalInterceptor2.OrderingIndex).Returns(3);

            var specificInterceptor = A.Fake<IExecutionInterceptorAsync<IExecutableAsync<string, string>, string, string>>();
            A.CallTo(() => specificInterceptor.OrderingIndex).Returns(2);

            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).AddSpecificInterceptors(specificInterceptor).AddGeneralInterceptors(generalInterceptor1, generalInterceptor2).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutorAsync>();

            _ = await Assert.ThrowsAsync(exception.GetType(), () => executor.ExecuteAsync<IExecutableAsync<string, string>, string, string>("parameter hello")).ConfigureAwait(false);

            // interceptors called in ascending OrderIndex order
            _ = A.CallTo(() => generalInterceptor1.BeforeAsync(executable, "parameter hello")).MustHaveHappenedOnceExactly()
                 .Then(A.CallTo(() => specificInterceptor.BeforeAsync(executable, "parameter hello")).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => generalInterceptor2.BeforeAsync(executable, "parameter hello")).MustHaveHappenedOnceExactly())
                 // executable called
                 .Then(A.CallTo(() => executable.ExecuteAsync("parameter hello")).MustHaveHappenedOnceExactly())
                 // interceptors called in descending OrderIndex order
                 .Then(A.CallTo(() => generalInterceptor2.AfterAsync(executable, "parameter hello", default(string), exception)).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => specificInterceptor.AfterAsync(executable, "parameter hello", default(string), exception)).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => generalInterceptor1.AfterAsync(executable, "parameter hello", default(string), exception)).MustHaveHappenedOnceExactly());
        }

        [Fact]
        public async Task ExecuteAsync_InterceptorsGetIEmptyWhenNoParameterOrReturnValue()
        {
            var executable = A.Fake<IExecutableVoidAsync>();

            var generalInterceptor1 = A.Fake<IExecutionInterceptorAsync>();
            A.CallTo(() => generalInterceptor1.OrderingIndex).Returns(1);

            var generalInterceptor2 = A.Fake<IExecutionInterceptorAsync>();
            A.CallTo(() => generalInterceptor2.OrderingIndex).Returns(3);

            var specificInterceptor = A.Fake<IExecutionInterceptorAsync<IExecutableVoidAsync, IEmpty, IEmpty>>();
            A.CallTo(() => specificInterceptor.OrderingIndex).Returns(2);

            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).AddSpecificInterceptors(specificInterceptor).AddGeneralInterceptors(generalInterceptor1, generalInterceptor2).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutorAsync>();

            await executor.ExecuteAsync<IExecutableVoidAsync>().ConfigureAwait(false);

            // interceptors called in ascending OrderIndex order
            _ = A.CallTo(() => generalInterceptor1.BeforeAsync(executable, default(IEmpty))).MustHaveHappenedOnceExactly()
                 .Then(A.CallTo(() => specificInterceptor.BeforeAsync(executable, default(IEmpty))).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => generalInterceptor2.BeforeAsync(executable, default(IEmpty))).MustHaveHappenedOnceExactly())
                 // executable called
                 .Then(A.CallTo(() => executable.ExecuteAsync()).MustHaveHappenedOnceExactly())
                 // interceptors called in descending OrderIndex order
                 .Then(A.CallTo(() => generalInterceptor2.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => specificInterceptor.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => generalInterceptor1.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly());
        }

        [Fact]
        public async Task ExecuteAsync_InterceptorsImplementIDiscardOtherInteceptors_TheOneWithSmallestOrderIndexIsCalled()
        {
            var executable = A.Fake<IExecutableVoidAsync>();

            var generalInterceptor1 = A.Fake<IExecutionInterceptorAsync>(x => x.Implements<IDiscardOtherInterceptors>());
            A.CallTo(() => generalInterceptor1.OrderingIndex).Returns(1);

            var generalInterceptor2 = A.Fake<IExecutionInterceptorAsync>(x => x.Implements<IDiscardOtherInterceptors>());
            A.CallTo(() => generalInterceptor2.OrderingIndex).Returns(3);

            var specificInterceptor = A.Fake<IExecutionInterceptorAsync<IExecutableVoidAsync, IEmpty, IEmpty>>(x => x.Implements<IDiscardOtherInterceptors>());
            A.CallTo(() => specificInterceptor.OrderingIndex).Returns(2);

            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).AddSpecificInterceptors(specificInterceptor).AddGeneralInterceptors(generalInterceptor1, generalInterceptor2).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutorAsync>();

            await executor.ExecuteAsync<IExecutableVoidAsync>().ConfigureAwait(false);

            _ = A.CallTo(() => generalInterceptor1.BeforeAsync(executable, default(IEmpty))).MustHaveHappenedOnceExactly()
                 .Then(A.CallTo(() => executable.ExecuteAsync()).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => generalInterceptor1.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly());

            A.CallTo(() => specificInterceptor.BeforeAsync(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor2.BeforeAsync(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor2.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
            A.CallTo(() => specificInterceptor.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
        }

        [Fact]
        public async Task ExecuteAsync_InterceptorsImplementIDiscardOtherInteceptorsAndIDiscardNonGenericInterceptors_TheIDiscardNonGenericInterceptorsIsCalled()
        {
            var executable = A.Fake<IExecutableVoidAsync>();

            var generalInterceptor1 = A.Fake<IExecutionInterceptorAsync>(x => x.Implements<IDiscardOtherInterceptors>());
            A.CallTo(() => generalInterceptor1.OrderingIndex).Returns(1);

            var generalInterceptor2 = A.Fake<IExecutionInterceptorAsync>(x => x.Implements<IDiscardOtherInterceptors>());
            A.CallTo(() => generalInterceptor2.OrderingIndex).Returns(3);

            var specificInterceptor = A.Fake<IExecutionInterceptorAsync<IExecutableVoidAsync, IEmpty, IEmpty>>(x => x.Implements<IDiscardOtherInterceptors>().Implements<IDiscardNonGenericInterceptors>());
            A.CallTo(() => specificInterceptor.OrderingIndex).Returns(2);

            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).AddSpecificInterceptors(specificInterceptor).AddGeneralInterceptors(generalInterceptor1, generalInterceptor2).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutorAsync>();

            await executor.ExecuteAsync<IExecutableVoidAsync>().ConfigureAwait(false);

            _ = A.CallTo(() => specificInterceptor.BeforeAsync(executable, default(IEmpty))).MustHaveHappenedOnceExactly()
                 .Then(A.CallTo(() => executable.ExecuteAsync()).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => specificInterceptor.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly());

            A.CallTo(() => generalInterceptor1.BeforeAsync(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor2.BeforeAsync(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor1.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor2.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
        }

        [Fact]
        public async Task ExecuteAsync_SpecificInterceptorImplementIDiscardOtherInteceptors_TheSpecificInterceptorsIsCalled()
        {
            var executable = A.Fake<IExecutableVoidAsync>();

            var generalInterceptor1 = A.Fake<IExecutionInterceptorAsync>();
            A.CallTo(() => generalInterceptor1.OrderingIndex).Returns(1);

            var generalInterceptor2 = A.Fake<IExecutionInterceptorAsync>();
            A.CallTo(() => generalInterceptor2.OrderingIndex).Returns(3);

            var specificInterceptor = A.Fake<IExecutionInterceptorAsync<IExecutableVoidAsync, IEmpty, IEmpty>>(x => x.Implements<IDiscardOtherInterceptors>());
            A.CallTo(() => specificInterceptor.OrderingIndex).Returns(2);

            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).AddSpecificInterceptors(specificInterceptor).AddGeneralInterceptors(generalInterceptor1, generalInterceptor2).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutorAsync>();

            await executor.ExecuteAsync<IExecutableVoidAsync>().ConfigureAwait(false);

            _ = A.CallTo(() => specificInterceptor.BeforeAsync(executable, default(IEmpty))).MustHaveHappenedOnceExactly()
                 .Then(A.CallTo(() => executable.ExecuteAsync()).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => specificInterceptor.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly());

            A.CallTo(() => generalInterceptor1.BeforeAsync(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor2.BeforeAsync(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor1.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor2.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
        }

        [Fact]
        public async Task ExecuteAsync_GeneralInterceptorImplementIDiscardOtherInteceptors_TheGeneralInterceptorsIsCalled()
        {
            var executable = A.Fake<IExecutableVoidAsync>();

            var generalInterceptor1 = A.Fake<IExecutionInterceptorAsync>();
            A.CallTo(() => generalInterceptor1.OrderingIndex).Returns(1);

            var generalInterceptor2 = A.Fake<IExecutionInterceptorAsync>(x => x.Implements<IDiscardOtherInterceptors>());
            A.CallTo(() => generalInterceptor2.OrderingIndex).Returns(3);

            var specificInterceptor = A.Fake<IExecutionInterceptorAsync<IExecutableVoidAsync, IEmpty, IEmpty>>();
            A.CallTo(() => specificInterceptor.OrderingIndex).Returns(2);

            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).AddSpecificInterceptors(specificInterceptor).AddGeneralInterceptors(generalInterceptor1, generalInterceptor2).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutorAsync>();

            await executor.ExecuteAsync<IExecutableVoidAsync>().ConfigureAwait(false);

            _ = A.CallTo(() => generalInterceptor2.BeforeAsync(executable, default(IEmpty))).MustHaveHappenedOnceExactly()
                 .Then(A.CallTo(() => executable.ExecuteAsync()).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => generalInterceptor2.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly());

            A.CallTo(() => specificInterceptor.BeforeAsync(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor1.BeforeAsync(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor1.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
            A.CallTo(() => specificInterceptor.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
        }

        [Fact]
        public async Task ExecuteAsync_SpecificInterceptorImplementsIDiscardNonGenericInterceptors_NonGenericInterceptorsAreNorCalled()
        {
            var executable = A.Fake<IExecutableVoidAsync>();

            var generalInterceptor1 = A.Fake<IExecutionInterceptorAsync>();
            A.CallTo(() => generalInterceptor1.OrderingIndex).Returns(1);

            var generalInterceptor2 = A.Fake<IExecutionInterceptorAsync>();
            A.CallTo(() => generalInterceptor2.OrderingIndex).Returns(3);

            var specificInterceptor = A.Fake<IExecutionInterceptorAsync<IExecutableVoidAsync, IEmpty, IEmpty>>(x => x.Implements<IDiscardNonGenericInterceptors>());
            A.CallTo(() => specificInterceptor.OrderingIndex).Returns(2);

            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).AddSpecificInterceptors(specificInterceptor).AddGeneralInterceptors(generalInterceptor1, generalInterceptor2).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutorAsync>();

            await executor.ExecuteAsync<IExecutableVoidAsync>().ConfigureAwait(false);

            _ = A.CallTo(() => specificInterceptor.BeforeAsync(executable, default(IEmpty))).MustHaveHappenedOnceExactly()
                 .Then(A.CallTo(() => executable.ExecuteAsync()).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => specificInterceptor.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly());

            A.CallTo(() => generalInterceptor1.BeforeAsync(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor2.BeforeAsync(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor1.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor2.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
        }

        [Fact]
        public async Task ExecuteAsync_GeneralInterceptorImplementsIDiscardNonGenericInterceptors_DoesNotAffectGeneralInterceptors()
        {
            var executable = A.Fake<IExecutableVoidAsync>();

            var generalInterceptor1 = A.Fake<IExecutionInterceptorAsync>(x => x.Implements<IDiscardNonGenericInterceptors>());
            A.CallTo(() => generalInterceptor1.OrderingIndex).Returns(1);

            var generalInterceptor2 = A.Fake<IExecutionInterceptorAsync>(x => x.Implements<IDiscardNonGenericInterceptors>());
            A.CallTo(() => generalInterceptor2.OrderingIndex).Returns(3);

            var specificInterceptor = A.Fake<IExecutionInterceptorAsync<IExecutableVoidAsync, IEmpty, IEmpty>>();
            A.CallTo(() => specificInterceptor.OrderingIndex).Returns(2);

            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).AddSpecificInterceptors(specificInterceptor).AddGeneralInterceptors(generalInterceptor1, generalInterceptor2).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutorAsync>();

            await executor.ExecuteAsync<IExecutableVoidAsync>().ConfigureAwait(false);

            // interceptors called in ascending OrderIndex order
            _ = A.CallTo(() => generalInterceptor1.BeforeAsync(executable, default(IEmpty))).MustHaveHappenedOnceExactly()
                 .Then(A.CallTo(() => specificInterceptor.BeforeAsync(executable, default(IEmpty))).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => generalInterceptor2.BeforeAsync(executable, default(IEmpty))).MustHaveHappenedOnceExactly())
                 // executable called
                 .Then(A.CallTo(() => executable.ExecuteAsync()).MustHaveHappenedOnceExactly())
                 // interceptors called in descending OrderIndex order
                 .Then(A.CallTo(() => generalInterceptor2.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => specificInterceptor.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => generalInterceptor1.AfterAsync(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly());
        }
    }
}
