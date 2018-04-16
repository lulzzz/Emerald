namespace Emerald.Utils
{
    public static class Int32Extension
    {
        public static bool IsGreaterThan(this int i, int value)
        {
            return i > value;
        }
        public static bool IsGreaterThanOrEqual(this int i, int value)
        {
            return i >= value;
        }
        public static bool IsLessThan(this int i, int value)
        {
            return i < value;
        }
        public static bool IsLessThanOrEqual(this int i, int value)
        {
            return i <= value;
        }
    }
}