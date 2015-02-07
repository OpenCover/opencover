using TargetFakes;

namespace Target
{
    public class MyStaticClassCaller
    {
        public static int MyStaticMethod()
        {
            return MyStaticClass.MyStaticMethod();
        }
    }
}
