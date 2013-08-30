using System.Collections.Generic;
using Android.Content;
using Mono.Android.Crasher.Data;
using Java.Lang;
using Java.Util;
using Java.IO;

namespace DroidNose
{
    public class LogDataReportProvider : ICustomReportDataProvider
    {
        public IDictionary<string, string> GetReportData(Context context)
        {
            StringBuilder log = new StringBuilder();
            try
            {
                var process = Runtime.GetRuntime().Exec("su -c logcat -d");
                var reader = new BufferedReader(new InputStreamReader(process.InputStream));

                string line;
                while ((line = reader.ReadLine()) != null)
                  log.Append(line);
            }
            catch (Exception e)
            {
				log.Append(e.Message);
            }
            
            return new Dictionary<string, string>
            {
                { "logcat",  log.ToString() }
            };
        }
    }
}