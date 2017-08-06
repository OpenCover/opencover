using TargetFakes;

namespace Target
{
    public class MyInstanceClassCaller
    {
        public static int MyInstanceMethod(MyInstanceClass instance)
        {
            return instance.MyInstanceMethod();
        }
    }
}