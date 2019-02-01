using System;
using System.Collections.Generic;
using App1.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using App1.Views;
using FoodLog.Common;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace App1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            DependencyService.Register<ApiWrapper>();

            AppCenter.Start("android=30c09476-8233-4db3-b375-d0a8323afce5;ios=f1be0ead-b517-4c95-b61a-8fa51d079d41;",
                typeof(Analytics), typeof(Crashes));

            Messenger.Instance.Register<Exception>("Exception", ex => { Reporter.ReportException(ex); });

            MainPage = new MainPage();
        }

        protected override async void OnStart()
        {
            try
            {
                if (await Crashes.HasCrashedInLastSessionAsync())
                {
                    var crashReport = await Crashes.GetLastSessionCrashReportAsync();
                    Reporter.ReportException(crashReport.Exception ?? new Exception("Crash from last run; no exception info."), new Dictionary<string, string> { { "Source", "AppCrash" } });
                }
            }
            catch
            {
                // ignored
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
