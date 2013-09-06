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
	public class HourView : TimeLayout
	{
		private static readonly int DefaultNumHours =
			TimeLayout.DefaultEndHour - TimeLayout.DefaultStartHour + 1;
		private static readonly int MinimumViewHeight = 550; //dp
		private static readonly int MaximumViewHeight = 800; //dp

		public LinkedScrollView ScrollView
		{
			get; private set;
		}
		private LinearLayout MainLayout = null;

		private ICollection<TimeLayout> HourHeightListeners;

		public HourView(Context context, int startHour, int endHour)
			: base(context, startHour, endHour)
		{
			ScrollView = new HourScrollView(context, this);
			if (Build.VERSION.SdkInt >= BuildVersionCodes.GingerbreadMr1)
            	ScrollView.OverScrollMode = OverScrollMode.Never;
			AddView(ScrollView);

			MainLayout = new LinearLayout(context);
			MainLayout.Orientation = Orientation.Vertical;
			ScrollView.AddView(MainLayout);

			HourHeightListeners = new List<TimeLayout>();
		}

		public void AddHourHeightListener(TimeLayout tl)
		{
			HourHeightListeners.Add(tl);
		}

		public void SetAvailableHeight(int height)
		{
			HourHeight = height / DefaultNumHours;
		}

		public override int HourHeight
		{
			get { return base.HourHeight; }
			set
			{
				base.HourHeight = value;
				foreach (TimeLayout tl in HourHeightListeners)
					tl.HourHeight = value;
			}
		}

		protected override void Update()
		{
			MainLayout.RemoveAllViews();

			for (int hour = StartHour; hour <= EndHour; hour++)
			{
				TextView v = new TextView(Context);
				v.SetHeight(HourHeight);
				v.SetPadding(Utils.DpToPx(8), 0, Utils.DpToPx(8), 0);
				v.Gravity = GravityFlags.CenterVertical | GravityFlags.Right;
				v.TextSize = 14;
				v.Text = hour.ToString();
				MainLayout.AddView(v);
			}
		}

		/*
		 * Een speciale ScrollView-klasse die de hourHeight van de
		 * hourView en de listeners set zodra deze bekend is.
		 * (Deze is afhankelijk van de grootte van de parentview.)
		 */
		private class HourScrollView : LinkedScrollView
		{
			private readonly HourView HourView;

			public HourScrollView(Context context, HourView hourView) : base(context)
			{
				VerticalScrollBarEnabled = false;
				HourView = hourView;
			}	

			private bool measured = false;
			protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				int parentWidth = View.MeasureSpec.GetSize(widthMeasureSpec);
				int parentHeight = View.MeasureSpec.GetSize(heightMeasureSpec);
				int minHeight = Utils.DpToPx(MinimumViewHeight);
				int maxHeight = Utils.DpToPx(MaximumViewHeight);

				if (!measured)
				{
					if (parentHeight > parentWidth)
						HourView.SetAvailableHeight(parentHeight);
					else
						HourView.SetAvailableHeight(Math.Min(Math.Max(parentWidth, minHeight), maxHeight));
					measured = true;
				}

				base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
			}
		}
	}
}

