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
        /// <summary>Create date time based on utc date time using specific offset.</summary>
        /// <param name="dateTime">Date time.</param>
        /// <param name="offset">Offset.</param>
        /// <returns>Date time.</returns>
        public static DateTime FromUtcDateTime(DateTime dateTime, TimeSpan offset)
        {
            var localDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
            localDateTime.AddMilliseconds(dateTime.Millisecond);
            return new DateTimeOffset(localDateTime, offset).DateTime;
        }
        /// <summary>Create date time based on utc date time using specific offset.</summary>
        /// <param name="dateTime">Date time.</param>
        /// <param name="offset">Offset in minutes.</param>
        /// <returns>Date time.</returns>
        public static DateTime FromUtcDateTime(DateTime dateTime, double offset)
        {
            var localDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
            localDateTime.AddMilliseconds(dateTime.Millisecond);
            return new DateTimeOffset(localDateTime, TimeSpan.FromMinutes(offset)).DateTime;
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