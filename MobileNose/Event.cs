using System;
using System.Collections.Generic;

namespace MobileNose
{
    [Serializable]
	public class Event : TimePeriod
	{
		public int Id { get; set; }
		public Course Course { get; set; }
        public ICollection<Group> Groups { get; set; }
		public string Type { get; set; }
        public string Description { get; set; }
		public ISet<string> Locations { get; set; }
		public ICollection<string> Staff { get; set; }

		public Event(int id, DateTime startTime, TimeSpan duration, Course course, ICollection<Group> groups,
		             string type, string description, ISet<string> locations, ICollection<string> staff)
			: base(startTime, duration)
		{
			Id = id;
			Course = course;
		    Groups = groups;
			Type = type;
		    Description = description;
			Locations = locations;
			Staff = staff;
		}

		public override string ToString()
		{
			return Type + " " + Course.Name + " op " + StartTime.ToShortDateString() + " van " +
				StartTime.ToShortTimeString() + " tot " + EndTime.ToShortTimeString();
		}
	}
}

