using System;
using Android.Content;
using Android.Util;
using Android.OS;
using Android.Views.InputMethods;

namespace MobileNose
{
	public static partial class Utils
    {
		private static Context _context = null;
		public static Context Context
		{
			get
			{
			    if (_context != null)
					return _context;
			    throw new Exception("Context not set");
			}
		    set
			{
			    if (value == null) return;
			    _context = value;
			    DisplayMetrics = value.Resources.DisplayMetrics;
			}
		}

		public static DisplayMetrics DisplayMetrics { get; private set; }

		public static float ScreenDensity
		{
			get { return DisplayMetrics.Density; }
		}

		public static int DpToPx(float dips)
		{
			if (dips == 0)
				return 0;
			return Math.Max((int)Math.Round(dips * ScreenDensity), 1);
		}

		public static int PxToDp(float pixels)
		{
			if (pixels == 0)
				return 0;
			return Math.Max((int)Math.Round(pixels / ScreenDensity), 1);
		}

		public static bool IsInPortraitMode
		{
			get { return DisplayMetrics.HeightPixels > DisplayMetrics.WidthPixels; }
		}

		public static void HideOnScreenKeyboard(IBinder window)
		{
			var manager = (InputMethodManager)Context.GetSystemService(Context.InputMethodService);
			manager.HideSoftInputFromWindow(window, HideSoftInputFlags.None);
		}
	}
}

