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
	public class LoadingView : RelativeLayout
	{
		public LoadingView(Context context) : this(context, null)
		{
		}

		public LoadingView(Context context, string message) : base(context)
		{
			LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);

			LinearLayout layout = new LinearLayout(context);

			ProgressBar pBar = new ProgressBar(context);
			pBar.Indeterminate = true;
			layout.AddView(pBar);

			if (message != null)
			{
				TextView text = new TextView(context);
				text.Text = message;
				text.TextSize = 18;
				text.Gravity = GravityFlags.CenterVertical;
				text.SetPadding(Utils.DpToPx(4), 0, 0, 0);
				layout.AddView(text, new LayoutParams(LayoutParams.WrapContent, LayoutParams.MatchParent));
			}

			var lp = new LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);
			lp.AddRule(LayoutRules.CenterInParent);
			AddView(layout, lp);
		}
	}
}

