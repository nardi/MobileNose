using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;
using MobileNose;

namespace DroidNose
{
	public class MultiDayView : TimeLayout
	{
		private static readonly int MinWidthPerDay = Utils.DpToPx(250);

		private Timetable Timetable;
		private HourView HourView;
		private TimetableFragment TimetableFragment;

		private DayScrollView ScrollView = null;
		private LinearLayout MainLayout = null;
		private List<DayView> DayViews = null;

		private int ContainerWidth;
		private int ContainerHeight;
		private int DaysOnScreen;
		private int DayWidth;

		private Day _StartDay;
		private Day StartDay
		{
			get { return _StartDay; }
			set
			{
				_StartDay = value;
				TimetableFragment.StartDay = value;
			}
		}
		private List<Day> DayList;

		private readonly Action<Day, IEnumerable<Event>> onUpdate;

		public MultiDayView(Context context, Timetable timetable, Day startDay, HourView hourView, TimetableFragment ttf)
			: base(context)
		{
			Timetable = timetable;
			HourView = hourView;
			TimetableFragment = ttf;

			_StartDay = startDay;

			ScrollView = new DayScrollView(Context, this);
			AddView(ScrollView);

			MainLayout = new LinearLayout(context);
			MainLayout.Orientation = Orientation.Horizontal;
			ScrollView.AddView(MainLayout);

			DayList = new List<Day>();
			DayViews = new List<DayView>();

			onUpdate = (day, events) =>
			{
				/*
				 * Check if new days need to be added to the view
				 * or hourRanges need to be changed
				 */
				CheckHourRange();
			};

			CheckHourRange();
		}

		private void CheckHourRange()
		{
			/*
			 * Als er dingen voor negenen of na vijfen gebeuren,
			 * moet de HourView hiervan op de hoogte zijn om zich uit te breiden
			 */
			int startHour = TimeLayout.DefaultStartHour,
				endHour = TimeLayout.DefaultEndHour;
			foreach (Event e in Timetable.Events)
			{
				Day day = e.StartTime.GetDay();

				double startTime = (e.StartTime - day.StartTime).TotalHours;
				startHour = (int)Math.Min(startHour, Math.Round(startTime));

				double endTime = (e.EndTime - day.StartTime).TotalHours;
				endHour = (int)Math.Max(endHour, Math.Round(endTime));
			}
			HourView.SetHourRange(startHour, endHour);
			foreach (DayView dv in DayViews)
				dv.SetHourRange(startHour, endHour);
		}

		protected override void Update()
		{
			if (ContainerWidth != 0)
			{
				DaysOnScreen = Math.Max(1, ContainerWidth / MinWidthPerDay);
				DayWidth = ContainerWidth / DaysOnScreen;

				MainLayout.RemoveAllViews();
				ScrollView.StepSize = DayWidth;
				ScrollView.Visibility = ViewStates.Invisible;

				DayList.Clear();
				DayViews.Clear();

				FillDays();

				int startDayIndex = DaysOnScreen;
				SetScrollBars(startDayIndex + DaysOnScreen - 1);
				ScrollView.Post(() =>
                {
					ScrollView.GoToStep(startDayIndex);
					ScrollView.Visibility = ViewStates.Visible;
				});
			}
		}

		private int FillDays()
		{
			if (DayList.Count == 0)
			{
				StartDay = GetFirstValidDay(StartDay, 1);
				Timetable.UpdateIfNeeded(StartDay, onUpdate, e => Console.WriteLine(e));
				AddDay(StartDay);
			}

			int startDayIndex = DayList.IndexOf(StartDay);
			int preDays = Math.Max(DaysOnScreen - startDayIndex, 0);
			int numDays = DayList.Count;
			int postDays = Math.Max((2 * DaysOnScreen) - (numDays - startDayIndex), 0);

			foreach (Day day in GetNonEmptyDays(DayList[0], -1, preDays))
				AddDay(day, 0);

			foreach (Day day in GetNonEmptyDays(DayList.Last(), 1, postDays))
				AddDay(day);

			return preDays;
		}

		private List<Day> GetNonEmptyDays(Day fromDay, int searchDirection, int amount)
		{
			List<Day> days = new List<Day>();
			Day currentDay = fromDay;
			for (int i = 0; i < amount; i++)
			{
				currentDay = currentDay.Add(searchDirection);
				currentDay = GetFirstValidDay(currentDay, searchDirection);
				Timetable.UpdateIfNeeded(currentDay, onUpdate, e => Console.WriteLine(e));
				days.Add(currentDay);
			}
			return days;
		}

		public static Day GetFirstValidDay(Day fromDay, int searchDirection)
		{
			Day day = fromDay;
			if (fromDay.StartTime.DayOfWeek == DayOfWeek.Saturday)
			{
				day = searchDirection == 1 ? fromDay.Add(2) : fromDay.Add(-1);
			}
			else if (fromDay.StartTime.DayOfWeek == DayOfWeek.Sunday)
			{
				day = searchDirection == 1 ? fromDay.Add(1) : fromDay.Add(-2);
			}

			return day;

			/*
			 * Modify to exclude empty weeks
			 */
		}

		private void AddDay(Day day)
		{
			AddDay(day, DayList.Count);
		}

		private void AddDay(Day day, int location)
		{
			DayList.Insert(location, day);
			DayView dayView = MakeDayView(day);
			DayViews.Insert(location, dayView);
			MainLayout.AddView(dayView, location);
		}

		private DayView MakeDayView(Day day)
		{
			DayView dayView = new DayView(Context, day, Timetable, HourView);
			dayView.HourHeight = HourHeight;
			dayView.LayoutParameters = new LayoutParams(DayWidth, ContainerHeight);
			return dayView;
		}

		public void SetScrollBars(int rightDayViewIndex)
		{
			foreach (DayView d in DayViews)
				d.VerticalScrollBarEnabled = false;

			if (DayViews.Count > rightDayViewIndex && rightDayViewIndex >= 0)
			{
				DayView rightDayView = DayViews[rightDayViewIndex];
				rightDayView.VerticalScrollBarEnabled = true;
			}
		}

		private class DayScrollView : StepScrollView
		{
			private readonly MultiDayView MultiDayView;

			public DayScrollView(Context context, MultiDayView mdv) : base(context)
			{
				HorizontalScrollBarEnabled = false;
				HorizontalFadingEdgeEnabled = false;
				MultiDayView = mdv;
			}

			private bool measured = false;
			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				int parentWidth = View.MeasureSpec.GetSize(widthMeasureSpec);
				int parentHeight = View.MeasureSpec.GetSize(heightMeasureSpec);

				if (!measured)
				{
					MultiDayView.ContainerWidth = parentWidth;
					MultiDayView.ContainerHeight = parentHeight;
					MultiDayView.Update();

					measured = true;
				}

				base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
			}

			protected override void OnStepChange(int step)
			{
				MultiDayView.StartDay = MultiDayView.DayList[step];
				int dayOffset = MultiDayView.FillDays();
				ScrollBy(dayOffset * StepSize, 0);

				int rightDayViewIndex = dayOffset + step + MultiDayView.DaysOnScreen - 1;
				MultiDayView.SetScrollBars(rightDayViewIndex);

				GoToStep(step + dayOffset);
			}
		}
	}
}

