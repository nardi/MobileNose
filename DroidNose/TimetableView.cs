using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using MobileNose;

namespace DroidNose
{
	public class TimetableView : LinearLayout
	{
		private LinearLayout hourLayout = null;
		private DateTitleView dummyDateView = null;
		private HourView hourView = null;
		private MultiDayView dayView = null;

		public TimetableView(Context context, Timetable timetable, Day day, TimetableFragment ttf) : base(context)
		{
			Orientation = Orientation.Horizontal;

			hourLayout = new LinearLayout(context);
			hourLayout.Orientation = Orientation.Vertical;
			AddView(hourLayout);

			/*
			 * Deze is er alleen om de uren op de goede plaats te krijgen
			 */
			dummyDateView = new DateTitleView(context, null);
			hourLayout.AddView(dummyDateView);

			hourView = new HourView(context, TimeLayout.DefaultStartHour, TimeLayout.DefaultEndHour);
			hourLayout.AddView(hourView);

			dayView = new MultiDayView(context, timetable, day, hourView, ttf);
			hourView.AddHourHeightListener(dayView);
			AddView(dayView);
		}
	}
}

