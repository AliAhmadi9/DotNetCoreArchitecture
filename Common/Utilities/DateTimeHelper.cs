using Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace Common.Utilities
{
    public static class DateTimeHelper
    {
        static DateTime EPOCH = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
        public static double ConvertDatetimeToUnixTimeStamp(DateTime date, int Time_Zone = 0)
        {
            TimeSpan The_Date = (date - EPOCH);
            return Math.Floor(The_Date.TotalSeconds) - (Time_Zone * 3600);
        }


        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public static DateTime ShamsiToGregorian(this string date)
        {
            PersianCalendar pc = new PersianCalendar();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            DateTime objResult;

            string[] objParts = date.Split('/');

            string strYear = objParts[0];
            if (strYear.Length == 2)
            {
                strYear = "13" + strYear;
            }

            string strDay = objParts[2];
            if (strDay.Length > 2)
            {
                strDay = strDay.Substring(0, 2);
            }

            objResult = pc.ToDateTime(Convert.ToInt16(strYear), Convert.ToInt16(objParts[1]), Convert.ToInt16(strDay), 0, 0, 0, 0);

            return (objResult);
        }
        public static string GregorianToShamsi(this DateTime date, string _separator = "/", bool showTime = false)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            System.Globalization.PersianCalendar pc = new System.Globalization.PersianCalendar();

            return showTime ?
                    string.Format("{0}{1}{2}{1}{3}-{4}:{5}", pc.GetYear(date), _separator, pc.GetMonth(date).ToString("00", CultureInfo.InvariantCulture), pc.GetDayOfMonth(date).ToString("00", CultureInfo.InvariantCulture), date.Hour.ToString("00"), date.Minute.ToString("00"))
                    : string.Format("{0}{1}{2}{1}{3}", pc.GetYear(date), _separator, pc.GetMonth(date).ToString("00", CultureInfo.InvariantCulture), pc.GetDayOfMonth(date).ToString("00", CultureInfo.InvariantCulture));
        }

        public static List<string> GetMonths()
        {
            return new List<string>()
                       {
                           "فروردین",
                           "اردیبهشت",
                           "خرداد",
                           "تیر",
                           "مرداد",
                           "شهریور",
                           "مهر",
                           "آبان",
                           "آذر",
                           "دی",
                           "بهمن",
                           "اسفند",
                       };
        }
        public static string GetShamsiMonth(object miladiMonthIndex)
        {
            string monthName = string.Empty;
            switch (Convert.ToString(miladiMonthIndex).Trim())
            {
                case "1":
                    monthName = "فروردین";
                    break;
                case "2":
                    monthName = "اردیبهشت";
                    break;
                case "3":
                    monthName = "خرداد";
                    break;
                case "4":
                    monthName = "تیر";
                    break;
                case "5":
                    monthName = "مرداد";
                    break;
                case "6":
                    monthName = "شهریور";
                    break;
                case "7":
                    monthName = "مهر";
                    break;
                case "8":
                    monthName = "آبان";
                    break;
                case "9":
                    monthName = "آذر";
                    break;
                case "10":
                    monthName = "دی";
                    break;
                case "11":
                    monthName = "بهمن";
                    break;
                case "12":
                    monthName = "اسفند";
                    break;
            }
            if (monthName == string.Empty) throw new AppException("شماره ماه وارد شده صحیح نمی باشد(باید عددی بین 1 تا 12 باشد)");

            return monthName;
        }

        public static int GetWeekOfYear(this DateTime date, CalendarWeekRule rule = CalendarWeekRule.FirstDay, DayOfWeek firstDayOfWeek = DayOfWeek.Saturday)
        {
            PersianCalendar pc = new PersianCalendar();
            return pc.GetWeekOfYear(date, rule, firstDayOfWeek);
        }

        public static void DiffDate(DateTime start, DateTime end, out int days, out int hours, out int minutes, out int seconds)
        {
            TimeSpan span = (end - start);

            days = span.Days;
            hours = span.Hours;
            minutes = span.Minutes;
            seconds = span.Seconds;
        }
    }
}
