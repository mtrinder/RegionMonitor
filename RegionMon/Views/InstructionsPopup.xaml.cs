using Rg.Plugins.Popup.Extensions;

namespace RegionMon.Views
{
    public partial class InstructionsPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        public InstructionsPopup()
        {
            InitializeComponent();
        }

        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            await Navigation.PopPopupAsync(true);
        }
    }
}
