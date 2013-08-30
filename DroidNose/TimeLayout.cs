using Android.Content;
using Android.Widget;

namespace DroidNose
{
	public class TimeLayout : LinearLayout
	{
		public static readonly int DefaultStartHour = 9;
		public static readonly int DefaultEndHour = 17;

		int _HourHeight = 0;
		public virtual int HourHeight
		{
			get { return _HourHeight; }
			set
			{
				_HourHeight = value;
				Update();
			}
		}
		public int StartHour { get; protected set; }
		public int EndHour { get; protected set; }

		public TimeLayout(Context context) : this(context, DefaultStartHour, DefaultEndHour)
		{
		}

		public TimeLayout(Context context, int startHour, int endHour)
			: base(context)
		{
			StartHour = startHour;
			EndHour = endHour;
			Orientation = Orientation.Vertical;
			SetWillNotDraw(false);
		}

		public void SetHourRange(int startHour, int endHour)
		{
			StartHour = startHour;
			EndHour = endHour;
			Update();
		}

		protected virtual void Update()
		{
		}
	}
}

