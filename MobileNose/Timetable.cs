using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MobileNose
{
    [Serializable]
	public abstract class Timetable
	{
		public static readonly TimeSpan UpdateInterval = TimeSpan.FromHours(24);

        private List<Event> _events = new List<Event>();
        public IEnumerable<Event> Events
        {
            get
            {
                return _events;
            }
            set
            {
                _events = value.ToList();
                _events.Sort();
            }
        }

        private IDictionary<Week, DateTime> _updateLog = new Dictionary<Week, DateTime>();
        public IDictionary<Week, DateTime> UpdateLog
        {
            get { return _updateLog; }
        }

        [NonSerialized]
		private ICollection<Week> _updatesInProgress;
        public ICollection<Week> UpdatesInProgress
        {
            get { return _updatesInProgress; }
        }

        [NonSerialized]
        private object _updateLock;

		protected abstract IEnumerable<Event> DownloadEvents(Week week);

        private void Initialize()
        {
            _updatesInProgress = new List<Week>();
            _updateLock = new object();
        }

        protected Timetable()
        {
            Initialize();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            Initialize();
        }

        public virtual Func<Week, IOrderedEnumerable<Event>> Update
        {
            get
            {
                return week =>
                {
                    bool inProgress;
                    lock (_updateLock)
                    {
                        inProgress = _updatesInProgress.Contains(week);
                    }
                    if (inProgress)
                    {
                        while (_updatesInProgress.Contains(week)) ;
                        return _events.Where(e => e.StartTime.IsDuring(week)).OrderBy(ev => ev);
                    }

                    lock (_updateLock)
                    {
                        _updatesInProgress.Add(week);
                    }

                    var newEvents = DownloadEvents(week).OrderBy(ev => ev);

                    _events.RemoveAll(e => e.StartTime.IsDuring(week));
                    _events.AddRange(newEvents);
                    _events.Sort();

                    UpdateLog[week] = DateTime.UtcNow;

                    _updatesInProgress.Remove(week);

                    return newEvents;
                };
            }
        }

		public bool UpdateNeeded(Week week)
		{
			return !UpdateLog.ContainsKey(week) || DateTime.UtcNow - UpdateLog[week] > UpdateInterval;
		}

		public bool UpdateIfNeeded(Day day, Action<Day, IEnumerable<Event>> onUpdate, Action<Exception> onError)
		{
			Week week = day.Week;
			if (UpdateNeeded(week))
			{
				Update.AsyncInvoke(week, events => onUpdate(day, events), onError);
				return true;
			}
			return false;
		}

		public IOrderedEnumerable<Event> GetEvents(Day day, Action<Day> onUpdateStart, Action<Day, IEnumerable<Event>> onUpdateFinish, Action<Exception> onError)
		{
			var currentDayEvents = _events.Where(e => e.StartTime.IsDuring(day)).OrderBy(ev => ev);
			Week week = day.Week;
			if (UpdateNeeded(week))
			{
				Utils.RunOnUiThread(onUpdateStart, day);
				Update.AsyncInvoke(week, events => onUpdateFinish(day, events), onError);
			}
			return currentDayEvents;
		}
	}
}

