using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DigitalHouse;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Shared;

namespace IoTDevice
{
    class Program
    {
        private static bool _isOn = true;
        static void Main(string[] args)
        {
            Device thisDevice = null;
            var thisDeviceFile = "data.json";
            if (File.Exists(thisDeviceFile))
            {
                var json = File.ReadAllText(thisDeviceFile, Encoding.ASCII);
                var data = JsonConvert.DeserializeObject<Device>(json);
                try
                {
                    thisDevice = HttpService.Get<Device>("api/devices/" + data.Id).Result;
                }
                catch (Exception) // bad developer
                {
                }
            }
            if (thisDevice == null)
            {
                thisDevice = HttpService.Post<Device>("api/devices", new {}).Result;
            }
            File.WriteAllText(thisDeviceFile, JsonConvert.SerializeObject(thisDevice));


            var client = DeviceClient.Create("azure-iot-demo.azure-devices.net", new DeviceAuthenticationWithRegistrySymmetricKey(thisDevice.Id.ToString(), thisDevice.Key),TransportType.Amqp);
            
            ListenForCommands(client);
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
        private static void ListenForCommands(DeviceClient client)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var message = await client.ReceiveAsync();
                    if (message == null) continue;
                    var messageContent = Encoding.ASCII.GetString(message.GetBytes());
                    var command = JsonConvert.DeserializeObject<DeviceCommand>(messageContent);
                    if (command.Command == ECommandType.TurnOff)
                    {
                        _isOn = false;
                    }
                    else if (command.Command == ECommandType.TurnOn)
                    {
                        _isOn = true;
                    }
                    Console.WriteLine("Message received:" + messageContent);
                    await client.CompleteAsync(message);
                }
            });
        }
    }
}
