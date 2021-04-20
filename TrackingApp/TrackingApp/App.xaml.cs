using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

namespace TrackingApp
{

    public partial class App : Application
    {
        public static double sleepLon;
        public static double sleepLat;



        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {


            Preferences.Set("lastLongitude", sleepLon);
            Preferences.Set("lastLatitude", sleepLat);

        }

        protected override void OnResume()
        {
        }
    }
}
