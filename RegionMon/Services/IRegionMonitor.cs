using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RegionMon.Services
{
    public enum RegionStatus
    {
        Success,        // Added to Monitoring List
        Monitoring,     // Already in Monitoring List
        Failed,         // Rejected by OS 
    }

    public struct Coordinate
    {
        public double Latitude;
        public double Longitude;
    }

    public interface IRegionMonitor
    {
        void ClearRegion();
        void ClearRegion(double latitude, double longitude, double radius);
        RegionStatus RegisterLocationWithRadius(double latitude, double longitude, double radius);
        Coordinate GetUserCoordinate();
    }
}
