using System;

namespace MobileNose
{
    [Serializable]
	public class Course
	{
		public int Id { get; set; }
		public string CatalogNumber { get; set; }
		public string Name { get; set; }
		public int AcademicYear { get; set; }

        public Course()
        {
        }

		public Course(int id, string catalogNumber, string name, int academicYear)
			: this()
		{
			Id = id;
			CatalogNumber = catalogNumber;
			Name = name;
			AcademicYear = academicYear;
		}
	}
}

