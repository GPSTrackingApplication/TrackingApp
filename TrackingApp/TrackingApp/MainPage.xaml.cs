using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Essentials;
using System.Diagnostics;

namespace TrackingApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            Pin pinUserDevice = new Pin()
            {
                Type = PinType.Place,
                Label = "Your Device",
                Position = new Position(35.71d, 139.81d),
                Tag = "id_userDevice"
            };
            map.Pins.Add(pinUserDevice);
            map.MoveToRegion(MapSpan.FromCenterAndRadius(pinUserDevice.Position, Distance.FromMeters(5000)));
        }

        private async void gpsSwitchOnToggled(object sender, ToggledEventArgs e)
        {
            await DisplayAlert("Alert", "Switch toggled", "OK");
            if (gpsSwitch.IsToggled == true)
            {

                // Take these lines out
                StartTrackingBtn.IsVisible = false;
                StopTrackingBtn.IsVisible = true;

                try
                {
                    var location = await Geolocation.GetLastKnownLocationAsync();
                    if (location == null)
                    {
                        /*
                        var request = new GeolocationRequest(GeolocationAccuracy.Low);
                        location = await Geolocation.GetLocationAsync(request);
                        */

                        location = await Geolocation.GetLocationAsync(new GeolocationRequest
                        {
                            DesiredAccuracy = GeolocationAccuracy.Low,
                            Timeout = TimeSpan.FromSeconds(30)

                        });

                    }

                    if (location == null)
                        await DisplayAlert("Alert", "There is no location on this device, please check your privacy settings!", "OK");
                    else
                        // Set a pin to equal your location, could cheat and set these to invisible labels then carry them across
                    StopTrackingBtn.Text = $"{location.Latitude} {location.Longitude}";
                    //double userlocation = $"{location.Latitude} {location.Longitude}";


                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Something is wrong");
                }
            }
        }

        private async void StartTrackingBtn_Clicked(object sender, EventArgs e)
        {
            StopTrackingBtn.IsVisible = false;
            StartTrackingBtn.IsVisible = true;

            if (gpsSwitch.IsToggled != true)
            {
                await DisplayAlert("Alert", "You cannot begin tracking unless the 'Send GPS Signal' switch is turned on!", "OK");
            }
        }

        private void StopTrackingBtn_Clicked(object sender, EventArgs e)
        {
            StopTrackingBtn.IsVisible = false;
            StartTrackingBtn.IsVisible = true;
        }
    }

/*TODO
 * Read GPS location
 * 1) If Send GPS signal switch has been flipped
 * 2) Set GPS location as the current pin, might require converting location into positioning coordinates
 * 3) Sending this to the backend.
 * 4) Can this work when the application is closed?
 * 5) Last known location set if Send GPS signal switch has not been flipped
 * 
 * Read Devices GPS location
 * 1) When the start tracking button has been pressed
 * 2) Set that devices location as a seperate pin
 * 
 * Make These two coordinates constantly update
 * Workbook!
 */

}