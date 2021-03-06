﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Shared;

namespace EventHandler
{
    class Program
    {
        private static EventHubClient _eventHubClient;

        static void Main()
        {
            var connectionString = "{connectionstring}";
            var iotHubD2cEndpoint = "messages/events";
            _eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);
            // create listener task per partition
            Task.WaitAll(_eventHubClient.GetRuntimeInformation().PartitionIds.Select(Listen).ToArray());
        }
        static async Task Listen(string partitionId)
        {
            Console.WriteLine("Listening to partition " + partitionId);

            // receive all events after UtcNow (replay events)
            var receiver = await _eventHubClient.GetDefaultConsumerGroup().CreateReceiverAsync(partitionId, DateTime.UtcNow);
            while (true)
            {
                // wait for a event to receive
                var eventData = await receiver.ReceiveAsync();
                if (eventData == null) continue;

                // read event data
                var json = Encoding.ASCII.GetString(eventData.GetBytes());
                var powerUsage = JsonConvert.DeserializeObject<PowerUsage>(json);
                var deviceId = eventData.SystemProperties["iothub-connection-device-id"].ToString();

                Console.WriteLine("Event received: " + json);

                // post to signalr (notify web clients)
                var signalrRequest = new DevicePowerUsage
                {
                    Id = Guid.Parse(deviceId),
                    Value = powerUsage.Value
                };
                await HttpService.Post("api/deviceupdate", signalrRequest);
            }
        }
    }
}
