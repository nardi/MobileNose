using System;

namespace MobileNose
{
    [Serializable]
	public class TimePeriod : IComparable, IEquatable<TimePeriod>
	{
		public DateTime StartTime { get; private set; }
		private TimeSpan _duration;
		public TimeSpan Duration
		{
			get { return _duration; }
			private set
			{
				_duration = value;
				_endTime = StartTime + value;
			}
		}
		private DateTime _endTime;
		public DateTime EndTime
		{
			get { return _endTime; }
			private set
			{
				_endTime = value;
				_duration = EndTime - StartTime;
			}
		}

		public TimePeriod(DateTime start, TimeSpan duration)
		{
			StartTime = start;
			Duration = duration;
		}

		public TimePeriod(DateTime start, DateTime end)
		{
			StartTime = start;
			EndTime = end;
		}

		public bool StartsDuring(TimePeriod tp)
		{
			return StartTime.IsDuring(tp);
		}

		public bool EndsDuring(TimePeriod tp)
		{
			return EndTime.IsDuring(tp) || EndTime == tp.EndTime;
		}

		public bool IsDuring(TimePeriod tp)
		{
			return StartsDuring(tp) && EndsDuring(tp);
		}

		public int CompareTo(TimePeriod tp)
		{
			return StartTime.CompareTo(tp.StartTime);
		}

        public virtual int CompareTo(object obj)
		{
			return CompareTo(obj as TimePeriod);
		}

		public bool Equals(TimePeriod tp)
		{
			return StartTime == tp.StartTime && EndTime == tp.EndTime;
		}

		public override bool Equals(object obj)
		{
			if (obj is TimePeriod)
				return Equals(obj as TimePeriod);
			return false;
		}

		public override int GetHashCode()
		{
			return StartTime.GetHashCode() + 31 * EndTime.GetHashCode();
		}
	}
}

