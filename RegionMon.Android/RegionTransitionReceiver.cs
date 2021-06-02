using Android.App;
using Android.Gms.Location;
using Android.Content;
using Android.Support.V4.App;

namespace RegionMon.Droid
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class RegionTransitionReceiver : BroadcastReceiver
    {
        Context _context;
        public override void OnReceive(Context context, Intent intent)
        {
            _context = context;

            var geofencingEvent = GeofencingEvent.FromIntent(intent);
            if (geofencingEvent.HasError) { return; }

            int geofenceTransition = geofencingEvent.GeofenceTransition;

            if (geofenceTransition == Geofence.GeofenceTransitionEnter ||
                geofenceTransition == Geofence.GeofenceTransitionExit)
            {
                string geofenceTransitionString = GetTransitionString(geofenceTransition);

                SendNotification(geofenceTransitionString);
            }
        }

        void SendNotification(string notificationDetails)
        {
            var notificationIntent = new Intent(_context, typeof(MainActivity));

            var stackBuilder = Android.Support.V4.App.TaskStackBuilder.Create(_context);
            stackBuilder.AddParentStack(Java.Lang.Class.FromType(typeof(MainActivity)));
            stackBuilder.AddNextIntent(notificationIntent);

            var notificationPendingIntent = stackBuilder.GetPendingIntent(0, (int)PendingIntentFlags.UpdateCurrent);

            var builder = new NotificationCompat.Builder(_context, MainActivity.ChannelId);

            builder.SetSmallIcon(Resource.Drawable.icon_about)
                .SetContentTitle(notificationDetails)
                .SetContentText("Region Triggered")
                .SetVisibility((int)NotificationVisibility.Public)
                .SetContentIntent(notificationPendingIntent);

            var notificationManager = (NotificationManager)_context.GetSystemService(Context.NotificationService);
            notificationManager.Notify(11011, builder.Build());
        }

        string GetTransitionString(int transitionType)
        {
            switch (transitionType)
            {
                case Geofence.GeofenceTransitionEnter:
                    return "You have entered the region";
                case Geofence.GeofenceTransitionExit:
                    return "You have left the region";
                default:
                    return "";
            }
        }
    }
}

