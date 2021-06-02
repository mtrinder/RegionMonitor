using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Android.OS;
using System;
using Android.Locations;

namespace RegionMon.Droid
{
    [Service]
    public class LocationService : Service
    {
        LocationListener _listener;

        Android.Locations.LocationManager _locationManager;

        private class LocationListener : Java.Lang.Object, ILocationListener
        {
            Location _location;

            public LocationListener(string provider)
            {
                _location = new Location(provider);
            }

            public void OnLocationChanged(Location location)
            {
                if (location != null)
                {
                    Console.WriteLine($"{location.Latitude}, {location.Longitude}");

                    _location.Set(location);
                }
            }

            public void OnProviderDisabled(string provider) { }
            public void OnProviderEnabled(string provider) { }
            public void OnStatusChanged(string provider, Availability status, Bundle extras) { }
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            base.OnCreate();

            _locationManager = (Android.Locations.LocationManager)this.ApplicationContext.GetSystemService(Context.LocationService);

            _listener = new LocationListener(Android.Locations.LocationManager.PassiveProvider);

            _locationManager.RequestLocationUpdates(Android.Locations.LocationManager.PassiveProvider, 1000, 10, _listener);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            base.OnStartCommand(intent, flags, startId);

            const int pendingIntentId = 0;

            var context = Application.Context;

            var pendingIntent = PendingIntent.GetActivity(context, pendingIntentId, intent, PendingIntentFlags.OneShot);

            var notification = new NotificationCompat.Builder(context)
                .SetChannelId(MainActivity.ChannelId)
                .SetContentTitle("Region Monitor")
                .SetContentText("Location tracking is running for background region triggers")
                .SetSmallIcon(Resource.Drawable.icon_about)
                .SetContentIntent(pendingIntent)
                .SetOngoing(true)
                .Build();

            StartForeground(10101, notification);

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            _locationManager.RemoveUpdates(_listener);
        }
    }
}

