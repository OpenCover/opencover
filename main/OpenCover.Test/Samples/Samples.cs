using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

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

        public bool HasCompleteIf(string input)
        {
            if (input.Contains("test"))
            {
                return true;
            }
            else
            {
                return false;
            }
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

        public bool HasSwitchWithDefault(int input)
        {
            switch (input)
            {
                case 1:
                    return true;
                case 2:
                    return false;
                case 3:
                    return true;
                default:
                    return false;
            }
        }

        public bool HasSwitchWithBreaks(int input)
        {
            bool ret = false;
            switch (input)
            {
                case 1:
                    ret = true;
                    break;
                case 2:
                    ret = false;
                    break;
                case 3:
                    ret = true;
                    break;
            }

            return ret;
        }

        public bool HasSwitchWithMultipleCases(int input)
        {
            switch (input)
            {
                case 1:
                case 3:
                    return true;
                default:
                    return false;
            }
        }

        public int HasUsing(int param)
        {
            byte[] buffer = BitConverter.GetBytes(param);
            using (var someDisposable = new MemoryStream(buffer))
            {                
                byte[] readBytes = new byte[sizeof(int)];
                someDisposable.Read(readBytes, 0, readBytes.Length);
                return BitConverter.ToInt32(readBytes, 0);
            }
        }
    }

    class DeclaredMethodClass
    {
        private string _propertyWithBackingField;
        void Method() {}

        string AutoProperty { get; set;}

        string PropertyWithBackingField
        {
            get { return _propertyWithBackingField; }
            set { _propertyWithBackingField = value; }
        }

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

    public class Anonymous
    {
        private Func<bool> x;

        [ExcludeMethodAttribute]
        public Func<bool> PropertyReturningFunc_EXCLUDE
        {
            get
            {
                return (() => false);
            }
            set
            {
                x = () => true;
            }
        }

        [ExcludeMethodAttribute]
        public Func<bool> MethodReturningFunc_EXCLUDE()
        {
            return (() => false);
        }

        public Func<bool> PropertyReturningFunc_INCLUDE
        {
            get
            {
                return (() => false);
            }
            set
            {
                x = () => true;
            }
        }

        public Func<bool> MethodReturningFunc_INCLUDE()
        {
            return (() => false);
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

    public class LinqIssue
    {
        public void Method()
        {
            var s = new ObservableCollection<string>();
            var x = (from a in s select new {a});
        }

        public object Property
        {
            get
            {
                var s = new ObservableCollection<string>();
                var x = (from a in s select new { a });
                return x;
            }
        }
    }
}
