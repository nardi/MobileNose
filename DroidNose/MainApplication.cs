using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using MobileNose;
using Mono.Android.Crasher;
using Mono.Android.Crasher.Attributes;
using Mono.Android.Crasher.Data.Submit;

namespace DroidNose
{
    [Application]
    [Crasher]//(UseCustomData = true, CustomDataProviders = new[] { typeof(LogDataReportProvider) })]
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