using System;

namespace MobileNose
{
    [Serializable]
	public class Week : TimePeriod
	{
		public int Year { get; set; }
		public int Number { get; set; }

		static DateTime StartTimeFromWeek(int year, int weekNumber)
		{
			var start = new DateTime(year, 1, 1, 0, 0, 0, 0, DateTimeAdditions.StandardCalendar, DateTimeKind.Utc);
			if (weekNumber > 1)
			{
				start = start.AddDays(DateTimeAdditions.GetDaysInFirstWeek(year));
				weekNumber--;
			}
			return start.AddDays(7 * (weekNumber - 1));
		}

		public Week(int year, int weekNumber)
			: base(StartTimeFromWeek(year, weekNumber),
			       TimeSpan.FromDays(weekNumber != 1 ? 7 : DateTimeAdditions.GetDaysInFirstWeek(year)))
		{
			Year = year;
			Number = weekNumber;
		}

		public static Week ThisWeek
		{
			get { return DateTime.Now.GetWeek(); }
		}
	}
}

