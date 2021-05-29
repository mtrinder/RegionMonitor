using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace RegionMon.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "Region Monitor";

            //OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamarin-quickstart"));
        }

        //public ICommand OpenWebCommand { get; }
    }
}