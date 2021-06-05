using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using RegionMon.Services;
using UIKit;
using Xamarin.Forms;

namespace RegionMon.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public RegionLocationManager Manager { get; set; }

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Rg.Plugins.Popup.Popup.Init();
            global::Xamarin.Forms.Forms.Init();
            Xamarin.FormsMaps.Init();

            LoadApplication(new App());

            Task.Delay(3000).ContinueWith(t =>
            {
                // Show Notifications Message
                InvokeOnMainThread(() => this.RegisterNotifications(app));

                t.ContinueWith(t2 => {
                    Task.Delay(3000).ContinueWith(t3 => {

                        // Create Region Manager Dependency Service
                        InvokeOnMainThread(() => Manager = (RegionLocationManager)DependencyService.Get<IRegionMonitor>());

                        t3.ContinueWith(t4 => Task.Delay(5000).ContinueWith(
                            t5 => InvokeOnMainThread(() => this.LocationAvailabilityChecks(app)))); // Do all availability checks
                    });
                });
            });

            return base.FinishedLaunching(app, options);
        }

        [Export("applicationWillEnterForeground:")]
        public override void WillEnterForeground(UIApplication application)
        {
            if (Manager != null)
            {
                Task.Delay(3000).ContinueWith(t =>
                {
                    InvokeOnMainThread(() =>
                    {
                        // Do all availability checks when returning from background in case the user changed something
                        this.LocationAvailabilityChecks(application);
                    });
                });
            }
        }

        public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        {
            // show an alert
            UIAlertController okayAlertController = UIAlertController.Create(notification.AlertAction, notification.AlertBody, UIAlertControllerStyle.Alert);
            okayAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(okayAlertController, true, null);

            // reset our badge
            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
        }

        [Export("applicationDidEnterBackground:")]
        public override void DidEnterBackground(UIApplication application)
        {
        }

        [Export("applicationWillTerminate:")]
        public override void WillTerminate(UIApplication application)
        {
        }
    }
}
