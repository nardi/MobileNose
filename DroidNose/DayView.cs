using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using MobileNose;

namespace DroidNose
{
	public class DayView : TimeLayout
	{
		private Day Day;
		private Timetable Timetable;
		private HourView HourView;

		private DateTitleView dtView = null;
		private LinkedScrollView dayScrollView = null;
		private bool showScroll = true;
		private Exception errorUpdating = null;

	    public override bool VerticalScrollBarEnabled
        {
            get { return showScroll; }
	        set
	        {
	            showScroll = value;
	            if (dayScrollView != null)
	                dayScrollView.VerticalScrollBarEnabled = value;
	        }
        }

	    public DayView(Context context, Day day, Timetable timetable, HourView hourView) : base(context)
		{
			Day = day;
			Timetable = timetable;
			HourView = hourView;
		}

		protected override void Update()
		{
			RemoveAllViews();

			dtView = new DateTitleView(Context, Day);
			AddView(dtView);

			var dayEvents = Timetable.GetEvents(Day, day => {}, (day, events) => Update(), e =>
            {
                errorUpdating = e;
                Console.WriteLine(e);
                Update();
            });

			if (errorUpdating != null && !dayEvents.Any())
			{
				TextView errorText = new TextView(Context);
				errorText.Text = "Er is een fout opgetreden:\n\n"
				 				 + errorUpdating.ToShortString() + "\n\n"
								 + "Raak deze tekst aan om het opnieuw te proberen.";
				errorText.Gravity = GravityFlags.Center;
				errorText.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
				int padding = Utils.DpToPx(40);
				errorText.SetPadding(padding, 0, padding, 0);
				errorText.Clickable = true;
				errorText.Click += (sender, e) =>
				{
					errorUpdating = null;
					Update();
				};
				AddView(errorText);
			}
			else
			{			
				if (Timetable.UpdatesInProgress.Contains(Day.Week))
				{
					AddView(new LoadingView(Context));
				}
				else
				{
					if (dayScrollView != null)
						dayScrollView.Unlink();
					dayScrollView = new LinkedScrollView(Context);
					dayScrollView.LinkTo(HourView.ScrollView);
					dayScrollView.VerticalScrollBarEnabled = showScroll;
					if (Build.VERSION.SdkInt >= BuildVersionCodes.GingerbreadMr1)
                    	dayScrollView.OverScrollMode = OverScrollMode.Never;
					AddView(dayScrollView);

					RelativeLayout layout = MakeLayout(dayEvents, StartHour);
					layout.SetPadding(0, HourHeight / 2, 0, 0);
                    int numHours = HourView.EndHour - HourView.StartHour + 1;
					layout.LayoutParameters.Width = LayoutParams.MatchParent;
                    layout.LayoutParameters.Height = numHours * HourView.HourHeight;
                    //layout.SetMinimumHeight(numHours * HourView.HourHeight);
                    //Console.WriteLine("MinHeight: " + numHours * HourView.HourHeight);
					dayScrollView.AddView(layout);
				}
			}
		}

		private RelativeLayout MakeLayout(IEnumerable<Event> events, double startHour)
		{
			RelativeLayout layout = new RelativeLayout(Context);
			layout.LayoutParameters = new LayoutParams(0, LayoutParams.WrapContent, 0.5f);

			List<SortedSet<Event>> horizontalEventGroups = GroupHorizontalEvents(events);
			SortedSet<Event> lastHEG = null;

			foreach (SortedSet<Event> horizontalEventGroup in horizontalEventGroups)
			{
				Event firstEvent = events.Min();
				double groupStart = (firstEvent.StartTime - Day.StartTime).TotalHours;

				LinearLayout horizontalEvents = new LinearLayout(Context);
				horizontalEvents.Orientation = Orientation.Horizontal;

				RelativeLayout.LayoutParams groupParams =
					new RelativeLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);

				groupParams.AddRule(LayoutRules.AlignParentTop);

				double emptyTime = groupStart - startHour;
				groupParams.TopMargin = (int)(emptyTime * HourHeight);

				List<SortedSet<Event>> verticalEventGroups = GroupVerticalEvents(horizontalEventGroup);

				foreach (SortedSet<Event> verticalEventGroup in verticalEventGroups)
				{
					Event ev = verticalEventGroup.First();
					double eventStart = (ev.StartTime - Day.StartTime).TotalHours;
					double emptyGroupTime = eventStart - groupStart;
					if (verticalEventGroup.Count == 1)
					{
						EventView view = new EventView(Context, ev);
						view.HourHeight = HourHeight;
						if (lastHEG != null)
						{
							view.SetBorder(true, !lastHEG.All(lastEvent => lastEvent.EndTime == ev.StartTime), true, true);
						}

						LayoutParams lp = (LayoutParams)view.LayoutParameters;
						lp.TopMargin = (int)(emptyGroupTime * HourHeight);
						view.LayoutParameters = lp;

						horizontalEvents.AddView(view);
					}
					else
					{
						RelativeLayout groupLayout = MakeLayout(verticalEventGroup, eventStart);
						groupLayout.SetPadding(0, (int)emptyGroupTime, 0, 0);
						horizontalEvents.AddView(groupLayout);
					}
				}

				layout.AddView(horizontalEvents, groupParams);

				lastHEG = horizontalEventGroup;
			}

			return layout;
		}

		private static List<SortedSet<Event>> GroupHorizontalEvents(IEnumerable<Event> events)
		{
			List<Event> eventsToCheck = new List<Event>(events);
			List<SortedSet<Event>> eventGroupsList = new List<SortedSet<Event>>();

			while (eventsToCheck.Count > 0)
			{
				SortedSet<Event> eventGroup = new SortedSet<Event>();
				eventGroup.Add(eventsToCheck[0]);

				foreach (Event candidate in eventsToCheck)
				{
					foreach (Event member in eventGroup)
					{
						if (candidate.StartsDuring(member))
						{
							eventGroup.Add(candidate);
							break;
						}	
					}
				}

				foreach (Event member in eventGroup)
				{
					eventsToCheck.Remove(member);
				}
				eventGroupsList.Add(eventGroup);
			}

			return eventGroupsList;
		}

		private static List<SortedSet<Event>> GroupVerticalEvents(IEnumerable<Event> events)
		{
			List<Event> eventsToCheck = new List<Event>(events);
			List<SortedSet<Event>> eventGroupsList = new List<SortedSet<Event>>();

			while (eventsToCheck.Count > 0)
			{
				SortedSet<Event> eventGroup = new SortedSet<Event>();
				eventGroup.Add(eventsToCheck[0]);

				Event currentEvent = eventsToCheck[0];
				foreach (Event candidate in eventsToCheck)
				{
					if (candidate.StartTime >= currentEvent.EndTime)
					{
						eventGroup.Add(candidate);
						currentEvent = candidate;
					}
				}

				foreach (Event member in eventGroup)
				{
					eventsToCheck.Remove(member);
				}
				eventGroupsList.Add(eventGroup);
			}

			return eventGroupsList;
		}
	}
}
