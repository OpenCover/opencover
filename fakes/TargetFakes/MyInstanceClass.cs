namespace TargetFakes
{
    public class MyInstanceClass
    {
        public MyInstanceClass() : this(42)
        {
        }

        public MyInstanceClass(int value)
        {
            Value = value;
        }

        public int MyInstanceMethod()
        {
            return Value;
        }

        public int Value { get; private set; }
    }
}