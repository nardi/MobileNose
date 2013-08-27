using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace DroidNose
{
	public class LinkedScrollView : ScrollView
	{
		private ISet<LinkedScrollView> Links;
		private ICollection<LinkedScrollView> ScrollOrigin;

		public LinkedScrollView(Context context) :
			base (context)
		{
			Links = new HashSet<LinkedScrollView>();
			ScrollOrigin = new List<LinkedScrollView>();
		}

		public void LinkTo(LinkedScrollView lsv)
		{
			if (lsv != null)
			{
				Links.Add(lsv);
				lsv.Links.Add(this);
				Post(() =>
				{
					ScrollTo(lsv.ScrollX, lsv.ScrollY);
				});	
			}
		}

		public void Unlink(LinkedScrollView lsv)
		{
			Links.Remove(lsv);
			lsv.Links.Remove(this);
		}

		public void Unlink()
		{
			while (Links.Count > 0)
				Unlink(Links.First());
		}

		private void ScrollBy(int x, int y, LinkedScrollView origin)
		{
			ScrollOrigin.Add(origin);
			base.ScrollBy(x, y);
		}

		private void ScrollTo(int x, int y, LinkedScrollView origin)
		{
			ScrollOrigin.Add(origin);
			base.ScrollTo(x, y);
		}

		protected override void OnScrollChanged(int x, int y, int oldx, int oldy)
		{
			foreach (var lsv in Links)
			{
				if (!ScrollOrigin.Contains(lsv))
					lsv.ScrollTo(x, y, this);
			}
			ScrollOrigin.Clear();
		}
	}
}

