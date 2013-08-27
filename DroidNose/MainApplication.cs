using System;
using Android.App;
using Android.Runtime;
using MobileNose;

namespace DroidNose
{
    [Application]
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
        }
    }
}