using System;
using CoreLocation;
using Foundation;
using RegionMon.Services;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace RegionMon.iOS
{
    public static class AppDelegateExt
    {
        public static void ShowMessage(this AppDelegate appDelegate, string title, string message)
        {
            var ignore = Preferences.Get(message, false);
            if (ignore) return;

            var newAlert = new UIAlertView() { Title = title, Message = message };
            newAlert.AddButton("OK");
            newAlert.AddButton("Don't Show Again");
            newAlert.Show();
            
            newAlert.Clicked += NewAlert_Clicked;
        }

        private static void NewAlert_Clicked(object sender, UIButtonEventArgs e)
        {
            var alert = (UIAlertView)sender;
            alert.Clicked -= NewAlert_Clicked;
            if (e.ButtonIndex == 1)
            {
                Preferences.Set(alert.Message, true);
            }
        }

        public static void RegisterNotifications(this AppDelegate appDelegate, UIApplication app)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                var notificationSettings = UIUserNotificationSettings.GetSettingsForTypes(
                    UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound, null
                );

                app.RegisterUserNotificationSettings(notificationSettings);
            }
        }


        public static bool LocationAvailabilityChecks(this AppDelegate appDelegate, UIApplication application)
        {
            const string Title = "{0} is Off";
            const string Message = "To ensure you get your Welcome Message on race day, go to settings and turn on {0}";

            var inet = Connectivity.NetworkAccess;
            if (inet == NetworkAccess.None || inet == NetworkAccess.Unknown)
            {
                appDelegate.ShowMessage(string.Format(Title, "The Internet"), string.Format(Message, "the internet"));
                return false;
            }

            if (CLLocationManager.LocationServicesEnabled)
            {
                if (application.BackgroundRefreshStatus == UIBackgroundRefreshStatus.Available)
                {
                    if (!CLLocationManager.IsMonitoringAvailable(typeof(CLCircularRegion)))
                    {
                        appDelegate.ShowMessage(string.Format(Title, "Tracking Unavailable"), "Your device is not supported. Do a manual check-in when you reach the track.");
                        return false;
                    }

                    if (CLLocationManager.Status == CLAuthorizationStatus.Denied ||
                        CLLocationManager.Status == CLAuthorizationStatus.NotDetermined ||
                        CLLocationManager.Status == CLAuthorizationStatus.Restricted)
                    {
                        appDelegate.ShowMessage(string.Format(Title, "Location Services"), "To ensure you get your Welcome Message on race day, authorize this app to use your location 'Always'");
                        return false;
                    }

                    if (!CLLocationManager.RegionMonitoringAvailable)
                    {
                        appDelegate.ShowMessage(string.Format(Title, "Your Location"), "We can't determine your current location. Do a manual check-in when you reach the track.");
                    }
                    else
                    {
                        ((RegionLocationManager)DependencyService.Get<IRegionMonitor>()).StartLocationUpdates();
                        
                        return true;
                    }
                }
                else
                {
                    appDelegate.ShowMessage(string.Format(Title, "Background Refresh"), string.Format(Message, "Background Refresh"));
                }
            }
            else
            {
                appDelegate.ShowMessage(string.Format(Title, "Location Services"), string.Format(Message, "Location Services"));
            }

            return false;
        }

    }
}
