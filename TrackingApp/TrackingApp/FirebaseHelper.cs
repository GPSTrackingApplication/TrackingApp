using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;

namespace TrackingApp
{

    
    public class FirebaseHelper
    {
        DeviceLocation dl = new DeviceLocation();
        
        FirebaseClient firebase = new FirebaseClient("https://iotdevicetracker-default-rtdb.firebaseio.com/");

        // Method to read all latitide and longitude values from the firebase database
        public async Task<List<DeviceLocation>> GetAllDeviceInformation()
        {
            return (await firebase
              .Child("IoTGPS")
              .OnceAsync<DeviceLocation>()).Select(item => new DeviceLocation
              {
                  lat = item.Object.lat,
                  lon = item.Object.lon,
                  Id = item.Object.Id

              }).ToList();
        }

        // Method to return the values from the database
        public async Task<DeviceLocation> GetDeviceLocation()
        {
            var deviceLocation = await GetAllDeviceInformation();
            await firebase
              .Child("IoTGPS")
              .OnceAsync<DeviceLocation>();
            return deviceLocation[0];
        }
    }
    
}

