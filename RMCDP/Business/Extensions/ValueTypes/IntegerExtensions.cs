namespace Business.Extensions.ValueTypes
{
    public static class IntegerExtensions
    {
        public static string Format(this int value1, int value2)
        {
            return string.Format("{0}_{1}", value1, value2);
        }
    }
}
