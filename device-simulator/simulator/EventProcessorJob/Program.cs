using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus.Messaging;
using System.Configuration;

namespace EventProcessorJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            var host = new JobHost();
            string iotHubConnectionString = ConfigurationManager.AppSettings["iotHubConnectionString"];
            
            //https://azure.microsoft.com/en-us/documentation/articles/iot-hub-devguide/#endpoints
            string iotHubD2cEndpoint = "messages/events";
            SKSEventProcessor.StorageConnectionString = ConfigurationManager.AppSettings["storageConnectionString"];
            SKSEventProcessor.ServiceBusConnectionString = ConfigurationManager.AppSettings["serviceBusConnectionString"];

            string eventProcessorHostName = Guid.NewGuid().ToString();
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, iotHubD2cEndpoint, EventHubConsumerGroup.DefaultGroupName, iotHubConnectionString, SKSEventProcessor.StorageConnectionString,"messages-events");

            Console.WriteLine("Registering EventProcessor...");
            eventProcessorHost.RegisterEventProcessorAsync<SKSEventProcessor>().Wait();

            Console.WriteLine("Receiving. Press enter key to stop worker.");
            host.RunAndBlock();
            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
        }
    }
}
