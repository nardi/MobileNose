using System;

namespace MobileNose
{
    [Serializable]
	public class Day : TimePeriod
	{
		public int Year { get; set; }
		public int Month { get; set; }
		public int Number { get; set; }

		public Day(int year, int month, int day)
			: base(new DateTime(year, month, day, 0, 0, 0, 0, DateTimeAdditions.StandardCalendar, DateTimeKind.Utc), TimeSpan.FromHours(24))
		{
			Year = year;
			Month = month;
			Number = day;
		}

		public Week Week
		{
			get { return StartTime.GetWeek(); }
		}

		public Day Add(int days)
		{
			DateTime nextDay = StartTime + TimeSpan.FromDays(days);
			return new Day(nextDay.Year, nextDay.Month, nextDay.Day);
		}

		public static Day Today
		{
			get { return DateTime.UtcNow.GetDay(); }
		}
	}
}

