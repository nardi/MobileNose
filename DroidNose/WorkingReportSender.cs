using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Android.App;
using Mono.Android.Crasher.Data;
using Mono.Android.Crasher.Data.Submit;

namespace DroidNose
{
	public class WorkingReportSender : IReportSender
	{
		public void Initialize(Application application)
		{
		}

		public void Send(ReportData errorContent)
		{
			FeedbackActivity.SendFeedback("Droidnose crash", errorContent.ToString());
		}
	}
}