using System.Collections.Generic;

namespace System
{

    public static class DateTimeHelper
    {
        public static readonly DateTime MIN_DATE_TIME = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime MAX_DATE_TIME = new DateTime(3000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public const string SqlStringFormat = "yyyy-MM-dd HH:mm:ss.ff";

        /// <summary>
        /// UTC
        /// </summary>
        public static DateTime Now => DateTime.UtcNow;

        /// <summary>
        /// Giờ UTC hiện tại sang chuỗi mô tả thời gian trong Sql: yyyy-MM-dd HH:mm:ss.ffffff
        /// </summary>
        public static string NowAsSqlString => DateTime.UtcNow.ToString(SqlStringFormat);

        /// <summary>
        /// Giờ Việt Nam hiện thời
        /// </summary>
        public static DateTime VNNow
        {
            get
            {
                var now = DateTime.UtcNow.AddHours(7);
                var local = DateTime.SpecifyKind(now, DateTimeKind.Local);
                return local;
            }
        }

        public static DateTime GetStartDateOfMonth(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1);
        }

        public static DateTime GetEndDateOfMonth(DateTime dt)
        {
            var m = dt.Month;
            if (m == 2)
            {
                return new DateTime(dt.Year, 3, 1).AddDays(-1);
            }
            if (m == 1 || m == 3 || m == 5 || m == 7 || m == 8 || m == 10 || m == 12)
            {
                return new DateTime(dt.Year, dt.Month, 31);
            }
            return new DateTime(dt.Year, dt.Month, 30);
        }

        public static DateTime GetStartDateOfYear(int year)
        {
            return new DateTime(year, 1, 1);
        }

        public static DateTime GetStartDateOfYear(DateTime dt)
        {
            return GetStartDateOfYear(dt.Year);
        }

        public static DateTime GetEndDateOfYear(DateTime dt)
        {
            return GetEndDateOfYear(dt.Year);
        }

        public static DateTime GetEndDateOfYear(int year)
        {
            return new DateTime(year, 12, 31);
        }

        public static DateTime ToVNTime(this DateTime dt, bool unspecifiedAsUtc = true)
        {
            DateTime result;
            if (dt.Kind == DateTimeKind.Local)
            {
                result = dt.ToUniversalTime().AddHours(7);
            }
            else if (dt.Kind == DateTimeKind.Utc)
            {
                result = dt.AddHours(7);
            }
            else
            {
                if (unspecifiedAsUtc)
                {
                    dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    result = dt.AddHours(7);
                }
                else
                {
                    result = dt.ToUniversalTime().AddHours(7);
                }
            }
            result = DateTime.SpecifyKind(result, DateTimeKind.Local);
            return result;
        }

        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="unspecifiedAsUtc"></param>
        /// <returns></returns>
        public static DateTime ToLocal(this DateTime dt, bool unspecifiedAsUtc = true)
        {
            if (dt.Kind == DateTimeKind.Local)
            {
                return dt;
            }
            if (dt.Kind == DateTimeKind.Utc)
            {
                return dt.ToLocalTime();
            }
            if (unspecifiedAsUtc)
            {
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                return dt.ToLocalTime();
            }
            return DateTime.SpecifyKind(dt, DateTimeKind.Local);
        }

        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="unspecifiedAsUtc"></param>
        /// <returns></returns>
        public static DateTime ToUtcTime(this DateTime dt, bool unspecifiedAsUtc = true)
        {
            if (dt.Kind == DateTimeKind.Utc)
            {
                return dt;
            }
            if (dt.Kind == DateTimeKind.Local)
            {
                return dt.ToUniversalTime();
            }
            if (unspecifiedAsUtc)
            {
                return DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            }
            dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);
            return dt.ToUniversalTime();
        }

        /// <summary>
        /// Chuyển đổi Unix timestamp sang DateTime theo UTC
        /// </summary>
        /// <param name="unixTimeStampInMilliseconds"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(long unixTimeStampInMilliseconds)
        {
            var result = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            result = result.AddMilliseconds(unixTimeStampInMilliseconds);
            return result;
        }

        public static DateTime GetMin(DateTime dt1, DateTime dt2)
        {
            if (dt1 < dt2)
            {
                return dt1;
            }

            return dt2;
        }

        public static DateTime GetMax(DateTime dt1, DateTime dt2)
        {
            if (dt1 > dt2)
            {
                return dt1;
            }

            return dt2;
        }

        public static long GetTimeStamp(DateTime date)
        {
            return (long)(date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
        }

        public static long GetTimeStamp()
        {
            return GetTimeStamp(DateTime.Now);
        }

        public static int GetCountInRange(DateTime fromDate, DateTime toDate)
        {
            int r = 0;
            DateTime dt = fromDate.Date;
            while (true)
            {
                if (dt > toDate.Date)
                {
                    break;
                }

                r += 1;

                dt = dt.AddDays(1);
            }

            return r;
        }

        public static int GetDayOfWeekCountInRange(DateTime fromDate, DateTime toDate, DayOfWeek dow)
        {
            int r = 0;
            DateTime dt = fromDate.Date;
            while (true)
            {
                if (dt > toDate.Date)
                {
                    break;
                }

                if (dt.DayOfWeek == dow)
                {
                    r += 1;
                }

                dt = dt.AddDays(1);
            }

            return r;
        }

        public static List<Tuple<DateTime, DateTime>> GetRangeWeeks(DateTime fromDate, DateTime toDate, DayOfWeek endDow)
        {
            return GetRanges(fromDate, toDate, (dt) => dt.DayOfWeek == endDow);
        }

        public static List<Tuple<DateTime, DateTime>> GetRangeMonths(DateTime fromDate, DateTime toDate)
        {
            return GetRanges(fromDate, toDate, (dt) => dt.Day == GetEndDateOfMonth(dt).Day);
        }

        public static List<Tuple<DateTime, DateTime>> GetRangeYears(DateTime fromDate, DateTime toDate)
        {
            return GetRanges(fromDate, toDate, (dt) => dt.Day == GetEndDateOfYear(dt).Day);
        }

        public static List<Tuple<DateTime, DateTime>> GetRanges(DateTime fromDate, DateTime toDate, Func<DateTime, bool> func)
        {
            var r = new List<Tuple<DateTime, DateTime>>();
            var current = fromDate;
            while (true)
            {
                if (current > toDate)
                {
                    break;
                }

                var start = current;
                if (func(fromDate) && current == fromDate)
                {
                    r.Add(Tuple.Create(fromDate, fromDate));
                    current = current.AddDays(1);
                    continue;
                }

                while (true)
                {
                    if (func(current) || current >= toDate)
                    {
                        r.Add(Tuple.Create(start, current));
                        current = current.AddDays(1);
                        break;
                    }

                    current = current.AddDays(1);
                }
            }

            return r;
        }

        public static List<DateTime> GetDates(DateTime fromDate, DateTime toDate)
        {
            var r = new List<DateTime>();
            var current = fromDate;
            while (true)
            {
                if (current > toDate)
                {
                    break;
                }

                r.Add(current);

                current = current.AddDays(1);
            }

            return r;
        }
    }
}
