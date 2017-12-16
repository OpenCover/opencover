// 
namespace Moq
{
    namespace AutoMocking
    {
        using System;
        using System.Collections.Generic;
        using System.Diagnostics;
        using System.Linq;
        using System.Reflection;
        using Unity;
        using Unity.Builder;
        using Unity.Builder.Strategy;
        using Unity.Container;
        using Unity.Extension;
        using Unity.Lifetime;
        using Unity.ObjectBuilder.BuildPlan.Selection;

        /// <summary>
        /// AutoMocking container that leverages the Unity IOC container and the Moq 
        /// mocking library to automatically mock classes resolved from the container.
        /// </summary>
        /// <remarks>
        /// taken from
        /// http://code.google.com/p/moq-contrib/source/browse/trunk/Source.Silverlight/Moq.Contrib.UnityAutoMocker.Silverlight/UnityAutoMockContainer.cs
        /// and updated to Enterprise Library 5
        /// </remarks>
        public class UnityAutoMockContainer
        {
            /// <summary>
            /// Value used when the internal Unity plugin needs to decide if an instance 
            /// class is to be created as a Mock(of T) or not.
            /// </summary>
            internal const string NameForMocking = "____FOR____MOCKING____57ebd55f-9831-40c7-9a24-b7d450209ad0";

            private readonly IAutoMockerBackingContainer _container;

            /// <summary>
            /// Same as calling <code>new UnityAutoMockContainer(new MockFactory(MockBehavior.Loose))</code>
            /// </summary>
            public UnityAutoMockContainer()
                : this(new MockRepository(MockBehavior.Loose))
            {
            }

            /// <summary>
            /// Allows you to specify the MockFactory that will be used when creating mocked items.
            /// </summary>
            public UnityAutoMockContainer(MockRepository factory)
            {
                _container = new UnityAutoMockerBackingContainer(factory);
            }

            #region Public interface

            /// <summary>
            /// This is just a pass through to the underlying Unity Container. It will
            /// register the instance with the ContainerControlledLifetimeManager (Singleton)
            /// </summary>
            public UnityAutoMockContainer RegisterInstance<TService>(TService instance)
            {
                _container.RegisterInstance(instance);
                return this;
            }

            /// <summary>
            /// This is just a pass through to the underlying Unity Container. It will
            /// register the type with the ContainerControlledLifetimeManager (Singleton)
            /// </summary>
            public UnityAutoMockContainer Register<TService, TImplementation>()
            where TImplementation : TService
            {
                _container.RegisterType<TService, TImplementation>();
                return this;
            }

            /// <summary>
            /// This will create a Mock(of T) for any Interface or Class requested.
            /// </summary>
            /// <remarks>Note: that the Mock returned will live as a Singleton, so if you setup any expectations on the Mock(of T) then they will life for the lifetime of this container.</remarks>
            /// <typeparam name="T">Interface or Class that to create a Mock(of T) for.</typeparam>
            /// <returns>Mocked instance of the type T.</returns>
            public Mock<T> GetMock<T>()
            where T : class
            {
                return _container.ResolveForMocking<T>().Mock;
            }

            /// <summary>
            /// This will resolve an interface or class from the underlying container.
            /// </summary>
            /// <remarks>
            /// 1. If T is an interface it will return the Mock(of T).Object instance
            /// 2. If T is a class it will just return that class
            ///     - unless the class was first created by using the GetMock(of T) in which case it will return a Mocked instance of the class
            /// </remarks>
            public T Resolve<T>()
            {
                return _container.Resolve<T>();
            }
            #endregion

            interface IAutoMockerBackingContainer
            {
                void RegisterInstance<TService>(TService instance);
                void RegisterType<TService, TImplementation>() where TImplementation : TService;
                T Resolve<T>();
                object Resolve(Type type);
                IMocked<T> ResolveForMocking<T>() where T : class;
            }

            private class UnityAutoMockerBackingContainer : IAutoMockerBackingContainer
            {
                private readonly IUnityContainer _unityContainer = new UnityContainer();

                public UnityAutoMockerBackingContainer(MockRepository factory)
                {
                    _unityContainer.AddExtension(new MockFactoryContainerExtension(factory, this));
                }

                public void RegisterInstance<TService>(TService instance)
                {
                    _unityContainer.RegisterInstance(instance, new ContainerControlledLifetimeManager());
                }

                public void RegisterType<TService, TImplementation>()
                where TImplementation : TService
                {
                    _unityContainer.RegisterType<TService, TImplementation>(new ContainerControlledLifetimeManager());
                }

                public T Resolve<T>()
                {
                    return _unityContainer.Resolve<T>();
                }

                public object Resolve(Type type)
                {
                    return _unityContainer.Resolve(type);
                }

                public IMocked<T> ResolveForMocking<T>()
                where T : class
                {
                    return (IMocked<T>)_unityContainer.Resolve<T>(NameForMocking);
                }

                private class MockFactoryContainerExtension : UnityContainerExtension
                {
                    private readonly MockRepository _mockFactory;
                    private readonly IAutoMockerBackingContainer _container;

                    public MockFactoryContainerExtension(MockRepository mockFactory, IAutoMockerBackingContainer container)
                    {
                        _mockFactory = mockFactory;
                        _container = container;
                    }

                    protected override void Initialize()
                    {
                        Context.Strategies.Add(new MockExtensibilityStrategy(_mockFactory, _container), UnityBuildStage.PreCreation);
                    }
                }

                private class MockExtensibilityStrategy : BuilderStrategy
                {
                    private readonly MockRepository _factory;
                    private readonly IAutoMockerBackingContainer _container;
                    private readonly MethodInfo _createMethod;
                    private readonly Dictionary<Type, Mock> _alreadyCreatedMocks = new Dictionary<Type, Mock>();
                    private MethodInfo _createMethodWithParameters;

                    public MockExtensibilityStrategy(MockRepository factory, IAutoMockerBackingContainer container)
                    {
                        _factory = factory;
                        _container = container;
                        _createMethod = factory.GetType().GetMethod("Create", new Type[] { });
                        Debug.Assert(_createMethod != null);
                    }

                    public override void PreBuildUp(IBuilderContext context)
                    {
                        var buildKey = context.BuildKey;
                        bool isToBeAMockedClassInstance = buildKey.Name == NameForMocking;
                        Type mockServiceType = buildKey.Type;

                        if (!mockServiceType.IsInterface && !isToBeAMockedClassInstance)
                        {
                            if (_alreadyCreatedMocks.ContainsKey(mockServiceType))
                            {
                                var mockedObject = _alreadyCreatedMocks[mockServiceType];
                                SetBuildObjectAndCompleteIt(context, mockedObject);
                            }
                            else
                            {
                                base.PreBuildUp(context);
                            }
                        }
                        else
                        {
                            Mock mockedObject;

                            if (_alreadyCreatedMocks.ContainsKey(mockServiceType))
                            {
                                mockedObject = _alreadyCreatedMocks[mockServiceType];
                            }
                            else
                            {
                                if (isToBeAMockedClassInstance && !mockServiceType.IsInterface)
                                {
                                    object[] mockedParametersToInject = GetConstructorParameters(context).ToArray();

                                    _createMethodWithParameters = _factory.GetType().GetMethod("Create", new[] { typeof(object[]) });

                                    MethodInfo specificCreateMethod = _createMethodWithParameters.MakeGenericMethod(new[] { mockServiceType });

                                    var x = specificCreateMethod.Invoke(_factory, new object[] { mockedParametersToInject });
                                    mockedObject = (Mock)x;
                                }
                                else
                                {
                                    MethodInfo specificCreateMethod = _createMethod.MakeGenericMethod(new[] { mockServiceType });
                                    mockedObject = (Mock)specificCreateMethod.Invoke(_factory, null);
                                }

                                _alreadyCreatedMocks.Add(mockServiceType, mockedObject);
                            }


                            SetBuildObjectAndCompleteIt(context, mockedObject);
                        }
                    }

                    private static void SetBuildObjectAndCompleteIt(IBuilderContext context, Mock mockedObject)
                    {
                        context.Existing = mockedObject.Object;
                        context.BuildComplete = true;
                    }

                    private List<object> GetConstructorParameters(IBuilderContext context)
                    {
                        var parameters = new List<object>();
                        var policy = new DefaultUnityConstructorSelectorPolicy();
                        var constructor = policy.SelectConstructor(context, new PolicyList());
                        ConstructorInfo constructorInfo;
                        if (constructor == null)
                        {
                            // Unit constructor selector doesn't seem to want to find abstract class protected constructors
                            // quickly find one here...
                            var buildKey = context.BuildKey;
                            var largestConstructor = buildKey.Type.GetConstructors(
                            BindingFlags.Public |
                            BindingFlags.NonPublic |
                            BindingFlags.Instance)
                            .OrderByDescending(o => o.GetParameters().Length)
                            .FirstOrDefault();

                            constructorInfo = largestConstructor;
                        }
                        else
                        {
                            constructorInfo = constructor.Constructor;
                        }

                        foreach (var parameterInfo in constructorInfo.GetParameters())
                            parameters.Add(_container.Resolve(parameterInfo.ParameterType));

                        return parameters;
                    }
                }
            }
        }
    }

    namespace AutoMocking.SelfTesting
    {
        using System;
        using System.Collections.Generic;
        using System.Diagnostics;
        using System.Linq;
        using System.Reflection;
        using System.Text.RegularExpressions;

       
        public class UnityAutoMockContainerFixture
        {
            protected UnityAutoMockContainer GetAutoMockContainer(MockRepository factory)
            {
                return new UnityAutoMockContainer(factory);
            }

            public static void RunAllTests(Action<string> messageWriter)
            {
                var fixture = new UnityAutoMockContainerFixture();
                RunAllTests(fixture, messageWriter);
            }

            public static void RunAllTests(UnityAutoMockContainerFixture fixture, Action<string> messageWriter)
            {
                messageWriter("Starting Tests...");
                foreach (var assertion in fixture.GetAllAssertions)
                {
                    assertion(messageWriter);
                }
                messageWriter("Completed Tests...");
            }

            public IEnumerable<Action<Action<string>>> GetAllAssertions
            {
                get
                {
                    var tests = new List<Action<Action<string>>>();

                    Func<string, string> putSpacesBetweenPascalCasedWords = (s) =>
                    {
                        var r = new Regex("([A-Z]+[a-z]+)");
                        return r.Replace(s, m => (m.Value.Length > 3 ? m.Value : m.Value.ToLower()) + " ");
                    };

                    var methodInfos = this
                    .GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(w => w.GetCustomAttributes(typeof(TestAttribute), true).Any())
                    .OrderBy(ob => ob.Name);

                    foreach (var methodInfo in methodInfos)
                    {
                        MethodInfo info = methodInfo;
                        Action<Action<string>> a = messageWriter =>
                        {
                            messageWriter("Testing - " + putSpacesBetweenPascalCasedWords(info.Name));
                            info.Invoke(this, new object[0]);
                        };

                        tests.Add(a);
                    }

                    foreach (var action in tests)
                        yield return action;
                }
            }

            [Test]
            public void CreatesLooseMocksIfFactoryIsMockBehaviorLoose()
            {
                var factory = GetAutoMockContainer(new MockRepository(MockBehavior.Loose));
                var component = factory.Resolve<TestComponent>();

                component.RunAll();
            }

            [Test]
            public void CanRegisterImplementationAndResolveIt()
            {
                var factory = GetAutoMockContainer(new MockRepository(MockBehavior.Loose));
                factory.Register<ITestComponent, TestComponent>();

                var testComponent = factory.Resolve<ITestComponent>();

                Assert.IsNotNull(testComponent);
                Assert.IsFalse(testComponent is IMocked<ITestComponent>);
            }

            [Test]
            public void ResolveUnregisteredInterfaceReturnsMock()
            {
                var factory = GetAutoMockContainer(new MockRepository(MockBehavior.Loose));

                var service = factory.Resolve<IServiceA>();

                Assert.IsNotNull(service);
                Assert.IsTrue(service is IMocked<IServiceA>);
            }

            [Test]
            public void DefaultConstructorWorksWithAllTests()
            {
                var factory = GetAutoMockContainer(new MockRepository(MockBehavior.Loose));
                var a = false;
                var b = false;

                factory.GetMock<IServiceA>().Setup(x => x.RunA()).Callback(() => a = true);
                factory.GetMock<IServiceB>().Setup(x => x.RunB()).Callback(() => b = true);

                var component = factory.Resolve<TestComponent>();

                component.RunAll();

                Assert.IsTrue(a);
                Assert.IsTrue(b);
            }


            [Test]
            public void ThrowsIfStrictMockWithoutExpectation()
            {
                var factory = GetAutoMockContainer(new MockRepository(MockBehavior.Strict));
                factory.GetMock<IServiceB>().Setup(x => x.RunB());

                var component = factory.Resolve<TestComponent>();
                Assert.ShouldThrow(typeof(MockException), component.RunAll);

            }


            [Test]
            public void StrictWorksWithAllExpectationsMet()
            {
                var factory = GetAutoMockContainer(new MockRepository(MockBehavior.Strict));
                factory.GetMock<IServiceA>().Setup(x => x.RunA());
                factory.GetMock<IServiceB>().Setup(x => x.RunB());

                var component = factory.Resolve<TestComponent>();
                component.RunAll();
            }

            [Test]
            public void GetMockedInstanceOfConcreteClass()
            {
                var factory = GetAutoMockContainer(new MockRepository(MockBehavior.Loose));
                var mockedInstance = factory.GetMock<TestComponent>();

                Assert.IsNotNull(mockedInstance);
                Assert.IsNotNull(mockedInstance.Object.ServiceA);
                Assert.IsNotNull(mockedInstance.Object.ServiceB);
            }

            [Test]
            public void GetMockedInstanceOfConcreteClassWithInterfaceConstructorParameter()
            {
                var factory = GetAutoMockContainer(new MockRepository(MockBehavior.Loose));
                var mockedInstance = factory.GetMock<TestComponent>();
                Assert.IsNotNull(mockedInstance);
            }

            [Test]
            public void WhenMockedInstanceIsRetrievedAnyFutureResolvesOfTheSameConcreteClassShouldReturnedTheMockedInstance()
            {
                var factory = GetAutoMockContainer(new MockRepository(MockBehavior.Loose));
                var mockedInstance = factory.GetMock<TestComponent>();

                var resolvedInstance = factory.Resolve<TestComponent>();

                Assert.IsTrue(Object.ReferenceEquals(resolvedInstance, mockedInstance.Object));
            }

            [Test]
            public void ShouldBeAbleToGetMockedInstanceOfAbstractClass()
            {
                var factory = new UnityAutoMockContainer();
                var mock = factory.GetMock<AbstractTestComponent>();
                Assert.IsNotNull(mock);
            }

            public interface IServiceA
            {
                void RunA();
            }

            public interface IServiceB
            {
                void RunB();
            }

            public class ServiceA : IServiceA
            {
                public ServiceA()
                {
                }

                public ServiceA(int count)
                {
                    Count = count;
                }

                public ServiceA(IServiceB b)
                {
                    ServiceB = b;
                }

                public IServiceB ServiceB { get; private set; }
                public int Count { get; private set; }

                public string Value { get; set; }

                public void RunA() { }
            }


            public interface ITestComponent
            {
                void RunAll();
                IServiceA ServiceA { get; }
                IServiceB ServiceB { get; }
            }

            public abstract class AbstractTestComponent
            {
                private readonly IServiceA _serviceA;
                private readonly IServiceB _serviceB;

                protected AbstractTestComponent(IServiceA serviceA, IServiceB serviceB)
                {
                    _serviceA = serviceA;
                    _serviceB = serviceB;
                }

                public abstract void RunAll();
                public IServiceA ServiceA { get { return _serviceA; } }
                public IServiceB ServiceB { get { return _serviceB; } }
            }

            public class TestComponent : ITestComponent
            {
                public TestComponent(IServiceA serviceA, IServiceB serviceB)
                {
                    ServiceA = serviceA;
                    ServiceB = serviceB;
                }

                public IServiceA ServiceA { get; private set; }
                public IServiceB ServiceB { get; private set; }

                public void RunAll()
                {
                    ServiceA.RunA();
                    ServiceB.RunB();
                }
            }
        }

        [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
        public class TestAttribute : Attribute { }

        internal static class Assert
        {
            public static void IsNotNull(object component)
            {
                Debug.Assert(component != null);
            }

            private static void IsNotNull(object component, string message)
            {
                Debug.Assert(component != null, message);
            }

            public static void IsFalse(bool condition)
            {
                Debug.Assert(condition == false);
            }

            public static void IsTrue(bool condition)
            {
                Debug.Assert(condition);
            }

            public static void ShouldThrow(Type exceptionType, Action method)
            {
                Exception exception = GetException(method);

                IsNotNull(exception, string.Format("Exception of type[{0}] was not thrown.", exceptionType.FullName));
                Debug.Assert(exceptionType == exception.GetType());
            }

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
            private static Exception GetException(Action method)
            {
                Exception exception = null;

                try
                {
                    method();
                }
                catch (Exception e)
                {
                    exception = e;
                }

                return exception;
            }
        }
    }
}