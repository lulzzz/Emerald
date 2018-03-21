using System;

namespace Emerald.Utils
{
    public static class DateTimeExtension
    {
        public static long ToUnixTimeSeconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime, TimeSpan.Zero).ToUnixTimeSeconds();
        }
        public static long ToUnixTimeMilliseconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime, TimeSpan.Zero).ToUnixTimeMilliseconds();
        }

        /// <summary>Convert date time with specific offset to utc date time.</summary>
        /// <param name="dateTime">Date time.</param>
        /// <param name="offset">Offset of current date time.</param>
        /// <returns>Utc date time.</returns>
        public static DateTime ToUtcDateTime(this DateTime dateTime, TimeSpan offset)
        {
            var localDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
            localDateTime.AddMilliseconds(dateTime.Millisecond);
            return new DateTimeOffset(localDateTime, offset).UtcDateTime;
        }
        /// <summary>Convert utc date time with specific offset to utc date time.</summary>
        /// <param name="dateTime">Date time.</param>
        /// <param name="offset">Offset of current date time in minutes.</param>
        /// <returns>Utc date time.</returns>
        public static DateTime ToUtcDateTime(this DateTime dateTime, double offset)
        {
            var localDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
            localDateTime.AddMilliseconds(dateTime.Millisecond);
            return new DateTimeOffset(localDateTime, TimeSpan.FromMinutes(offset)).UtcDateTime;
        }
    }
}