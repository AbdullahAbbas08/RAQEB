using System.Globalization;

namespace Raqeb.Shared.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToEGCultureDateTime(this DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy hh:mm:ss tt");
        }

        public static string ToEGCultureDate(this DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy");
        }

        public static string ToEGCultureTime(this DateTime dateTime)
        {
            return dateTime.ToString("hh:mm:ss tt");
        }

        public static IEnumerable<DateTime> EachDay(this DateTime from, DateTime to)
        {
            for (var day = from.Date; day.Date <= to.Date; day = day.AddDays(1))
                yield return day;
        }

        public static string ChangeCulture(this DateTime dateTime, string culture)
        {
            CultureInfo cultureInfo = new(culture, true);
            return dateTime.ToString(cultureInfo);
        }
    }
}
