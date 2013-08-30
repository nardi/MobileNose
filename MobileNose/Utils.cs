using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
#if __ANDROID__
using Android;
using Android.OS;
using Android.Content;
#endif

namespace MobileNose
{
	public static partial class Utils
	{
        public static Stream ReadFile(string filename)
        {
#if __ANDROID__
			return Utils.Context.OpenFileInput(filename);
#else
            throw new Exception("ReadFile was called but no suitable implementation was found.");
#endif
        }

        public static Stream CreateFile(string filename)
        {
#if __ANDROID__
			return Utils.Context.OpenFileOutput(filename, FileCreationMode.Private);
#else
            throw new Exception("CreateFile was called but no suitable implementation was found.");
#endif
        }

        public static Stream AppendToFile(string filename)
        {
#if __ANDROID__
			return Utils.Context.OpenFileOutput(filename, FileCreationMode.Append);
#else
            throw new Exception("AppendToFile was called but no suitable implementation was found.");
#endif
        }

		public static string UnescapeUnicode(string s)
		{    
			int length = s.Length;
			var sb = new StringBuilder(length);

			int i = 0;
		    while (i < length)
			{
				char c = s[i];
				if (i + 1 < length && s.Substring(i, 2) == "\\u")
				{
					i += 2;
					c = (char)Convert.ToInt32(s.Substring(i, 4), 16);
					i += 4;
				}
				else
				{
					i++;
				}
				sb.Append(c);
			}
			return sb.ToString();
		}

		public static string ToShortString(this Exception e)
		{
		    var sourceMatch = Regex.Match(e.StackTrace, @"at \w+Nose\.(\S+)");
		    var source = sourceMatch.Success ? sourceMatch.Groups[1].Value : e.Source;
            e = e.GetBaseException();
		    if (string.IsNullOrWhiteSpace(source))
		        source = e.Source;
			var message = e.Message;
            if (message.StartsWith("<?xml")) using (var reader = XmlReader.Create(new StringReader(message)))
			{
				reader.ReadToDescendant("message");
				message = reader.ReadElementContentAsString();
			}
			var s = e.GetType().Name;
			if (!string.IsNullOrWhiteSpace(message))
				s += ": \"" + message + "\"";
			if (!string.IsNullOrWhiteSpace(source))
				s += " in " + source;
			return s;
		}

	    public static string JoinStrings(this IEnumerable<string> strings, string glue)
	    {
	        return strings.Aggregate((s1, s2) => s1 + glue + s2);
	    }

		public static void RunOnUiThread(Action action)
		{
#if __ANDROID__
			new Handler(Looper.MainLooper).Post(action);
#else
#if WINDOWS_PHONE
			Deployment.Current.Dispatcher.BeginInvoke(action);
#else
            throw new Exception("RunOnUiThread was called but no suitable implementation was found.");
#endif
#endif
		}

		public static void RunOnUiThread<T>(Action<T> action, T arg)
		{
			RunOnUiThread(() => action(arg));
		}

		public static Task AsyncInvoke<T, TResult>(this Func<T, TResult> toInvoke, T arg, Action<TResult> callback, Action<Exception> onError)
		{
			return Task.Factory.StartNew(() =>
			{
				try
				{
					RunOnUiThread(callback, toInvoke(arg));
				}
				catch (Exception e)
				{
					RunOnUiThread(onError, e);
				}
			});
		}

		public static Task AsyncInvoke<TResult>(this Func<TResult> toInvoke, Action<TResult> callback, Action<Exception> onError)
		{
			return Task.Factory.StartNew(() =>
			{
				try
				{
					RunOnUiThread(callback, toInvoke());
				}
				catch (Exception e)
				{
					RunOnUiThread(onError, e);
				}
			});
		}

		public static Task AsyncInvoke<T>(this Action<T> toInvoke, T arg, Action callback, Action<Exception> onError)
		{
			return Task.Factory.StartNew(() =>
			{
				try
				{
					toInvoke(arg);
					RunOnUiThread(callback);
				}
				catch (Exception e)
				{
					RunOnUiThread(onError, e);
				}
			});
		}

		public static Task AsyncInvoke(this Action toInvoke, Action callback, Action<Exception> onError)
		{
			return Task.Factory.StartNew(() =>
			{
				try
				{
					toInvoke();
					RunOnUiThread(callback);
				}
				catch (Exception e)
				{
					RunOnUiThread(onError, e);
				}
			});
		}

	    public static Task ContinueHere(this Task task, Action<Task> continuationAction, TaskContinuationOptions taskContinuationOptions)
	    {
	        return task.ContinueWith(continuationAction, CancellationToken.None, taskContinuationOptions,
	            TaskScheduler.FromCurrentSynchronizationContext());
	    }

        public static Task ContinueHere(this Task task, Action<Task> continuationAction)
        {
            return task.ContinueWith(continuationAction, TaskScheduler.FromCurrentSynchronizationContext());
        }

		public static Tuple<Task, Task> ContinueHere(this Task task, Action<Task> successContinuation, Action<Task> faultContinuation)
		{
			return Tuple.Create( 
                task.ContinueHere(successContinuation, TaskContinuationOptions.OnlyOnRanToCompletion),
                task.ContinueHere(faultContinuation, TaskContinuationOptions.OnlyOnFaulted)
			);
		}

        public static Task ContinueHere<T>(this Task<T> task, Action<Task<T>> continuationAction, TaskContinuationOptions taskContinuationOptions)
        {
            return task.ContinueWith(continuationAction, CancellationToken.None, taskContinuationOptions,
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static Task ContinueHere<T>(this Task<T> task, Action<Task<T>> continuationAction)
        {
            return task.ContinueWith(continuationAction, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static Tuple<Task, Task> ContinueHere<T>(this Task<T> task, Action<Task<T>> successContinuation, Action<Task<T>> faultContinuation)
        {
			return Tuple.Create( 
            	task.ContinueHere(successContinuation, TaskContinuationOptions.OnlyOnRanToCompletion),
                task.ContinueHere(faultContinuation, TaskContinuationOptions.OnlyOnFaulted)
			);
        }
	}
}

