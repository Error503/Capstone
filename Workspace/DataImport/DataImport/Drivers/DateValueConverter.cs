using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataImport.Drivers
{
    public class DateValueConverter
    {
        public const string kDateFormat = "yyyyMMdd";
        public const string kDisplayDateFormat = "yyyy-MM-dd";

        /// <summary>
        /// Converts the given DateTime object into a long representation of the given date.
        /// </summary>
        /// <param name="date"></param>
        /// <returns>A long representing the given DateTime</returns>
        public static long ToLongValue(DateTime date)
        {
            return long.Parse(date.ToString(kDateFormat));
        }

        /// <summary>
        /// Converts the given long into a 
        /// </summary>
        /// <param name="dateLong">The long representation of a date</param>
        /// <returns>The DateTime object represented by the given long</returns>
        public static DateTime ToDateTime(long dateLong)
        {
            const int kYearDivisor = 10000;
            const int kMonthDivisor = 100;
            return new DateTime((int)(dateLong / kYearDivisor), (int)((dateLong % kYearDivisor) / kMonthDivisor), (int)(dateLong % kMonthDivisor));
        }

        public static string ToDisplayFormat(DateTime? date)
        {
            string result = "";

            if (date.HasValue)
            {
                result = date.Value.ToString("yyyy-MM-dd");
            }

            return result;
        }
    }
}