using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Support.V4.App;
using Android.App;
using Android.Content;
using Android.Text;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using MobileNose;

namespace DroidNose
{
	using LayoutParams = ViewGroup.LayoutParams;
    using Android.Graphics;

	public class StudentIdFragment : Android.Support.V4.App.DialogFragment
	{
		public Action<int> OnStudentId = studentId => {};

		private string message = "Voer hier het studentnummer in:";
		private string defaultInput = "";
	    private bool cancelable;

		private TextView text;
		private EditText input;

		public StudentIdFragment(string message, string defaultInput, bool cancelable)
		{
			RetainInstance = true;
			if (message != null)
				this.message = message;
			if (defaultInput != null)
				this.defaultInput = defaultInput;
		    this.cancelable = cancelable;
		}

		public override void OnDestroyView()
		{
			if (Dialog != null)
				Dialog.SetDismissMessage(null);
			base.OnDestroyView();
		}

		public override Dialog OnCreateDialog(Bundle savedInstanceState)
		{
			var builder = new AlertDialog.Builder(Activity);

			int padding = Utils.DpToPx(0.02f * Utils.DisplayMetrics.WidthPixels);

			LinearLayout layout = new LinearLayout(Activity);
			layout.Orientation = Orientation.Vertical;
			layout.SetPadding(padding, padding, padding, padding);
            layout.SetBackgroundColor(Color.White);

			text = new TextView(Activity);
			text.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
			text.Text = message;
			text.TextSize = 16;
			text.Gravity = GravityFlags.Center;
			text.SetPadding(0, padding, 0, 0);
			layout.AddView(text);

			input = new EditText(Activity);
			input.InputType = InputTypes.ClassNumber;
			input.Text = defaultInput;
			input.AfterTextChanged += (sender, atce) =>
			{
				defaultInput = input.Text;
			};
			layout.AddView(input);

			builder.SetView(layout);

			builder.SetPositiveButton("Klaar!", (sender, dce) => {});
            if (cancelable)
                builder.SetNegativeButton("Laat maar", (sender, dce) => {});

			return builder.Create();
		}

		public override void OnResume()
		{
			base.OnResume();

			AlertDialog ad = (AlertDialog)Dialog;
			ad.CancelEvent += (sender, dce) => OnStudentId(-1);
            if (!cancelable)
			    ad.SetCanceledOnTouchOutside(false);

			ad.GetButton((int)DialogButtonType.Positive).Click += (sender, dce) =>
			{
				int studentId;
				if (string.IsNullOrWhiteSpace(input.Text) || !int.TryParse(input.Text, out studentId))
				{
					text.Text = message = "De ingevoerde tekst lijkt geen nummer te zijn. Typfoutje?";
				}
				else
				{
					Utils.HideOnScreenKeyboard(input.WindowToken);
					OnStudentId(studentId);
					ad.Dismiss();
				}
			};
		}
	}
}

