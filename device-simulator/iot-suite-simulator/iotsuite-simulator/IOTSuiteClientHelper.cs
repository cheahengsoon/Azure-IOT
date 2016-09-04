using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace simulator
{
    [DataContract]
    public class DeviceProperties
    {
        [DataMember]
        public string DeviceID;
        [DataMember]
        public bool HubEnabledState;
        [DataMember]
        public string CreatedTime;
        [DataMember]
        public string DeviceState;
        [DataMember]
        public string UpdatedTime;
        [DataMember]
        public string Manufacturer;
        [DataMember]
        public string ModelNumber;
        [DataMember]
        public string SerialNumber;
        [DataMember]
        public string FirmwareVersion;
        [DataMember]
        public string Platform;
        [DataMember]
        public string Processor;
        [DataMember]
        public string InstalledRAM;
        [DataMember]
        public double Latitude;
        [DataMember]
        public double Longitude;

    }
    [DataContract]
    public class CommandParameter
    {
        [DataMember]
        public string Name;
        [DataMember]
        public string Type;
    }
    [DataContract]
    public class Command
    {
        [DataMember]
        public string Name;
        [DataMember]
        public CommandParameter[] Parameters;
    }
    [DataContract]
    public class Thermostat
    {
        [DataMember]
        public DeviceProperties DeviceProperties;
        [DataMember]
        public Command[] Commands;
        [DataMember]
        public bool IsSimulatedDevice;
        [DataMember]
        public string Version;
        [DataMember]
        public string ObjectType;
    }

    public class IOTSuiteTelemetryData
    {
        public string DeviceId { get; set; }
        public int _0101 { get; set; }
        public int _0102 { get; set; }
        public int _0103 { get; set; }
        public int _0104 { get; set; }
        public int _0105 { get; set; }

        public static string Random(string deviceId)
        {
            IOTSuiteTelemetryData data = new IOTSuiteTelemetryData();
            var random = new Random((int)DateTime.Now.Ticks);
            data.DeviceId = deviceId;
            data._0101 = random.Next(0, 4);//1
            data._0102 = random.Next(0, 4);//2;
            data._0103 = random.Next(23, 33);//32;
            data._0104 = random.Next(36, 46);//45;
            data._0104 = random.Next(78, 88);//87;

            return JsonConvert.SerializeObject(data);
        }
    }

    internal class IOTSuiteClientHelper
    {
        internal static string GetInitiateDeviceData(string deviceId)
        {
            DeviceProperties device = new DeviceProperties();
            Thermostat thermostat = new Thermostat();
            thermostat.ObjectType = "DeviceInfo";
            thermostat.IsSimulatedDevice = false;
            thermostat.Version = "1.0";

            device.HubEnabledState = true;
            device.DeviceID = deviceId;
            device.Manufacturer = "Microsoft";
            device.ModelNumber = "Lumia950";
            device.SerialNumber = "5849735293875";
            device.FirmwareVersion = "10";
            device.Platform = "Windows 10";
            device.Processor = "SnapDragon";
            device.InstalledRAM = "3GB";
            device.DeviceState = "normal";

            thermostat.DeviceProperties = device;
            Command TriggerAlarm = new Command();
            TriggerAlarm.Name = "TriggerAlarm";
            CommandParameter param = new CommandParameter();
            param.Name = "Message";
            param.Type = "String";
            TriggerAlarm.Parameters = new CommandParameter[] { param };

            Command demoCommand = new Command();
            demoCommand.Name = "DemoCommand";
            CommandParameter demoParam = new CommandParameter();
            demoParam.Name = "CommandType";
            demoParam.Type = "String";
            demoCommand.Parameters = new CommandParameter[] { demoParam };

            thermostat.Commands = new Command[] { TriggerAlarm, demoCommand };

            var ret = JsonConvert.SerializeObject(thermostat);

            return ret;
        }
    }
}
