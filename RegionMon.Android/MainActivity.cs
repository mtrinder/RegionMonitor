using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android;
using Xamarin.Forms;
using RegionMon.Services;
using System.Threading.Tasks;

namespace RegionMon.Droid
{
    [Activity(Label = "RegionMon", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static string ChannelId = "100101";

        const int RequestLocationId = 0;

        public RegionLocationManager Manager { get; set; }

        readonly string[] LocationPermissions =
        {
            Manifest.Permission.AccessCoarseLocation,
            Manifest.Permission.AccessFineLocation,
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Rg.Plugins.Popup.Popup.Init(this);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            Xamarin.FormsMaps.Init(this, savedInstanceState);

            // Create a notification channel to show local Notifications
            CreateNotificationChannel();

            LoadApplication(new App());

            Task.Delay(2000).ContinueWith(t =>
            {
                Manager = (RegionLocationManager)DependencyService.Get<IRegionMonitor>(); // Create Region Manager

                t.ContinueWith(t2 => {
                    Task.Delay(2000).ContinueWith(t3 => {
                        Device.BeginInvokeOnMainThread(() => Manager.StartLocationUpdates(this)); // Start getting locations

                        // We need to have this service running in foreground so that
                        // when the app moves into the background we continue to get region (geofence) updates
                        t3.ContinueWith(t4 => Task.Delay(2000).ContinueWith(
                            t5 => Device.BeginInvokeOnMainThread(() => StartService(new Android.Content.Intent(this, typeof(LocationService))))));
                    });
                });
            });

        }

        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            var channel = new NotificationChannel(ChannelId, "Region Monitor", NotificationImportance.Default)
            {
                Description = "Region Monitor Test Harness"
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            if (requestCode == RequestLocationId)
            {
                if ((grantResults.Length == 1) && (grantResults[0] == (int)Permission.Granted))
                {
                    // Permissions granted - display a message.
                }
                else
                {
                    // Permissions denied - display a message.
                }
            }
            else
            {
                Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            }
        }

        protected override void OnStart()
        {
            base.OnStart();

            if ((int)Build.VERSION.SdkInt >= 23)
            {
                if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) != Permission.Granted)
                {
                    RequestPermissions(LocationPermissions, RequestLocationId);
                }
                else
                {
                    // Permissions already granted - display a message.
                }
            }
        }

        public override void OnBackPressed()
        {
            Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed);
        }
    }
}