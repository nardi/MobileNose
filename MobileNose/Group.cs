using System;

namespace MobileNose
{
    [Serializable]
	public class Group
	{
		public int Id { get; set; }
		public string Identifier { get; set; }
		public Course Course { get; set; }

        public Group()
        {
        }

		public Group(int id, string identifier, Course course)
			: this()
		{
			Id = id;
			Identifier = identifier;
			Course = course;
		}
	}
}

