using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Support.V4.App;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using LayoutParams = Android.Views.ViewGroup.LayoutParams;
using MobileNose;

namespace DroidNose
{
	public class DateTitleView : TextView
	{
		public DateTitleView(Context context, Day date) : base(context)
		{
			Date = date;
			TextSize = 14;

			int padding = Utils.DpToPx(8);
			SetPadding(0, padding, 0, padding);
			Gravity = GravityFlags.Center;
			LayoutParameters = new LayoutParams(LayoutParams.MatchParent, Utils.DpToPx(40));
		}

		private Day _Date;
		public Day Date
		{
			get { return _Date; }
			set
			{
				_Date = value;
				if (_Date == null)
					Text = "";
				else
					Text = _Date.StartTime.ToString("dddd d MMMM");
			}
		}
	}
}

