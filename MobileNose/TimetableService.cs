using System;

namespace MobileNose
{
	public partial class TimetableService
	{
		public static readonly Uri ServiceUri = new Uri("http://content.datanose.nl/Timetable.svc/");

		public TimetableService() : this(ServiceUri)
		{
		}
	}
}