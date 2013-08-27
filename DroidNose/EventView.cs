using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Support = Android.Support.V4.App;
using Android.App;
using Android.Graphics;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using MobileNose;

namespace DroidNose
{
	public class EventView : TimeLayout
	{
		public readonly Event Event;

		private RelativeLayout layout = null;
		private bool leftBorder = true,
					 topBorder = true,
					 bottomBorder = true,
					 rightBorder = true;
		private readonly int borderSize = 1;
		private Paint Paint = new Paint();

		public EventView(Context context, Event ev)
			: base(context)
		{
			Event = ev;
			Paint.Alpha = 0xAA;
			Paint.SetStyle(Paint.Style.Stroke);
			Paint.Color = Color.Black;
			Paint.StrokeWidth = borderSize * 2;
			LayoutParameters = new LayoutParams(0, 0, 0.5f);
			SetBackgroundColor(Color.Rgb(234, 234, 234));
		    if (Context is Support.FragmentActivity)
		    {
		        var fm = (Context as Support.FragmentActivity).SupportFragmentManager;
		        Clickable = true;
		        Click += (sender, e) => EventDetailFragment.Create(Event).Show(fm, "eventDetail");
		    }
		}

		protected override void Update()
		{
			RemoveAllViews();

			LayoutParameters.Height = (int)(HourHeight * Event.Duration.Hours);

			layout = new RelativeLayout(Context);
			layout.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent, 1);
			AddView(layout);

			LinearLayout textLayout = new LinearLayout(Context);
			textLayout.Orientation = Orientation.Vertical;
			int padding = Utils.DpToPx(8);
			textLayout.SetPadding(padding, padding, padding, padding);

			var textParams = new RelativeLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);
			textParams.AddRule(LayoutRules.CenterHorizontal);
			textParams.TopMargin = borderSize;

			layout.AddView(textLayout, textParams);

			TextView course = new TextView(Context);
			course.Gravity = GravityFlags.CenterHorizontal;
			course.TextSize = 17;
			course.Text = Event.Course.Name;
			course.SetTextColor(Color.Black);
			textLayout.AddView(course);

			TextView other = new TextView(Context);
			other.Gravity = GravityFlags.CenterHorizontal;
			other.TextSize = 15;
            other.Text = Event.Type;
            if (Event.Locations.Count > 0)
                other.Text += ", " + Event.Locations.JoinStrings(", ");
			other.SetTextColor(Color.Black);
			textLayout.AddView(other);
		}

		public void SetBorder(bool l, bool t, bool b, bool r)
		{
			leftBorder = l;
			topBorder = t;
			bottomBorder = b;
			rightBorder = r;
		}

		protected override void OnDraw(Canvas canvas)
		{
			base.OnDraw(canvas);

			if (topBorder)
			{
				canvas.DrawLine(0, borderSize, Width, borderSize, Paint);
			}
			if (leftBorder)
			{
				canvas.DrawLine(borderSize, topBorder ? borderSize : 0, borderSize, Height, Paint);
			}
			if (rightBorder)
			{
				canvas.DrawLine(Width - borderSize, topBorder ? borderSize : 0, Width - borderSize, Height, Paint);
			}
			if (bottomBorder)
			{
				canvas.DrawLine(leftBorder ? borderSize : 0, Height - borderSize,
				                Width - (leftBorder ? borderSize : 0), Height - borderSize, Paint);
			}
		}
	}
}

