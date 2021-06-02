using Xamarin.Forms;

namespace RegionMon.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (CarAnimation != null)
                {
                    CarAnimation.PlayAnimation();
                }
            });
        }
    }
}