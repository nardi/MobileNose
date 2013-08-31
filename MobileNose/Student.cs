using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;

namespace MobileNose
{
    [Serializable]
	public class Student
	{
		public int Id { get; set; }
		public ISet<Course> Courses { get; set; }
		public IDictionary<Course, Group> Groups { get; set; }
		public DateTime UpdateTime { get; set; }

		public void AddCourse(TTCourse ttc)
		{
			Courses.Add(new Course(ttc.ID,
			                       ttc.CatalogNumber,
			                       Utils.UnescapeUnicode(ttc.Name),
			                       ttc.AcademicYear));
		}

        public void Update()
        {
            var svc = new TimetableService();
            var courseUri = new Uri("GetCoursesByStudent?id=" + Id, UriKind.Relative);
            var groupUri = new Uri("GetGroupsByStudent?id=" + Id, UriKind.Relative);
            var responses = svc.ExecuteBatch(new DataServiceRequest<TTCourse>(courseUri),
                                             new DataServiceRequest<TTGroup>(groupUri));

            Courses = new HashSet<Course>();
            var groups = new Dictionary<Course, Group>();

            foreach (QueryOperationResponse response in responses)
            {
                if (response.Query.ElementType == typeof(TTCourse))
                {
                    foreach (TTCourse ttc in response)
                    {
						AddCourse(ttc);
                    }
                }
                else if (response.Query.ElementType == typeof(TTGroup))
                {
                    foreach (TTGroup ttg in response)
                    {
                        var course = Courses.FirstOrDefault(c => c.CatalogNumber == ttg.CatalogNumber);
                        if (course != null)
                            groups[course] = new Group(ttg.ID, ttg.Identifier, course);
                    }
                }
            }

            Groups = groups;
            UpdateTime = DateTime.UtcNow;
        }

		public Student()
		{
		}

		protected Student(int studentId)
			: this()
		{
			Id = studentId;
			Update();
		}

		public static readonly Func<int, Student> Download = studentId =>
		{
			return new Student(studentId);
		};

        public static readonly Func<int, Student> Load = studentId =>
        {
            return Download(studentId);
        };
	}
}

