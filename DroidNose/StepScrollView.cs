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
	public abstract class StepScrollView : HorizontalScrollView
	{
		private static readonly double ScrollTolerance = 0.11;

		public int StepSize
		{
			get; set;
		}
		public int CurrentStep
		{
			get; private set;
		}

		public StepScrollView(Context context) : base(context)
		{
			Touch += (sender, tea) =>
			{
				tea.Handled = false;
				var action = tea.Event.Action;
				if ((action == MotionEventActions.Up || action == MotionEventActions.Cancel) && StepSize != 0)
				{
					double stepPosition = (double)ScrollX / StepSize;
					double distanceFromCurrentStep = stepPosition - CurrentStep;
					int stepDiff = (int)(Math.Sign(distanceFromCurrentStep) *
					                     Math.Ceiling(Math.Abs(distanceFromCurrentStep)));

					if (Math.Abs(distanceFromCurrentStep) > ScrollTolerance)
						CurrentStep += stepDiff;

					OnStepChange(CurrentStep);

					tea.Handled = true;
				}
			};
		}

		public void GoToStep(int step)
		{
			CurrentStep = step;
			Post(() =>
	     	{
				SmoothScrollTo(CurrentStep * StepSize, 0);
			});
		}

		protected abstract void OnStepChange(int step);
	}
}

