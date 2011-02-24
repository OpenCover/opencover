using Moq.AutoMocking;
using NUnit.Framework;

namespace OpenCover.Test.MoqFramework
{
    public abstract class UnityAutoMockContainerBase<TI, TC>
        where TC : class, TI 
        where TI : class
    {
        protected UnityAutoMockContainer Container;

        private TC _instance;

        protected TC Instance
        {
            get { return _instance ?? (_instance = (TC) Container.Resolve<TI>()); }
        }

        public virtual void OnSetup() { }
        public virtual void OnTeardown() { }

        [SetUp]
        public void SetUp()
        {
            Container = new UnityAutoMockContainer();
            Container.Register<TI, TC>();
            OnSetup();
        }

        [TearDown]
        public void TearDown()
        {
            OnTeardown();
            _instance = default(TC);
            Container = null;
        }   
    }
}
