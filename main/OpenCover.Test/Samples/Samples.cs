using System;

namespace OpenCover.Test.Samples
{
    class ConstructorNotDeclaredClass
    {
        
    }

    class DeclaredConstructorClass
    {
        DeclaredConstructorClass() { }
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
}
