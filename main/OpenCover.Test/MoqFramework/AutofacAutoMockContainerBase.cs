using Autofac;
using Autofac.Extras.Moq;
using Moq;
using NUnit.Framework;

namespace OpenCover.Test.MoqFramework
{
    public abstract class AutofacAutoMockContainerBase<TI, TC>
    where TC : class, TI
    where TI : class
    {
        protected AutoMock Container;

        private TC _instance;

        protected TC Instance
        {
            get { return _instance ?? (_instance = Container.Create<TC>()); }
        }

        public virtual void OnSetup(ContainerBuilder cfg) { }
        public virtual void OnTeardown() { }

        [SetUp]
        public void SetUp()
        {
            Container = AutoMock.GetLoose(cfg => OnSetup(cfg));
            //OnSetup();
        }

        [TearDown]
        public void TearDown()
        {
            OnTeardown();
            _instance = default(TC);
            Container.Dispose();
        }
    }

    public static class AutofacAutoMockExtensions
    {
        public static Mock<T> GetMock<T>(this AutoMock mock, params Autofac.Core.Parameter[] parameters)
            where T : class
        {
            return mock.Mock<T>(parameters);
        }
    }
}
