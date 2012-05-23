using System;

namespace OpenCover.Test.Samples
{
    class ConstructorNotDeclaredClass
    {
        
    }

    class DeclaredConstructorClass
    {
        DeclaredConstructorClass() { }

        public bool HasSingleDecision(string input)
        {
            if (input.Contains("test")) return true;
            return false;
        }

        public bool HasTwoDecisions(string input)
        {
            if (input.Contains("test")) return true;
            if (input.Contains("xxx")) return true;
            return false;
        }

        public bool HasSwitch(int input)
        {
            switch (input)
            {
                case 0:
                    return true;
                case 1:
                    return false;
                case 2:
                    return true;
            }
            return false;
        }
    }

    class DeclaredMethodClass
    {
        void Method() {}

        string AutoProperty { get; set;}

        void DoThing(Action<object> doThing)
        {
            doThing(1);
        }

        void CallDoThing()
        {
            DoThing((x) => { Console.WriteLine(x.ToString()); });
        }
    }

    public abstract class AbstractBase
    {
        public abstract string Name { get; set; }
        public abstract void Method();
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ExcludeClassAttribute : Attribute
    {   
    }

    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Property|AttributeTargets.Constructor)]
    public class ExcludeMethodAttribute : Attribute
    {
    }

    [ExcludeClassAttribute]
    public class Concrete : AbstractBase
    {
        [ExcludeMethodAttribute]
        public Concrete()
        {
            
        }

        [ExcludeMethodAttribute]
        public override string Name
        {
            get { return "Me!"; }
            set { }
        }

        [ExcludeMethodAttribute]
        public override void Method()
        {
            throw new NotImplementedException();
        }
    }

    public class Issue99
    {
        [ExcludeMethodAttribute]
        public Func<bool> PropertyReturningFunc
        {
            get
            {
                return (() => false);
            }
        }
    }

    public struct NotCoveredStruct
    {
        public int Number { get; set; }
    }

    public struct CoveredStruct
    {
        private int number;
// ReSharper disable ConvertToAutoProperty
        public int Number { get { return number; } set { number = value; } }
// ReSharper restore ConvertToAutoProperty
    }

}
