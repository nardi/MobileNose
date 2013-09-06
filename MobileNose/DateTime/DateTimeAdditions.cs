using System;
using System.Globalization;

namespace MobileNose
{
	public static class DateTimeAdditions
	{
		public static bool IsDuring(this DateTime t, TimePeriod tp)
		{
			return tp.StartTime <= t && t < tp.EndTime;
		}

		public static Calendar StandardCalendar { get; set; }
		public static CalendarWeekRule StandardWeekRule { get; set; }
		public static DayOfWeek StandardFirstDayOfWeek { get; set; }

		static DateTimeAdditions()
		{
			StandardCalendar = new GregorianCalendar();
			StandardWeekRule = CalendarWeekRule.FirstFourDayWeek;
			StandardFirstDayOfWeek = DayOfWeek.Monday;
		}

		public static int GetDaysInFirstWeek(int year)
		{
			var start = new DateTime(year, 1, 1, 0, 0, 0, 0, DateTimeAdditions.StandardCalendar, DateTimeKind.Utc);
			int counter = 0;
			while (start.GetWeekOfYear() == 1)
			{
				start = start.AddDays(1);
				counter++;
			}
			return counter;
		}

		public static int GetWeekOfYear(this DateTime t)
		{
			return StandardCalendar.GetWeekOfYear(t, StandardWeekRule, StandardFirstDayOfWeek);
		}

		public static Day GetDay(this DateTime t)
		{
			return new Day(t.Year, t.Month, t.Day);
		}

		public static Week GetWeek(this DateTime t)
		{
			return new Week(t.Year, t.GetWeekOfYear());
		}
	}
}

