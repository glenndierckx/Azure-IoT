using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Shared;

namespace EventHandler
{
    class Program
    {
        private static JobHost _host;
        private static EventHubClient _eventHubClient;
        static string connectionString = "HostName=azure-iot-demo.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=Zma9R1DQz5TrXzIOggualsAm1OxZp4/KFGLq8Fbce70=";
        static string iotHubD2cEndpoint = "messages/events";

        static void Main(string[] args)
        {
            _eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);
            Task.WaitAll(_eventHubClient.GetRuntimeInformation().PartitionIds.Select(Listen).ToArray());
        }
        static async Task Listen(string partitionId)
        {
            Console.WriteLine("Listening to partition " + partitionId);
            var receiver = await _eventHubClient.GetDefaultConsumerGroup().CreateReceiverAsync(partitionId, DateTime.UtcNow);
            while (true)
            {
                var eventData = await receiver.ReceiveAsync();
                if (!HasEventData(eventData)) continue;

                var json = Encoding.ASCII.GetString(eventData.GetBytes());
                var powerUsage = JsonConvert.DeserializeObject<PowerUsage>(json);
                var deviceId = GetDeviceIdFromMessage(eventData);

                Console.WriteLine("Event received: " + json);

                var httpClient = new HttpClient();
                var signalrRequest = new DevicePowerUsage
                {
                    Id = Guid.Parse(deviceId),
                    Value = powerUsage.Value
                };
                await PostSignalrRequest(httpClient, signalrRequest);
            }
        }

        private static async Task PostSignalrRequest(HttpClient httpClient, DevicePowerUsage signalrRequest)
        {
            await HttpService.Post("api/deviceupdate", signalrRequest);
        }

        private static string GetDeviceIdFromMessage(EventData eventData)
        {
            return eventData.SystemProperties["iothub-connection-device-id"].ToString();
        }

        private static bool HasEventData(EventData eventData)
        {
            return eventData != null;
        }
    }
}
