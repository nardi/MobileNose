using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using MobileNose;

namespace DroidNose
{
	public class ChooseDateFragment : Android.Support.V4.App.DialogFragment
	{
		private Day StartDay;
		private Action<Day> OnDatePicked;

		public ChooseDateFragment(Day startDay, Action<Day> onDatePicked)
		{
			StartDay = startDay;
			OnDatePicked = onDatePicked;
			RetainInstance = true;
		}

		public override void OnDestroyView()
		{
			if (Dialog != null)
				Dialog.SetDismissMessage(null);
			base.OnDestroyView();
		}

		private BetterDatePickerDialog DatePickerDialog;

		public override Dialog OnCreateDialog(Bundle savedInstanceState)
		{
			DatePickerDialog = new BetterDatePickerDialog(Activity, null, StartDay.Year, StartDay.Month - 1, StartDay.Number);

			if (Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb)
			{
				DatePicker datePicker = DatePickerDialog.DatePicker;
				if (Utils.IsInPortraitMode)
				{
					datePicker.CalendarViewShown = true;
					datePicker.SpinnersShown = false;

					if (Build.VERSION.SdkInt >= BuildVersionCodes.HoneycombMr1)
					{
						CalendarView cv = datePicker.CalendarView;
						cv.FirstDayOfWeek = Java.Util.Calendar.Monday;
						cv.ShowWeekNumber = true;
					}
				}
				else
				{
					datePicker.CalendarViewShown = false;
					datePicker.SpinnersShown = true;
				}
			}

			DatePickerDialog.SetPermanentTitle("Kies een datum");
			DatePickerDialog.SetButton((int)DialogButtonType.Positive, "Klaar!", (sender, dce) =>
			{
				Day day = new Day(DatePickerDialog.Year, DatePickerDialog.MonthOfYear + 1, DatePickerDialog.DayOfMonth);
				OnDatePicked(day);
			});
			DatePickerDialog.SetButton((int)DialogButtonType.Negative, "Laat maar", (sender, dce) => {});

			return DatePickerDialog;
		}

		private class BetterDatePickerDialog : DatePickerDialog
		{
			public BetterDatePickerDialog(Context context, EventHandler<DateSetEventArgs> callBack, int year, int monthOfYear, int dayOfMonth)
				: base(context, callBack, year, monthOfYear, dayOfMonth)
			{
				Year = year;
				MonthOfYear = monthOfYear;
				DayOfMonth = dayOfMonth;
			}

			private string _title = null;
			public int Year, MonthOfYear, DayOfMonth;

			public void SetPermanentTitle(string title)
			{
				_title = title;
				SetTitle(title);
			}

			public override void OnDateChanged(DatePicker view, int year, int monthOfYear, int dayOfMonth)
			{
				base.OnDateChanged(view, year, monthOfYear, dayOfMonth);
				if (_title != null)
					SetTitle(_title);
				Year = year;
				MonthOfYear = monthOfYear;
				DayOfMonth = dayOfMonth;
			}
		}
	}
}

