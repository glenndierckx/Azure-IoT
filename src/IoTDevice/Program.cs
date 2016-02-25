using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Shared;

namespace FakeDevice
{
    class Program
    {
        static void Main()
        {
            var myDevice = GetOrCreateDevice();
        }

        static MyDevice GetOrCreateDevice()
        {
            MyDevice thisDevice = null;
            var thisDeviceFile = "data.json";
            if (File.Exists(thisDeviceFile))
            {
                var json = File.ReadAllText(thisDeviceFile, Encoding.ASCII);
                var data = JsonConvert.DeserializeObject<MyDevice>(json);
                try
                {
                    thisDevice = HttpService.Get<MyDevice>("api/devices/" + data.Id).Result;
                }
                catch (Exception) // bad developer
                {
                }
            }
            if (thisDevice == null)
            {
                thisDevice = HttpService.Post<MyDevice>("api/devices", new {}).Result;
            }
            File.WriteAllText(thisDeviceFile, JsonConvert.SerializeObject(thisDevice));
            return thisDevice;
        }
    }
}
