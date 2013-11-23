using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace MobileNose
{
    [Serializable]
    public class StudentTimetable : Timetable
    {
        public Student Student { get; set; }

        protected override IEnumerable<Event> DownloadEvents(Week week)
        {
            var events = new List<Event>();

            var svc = new TimetableService();
            var academicYear = week.Number >= 36 ? week.Year : week.Year - 1;
            var firstWeek = new Week(academicYear, 36);
            var academicWeek = (int)Math.Round((week.StartTime - firstWeek.StartTime).TotalDays/7);
            var activityUri = new Uri("GetActivitiesByStudent?id=" + Student.Id +
                                      "&week=" + academicWeek + "&acyear=" + academicYear,
                                      UriKind.Relative);

            var activityResponse = svc.Execute<TTActivity>(activityUri);
            var locationStaffRequests = new List<DataServiceRequest>();

            if (activityResponse != null)
            {
                foreach (TTActivity tta in activityResponse)
                {
                    var startTime = week.StartTime.AddDays(Math.Log(tta.Day)/Math.Log(2)).AddHours(tta.StartTime);
                    var duration = TimeSpan.FromHours(tta.Duration);
                    var course = Student.Courses.FirstOrDefault(c => tta.Name.Contains(c.CatalogNumber));
                    // If we don't know about this course, the student information needs to be updated
                    if (course == null)
                    {
                        Student.Update();
                        course = Student.Courses.FirstOrDefault(c => tta.Name.Contains(c.CatalogNumber));
                    }
                    var groups = new List<Group>();
                    if (course != null)
                    {
                        Group group;
                        if (Student.Groups.TryGetValue(course, out group) && tta.Groups.Contains(group.Identifier))
                            groups.Add(group);
                    }

                    if (course == null)
                    {
                        course = new Course(0, tta.Name, tta.Description, academicYear);
                    }

                    events.Add(new Event(tta.ID, startTime, duration, course, groups, tta.ActivityType,
                        tta.Description, new HashSet<Location>(), new List<string>()));

                    locationStaffRequests.Add(new DataServiceRequest<TTLocation>(
                        new Uri("GetLocationsByActivity?id=" + tta.ID, UriKind.Relative)));

                    locationStaffRequests.Add(new DataServiceRequest<TTStaff>(
                        new Uri("GetStaffByActivity?id=" + tta.ID, UriKind.Relative)));
                }

                if (locationStaffRequests.Count > 0)
                {
                    var locationResponses = svc.ExecuteBatch(locationStaffRequests.ToArray());

                    foreach (QueryOperationResponse response in locationResponses)
                    {
                        int activityId = int.Parse(response.Query.RequestUri.ToString().Split('=').Last());
                        var ev = events.Find(e => e.Id == activityId);
                        if (response.Query.ElementType == typeof (TTLocation))
                        {
							foreach (TTLocation ttl in response)
								ev.Locations.Add(new Location(ttl.ID, ttl.Name, ttl.InfoURL));
                        }
                        else if (response.Query.ElementType == typeof(TTStaff))
                        {
                            foreach (TTStaff tts in response) ev.Staff.Add(tts.Name);
                        }
                    }
                }
            }

            return events;
        }

        public override void Update(Week week, Action<IEnumerable<Event>> onResult, Action<Exception> onError)
        {
            if (DateTime.UtcNow - Student.UpdateTime > Timetable.UpdateInterval)
            {
				Task.Run(() => Student.Update())
                    .ContinueHere(task => Update(week, onResult, onError));
            }
            else
            {
                base.Update(week, events => 
                {
                    try
                    {
                        using (var file = Utils.CreateFile("StudentTimetable_" + Student.Id))
                        {
                            new BinaryFormatter().Serialize(file, this);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    onResult(events);
                }, Console.WriteLine);
            }
        }

        private StudentTimetable(int studentId)
        {
            Student = Student.Download(studentId);
        }

        public static Func<int, StudentTimetable> Download
        {
            get
            {
                return studentId => new StudentTimetable(studentId);
            }
        }

        public static Func<int, StudentTimetable> Load
        {
            get
            {
                return studentId =>
                {
                    try
                    {
                        using (var file = Utils.ReadFile("StudentTimetable_" + studentId))
                        {
                            return new BinaryFormatter().Deserialize(file) as StudentTimetable;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return Download(studentId);
                    }
                };
            }
        }
    }
}

