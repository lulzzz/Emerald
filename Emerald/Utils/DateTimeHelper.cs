using System;
using System.Collections.Generic;

namespace Emerald.Utils
{
    public static class DateTimeHelper
    {
        public static DateTime GetUtcDateTimeFromUnixTimeMilliseconds(long milliseconds)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;
        }

        public static long ToUnixTimeSeconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime, TimeSpan.Zero).ToUnixTimeSeconds();
        }
        public static long ToUnixTimeMilliseconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime, TimeSpan.Zero).ToUnixTimeMilliseconds();
        }

        public static IEnumerable<DateTime> GetWeekDates(DateTime weekDate, DayOfWeek startDayOfWeek)
        {
            var startDateOfWeek = weekDate.AddDays(-1 * ((7 + (weekDate.DayOfWeek - startDayOfWeek)) % 7)).Date;
            var weekDateList = new List<DateTime>(7) { startDateOfWeek };

            for (var i = 1; i < 7; i++)
            {
                weekDateList.Add(startDateOfWeek.AddDays(i));
            }

            return weekDateList;
        }
    }
}