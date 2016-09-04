using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices;
using System.Configuration;
using Newtonsoft.Json;
using Microsoft.ServiceBus.Messaging;

namespace c2d_alarm
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([ServiceBusTrigger("c2dalarm")] BrokeredMessage message, TextWriter log)
        {
            var text = GetMessagePayload(message);
            dynamic obj = JsonConvert.DeserializeObject(text);
            var registryMgr = RegistryManager.CreateFromConnectionString(ConfigurationManager.AppSettings["iotHubConnectionString"]);
            try
            {
                var device = registryMgr.GetDeviceAsync((string)obj.DeviceId).GetAwaiter().GetResult();
                var deviceConnString = $"HostName={ConfigurationManager.AppSettings["iotHubUri"]};DeviceId={device.Id};SharedAccessKey={device.Authentication.SymmetricKey.PrimaryKey}";
                var dc = DeviceClient.CreateFromConnectionString(deviceConnString);
                ServiceClient sc = ServiceClient.CreateFromConnectionString(ConfigurationManager.AppSettings["iotHubConnectionString"]);
                sc.SendAsync(device.Id,
                    new Microsoft.Azure.Devices.Message(Encoding.UTF8.GetBytes($"Command:{text}"))).Wait();
                sc.CloseAsync().Wait();

                message.Complete();
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine($"Exception:{exp.Message}");

            }
        }

        private static string GetMessagePayload(BrokeredMessage message)
        {
            string ret = string.Empty;
            //try
            //{
            //    var o = message.GetBody<System.IO.Stream>();
            //    using (var r = new StreamReader(o))
            //    {
            //        ret = r.ReadToEnd();
            //    }

            //}
            //catch
            //{
            //    ret = message.GetBody<string>();
            //}
            //return ret;
            if (message.Properties.Keys.Contains("message-source") && (string)message.Properties["message-source"] == "evh")
            {
                var o = message.GetBody<System.IO.Stream>();
                using (var r = new StreamReader(o))
                {
                    ret = r.ReadToEnd();
                }
            }
            else
            {
                ret = message.GetBody<string>();
                
            }
            return ret;
        }
    }
}
