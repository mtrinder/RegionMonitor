using RegionMon.Services;
using RegionMon.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Maps;

namespace RegionMon.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapPage : ContentPage
    {
        int count;
        readonly MapViewModel vm;

        public MapPage()
        {
            InitializeComponent();
            this.BindingContext = vm = new MapViewModel();

            map.MoveToRegion(
                MapSpan.FromCenterAndRadius(
                    new Position(-37.82378470509698, 144.95942164858394), Distance.FromKilometers(50)));

            MessagingCenter.Subscribe<IRegionMonitor>(this, "locationUpdated", rm =>
            {
                var coord = DependencyService.Get<IRegionMonitor>().GetUserCoordinate();
                if (count++ < 10)
                {
                    map.MoveToRegion(
                        MapSpan.FromCenterAndRadius(
                            new Position(coord.Latitude, coord.Longitude), Distance.FromKilometers(2)));
                }

                vm.SetLocation(coord.Latitude, coord.Longitude);
            });
        }
    }
}