using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Shared;

namespace FakeDevice
{
    class Program
    {
        private static bool _isOn = true;

        static void Main()
        {
            var myDevice = GetOrCreateDevice();
            var client = DeviceClient.Create("azure-iot-demo.azure-devices.net", 
                new DeviceAuthenticationWithRegistrySymmetricKey(myDevice.Id.ToString(), myDevice.Key),
                TransportType.Amqp);
            SendDummyUpdates(client);
            Console.ReadLine();
            client.CloseAsync().Wait();
        }
        private static void SendDummyUpdates(DeviceClient client)
        {
            var random = new Random();
            Task.Run(async () =>
            {
                while (true)
                {
                    if (_isOn)
                    {
                        var usage = new PowerUsage {Value = random.NextDouble()*50d + 1000d - 25d};
                        var message = JsonConvert.SerializeObject(usage);
                        await client.SendEventAsync(new Message(Encoding.ASCII.GetBytes(message)));
                        Console.WriteLine("Send " + message);
                    }
                    await Task.Delay(1000);
                }
            });
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
