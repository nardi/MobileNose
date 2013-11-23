using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;

using MobileNose;

namespace DroidNose
{
	public class TimetableFragment : Fragment
	{
		public static readonly string PreferencesFile = "DroidNoseSettings";
		public static readonly string StudentId = "StudentId";
		public static readonly string StudentIdHistory = "StudentIdHistory";
		public static readonly int PreloadDays = 6;

		public static int CurrentStudentId { get; private set; }

		public TimetableFragment() : base()
		{
			RetainInstance = true;
			base.SetHasOptionsMenu(true);
		}

		public bool SelectingStudentId = false;
		public bool IsLoading = false;
		public StudentTimetable Timetable = null;
		public Day StartDay = null;

		public LinearLayout Layout { get; protected set; }

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
            Activity.Title = "Persoonlijk rooster";
            Layout = new LinearLayout(Activity);

			if (Timetable != null)
			{
				ShowTimetableView();
			}
            else if (IsLoading)
            {
                ShowLoadingView();
            }
            else
            {
                TryLoadStudentId();
            }

			return Layout;
		}

		public void SetContentView(View v)
		{
			Layout.RemoveAllViews();
			Layout.AddView(v);
		}

		private void TryLoadStudentId()
		{
			var settings = Activity.GetSharedPreferences(PreferencesFile, FileCreationMode.Private);
			var studentId = settings.GetInt(StudentId, -1);

			if (studentId != -1)
				LoadTimetableUsingStudentId(studentId);
			else
				GetNewStudentId("Er is nog geen studentnummer bekend. Deze kan je hier invullen, als je wilt:");
		}

		private void LoadTimetableUsingStudentId(int studentId)
		{
			ShowLoadingView();
			Timetable = null;
			CurrentStudentId = studentId;
			StudentTimetable.Load.AsyncInvoke(studentId, ShowTimetableView, e =>
			{
                Console.WriteLine(e);
				GetNewStudentId("Er is een fout opgetreden bij het ophalen "
							  + "van het rooster voor dit studentnummer:\n\n"
							  + e.ToShortString() + "\n\n"
							  + "Controleer aub of het ingevoerde nummer klopt.",
								studentId.ToString());
			});
		}

		public void GetNewStudentId(string message)
		{
			GetNewStudentId(message, null);
		}

		public void GetNewStudentId(string message, string defaultInput)
		{
            var existingFragment = FragmentManager.FindFragmentByTag(StudentId);
            if (existingFragment == null)
            {
                var fragment = new StudentIdFragment(message, defaultInput, Timetable != null);
                fragment.OnStudentId = studentId =>
                {
                    SelectingStudentId = false;
                    if (studentId != -1)
                    {
                        LoadTimetableUsingStudentId(studentId);
                    }
                    else if (Timetable != null)
                    {
                        ShowTimetableView();
                    }
                    else
                    {
                        Activity.Finish();
                    }
                };
                fragment.Show(FragmentManager, StudentId);
                SelectingStudentId = true;
            }
		}

		public void ShowLoadingView()
		{
			IsLoading = true;

			ShowOptionMenu(false);
			
			SetContentView(new LoadingView(Activity, "Rooster wordt geladen..."));
		}

		public void ShowTimetableView()
		{
			ShowTimetableView(null, null);
		}

		public void ShowTimetableView(StudentTimetable timetable)
		{
			ShowTimetableView(timetable, null);
		}

		public void ShowTimetableView(Day startDay)
		{
			ShowTimetableView(null, startDay);
		}

		public void ShowTimetableView(StudentTimetable timetable, Day startDay)
		{
			if (startDay == null)
			{
				if (StartDay != null)
					startDay = StartDay;
				else
					startDay = Day.Today;
			}
			if (timetable == null)
			{
				if (Timetable != null)
					timetable = Timetable;
				else
					return;
			}

			TimetableView timetableView = new TimetableView(Activity, timetable, startDay, this);
			SetContentView(timetableView);

			IsLoading = false;
			Timetable = timetable;
			StartDay = startDay;

			ShowOptionMenu(true);

			var settings = Activity.GetSharedPreferences(PreferencesFile, FileCreationMode.Private);

            var studentIdHistory = settings.GetStringSet(StudentIdHistory, new List<string>(1));
			studentIdHistory.Add(Timetable.Student.Id.ToString());

			var settingsEditor = settings.Edit();
			settingsEditor.PutInt(StudentId, Timetable.Student.Id);
			settingsEditor.PutStringSet(StudentIdHistory, studentIdHistory);
			settingsEditor.Commit();

			Day currentDay = startDay;
			for (int i = 0; i < PreloadDays; i++)
			{
				Timetable.UpdateIfNeeded(currentDay, (day, events) => {}, e => Console.WriteLine(e));
				currentDay = MultiDayView.GetFirstValidDay(currentDay, 1);
         	}
		}

		List<IMenuItem> menuItems = new List<IMenuItem>();
		ISubMenu dateSubMenu;
		IMenuItem chooseWeek, chooseDate, newStudentId, toToday, manualRefresh, feedback;

		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
		{
			menuItems.Clear();

			int itemId = 0, order = 0;
			dateSubMenu = menu.AddSubMenu(1, itemId++, order++, "Datumopties");
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb)
                dateSubMenu.Item.SetShowAsAction(ShowAsAction.Always);
			dateSubMenu.Item.SetIcon(Android.Resource.Drawable.IcMenuToday);
			menuItems.Add(dateSubMenu.Item);
			if (Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb)
			{
				chooseWeek = dateSubMenu.Add(1, itemId++, order++, "Ga naar week");
				menuItems.Add(chooseWeek);
			}
			chooseDate = dateSubMenu.Add(1, itemId++, order++, "Ga naar datum");
			menuItems.Add(chooseDate);
			toToday = dateSubMenu.Add(1, itemId++, order++, "Naar vandaag");
			menuItems.Add(toToday);
			newStudentId = menu.Add(2, itemId++, order++, "Verander studentnummer");
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb)
			    newStudentId.SetShowAsAction(ShowAsAction.IfRoom);
			newStudentId.SetIcon(Android.Resource.Drawable.IcMenuMyCalendar);
			menuItems.Add(newStudentId);
			manualRefresh = menu.Add(3, itemId++, order++, "Handmatig verversen");
			menuItems.Add(manualRefresh);
			feedback = menu.Add(4, itemId++, order++, "Feedback geven");
			menuItems.Add(feedback);

            ShowOptionMenu(!IsLoading);
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			if (item == chooseDate)
			{
				DialogFragment newFragment = new ChooseDateFragment(StartDay, ShowTimetableView);
				newFragment.Show(FragmentManager, "chooseDate");
				return true;
			}
			else if (item == chooseWeek)
			{
				DialogFragment newFragment = new ChooseWeekFragment(StartDay.Week,
					week => ShowTimetableView(week.StartTime.GetDay()));
				newFragment.Show(FragmentManager, "chooseWeek");
				return true;
			}
			else if (item == newStudentId)
			{
				string defaultInput = null;
				if (Timetable != null)
					defaultInput = Timetable.Student.Id.ToString();
				GetNewStudentId("Voer hier het nieuwe studentnummer in:", defaultInput);
				return true;
			}
			else if (item == toToday)
			{
				ShowTimetableView(Day.Today);
				return true;
			}
			else if (item == manualRefresh)
			{
				if (Timetable != null)
				{
					StudentTimetable.Download.AsyncInvoke(Timetable.Student.Id, ShowTimetableView,
                        e => Toast.MakeText(Activity, "Fout bij handmatig verversen", ToastLength.Long).Show());
					return true;
				}
				return false;
			}
			else if (item == feedback)
			{
				Intent intent = new Intent(Activity, typeof(FeedbackActivity));
				StartActivity(intent);
				return true;
			}
			
            return base.OnOptionsItemSelected(item);
		}

		private void ShowOptionMenu(bool show)
		{
			Utils.RunOnUiThread(() =>
			{
				foreach (IMenuItem m in menuItems)
					m.SetVisible (show);
			});
		}
	}
}

