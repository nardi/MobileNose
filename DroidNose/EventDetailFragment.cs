using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Hosting;
using System.Text;

using Android.App;
using Android.Locations;
using Support = Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using MobileNose;

namespace DroidNose
{
    public class EventDetailFragment : Support.DialogFragment
    {
        public static EventDetailFragment Create(Event ev)
        {
            var arguments = new Bundle();

            arguments.PutString("titleText",
                ev.Type + " " + ev.Course.Name + " (" + ev.Course.CatalogNumber + ")");
            if (!string.IsNullOrWhiteSpace(ev.Description))
                arguments.PutString("groupText", ev.Description);
            else if (ev.Groups.Count > 0)
                arguments.PutString("groupText", "Groep " + ev.Groups.First().Identifier);
            arguments.PutString("programmeText",
                "");
            arguments.PutString("dateText",
                ev.StartTime.ToString("ddd ") + ev.StartTime.ToShortDateString());
            arguments.PutString("weekText",
                "Week " + ev.StartTime.GetWeekOfYear());
            arguments.PutString("timeText",
                ev.StartTime.ToShortTimeString() + " - " + ev.EndTime.ToShortTimeString());

            var length = ev.Locations.Count;
            var groupTexts = new string[length];
            var staffTexts = new string[length];
            var locationTexts = new string[length];
			var locationInfo = new string[length];
            var capacityTexts = new string[length];

            int i = 0;
            foreach (var location in ev.Locations)
            {
                groupTexts[i] = ev.Groups.Count > 0 ? "Groep " + ev.Groups.First().Identifier : "";
                staffTexts[i] = ev.Staff.Count > 0 ? ev.Staff.JoinStrings(", ") : "";
                locationTexts[i] = location.Name;
				locationInfo[i] = location.InfoUrl;
                capacityTexts[i] = "";
                i++;
            }

            arguments.PutStringArray("groupTexts", groupTexts);
            arguments.PutStringArray("staffTexts", staffTexts);
            arguments.PutStringArray("locationTexts", locationTexts);
			arguments.PutStringArray("locationInfo", locationInfo);
            arguments.PutStringArray("capacityTexts", capacityTexts);

            return new EventDetailFragment { Arguments = arguments };
        }

        public override Dialog OnCreateDialog(Bundle bundle)
        {
            var layout = Activity.LayoutInflater.Inflate(Resource.Layout.EventDetailLayout, null);

            layout.FindViewById<TextView>(Resource.Id.titleText).Text
                = Arguments.GetString("titleText");
            var groupText = Arguments.GetString("groupText");
            if (groupText != null)
                layout.FindViewById<TextView>(Resource.Id.groupText).Text = groupText;
            else
                layout.FindViewById<TextView>(Resource.Id.groupText).Visibility = ViewStates.Gone;
            layout.FindViewById<TextView>(Resource.Id.programmeText).Text
                = Arguments.GetString("programmeText");
            layout.FindViewById<TextView>(Resource.Id.dateText).Text
                = Arguments.GetString("dateText");
            layout.FindViewById<TextView>(Resource.Id.weekText).Text
                = Arguments.GetString("weekText");
            layout.FindViewById<TextView>(Resource.Id.timeText).Text
                = Arguments.GetString("timeText");

            var detailsLayout = layout.FindViewById<BorderedLinearLayout>(Resource.Id.detailsLayout);
            var groupTexts = Arguments.GetStringArray("groupTexts");
            var staffTexts = Arguments.GetStringArray("staffTexts");
            var locationTexts = Arguments.GetStringArray("locationTexts");
			var locationInfo = Arguments.GetStringArray("locationInfo");
            var capacityTexts = Arguments.GetStringArray("capacityTexts");
            var length = locationTexts.Length;

            if (length == 0)
                detailsLayout.Visibility = ViewStates.Gone;

            for (int i = 0; i < length; i++)
            {
                var details = Activity.LayoutInflater.Inflate(Resource.Layout.EventDetailsListLayout, null);
                if (!string.IsNullOrWhiteSpace(groupTexts[i]))
                    details.FindViewById<TextView>(Resource.Id.groupText).Text = groupTexts[i];
                else
                    details.FindViewById<TextView>(Resource.Id.groupText).Visibility = ViewStates.Gone;
                if (!string.IsNullOrWhiteSpace(staffTexts[i]))
                    details.FindViewById<TextView>(Resource.Id.staffText).Text = staffTexts[i];
                else
                    details.FindViewById<TextView>(Resource.Id.staffText).Visibility = ViewStates.Gone;
				var locationView = details.FindViewById<TextView>(Resource.Id.locationText);
				locationView.Text = locationTexts[i];
				locationView.Clickable = true;
				var locationId = i;
				locationView.Click += (sender, e) => 
				{
                    try
                    {
                        StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse(locationInfo[locationId])));
                    }
                    catch {}
				};
                details.FindViewById<TextView>(Resource.Id.capacityText).Text = capacityTexts[i];
                detailsLayout.AddView(details);
            }

            return new AlertDialog.Builder(Activity).SetView(layout).Create();
        }
    }
}