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
        static double userLon;
        static double userLat;
        public static double sleepLon;
        public static double sleepLat;
        static double lastKnownLat;
        static double lastKnownLon;
        static int launchCounter = 0;




        public MainPage()
        {
            InitializeComponent();
            // Make it so that this doesnt run on launch somehow
            gpsSwitch.IsToggled = Preferences.Get("gpsSwitch", false);
            launchCounter++;


            // BUG shared preferences arent working anymore 
            if (gpsSwitch.IsToggled == true)
            {
                userLon = Preferences.Get("sleepLonitude", sleepLon);
                userLat = Preferences.Get("sleepLatitude", sleepLat);

                if (userLon != 0 && userLat != 0)
                {
                    Pin pinUserDevice = new Pin()
                    {
                        Type = PinType.Place,
                        Label = "Device",
                        Position = new Position(userLat, userLon),
                        Tag = "id_DeviceLocation"
                    };
                    map.Pins.Add(pinUserDevice);
                    map.MoveToRegion(MapSpan.FromCenterAndRadius(pinUserDevice.Position, Distance.FromMeters(5000)));
                }
                
                // something here that makes this call every few seconds, then needs to update the pin, clear all pins and add a new one. Timercallback()
                // Should this actually be in ontoggleswitch or getlocation()?

                // Timercallback();
                //getLocation();
                // Pin update

                // -- BUG This isnt saving
                App.sleepLat = userLat;
                App.sleepLon = userLon;
            }

            if (gpsSwitch.IsToggled == false){

                userLon = Preferences.Get("lastKnownLon", lastKnownLon);
                userLat = Preferences.Get("lastKnownLat", lastKnownLat);

                if(userLon != 0 && userLat != 0)
                {
                    Pin pinUserDevice = new Pin()
                    {
                        Type = PinType.Place,
                        Label = "Last Known Device Location",
                        Position = new Position(lastKnownLon, lastKnownLat),
                        Tag = "id_LastDeviceLocation"
                    };
                    map.Pins.Add(pinUserDevice);
                    map.MoveToRegion(MapSpan.FromCenterAndRadius(pinUserDevice.Position, Distance.FromMeters(5000)));
                }
            }           
        }

        public async void getLocation()
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location == null)
                {
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
                // StopTrackingBtn.Text = $"{location.Latitude} {location.Longitude}";

                userLon = location.Longitude;
                userLat = location.Latitude;
                await DisplayAlert("Alert", "Longitude" + userLon + "latitude" + userLat, "OK");
                map.Pins.Clear();

                Pin pinUserDevice = new Pin()
                {
                    Type = PinType.Place,
                    Label = "Device Location",
                    Position = new Position(userLat, userLon),
                    Tag = "id_DeviceLocation"
                };
                map.Pins.Add(pinUserDevice);
                map.MoveToRegion(MapSpan.FromCenterAndRadius(pinUserDevice.Position, Distance.FromMeters(5000)));


            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Something is wrong");
            }
        }




        private async void gpsSwitchOnToggled(object sender, ToggledEventArgs e)
        {
            // Make it so that this doesnt run on launch somehow
            Preferences.Set("gpsSwitch", gpsSwitch.IsToggled);

            if (launchCounter != 0)
            {
                if (gpsSwitch.IsToggled == false)
                {
                    try
                    {
                        // BUG HERE LASTKNOWNLON IS SET TO NOTHING
                        
                        lastKnownLon = userLon;
                        lastKnownLat = userLat;



                        Preferences.Set("lastKnownLon", lastKnownLon);
                        Preferences.Set("lastKnownLat", lastKnownLat);
                        var location = await Geolocation.GetLastKnownLocationAsync();

                        map.Pins.Clear();

                        Pin pinUserDevice = new Pin()
                        {
                            Type = PinType.Place,
                            Label = "Last Known Device Location",
                            Position = new Position(userLat, userLon),
                            Tag = "id_LastDeviceLocation"
                        };
                        map.Pins.Add(pinUserDevice);
                        map.MoveToRegion(MapSpan.FromCenterAndRadius(pinUserDevice.Position, Distance.FromMeters(5000)));
                    }

                    catch
                    {
                        Debug.WriteLine($"Something is wrong");
                    }
                }
                if(gpsSwitch.IsToggled == true)
                {
                    getLocation();
                }
            }
        }

        private async void StartTrackingBtn_Clicked(object sender, EventArgs e)
        {
            StopTrackingBtn.IsVisible = true;
            StartTrackingBtn.IsVisible = false;

            //await DisplayAlert("Alert", "" + userLon + "" + userLat, "OK");
            //getLocation();


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
 * 1) Send GPS signal if switch has been flipped
 * 2) Set GPS location as the current pin, might require converting location into positioning coordinates
 * 3) Sending this to the backend.
 * 4) Can this work when the application is closed?
 * 5) Last known location set if Send GPS signal switch has not been flipped
 * 
 * Read Devices GPS location
 * 1) When the start tracking button has been pressed
 * 2) Set that devices location as a seperate pin
 * 
 * Save the toggle state of the send gps thing in saved state, and last known location when app is closed
 * 
 * Make These two coordinates constantly update
 * Workbook!
 */

}