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

        /*
        public async Task<DeviceLocation> GetDeviceLocation(int personId)
        {
            var deviceLocation = await GetDeviceLocation();
            await firebase
              .Child("Napier48")
              .OnceAsync<DeviceLocation>();
            return deviceLocation.Where(a => a.PersonId == personId).FirstOrDefault();
        }
        */




        public async Task<List<DeviceLocation>> GetAllDeviceLocation()
        {
            return (await firebase
              .Child("IoTGPS")
              .OnceAsync<DeviceLocation>()).Select(item => new DeviceLocation
              {
                  DeviceLat = item.Object.DeviceLat,
                  DeviceLon = item.Object.DeviceLon

              }).ToList();
        }


        public async Task<DeviceLocation> GetDeviceLocation()
        {
            var deviceLocation = await GetAllDeviceLocation();
            await firebase
              .Child("IoTGPS")
              .OnceAsync<DeviceLocation>();
            return deviceLocation.Where(a => a.Id == "napier48").FirstOrDefault();
        }
    }
    
}

