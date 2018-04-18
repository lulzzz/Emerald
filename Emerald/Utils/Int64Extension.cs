using System;

namespace Emerald.Utils
{
    public static class Int64Extension
    {
        public static DateTime ToUtcDateTime(this long milliseconds)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;
        }
        public static DateTime ToDateTime(this long milliseconds, double offset)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).ToOffset(TimeSpan.FromMinutes(offset)).DateTime;
        }
    }
}