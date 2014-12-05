using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

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
        private IDictionary<Week, Task> _updateTasks;

        [NonSerialized]
        private object _updateLock;

		protected abstract IEnumerable<Event> DownloadEvents(Week week);

        private void Initialize()
        {
            _updatesInProgress = new List<Week>();
            _updateTasks = new Dictionary<Week, Task>();
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

        public virtual void Update(Week week, Action<IEnumerable<Event>> onResult, Action<Exception> onError)
        {
            lock (_updateLock)
            {
                if (!_updatesInProgress.Contains(week))
                {
                    _updatesInProgress.Add(week);
                    _updateTasks[week] = Task.Run(() =>
                    {
                        var newEvents = DownloadEvents(week).OrderBy(ev => ev);

                        lock (_updateLock)
                        {
                            _events.RemoveAll(e => e.StartTime.IsDuring(week));
                            _events.AddRange(newEvents);
                            _events.Sort();

                            UpdateLog[week] = DateTime.UtcNow;

                            _updatesInProgress.Remove(week);
                        }
                    });
                }
                _updateTasks[week].ContinueHere(task =>
                {
                    onResult(_events.Where(e => e.StartTime.IsDuring(week)).OrderBy(ev => ev));
				}, task =>
				{
                    onError(task.Exception.InnerException);
                });
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
				Update(week, events => onUpdate(day, events), onError);
				return true;
			}
			return false;
		}

		public IOrderedEnumerable<Event> GetEvents(Day day, Action<Day> onUpdateStart, Action<Day, IEnumerable<Event>> onUpdateFinish, Action<Exception> onError)
		{
			var currentDayEvents = new List<Event>();
			foreach (var ev in _events)
			{
				if (ev.StartsDuring(day))
					currentDayEvents.Add(ev);
			}
			Week week = day.Week;
			if (UpdateNeeded(week))
			{
				Utils.RunOnUiThread(onUpdateStart, day);
				Update(week, events => onUpdateFinish(day, events), onError);
			}
			return currentDayEvents.OrderBy(ev => ev);
		}
	}
}

