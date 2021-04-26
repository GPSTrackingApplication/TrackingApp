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
using System.Threading;
using Firebase.Database;
using Firebase.Database.Query;

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
        bool startTracking = false;


        FirebaseClient firebase = new FirebaseClient("https://iotdevicetracker-default-rtdb.firebaseio.com/");
        FirebaseHelper fb = new FirebaseHelper();

        public MainPage()
        {
            InitializeComponent();

            // Gets the most recent state of the GPS switch and sets it
            gpsSwitch.IsToggled = Preferences.Get("gpsSwitch", false);
            
            // Launch counter is set so that the GPS switch on toggled event handler is not set off when the application is launched
            launchCounter++;

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

                // -- BUG This isnt saving
                App.sleepLat = userLat;
                App.sleepLon = userLon;

            }

            if (gpsSwitch.IsToggled == false)
            {

                userLon = Preferences.Get("lastKnownLon", lastKnownLon);
                userLat = Preferences.Get("lastKnownLat", lastKnownLat);

                if (userLon != 0 && userLat != 0)
                {
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
            }


            /*
            // Will crash
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(30);


            var timer = new System.Threading.Timer((e) =>
            {
                getLocation();
                Pin pinUserDevice = new Pin()
                {
                    Type = PinType.Place,
                    Label = "Device Location",
                    Position = new Position(userLat, userLon),
                    Tag = "id_DeviceLocation"
                };
                map.Pins.Add(pinUserDevice);
                map.MoveToRegion(MapSpan.FromCenterAndRadius(pinUserDevice.Position, Distance.FromMeters(5000)));

            }, null, startTimeSpan, periodTimeSpan);
            */
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
                else

                userLon = location.Longitude;
                userLat = location.Latitude;

                //await DisplayAlert("Alert", "Longitude" + userLon + "latitude" + userLat, "OK");
                map.Pins.Clear();
                sendLocation(userLat, userLon);
                
                

                
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
            // Set the gpsSwitch to its current state, save this in sharedpreferences
            Preferences.Set("gpsSwitch", gpsSwitch.IsToggled);

            // Launch counter set so that these are not triggered when the application is launched.
            if (launchCounter != 0)
            {
                if (gpsSwitch.IsToggled == false)
                {
                    try
                    {
                        lastKnownLon = userLon;
                        lastKnownLat = userLat;



                        Preferences.Set("lastKnownLon", lastKnownLon);
                        Preferences.Set("lastKnownLat", lastKnownLat);
                        var location = await Geolocation.GetLastKnownLocationAsync();

                        map.Pins.Clear();

                        if(userLat != 0 && userLon != 0)
                        {
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
                        if (startTracking == true)
                        {
                            getDeviceLocation();
                            await DisplayAlert("Alert", "You are not currently tracking your mobiles location, this may make it more difficult to find the tracker!", "OK");
                        }
                    }

                    catch
                    {
                        Debug.WriteLine($"Something is wrong");
                    }
                }
                if (gpsSwitch.IsToggled == true)
                {
                    getLocation();
                }
            }
        }

        private async void StartTrackingBtn_Clicked(object sender, EventArgs e)
        {
                StopTrackingBtn.IsVisible = true;
                StartTrackingBtn.IsVisible = false;
                startTracking = true;
                getDeviceLocation();   
        }

        private void StopTrackingBtn_Clicked(object sender, EventArgs e)
        {
            startTracking = false;

            StopTrackingBtn.IsVisible = false;
            StartTrackingBtn.IsVisible = true;

            map.Pins.Clear();
            if (gpsSwitch.IsToggled == true)
            {
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
            
            if (gpsSwitch.IsToggled == false)
            {
                userLon = Preferences.Get("lastKnownLon", lastKnownLon);
                userLat = Preferences.Get("lastKnownLat", lastKnownLat);

                if (userLon != 0 && userLat != 0)
                {
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
            }
        }

        public async Task sendLocation(double userLat, double userLon)
        {
            await firebase
                .Child("UserLocation")
                .PutAsync(new Location() { UserLat = userLat, UserLon = userLon });
        }

        public async Task getDeviceLocation()
        {
            var deviceLocation = await fb.GetDeviceLocation();
            if (deviceLocation != null)
            {
                Pin pinUserTracker = new Pin()
                {
                    Type = PinType.Place,
                    Label = "Tracker Location",
                    Position = new Position(deviceLocation.lat, deviceLocation.lon),
                    Tag = "id_TrackerLocation"
                };
                map.Pins.Add(pinUserTracker);
                map.MoveToRegion(MapSpan.FromCenterAndRadius(pinUserTracker.Position, Distance.FromMeters(5000)));
            }
        }


        /*TODO
         * Read GPS location
         * 5) Refresh pins every minute or so. Make These two coordinates constantly update
      
         * Extras
         * 1) Workbook!
         * 2) Clean code
         * 3) Comment code
         */

    }
}