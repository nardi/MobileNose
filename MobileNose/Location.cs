using System;

namespace MobileNose
{
	[Serializable]
	public class Location : IEquatable<Location>
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string InfoUrl { get; set; }

		public Location()
		{
			Id = 0;
			Name = "";
			InfoUrl = "";
		}

		public Location(int id, string name, string infoUrl)
			: this()
		{
			Id = id;
			Name = name;
			InfoUrl = infoUrl;
		}

		public bool Equals(Location l)
		{
			return Id == l.Id;
		}

		public override bool Equals(object obj)
		{
			if (obj is Location)
				return Equals(obj as Location);
			return false;
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}
	}
}

