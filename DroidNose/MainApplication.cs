using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using MobileNose;
using Mono.Android.Crasher;
using Mono.Android.Crasher.Attributes;
using Mono.Android.Crasher.Data.Submit;
using System.Threading;

namespace DroidNose
{
	#if DEBUG
	[Application(Debuggable=true)]
	#else
	[Application(Debuggable=false)]
	#endif
    [Crasher(UseCustomData = true, CustomDataProviders = new[] { typeof(CustomDataReportProvider) })]
    public class MainApplication : Application
    {
        public MainApplication(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();
            Utils.Context = ApplicationContext;
            CrashManager.Initialize(this);
            CrashManager.AttachSender(() => new WorkingReportSender());
        }
    }
}