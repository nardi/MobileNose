using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MobileNose;

namespace DroidNose
{
	public class NewMultiDayView : TimeLayout
	{
		private static readonly int MinWidthPerDay = Utils.DpToPx(250);

		private Timetable Timetable;
		private HourView HourView;
		private TimetableFragment TimetableFragment;

		private DayScrollView ScrollView;
		private LinearLayout TopLayout;
		private List<DateTitleView> DateTitleViews;

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

		public NewMultiDayView(Context context, Timetable timetable, Day startDay, HourView hourView, TimetableFragment ttf)
			: base(context)
		{
			Timetable = timetable;
			HourView = hourView;
			TimetableFragment = ttf;

			_StartDay = startDay;

			ScrollView = new DayScrollView(Context, this);
			AddView(ScrollView);

			TopLayout = new LinearLayout(context);
			TopLayout.Orientation = Orientation.Horizontal;
			ScrollView.AddView(TopLayout);

			DayList = new List<Day>();
			DateTitleViews = new List<DateTitleView>();

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
			//foreach (DayView dv in DayViews)
			//dv.SetHourRange(startHour, endHour);
		}

		private void Create()
		{
			TopLayout.RemoveAllViews();

			DayList.Clear();
			DateTitleViews.Clear();

			DaysOnScreen = Math.Max(1, ContainerWidth / MinWidthPerDay);
			DayWidth = ContainerWidth / DaysOnScreen;

			FillDays();
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
		}

		protected override void Update()
		{
			// Set eventView HourHeights
		}

		private class DayScrollView : StepScrollView
		{
			private readonly NewMultiDayView MultiDayView;

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
					MultiDayView.Create();

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

