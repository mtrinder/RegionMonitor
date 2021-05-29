using System;
using Foundation;
using UIKit;
using Xamarin.Essentials;

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

        public static void StartLocationManager(this AppDelegate appDelegate)
        {
            //appDelegate.Manager.LocationUpdated += HandleLocationChanged;
            //appDelegate.Manager.StartLocationUpdates();
        }
    }
}
