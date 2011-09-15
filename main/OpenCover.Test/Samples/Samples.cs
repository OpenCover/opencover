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
        public abstract string Name { get; }
    }

    public class Concrete : AbstractBase
    {
        public override string Name { get { return "Me!"; } }
    }
}
