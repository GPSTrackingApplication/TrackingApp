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
using Firebase.Database;
using Firebase.Database.Query;
using System.Timers;

namespace TrackingApp
{
    [DesignTimeVisible(false)]

    public partial class MainPage : ContentPage
    {

        // The following variables are taken from a 'progress value' timer project found here https://youtu.be/DjfEBnPb4ns
        // This project has been reworked to remove the graphics and instead use the timer to call certain methods depedning on the current state of the application

        // Double to establish the current progress of the timer, the number it is currently on
        private double _ProgressValue;
        public double ProgressValue
        {
            get
            {
                return _ProgressValue;
            }
            set
            {
                _ProgressValue = value;
                OnPropertyChanged();
            }
        }

        // The minimum number which the timer is attempting to reach, in our case this number is set as 0
        private double _Minimum;
        public double Minimum
        {
            get
            {
                return _Minimum;
            }
            set
            {
                _Minimum = value;
                OnPropertyChanged();
            }
        }

        // The maximum number which the timer counts down from, in our case we want the timer to run every minute, so the value is held as 60
        private double _Maximum;
        public double Maximum
        {
            get
            {
                return _ProgressValue;
            }
            set
            {
                _ProgressValue = value;
                OnPropertyChanged();
            }
        }

        // Creates new timer
        private Timer time = new Timer();

        // Boolean to establish if the timer is running or not, changed each time the timer stops or starts
        private bool timerRunning;

        // Global variables so all classes can read and change the current longitude and latitude of the users mobile device
        static double userLon;
        static double userLat;

        // Sleeplon/lat are used to save the current longitude and latitude if the users device goes into sleep mode
        public static double sleepLon;
        public static double sleepLat;

        // Lastknownlon/lat are saved when the gpsswitch is toggled off. It saves the most recent lon and lat into sharedpreferences to be retreived later
        static double lastKnownLat;
        static double lastKnownLon;

        // This counter is created to stop the gpsSwitchOnToggled method is not triggered if the last state of the switch was on
        static int launchCounter = 0;
        bool startTracking = false;
        // Counts the number of times the timer has been completed
        int timerCount = 0;


        // Links the application with our google firebase server
        FirebaseClient firebase = new FirebaseClient("https://iotdevicetracker-default-rtdb.firebaseio.com/");
        FirebaseHelper fb = new FirebaseHelper();

        // MainPage which runs when the application is launched
        public MainPage()
        {
            InitializeComponent();

            // Setting the timer variables on launch
            BindingContext = this;
            Minimum = 0;
            Maximum = 60;
            ProgressValue = 60;
            timerRunning = false;

            // Gets the most recent state of the GPS switch and sets it
            gpsSwitch.IsToggled = Preferences.Get("gpsSwitch", false);
            
            // Launch counter is set so that the GPS switch on toggled event handler is not set off when the application is launched
            launchCounter++;

            // If the gpsswitch is toggled then get the sleeplon and lat from shared preferences
            if (gpsSwitch.IsToggled == true)
            {
                userLon = Preferences.Get("sleepLonitude", sleepLon);
                userLat = Preferences.Get("sleepLatitude", sleepLat);

                // If these values or not 0 then create a pin on the google map with the mobile devices current location
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

                // If the application goes into sleep mode then save the sleepLat/sleepLon as the current longitude and latitude
                App.sleepLat = userLat;
                App.sleepLon = userLon;

                // If the GPS toggle is turned on then get the current location of the device
                getLocation();
            }

            //  if the gpsSwitch is not toggled then get set userLon and the last known location from shared preferences
            if (gpsSwitch.IsToggled == false)
            {
                userLon = Preferences.Get("lastKnownLon", lastKnownLon);
                userLat = Preferences.Get("lastKnownLat", lastKnownLat);
                
                // set the last known location pin as long as the values for lon and lat are not 0
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

        // Method which begins the timer. When the timer hits 0 certain methods will run
        public async void startTimer()
        {
            timerRunning = true;
            time.Start();
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                if (ProgressValue > Minimum)
                {
                    ProgressValue--;
                    return true;
                }
                else if (ProgressValue == Minimum)
                {
                    BindingContext = this;
                    Minimum = 0;
                    Maximum = 60;
                    ProgressValue = 60;
                    timerCount++;

                // if the GPSswtich is turned on then run the getlocation and sendlocation methods, which retreive the location from the user then send this data to the firebase
                if (gpsSwitch.IsToggled != false)
                    {
                        getLocation();
                        sendLocation(userLat, userLon);
                    }

                    // If start tracking is true then run the getDeviceLocation method which reads the device location from the firebase
                    if (startTracking == true)
                    {
                        getDeviceLocation();
                    }

                    // Restart the timer after these methods have been run
                    time.Stop();
                    timerRunning = true;
                    time.Start();

                    return true;
                }
                else
                {
                    return true;
                }
            });
        }

        // Get location method which reads the users mobile geolocation to retreive its longitude and latitude
        public async void getLocation()
        {
            // Attempts to get the geolocation, this is done through the xamarin.essentials library
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

                // If the location is found set the lonitude and latitude to our userLon and userLat variables
                else
                userLon = location.Longitude;
                userLat = location.Latitude;

                // Clear the previous pins on the map
                map.Pins.Clear();

                // Runs the send location method which sends our userLon/userLat to the firebase
                sendLocation(userLat, userLon);
                
                // Set a new pin on the map with our new updated location
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

            // Throw an error to the debug console if there are any issues
            catch (Exception ex)
            {
                Debug.WriteLine($"Something is wrong");
            }
        }

        // Event handler for when the gpsSwitch is toggled
        private async void gpsSwitchOnToggled(object sender, ToggledEventArgs e)
        {
            // Set the gpsSwitch to its current state, save this in sharedpreferences
            Preferences.Set("gpsSwitch", gpsSwitch.IsToggled);

            // If the switch is turned on then run the startTimer method to start the timer
            if (gpsSwitch.IsToggled == true)
            {
                startTimer();
            }
            // Launch counter set so that these are not triggered when the application is launched.
            if (launchCounter != 0)
            {
                // If the switch is turned off then stop the timer, then save the current lat and lon into shared preferences
                if (gpsSwitch.IsToggled == false)
                {
                    timerRunning = false;
                    time.Stop();

                    try
                    {
                        lastKnownLon = userLon;
                        lastKnownLat = userLat;

                        Preferences.Set("lastKnownLon", lastKnownLon);
                        Preferences.Set("lastKnownLat", lastKnownLat);
                        var location = await Geolocation.GetLastKnownLocationAsync();

                        // Clear the current pins on the map and if the values for lat and lon are not 0 then add a new pin for the last known location of the mobile device
                        map.Pins.Clear();

                        if (userLat != 0 && userLon != 0)
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
                        // If the user is currently tracking the device get the current device location by running the getDeviceLocation method and throw a warning message
                        if (startTracking == true)
                        {
                            getDeviceLocation();
                            await DisplayAlert("Alert", "You are not currently tracking your mobiles location, this may make it more difficult to find the tracker!", "OK");
                        }
                    }
                    // If this does not work through an error to the debug console
                    catch
                    {
                        Debug.WriteLine($"Something is wrong");
                    }
                }
            }
        }

        // Event handler to run if the starttracking button has been pressed
        private async void StartTrackingBtn_Clicked(object sender, EventArgs e)
        {
            // When the button is pressed make the current button invisible, and replace it with the stop tracking button
            StopTrackingBtn.IsVisible = true;
            StartTrackingBtn.IsVisible = false;
            
            // set startTracking bool as true
            startTracking = true;

            // Run the start timer method and getDeviceLocation method
            startTimer();
            getDeviceLocation();
            
            if (startTracking == false)
            {
                time.Stop();
            }
        }

        // Event handler to run if the stoptracking button has been pressed
        private void StopTrackingBtn_Clicked(object sender, EventArgs e)
        {
            // set startTracking bool as false
            startTracking = false;
            
            // If the gpsSwitch is turned off then stop the timer
            if(gpsSwitch.IsToggled == false)
            {
                time.Stop();
            }

            // When the button is pressed make the current button invisible, and replace it with the start tracking button
            StopTrackingBtn.IsVisible = false;
            StartTrackingBtn.IsVisible = true;

            // Clear the pins on the map, then replace it with either the current location or last known location depending on if the gpsSwitch is turned on or off
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

        // Send location method which sends the current userLon/userLat to the firebase database
        public async Task sendLocation(double userLat, double userLon)
        {
            await firebase
                .Child("UserLocation")
                .PutAsync(new Location() { UserLat = userLat, UserLon = userLon });
        }

        // Method to get the current device location from the firebase database
        public async Task getDeviceLocation()
        {
            // call the getDeviceLocation method in the FirebaseHelper class
            var deviceLocation = await fb.GetDeviceLocation();

            // If the device location is not null then create a new pin with the current location on the map
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
    }
}