using System;

namespace Emerald.Utils
{
    public static class DateTimeExtension
    {
        public static long ToUnixTimeMilliseconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime, TimeSpan.Zero).ToUnixTimeMilliseconds();
        }
        public static DateTime[] GetWeekDates(this DateTime dateTime, DayOfWeek startDayOfWeek)
        {
            var weekDateArray = new DateTime[7];
            weekDateArray[0] = dateTime.AddDays(-1 * ((7 + (dateTime.DayOfWeek - startDayOfWeek)) % 7)).Date;
            for (var i = 1; i < 7; i++) weekDateArray[i] = weekDateArray[0].AddDays(i);
            return weekDateArray;
        }
    }
}