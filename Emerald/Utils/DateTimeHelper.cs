using System;

namespace Emerald.Utils
{
    public static class DateTimeHelper
    {
        public static DateTime? GetUtcDateTimeFromUnixTimeMilliseconds(long? milliseconds)
        {
            return milliseconds.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(milliseconds.Value).UtcDateTime : (DateTime?)null;
        }

        public static long ToUnixTimeMilliseconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime, TimeSpan.Zero).ToUnixTimeMilliseconds();
        }
    }
}