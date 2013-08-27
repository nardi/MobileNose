using System;
using System.Linq;
using Android.App;
using Android.Support.V4.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using MobileNose;

namespace DroidNose
{
	[Activity(Label = "DataNose", MainLauncher = true)]
	public class MainActivity : FragmentActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);
		}
	}
}


