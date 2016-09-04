using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.NotificationHubs;
using Newtonsoft.Json;
using System.Configuration;
using Microsoft.ServiceBus.Messaging;

namespace sks_webjob_sdk_version
{
    public class Functions
    {
        private static string nhConnectionString = ConfigurationManager.AppSettings["nhConnectionString"];//"Endpoint=sb://skshub.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=+toF6XlnEcBSPlDTN0WPW2XBNmpIEQnZbQUTfO2y/aY=";
        private static string hubName = ConfigurationManager.AppSettings["nhHubName"];//"sks-notification";
        private static string sbQueueName = ConfigurationManager.AppSettings["serviceBusQueueName"];//"sksdemo";

        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([ServiceBusTrigger("wistrondemo")] BrokeredMessage message,
                TextWriter logger)
        {
            logger.WriteLine(message);
            logger.WriteLine($"{message} received at {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}");
            if (message.Properties.Keys.Contains("message-source") && (string)message.Properties["message-source"] == "evh")
            {
                var o = message.GetBody<System.IO.Stream>();
                using (var r = new StreamReader(o))
                {
                    var msg = r.ReadToEnd();
                    Console.WriteLine("Body: " + msg);
                    Console.WriteLine("MessageID: " + message.MessageId);
                    SendNotificationAsync(msg);
                    // Remove message from queue.
                    message.Complete();
                }

            }
            else
            {
                // Process message from stream analytics.
                var msg = message.GetBody<string>();

                Console.WriteLine("Body: " + msg);
                Console.WriteLine("MessageID: " + message.MessageId);
                SendNotificationAsync(msg);
                // Remove message from queue.
                message.Complete();
            }

        }

        private static async Task SendNotificationAsync(string msg)
        {
            await SendWindowsNotificationAsync(msg);
            await SendAndroidNotificationAsync(msg);
        }
        private static async Task SendWindowsNotificationAsync(string msg)
        {
            NotificationHubClient hub = NotificationHubClient
                .CreateClientFromConnectionString(nhConnectionString, hubName);
            var toast = $"<toast launch=\"launch_arguments\"><visual><binding template=\"ToastText01\"><text id=\"1\">{msg}</text></binding></visual></toast>";
            var results = await hub.SendWindowsNativeNotificationAsync(toast);
        }
        private static async Task SendAndroidNotificationAsync(string msg)
        {
            NotificationHubClient hub = NotificationHubClient
                .CreateClientFromConnectionString(nhConnectionString, hubName);
            Newtonsoft.Json.Linq.JObject o = JsonConvert.DeserializeObject(msg) as Newtonsoft.Json.Linq.JObject;
            var toast = "{data:{message:'{device} alert at {time}'}}".Replace("{device}", (string)o["deviceid"]).Replace("{time}", (string)o["time"]);
            var results = await hub.SendGcmNativeNotificationAsync(toast);
            Console.WriteLine($"***{msg}");
        }
    }
}
