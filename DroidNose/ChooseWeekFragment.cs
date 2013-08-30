using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using MobileNose;
using Android.Views;

namespace DroidNose
{
	public class ChooseWeekFragment : Android.Support.V4.App.DialogFragment
	{
		private int currentWeek, currentYear;
		private Action<Week> OnWeekPicked;

		public ChooseWeekFragment(Week startWeek, Action<Week> onWeekPicked)
		{
			currentWeek = startWeek.Number;
			currentYear = startWeek.Year;
			OnWeekPicked = onWeekPicked;
			RetainInstance = true;
		}

		public override void OnDestroyView()
		{
			if (Dialog != null)
				Dialog.SetDismissMessage(null);
			base.OnDestroyView();
		}

		public override Dialog OnCreateDialog(Bundle savedInstanceState)
		{
			var layout = Activity.LayoutInflater.Inflate(Resource.Layout.WeekPickerLayout, null);
			var weekPicker = layout.FindViewById<NumberPicker>(Resource.Id.weekNumberPicker);
		    weekPicker.MinValue = 1;
		    weekPicker.MaxValue = 52;
			weekPicker.Value = currentWeek;
			weekPicker.ValueChanged += (sender, vce) => { currentWeek = vce.NewVal; };
			var yearPicker = layout.FindViewById<NumberPicker>(Resource.Id.yearNumberPicker);
		    yearPicker.MinValue = 2008;
		    yearPicker.MaxValue = DateTime.Now.Year + 1;
			yearPicker.Value = currentYear;
			yearPicker.ValueChanged += (sender, vce) => { currentYear = vce.NewVal; };

			return new AlertDialog.Builder(new ContextThemeWrapper(Activity, Resource.Style.AppTheme))
                .SetTitle("Kies een week")
				.SetView(layout).SetPositiveButton("Klaar!",
                    (sender, dce) => OnWeekPicked(new Week(currentYear, currentWeek)))
                .SetNegativeButton("Laat maar", (sender, dce) => {}).Create();
		}
	}
}

