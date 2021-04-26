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
        static double deviceLat;
        static double deviceLon;

        public static double sleepLon;
        public static double sleepLat;
        static double lastKnownLat;
        static double lastKnownLon;
        static int launchCounter = 0;

        FirebaseClient firebase = new FirebaseClient("https://iotdevicetracker-default-rtdb.firebaseio.com/");
        FirebaseHelper fb = new FirebaseHelper();

        public MainPage()
        {
            InitializeComponent();



            gpsSwitch.IsToggled = Preferences.Get("gpsSwitch", false);
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
                        Position = new Position(lastKnownLon, lastKnownLat),
                        Tag = "id_LastDeviceLocation"
                    };
                    map.Pins.Add(pinUserDevice);
                    map.MoveToRegion(MapSpan.FromCenterAndRadius(pinUserDevice.Position, Distance.FromMeters(5000)));
                }
            }



            // Will crash
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromSeconds(30);

            /*
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
                deviceLocation();

                /*
                Pin pinUserDevice = new Pin()
                {
                    Type = PinType.Place,
                    Label = "Device Location",
                    Position = new Position(userLat, userLon),
                    Tag = "id_DeviceLocation"
                };
                map.Pins.Add(pinUserDevice);
                map.MoveToRegion(MapSpan.FromCenterAndRadius(pinUserDevice.Position, Distance.FromMeters(5000)));
                */

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

            //await DisplayAlert("Alert", "" + userLon + "" + userLat, "OK");
            getLocation();


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

        public async Task sendLocation(double userLat, double userLon)
        {
            await firebase
                .Child("UserLocation")
                .PutAsync(new Location() { UserLat = userLat, UserLon = userLon });
            // Pins no longer updating


        }

        /*
        public async Task <DeviceLocation> RecieveLocation(double deviceLat)
        {
            var deviceLocation = await getDeviceLocation();
            await firebase
                .Child("Napier48")
                .OnceAsync<DeviceLocation>();
            return deviceLocation.Where(a => a.lat == deviceLat).FirstOrDefault();


            //.OnceAsync(new DeviceLocation() { DeviceLat = deviceLat, DeviceLon = deviceLon });
        }
        */



        public async Task deviceLocation()
        {
            var deviceLocation = await fb.GetDeviceLocation();
            if (deviceLocation != null)
            {
                await DisplayAlert("Success", "Person Retrive Successfully", "OK");

            }


        }



        /*
        private bool TimerCallBack()
        {

            map.Pins.Clear();
            cargarPinesEnMapa();

            return true;
        }

        private void cargarPinesEnMapa()
        {
            for (int i = 0; i < listaPines.Count; i++)
                map.Pins.Add((Pin)listaPines[i]);
        }
        */

        /*TODO
         * Read GPS location
         * 4) Fix sleep location bug.
         * 5) Refresh pins every minute or so
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
}