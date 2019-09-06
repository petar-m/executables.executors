using System;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace M.Executables.Executors.NetCore.UnitTests
{
    public class NetCoreExecutorTests
    {
        [Fact]
        public void Execute_IsCalledOnVoidExecutable()
        {
            var executable = A.Fake<IExecutableVoid>();
            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutor>();

            executor.Execute<IExecutableVoid>();

            A.CallTo(() => executable.Execute()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Execute_IsCalledOnVoidExecutableWithParameter()
        {
            var executable = A.Fake<IExecutableVoid<string>>();
            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutor>();

            executor.Execute<IExecutableVoid<string>, string>("parameter hello");

            A.CallTo(() => executable.Execute("parameter hello")).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public void Execute_IsCalledOnExecutable()
        {
            var executable = A.Fake<IExecutable<string>>();
            A.CallTo(() => executable.Execute()).Returns("return value hello");
            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutor>();

            var result = executor.Execute<IExecutable<string>, string>();

            A.CallTo(() => executable.Execute()).MustHaveHappenedOnceExactly();
            Assert.Equal("return value hello", result);
        }

        [Fact]
        public void Execute_IsCalledOnExecutableWithParameter()
        {
            var executable = A.Fake<IExecutable<string, string>>();
            A.CallTo(() => executable.Execute("parameter hello")).Returns("return value hello");
            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutor>();

            var result = executor.Execute<IExecutable<string, string>, string, string>("parameter hello");

            A.CallTo(() => executable.Execute("parameter hello")).MustHaveHappenedOnceExactly();
            Assert.Equal("return value hello", result);
        }

        [Fact]
        public void Execute_InterceptorsAreCalledInOrder()
        {
            var executable = A.Fake<IExecutable<string, string>>();
            A.CallTo(() => executable.Execute("parameter hello")).Returns("return value hello");

            var generalInterceptor1 = A.Fake<IExecutionInterceptor>();
            A.CallTo(() => generalInterceptor1.OrderingIndex).Returns(1);

            var generalInterceptor2 = A.Fake<IExecutionInterceptor>();
            A.CallTo(() => generalInterceptor2.OrderingIndex).Returns(3);

            var specificInterceptor = A.Fake<IExecutionInterceptor<IExecutable<string, string>, string, string>>();
            A.CallTo(() => specificInterceptor.OrderingIndex).Returns(2);

            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).AddSpecificInterceptors(specificInterceptor).AddGeneralInterceptors(generalInterceptor1, generalInterceptor2).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutor>();

            var result = executor.Execute<IExecutable<string, string>, string, string>("parameter hello");

            Assert.Equal("return value hello", result);
            // interceptors called in ascending OrderIndex order
            A.CallTo(() => generalInterceptor1.Before(executable, "parameter hello")).MustHaveHappenedOnceExactly()
             .Then(A.CallTo(() => specificInterceptor.Before(executable, "parameter hello")).MustHaveHappenedOnceExactly())
             .Then(A.CallTo(() => generalInterceptor2.Before(executable, "parameter hello")).MustHaveHappenedOnceExactly())
             // executable called
             .Then(A.CallTo(() => executable.Execute("parameter hello")).MustHaveHappenedOnceExactly())
             // interceptors called in descending OrderIndex order
             .Then(A.CallTo(() => generalInterceptor2.After(executable, "parameter hello", "return value hello", null)).MustHaveHappenedOnceExactly())
             .Then(A.CallTo(() => specificInterceptor.After(executable, "parameter hello", "return value hello", null)).MustHaveHappenedOnceExactly())
             .Then(A.CallTo(() => generalInterceptor1.After(executable, "parameter hello", "return value hello", null)).MustHaveHappenedOnceExactly());
        }

        [Fact]
        public void Execute_ExceptionIsPassedToInterceptors()
        {
            var executable = A.Fake<IExecutable<string, string>>();
            var exception = new InvalidOperationException();
            A.CallTo(() => executable.Execute("parameter hello")).Throws(exception);

            var generalInterceptor1 = A.Fake<IExecutionInterceptor>();
            A.CallTo(() => generalInterceptor1.OrderingIndex).Returns(1);

            var generalInterceptor2 = A.Fake<IExecutionInterceptor>();
            A.CallTo(() => generalInterceptor2.OrderingIndex).Returns(3);

            var specificInterceptor = A.Fake<IExecutionInterceptor<IExecutable<string, string>, string, string>>();
            A.CallTo(() => specificInterceptor.OrderingIndex).Returns(2);

            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).AddSpecificInterceptors(specificInterceptor).AddGeneralInterceptors(generalInterceptor1, generalInterceptor2).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutor>();

            _ = Assert.Throws(exception.GetType(), () => executor.Execute<IExecutable<string, string>, string, string>("parameter hello"));

            // interceptors called in ascending OrderIndex order
            _ = A.CallTo(() => generalInterceptor1.Before(executable, "parameter hello")).MustHaveHappenedOnceExactly()
                 .Then(A.CallTo(() => specificInterceptor.Before(executable, "parameter hello")).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => generalInterceptor2.Before(executable, "parameter hello")).MustHaveHappenedOnceExactly())
                 // executable called
                 .Then(A.CallTo(() => executable.Execute("parameter hello")).MustHaveHappenedOnceExactly())
                 // interceptors called in descending OrderIndex order
                 .Then(A.CallTo(() => generalInterceptor2.After(executable, "parameter hello", default(string), exception)).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => specificInterceptor.After(executable, "parameter hello", default(string), exception)).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => generalInterceptor1.After(executable, "parameter hello", default(string), exception)).MustHaveHappenedOnceExactly());
        }

        [Fact]
        public void Execute_InterceptorsGetIEmptyWhenNoParameterOrReturnValue()
        {
            var executable = A.Fake<IExecutableVoid>();

            var generalInterceptor1 = A.Fake<IExecutionInterceptor>();
            A.CallTo(() => generalInterceptor1.OrderingIndex).Returns(1);

            var generalInterceptor2 = A.Fake<IExecutionInterceptor>();
            A.CallTo(() => generalInterceptor2.OrderingIndex).Returns(3);

            var specificInterceptor = A.Fake<IExecutionInterceptor<IExecutableVoid, IEmpty, IEmpty>>();
            A.CallTo(() => specificInterceptor.OrderingIndex).Returns(2);

            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).AddSpecificInterceptors(specificInterceptor).AddGeneralInterceptors(generalInterceptor1, generalInterceptor2).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutor>();

            executor.Execute<IExecutableVoid>();

            // interceptors called in ascending OrderIndex order
            _ = A.CallTo(() => generalInterceptor1.Before(executable, default(IEmpty))).MustHaveHappenedOnceExactly()
                 .Then(A.CallTo(() => specificInterceptor.Before(executable, default(IEmpty))).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => generalInterceptor2.Before(executable, default(IEmpty))).MustHaveHappenedOnceExactly())
                 // executable called
                 .Then(A.CallTo(() => executable.Execute()).MustHaveHappenedOnceExactly())
                 // interceptors called in descending OrderIndex order
                 .Then(A.CallTo(() => generalInterceptor2.After(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => specificInterceptor.After(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => generalInterceptor1.After(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly());
        }

        [Fact]
        public void Execute_InterceptorsImplementIDiscardOtherInteceptors_TheOneWithSmallestOrderIndexIsCalled()
        {
            var executable = A.Fake<IExecutableVoid>();

            var generalInterceptor1 = A.Fake<IExecutionInterceptor>(x => x.Implements<IDiscardOtherInterceptors>());
            A.CallTo(() => generalInterceptor1.OrderingIndex).Returns(1);

            var generalInterceptor2 = A.Fake<IExecutionInterceptor>(x => x.Implements<IDiscardOtherInterceptors>());
            A.CallTo(() => generalInterceptor2.OrderingIndex).Returns(3);

            var specificInterceptor = A.Fake<IExecutionInterceptor<IExecutableVoid, IEmpty, IEmpty>>(x => x.Implements<IDiscardOtherInterceptors>());
            A.CallTo(() => specificInterceptor.OrderingIndex).Returns(2);

            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).AddSpecificInterceptors(specificInterceptor).AddGeneralInterceptors(generalInterceptor1, generalInterceptor2).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutor>();

            executor.Execute<IExecutableVoid>();

            _ = A.CallTo(() => generalInterceptor1.Before(executable, default(IEmpty))).MustHaveHappenedOnceExactly()
                 .Then(A.CallTo(() => executable.Execute()).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => generalInterceptor1.After(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly());

            A.CallTo(() => specificInterceptor.Before(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor2.Before(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor2.After(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
            A.CallTo(() => specificInterceptor.After(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
        }

        [Fact]
        public void Execute_InterceptorsImplementIDiscardOtherInteceptorsAndIDiscardNonGenericInterceptors_TheIDiscardNonGenericInterceptorsIsCalled()
        {
            var executable = A.Fake<IExecutableVoid>();

            var generalInterceptor1 = A.Fake<IExecutionInterceptor>(x => x.Implements<IDiscardOtherInterceptors>());
            A.CallTo(() => generalInterceptor1.OrderingIndex).Returns(1);

            var generalInterceptor2 = A.Fake<IExecutionInterceptor>(x => x.Implements<IDiscardOtherInterceptors>());
            A.CallTo(() => generalInterceptor2.OrderingIndex).Returns(3);

            var specificInterceptor = A.Fake<IExecutionInterceptor<IExecutableVoid, IEmpty, IEmpty>>(x => x.Implements<IDiscardOtherInterceptors>().Implements<IDiscardNonGenericInterceptors>());
            A.CallTo(() => specificInterceptor.OrderingIndex).Returns(2);

            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).AddSpecificInterceptors(specificInterceptor).AddGeneralInterceptors(generalInterceptor1, generalInterceptor2).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutor>();

            executor.Execute<IExecutableVoid>();

            _ = A.CallTo(() => specificInterceptor.Before(executable, default(IEmpty))).MustHaveHappenedOnceExactly()
                 .Then(A.CallTo(() => executable.Execute()).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => specificInterceptor.After(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly());

            A.CallTo(() => generalInterceptor1.Before(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor2.Before(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor1.After(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor2.After(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
        }

        [Fact]
        public void Execute_SpecificInterceptorImplementIDiscardOtherInteceptors_TheSpecificInterceptorsIsCalled()
        {
            var executable = A.Fake<IExecutableVoid>();

            var generalInterceptor1 = A.Fake<IExecutionInterceptor>();
            A.CallTo(() => generalInterceptor1.OrderingIndex).Returns(1);

            var generalInterceptor2 = A.Fake<IExecutionInterceptor>();
            A.CallTo(() => generalInterceptor2.OrderingIndex).Returns(3);

            var specificInterceptor = A.Fake<IExecutionInterceptor<IExecutableVoid, IEmpty, IEmpty>>(x => x.Implements<IDiscardOtherInterceptors>());
            A.CallTo(() => specificInterceptor.OrderingIndex).Returns(2);

            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).AddSpecificInterceptors(specificInterceptor).AddGeneralInterceptors(generalInterceptor1, generalInterceptor2).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutor>();

            executor.Execute<IExecutableVoid>();

            _ = A.CallTo(() => specificInterceptor.Before(executable, default(IEmpty))).MustHaveHappenedOnceExactly()
                 .Then(A.CallTo(() => executable.Execute()).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => specificInterceptor.After(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly());

            A.CallTo(() => generalInterceptor1.Before(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor2.Before(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor1.After(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor2.After(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
        }

        [Fact]
        public void Execute_GeneralInterceptorImplementIDiscardOtherInteceptors_TheGeneralInterceptorsIsCalled()
        {
            var executable = A.Fake<IExecutableVoid>();

            var generalInterceptor1 = A.Fake<IExecutionInterceptor>();
            A.CallTo(() => generalInterceptor1.OrderingIndex).Returns(1);

            var generalInterceptor2 = A.Fake<IExecutionInterceptor>(x => x.Implements<IDiscardOtherInterceptors>());
            A.CallTo(() => generalInterceptor2.OrderingIndex).Returns(3);

            var specificInterceptor = A.Fake<IExecutionInterceptor<IExecutableVoid, IEmpty, IEmpty>>();
            A.CallTo(() => specificInterceptor.OrderingIndex).Returns(2);

            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).AddSpecificInterceptors(specificInterceptor).AddGeneralInterceptors(generalInterceptor1, generalInterceptor2).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutor>();

            executor.Execute<IExecutableVoid>();

            _ = A.CallTo(() => generalInterceptor2.Before(executable, default(IEmpty))).MustHaveHappenedOnceExactly()
                 .Then(A.CallTo(() => executable.Execute()).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => generalInterceptor2.After(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly());

            A.CallTo(() => specificInterceptor.Before(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor1.Before(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor1.After(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
            A.CallTo(() => specificInterceptor.After(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
        }

        [Fact]
        public void Execute_SpecificInterceptorImplementsIDiscardNonGenericInterceptors_NonGenericInterceptorsAreNorCalled()
        {
            var executable = A.Fake<IExecutableVoid>();

            var generalInterceptor1 = A.Fake<IExecutionInterceptor>();
            A.CallTo(() => generalInterceptor1.OrderingIndex).Returns(1);

            var generalInterceptor2 = A.Fake<IExecutionInterceptor>();
            A.CallTo(() => generalInterceptor2.OrderingIndex).Returns(3);

            var specificInterceptor = A.Fake<IExecutionInterceptor<IExecutableVoid, IEmpty, IEmpty>>(x => x.Implements<IDiscardNonGenericInterceptors>());
            A.CallTo(() => specificInterceptor.OrderingIndex).Returns(2);

            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).AddSpecificInterceptors(specificInterceptor).AddGeneralInterceptors(generalInterceptor1, generalInterceptor2).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutor>();

            executor.Execute<IExecutableVoid>();

            _ = A.CallTo(() => specificInterceptor.Before(executable, default(IEmpty))).MustHaveHappenedOnceExactly()
                 .Then(A.CallTo(() => executable.Execute()).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => specificInterceptor.After(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly());

            A.CallTo(() => generalInterceptor1.Before(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor2.Before(executable, default(IEmpty))).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor1.After(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
            A.CallTo(() => generalInterceptor2.After(executable, default(IEmpty), default(IEmpty), null)).MustNotHaveHappened();
        }

        [Fact]
        public void Execute_GeneralInterceptorImplementsIDiscardNonGenericInterceptors_DoesNotAffectGeneralInterceptors()
        {
            var executable = A.Fake<IExecutableVoid>();

            var generalInterceptor1 = A.Fake<IExecutionInterceptor>(x => x.Implements<IDiscardNonGenericInterceptors>());
            A.CallTo(() => generalInterceptor1.OrderingIndex).Returns(1);

            var generalInterceptor2 = A.Fake<IExecutionInterceptor>(x => x.Implements<IDiscardNonGenericInterceptors>());
            A.CallTo(() => generalInterceptor2.OrderingIndex).Returns(3);

            var specificInterceptor = A.Fake<IExecutionInterceptor<IExecutableVoid, IEmpty, IEmpty>>();
            A.CallTo(() => specificInterceptor.OrderingIndex).Returns(2);

            ServiceProvider serviceProvider = new ServiceCollection().AddExecutor().AddExecutable(executable).AddSpecificInterceptors(specificInterceptor).AddGeneralInterceptors(generalInterceptor1, generalInterceptor2).BuildServiceProvider();
            var executor = serviceProvider.GetRequiredService<IExecutor>();

            executor.Execute<IExecutableVoid>();

            // interceptors called in ascending OrderIndex order
            _ = A.CallTo(() => generalInterceptor1.Before(executable, default(IEmpty))).MustHaveHappenedOnceExactly()
                 .Then(A.CallTo(() => specificInterceptor.Before(executable, default(IEmpty))).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => generalInterceptor2.Before(executable, default(IEmpty))).MustHaveHappenedOnceExactly())
                 // executable called
                 .Then(A.CallTo(() => executable.Execute()).MustHaveHappenedOnceExactly())
                 // interceptors called in descending OrderIndex order
                 .Then(A.CallTo(() => generalInterceptor2.After(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => specificInterceptor.After(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly())
                 .Then(A.CallTo(() => generalInterceptor1.After(executable, default(IEmpty), default(IEmpty), null)).MustHaveHappenedOnceExactly());
        }
    }
}
