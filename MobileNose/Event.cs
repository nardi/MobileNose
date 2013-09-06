using System;
using System.Collections.Generic;

namespace MobileNose
{
    [Serializable]
	public class Event : TimePeriod, IEquatable<Event>
	{
		public int Id { get; set; }
		public Course Course { get; set; }
        public ICollection<Group> Groups { get; set; }
		public string Type { get; set; }
        public string Description { get; set; }
		public ISet<Location> Locations { get; set; }
		public ICollection<string> Staff { get; set; }

		public Event(int id, DateTime startTime, TimeSpan duration, Course course, ICollection<Group> groups,
		             string type, string description, ISet<Location> locations, ICollection<string> staff)
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

        public bool Equals(Event ev)
        {
            return Id == ev.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj is Event)
                return Equals(obj as Event);
            return false;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public int CompareTo(Event ev)
        {
            var timeCompare = base.CompareTo(ev);
            if (timeCompare == 0 && !this.Equals(ev))
                return Id < ev.Id ? -1 : 1;
            return timeCompare;
        }

        public override int CompareTo(object obj)
        {
            return CompareTo(obj as Event);
        }
	}
}

