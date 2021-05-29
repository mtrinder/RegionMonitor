using System;
using System.ComponentModel;
using RegionMon.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace RegionMon.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();

            MessagingCenter.Subscribe<Application>(this, "refresh", (sender) => {
                CarAnimation.PlayAnimation();
            });
        }
    }
}