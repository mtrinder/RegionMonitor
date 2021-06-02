using RegionMon.Services;
using RegionMon.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

using Rg.Plugins.Popup.Extensions;

namespace RegionMon.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestMapPage : ContentPage
    {
        int count;
        readonly MapViewModel vm;

        public TestMapPage()
        {
            InitializeComponent();

            MessagingCenter.Subscribe<IRegionMonitor>(this, "regionLeft", (sender) => {
                // Nothing do here ... A local "push" notification will be fire from the Native OS.
            });

            MessagingCenter.Subscribe<IRegionMonitor>(this, "regionEntered", (sender) => {
                // Nothing do here ... A local "push" notification will be fire from the Native OS.
            });

            MessagingCenter.Subscribe<IRegionMonitor>(this, "monitoringRegion", async (sender) => {
                await Navigation.PushPopupAsync(new RegionPopup());
            });

            this.BindingContext = vm = new MapViewModel();

            // Set the map to default postion
            map.MoveToRegion(
                MapSpan.FromCenterAndRadius(
                    new Position(-33.89449312993648, 151.1413278143307), Distance.FromKilometers(800)));

            // The user location will be zoomed to for a few seconds with a 2km radius
            // After 2 seconds the location will continue to be saved but the map will stop zooming
            MessagingCenter.Subscribe<IRegionMonitor>(this, "locationUpdated", rm =>
            {
                var regionMonitor = DependencyService.Get<IRegionMonitor>();
                if (regionMonitor != null)
                {
                    var coord = regionMonitor.GetUserCoordinate();
                    //if (count++ < 3)
                    {
                        map.MoveToRegion(
                            MapSpan.FromCenterAndRadius(
                                new Position(coord.Latitude, coord.Longitude), Distance.FromKilometers(2)));
                    }

                    vm.SetLocation(coord.Latitude, coord.Longitude);
                }
            });
        }

        bool showOnce;
        protected async override void OnAppearing()
        {
            base.OnAppearing();

            if (!showOnce)
            {
                showOnce = true;
                await Navigation.PushPopupAsync(new InstructionsPopup());
            }
        }
    }
}
