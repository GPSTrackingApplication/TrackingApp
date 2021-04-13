using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace TrackingApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            Pin pinTokyo = new Pin()
            {
                Type = PinType.Place,
                Label = "Tokyo SKYTREE",
                Address = "Sumida-Ku, Tokyo, Japan",
                Position = new Position(35.71d, 139.81d),
                Rotation = 33.3f,
                Tag = "id_tokyo"
            };
            map.Pins.Add(pinTokyo);
            map.MoveToRegion(MapSpan.FromCenterAndRadius(pinTokyo.Position, Distance.FromMeters(5000)));
        }

        private void StartTrackingBtn_Clicked(object sender, EventArgs e)
        {
            StartTrackingBtn.IsVisible = false;
            StopTrackingBtn.IsVisible = true;


        }

        private void StopTrackingBtn_Clicked(object sender, EventArgs e)
        {
            StopTrackingBtn.IsVisible = false;
            StartTrackingBtn.IsVisible = true;
        }
    }
}
