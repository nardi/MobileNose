using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Text;
using Android.Text.Style;
using Android.Graphics;

using MobileNose;

namespace DroidNose
{
	using LayoutParams = LinearLayout.LayoutParams;

	[Activity]
	public class FeedbackActivity : Activity
	{
    	private const string ServiceUrl = "http://nardilam.nl/dnfb.php";
    	private const string SuccessResponse = "success";

    	/* private const string Message = "Ben je helemaal tevreden met deze prachtige applicatie"
			+ " en wil je dit graag aan iedereen laten weten? Schrijf hieronder maar een" 
			+ " berichtje, dan geef ik het wel door.\nOok als je wel iets op te merken hebt aan dit"
			+ " hemels stukje programmeerwerk kan je hier wat achterlaten en krijg je zo snel mogelijk"
			+ " bericht terug, zodat je tenminste weet waarom je nu precies fout zit.";
		private const string ItalicsString = "waarom"; */

		private const string Message = "Heb je iets wat je graag over deze applicatie wil laten weten? Dat kan je hier doen:";

		private LinearLayout layout;
		private TextView messageText;
		private ProgressBar loading;
		private Button submit;
		private LayoutParams submitParams;
		private bool isLoading = false;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			int margin = Utils.DpToPx(0.02f * Utils.DisplayMetrics.WidthPixels);
            int id = 0;

			/* SpannableStringBuilder prettyMessage = new SpannableStringBuilder(Message);
			int italicsFrom = Message.IndexOf(ItalicsString), italicsTo = italicsFrom + ItalicsString.Length;
			prettyMessage.SetSpan(new StyleSpan(TypefaceStyle.Italic), italicsFrom, italicsTo, SpanTypes.ExclusiveExclusive); */

			layout = new LinearLayout(this);
			layout.Orientation = Orientation.Vertical;
            layout.SetBackgroundColor(Color.White);

			ScrollView sv = new ScrollView(this);
			LayoutParams scrollParams = new LayoutParams(LayoutParams.MatchParent, 0);
			scrollParams.Weight = 1;
			//this.layout.addView(sv, scrollParams);

			LinearLayout innerLayout = new LinearLayout(this);
			innerLayout.Orientation = Orientation.Vertical;
			LayoutParams ilParams = new LayoutParams(LayoutParams.MatchParent, 0);
			ilParams.Weight = 1;
			sv.AddView(innerLayout, ilParams);

			messageText = new TextView(this);
			messageText.Text = Message;
			//messageText.SetText(prettyMessage, TextView.BufferType.Spannable);
			messageText.TextSize = 14;
			messageText.Gravity = GravityFlags.Left | GravityFlags.CenterVertical;        
			LayoutParams textParams = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
			textParams.Weight = 0;
			textParams.Gravity = GravityFlags.Left | GravityFlags.CenterVertical;
			textParams.SetMargins(margin, margin, margin, margin/2);
			layout.AddView(messageText, textParams);

            EditText detail = new EditText(this);
            detail.Id = id++;
            detail.Hint = "Wat wil je zeggen?";
            detail.InputType = InputTypes.ClassText | InputTypes.TextFlagMultiLine;
            detail.Gravity = GravityFlags.Top;
            LayoutParams detailParams = new LayoutParams(LayoutParams.MatchParent, 0);
            detailParams.Weight = 0.5f;
            detailParams.SetMargins(margin, 0, margin, 0);
            layout.AddView(detail, detailParams);

			EditText sender = new EditText(this);
            sender.Id = id++;
			sender.InputType = InputTypes.ClassText;
			sender.Hint = "Hoe kan ik je bereiken?";
			LayoutParams senderParams = new LayoutParams(LayoutParams.MatchParent, LayoutParams.WrapContent);
			senderParams.Weight = 0;
			senderParams.SetMargins(margin, 0, margin, 0);
			layout.AddView(sender, senderParams);

			loading = new ProgressBar(this) { Indeterminate = true };

			submit = new Button(this);
			submit.Text = "Versturen";
			submitParams = new LayoutParams(LayoutParams.WrapContent, LayoutParams.WrapContent);
			submitParams.Weight = 0;
			submitParams.SetMargins(0, 0, 0, margin);
			submitParams.Gravity = GravityFlags.Center;

			submit.Click += (s, e) =>
			{
				Utils.HideOnScreenKeyboard(sender.WindowToken);
				Utils.HideOnScreenKeyboard(detail.WindowToken);
				layout.RemoveView(submit);
				layout.AddView(loading, submitParams);

				Task.Factory.StartNew(() => SendFeedback(sender.Text, detail.Text)).ContinueHere(
					completeTask =>
				{
					Toast.MakeText(Utils.Context, "Je feedback is verzonden!", ToastLength.Short).Show();
					Finish();
				},
					faultedTask =>
				{
					Console.WriteLine(faultedTask.Exception.InnerException);
					messageText.SetHeight(messageText.Height);
					messageText.Gravity = GravityFlags.Center;
					messageText.Text = "Er ging iets mis met het versturen van je feedback.\n"
							+ "Misschien moet je het later nog eens proberen?";
					layout.RemoveView(loading);
					layout.AddView(submit, submitParams);
					isLoading = false;
				});

				isLoading = true;
			};

			if (isLoading)
				layout.AddView(loading, submitParams);
			else
				layout.AddView(submit, submitParams);

			SetContentView(layout);
		}

		public static void SendFeedback(string type, string detail)
		{
			var request = WebRequest.Create(ServiceUrl);
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			var queryBuilder = new StringBuilder("fb_type=")
		        .Append(Uri.EscapeDataString(type))
		        .Append("&fb_detail=")
		        .Append(Uri.EscapeDataString(detail));
			request.ContentLength = queryBuilder.Length;

			using (var output = new StreamWriter(request.GetRequestStream()))
				output.Write(queryBuilder.ToString());

			using (var input = new StreamReader(request.GetResponse().GetResponseStream()))
			{
				var response = input.ReadLine();
				if (response != SuccessResponse)
					throw new Exception("Error on server-side: " + response);
			}
		}
    }
}