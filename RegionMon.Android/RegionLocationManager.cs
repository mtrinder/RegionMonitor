using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Gms.Location;
using Android.Gms.Tasks;
using RegionMon.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(RegionMon.Droid.RegionLocationManager))]
namespace RegionMon.Droid
{
    public class RegionLocationManager : LocationCallback, IRegionMonitor, IOnCompleteListener
    {
        double currentLatitude, currentLongitude;

        MainActivity _activity;
        GeofencingClient _geofencingClient;
        PendingIntent _geofencePendingIntent;
        FusedLocationProviderClient _fusedLocationProviderClient;

        public RegionLocationManager() { }

        GeofencingRequest GetGeofencingRequest(IGeofence region)
        {
            var builder = new GeofencingRequest.Builder();

            builder.SetInitialTrigger(GeofencingRequest.InitialTriggerEnter);
            builder.AddGeofences(new List<IGeofence> { region });

            return builder.Build();
        }

        PendingIntent GetGeofencePendingIntent()
        {
            if (_geofencePendingIntent != null)
            {
                return _geofencePendingIntent;
            }

            // RegionTransitionReceiver (BroadcastReceiver) gets the geofence triggers
            var intent = new Intent(_activity, typeof(RegionTransitionReceiver));

            return _geofencePendingIntent = PendingIntent.GetBroadcast(_activity, 0, intent, PendingIntentFlags.UpdateCurrent);
        }

        public void OnComplete(Android.Gms.Tasks.Task task)
        {
            if (!task.IsSuccessful)
            {
                Console.WriteLine("AddOnCompleteListener failed");
            }
        }

        public override void OnLocationAvailability(LocationAvailability locationAvailability)
        {
            Console.WriteLine("OnLocationAvailability");
        }

        public override void OnLocationResult(LocationResult result)
        {
            if (result.Locations.Any())
            {
                var location = result.Locations.First();

                currentLatitude = location.Latitude;
                currentLongitude = location.Longitude;

                MessagingCenter.Send<IRegionMonitor>(this, "locationUpdated");
            }
        }

        /// <summary>
        /// Start caching the current position with the FusedLocationProvider
        /// </summary>
        /// <param name="activity"></param>
        [Obsolete]
        public async void StartLocationUpdates(MainActivity activity)
        {
            if (_activity == null)
            {
                _activity = activity;
            }

            if (_geofencingClient == null)
            {
                _geofencingClient = LocationServices.GetGeofencingClient(activity);
            }

            if (_fusedLocationProviderClient == null)
            {
                _fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(activity);
            }

            await _fusedLocationProviderClient.RequestLocationUpdatesAsync(new LocationRequest()
                                  .SetPriority(LocationRequest.PriorityHighAccuracy)
                                  .SetInterval(20 * 1000)
                                  .SetFastestInterval(10 * 1000), this);
        }

        /// <summary>
        /// Clear the cached region
        /// </summary>
        public void ClearRegion()
        {
            if (_geofencingClient != null)
            {
                _geofencingClient.RemoveGeofences(GetGeofencePendingIntent()).AddOnCompleteListener(this);
            }
        }

        /// <summary>
        /// Start monitoring the region with the GeofencingClient
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public RegionStatus RegisterLocationWithRadius(double latitude, double longitude, double radius)
        {
            try
            {
                if (_geofencingClient == null)
                {
                    return RegionStatus.Failed;
                }

                var id = string.Format("{0}:{1}:{2}", latitude, longitude, radius);

                var region = new GeofenceBuilder()
                    .SetRequestId(id)
                    .SetCircularRegion(latitude, longitude, (float)radius)
                    .SetTransitionTypes(Geofence.GeofenceTransitionEnter | Geofence.GeofenceTransitionExit)
                    .SetExpirationDuration(Geofence.NeverExpire)
                    .Build();

                _geofencingClient.AddGeofences(GetGeofencingRequest(region), GetGeofencePendingIntent()).AddOnCompleteListener(this);

                MessagingCenter.Send<IRegionMonitor>(this, "monitoringRegion");

                return RegionStatus.Success;
            }
            catch (Exception)
            {
                return RegionStatus.Failed;
            }
        }

        /// <summary>
        /// Get the last position
        /// </summary>
        /// <returns></returns>
        public Coordinate GetUserCoordinate()
        {
            return new Coordinate { Latitude = currentLatitude, Longitude = currentLongitude };
        }
    }
}
