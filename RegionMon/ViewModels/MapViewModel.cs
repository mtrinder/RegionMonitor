using RegionMon.Services;
using Xamarin.Forms;

namespace RegionMon.ViewModels
{
    public class MapViewModel : BaseViewModel
    {
        public Command ClearCommand { get; }
        public Command SetCommand { get; }

        double latitude, longitude;

        public MapViewModel()
        {
            Title = "Map";

            ClearCommand = new Command(OnClearClicked);
            SetCommand = new Command(OnSetClicked);
        }

        private void OnSetClicked(object obj)
        {
            var regionMonitor = DependencyService.Get<IRegionMonitor>();
            if (regionMonitor != null)
            {
                regionMonitor.ClearRegion();
                regionMonitor.RegisterLocationWithRadius(latitude, longitude, 500); // 500 metre radius
            }
        }

        private void OnClearClicked(object obj)
        {
            var regionMonitor = DependencyService.Get<IRegionMonitor>();
            if (regionMonitor != null)
            {
                regionMonitor.ClearRegion();
            }
        }

        public void SetLocation(double lat, double lon)
        {
            latitude = lat;
            longitude = lon;
        }
    }
}
