using System;
using CoreLocation;
using Foundation;
using RegionMon.Services;
using UIKit;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(RegionMon.iOS.RegionLocationManager))]
namespace RegionMon.iOS
{
    public class RegionLocationManager : IRegionMonitor
    {
        const string Title = "{0} is Off";
        const string Message = "To ensure you get your Welcome Message on race day, go to settings and turn on {0}";

        bool started;
        double _currentLatitude, _currentLongitude;

        protected CLLocationManager _locationManager;

        public RegionLocationManager()
        {
            Preferences.Set("LastInEvent", default(DateTime));
            Preferences.Set("LastOutEvent", default(DateTime));

            _locationManager = new CLLocationManager
            {
                PausesLocationUpdatesAutomatically = false
            };

            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                _locationManager.AllowsBackgroundLocationUpdates = true;
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                _locationManager.RequestAlwaysAuthorization();
            }

            _locationManager.DidStartMonitoringForRegion += DidStartMonitoringRegion;
            _locationManager.RegionEntered += RegionEntered;
            _locationManager.RegionLeft += RegionLeft;
        }

        void RegionLeft(object sender, CLRegionEventArgs e)
        {
            var timeStamp = Preferences.Get("LastOutEvent", default(DateTime));

            if (timeStamp == default || DateTime.Now - timeStamp > TimeSpan.FromSeconds(20))
            {
                MessagingCenter.Send<IRegionMonitor>(this, "regionLeft");

                ShowRegionNotificationWithMessage("You have left the region");

                Preferences.Set("LastOutEvent", DateTime.Now);
            }
        }

        void RegionEntered(object sender, CLRegionEventArgs e)
        {
            var timeStamp = Preferences.Get("LastInEvent", default(DateTime));

            if (timeStamp == default || DateTime.Now - timeStamp > TimeSpan.FromSeconds(20))
            {
                MessagingCenter.Send<IRegionMonitor>(this, "regionEntered");

                ShowRegionNotificationWithMessage("You have entered the region");

                Preferences.Set("LastInEvent", DateTime.Now);
            }
        }

        void DidStartMonitoringRegion(object sender, CLRegionEventArgs e)
        {
            MessagingCenter.Send<IRegionMonitor>(this, "monitoringRegion");
        }

        void ShowRegionNotificationWithMessage(string message)
        {
            var notification = new UILocalNotification
            {
                FireDate = NSDate.FromTimeIntervalSinceNow(2),
                AlertAction = "Region Trigger",
                AlertBody = message,
                ApplicationIconBadgeNumber = 1,
                SoundName = UILocalNotification.DefaultSoundName
            };

            UIApplication.SharedApplication.ScheduleLocalNotification(notification);
        }

        void StartLocationUpdates()
        {
            if (!started)
            {
                _locationManager.LocationsUpdated += (object sender, CLLocationsUpdatedEventArgs e) =>
                {
                    if (e.Locations.Length > 0)
                    {
                        _currentLatitude = e.Locations[0].Coordinate.Latitude;
                        _currentLongitude = e.Locations[0].Coordinate.Longitude;
                    }

                    MessagingCenter.Send<IRegionMonitor>(this, "locationUpdated");
                };

                _locationManager.StartUpdatingLocation();
            }
        }

        /// <summary>
        /// Check that we can get position and do region monitoring
        /// </summary>
        /// <param name="appDelegate"></param>
        /// <param name="application"></param>
        /// <returns></returns>
        public bool LocationAvailabilityChecks(AppDelegate appDelegate, UIApplication application)
        {
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
                        StartLocationUpdates();
                        return started = true;
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

        /// <summary>
        /// Clear the cached region
        /// </summary>
        public void ClearRegion()
        {
            var regionId = Preferences.Get("CurrentRegion", string.Empty);

            if (!string.IsNullOrEmpty(regionId))
            {
                var parts = regionId.Split(":", StringSplitOptions.None);

                double latitude = double.Parse(parts[0]);
                double longitude = double.Parse(parts[1]);
                double radius = double.Parse(parts[2]);

                var circularRegion = new CLCircularRegion(new CLLocationCoordinate2D(latitude, longitude), radius, regionId);

                _locationManager.StopMonitoring(circularRegion);

                Preferences.Remove("CurrentRegion");
            }
        }

        /// <summary>
        /// Start monitoring the region.
        /// Check if the region is aleady registered.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public RegionStatus RegisterLocationWithRadius(double latitude, double longitude, double radius)
        {
            if (radius > _locationManager.MaximumRegionMonitoringDistance)
            {
                radius = _locationManager.MaximumRegionMonitoringDistance;
            }

            try
            {
                var id = string.Format("{0}:{1}:{2}", latitude, longitude, radius);

                var circularRegion = new CLCircularRegion(new CLLocationCoordinate2D(latitude, longitude), radius, id);

                if (!_locationManager.MonitoredRegions.Contains(circularRegion))
                {
                    _locationManager.StartMonitoring(circularRegion);
                }
                else
                {
                    return RegionStatus.Monitoring;
                }

                Preferences.Set("CurrentRegion", id);

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
            return new Coordinate { Latitude = _currentLatitude, Longitude = _currentLongitude };
        }
    }
}
