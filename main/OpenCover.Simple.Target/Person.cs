namespace Target
{
    public class Person
    {
        public int Age { get; set; }
        public int Height { get; set; }

        public override string ToString()
        {
            return $"Age={Age} Height={Height}";
        }
    }
}