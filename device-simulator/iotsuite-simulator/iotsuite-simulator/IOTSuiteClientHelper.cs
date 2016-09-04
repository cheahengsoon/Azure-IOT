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
    public class TelemetryMember
    {
        [DataMember]
        public string Name;
        [DataMember]
        public string Type;
        [DataMember]
        public string DisplayName;
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

        [DataMember]
        public TelemetryMember[] Telemetry;
    }

    public class IOTSuiteTelemetryData
    {
        /*
         * {“_0”：“324”，“_100”：“324”，“_101”：“324”，“_102”：“324”}
格式说明：{“0代表对应数值为设备ID”：“设备ID”，“协议地址A”：“电压”，“协议地址B”：“电流”“协议地址B”：“功率”}这一个代表一帧
    */
        public string DeviceId { get; set; }
        //设备ID”
        public int _0 { get; set; }
        //电压
        public int _100 { get; set; }
        //电流
        public int _101 { get; set; }
        //功率
        public int _102 { get; set; }

        //故障代碼
        public int _0106 { get; set; }
        //狀態 0:故障, 1:關網, 2:離網, 3:待機
        public int? _104 { get; set; }

        const int _106_Change_Interval = 10;
        static int _106_Changed = 0;
        static int _106_LastValue = 0;
        private static int GetErrorCode()
        {
            var random = new Random((int)DateTime.Now.Ticks);
            if (random.Next(0, 101) > 80) return 0;
            if (random.Next(0, 101) > 50) return 1001;
            return 1002;
        }
        public static string Random(string deviceId)
        {
            IOTSuiteTelemetryData data = new IOTSuiteTelemetryData();
            var random = new Random((int)DateTime.Now.Ticks);
            data.DeviceId = deviceId;
            data._0 = random.Next(300, 400);//1
            data._100 = random.Next(300, 400);//2;
            data._101 = random.Next(300, 400);//32;
            data._102 = random.Next(300, 400);//45;
            data._0106 = (_106_Changed % _106_Change_Interval == 0 ? GetErrorCode(): _106_LastValue);
            _106_LastValue = data._0106;
            if (data._0106 == 1001 || data._0106 == 1002)
            {
                data._104 = 0;
            }
            else if(data._0106 == 0)
            {
                data._104 = random.Next(1, 4);
            }
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
            device.Manufacturer = "BYD";
            device.ModelNumber = "RTU-001";
            device.SerialNumber = "5849735293875";
            device.FirmwareVersion = "10";
            device.Platform = "uC/OS-II";
            device.Processor = "ARM M4";
            device.InstalledRAM = "64 MB";
            device.DeviceState = "normal";

            thermostat.DeviceProperties = device;
            Command TriggerAlarm = new Command();
            TriggerAlarm.Name = "TriggerAlarm";
            CommandParameter param = new CommandParameter();
            param.Name = "Message";
            param.Type = "String";
            TriggerAlarm.Parameters = new CommandParameter[] { param };

            Command demoCommand = new Command();
            demoCommand.Name = "DemoCommand2";
            CommandParameter demoParam = new CommandParameter();
            demoParam.Name = "CommandType";
            demoParam.Type = "String";
            CommandParameter demoParam2 = new CommandParameter();
            demoParam2.Name = "CommandType2";
            demoParam2.Type = "String";
            demoCommand.Parameters = new CommandParameter[] { demoParam, demoParam2 };

            thermostat.Commands = new Command[] { TriggerAlarm, demoCommand };
            thermostat.Telemetry = new TelemetryMember[] {
                
             new TelemetryMember {
                    Name = "_100",
                    Type = "int",
                    DisplayName = "電壓"
                },
            new TelemetryMember {
                    Name = "_101",
                    Type = "int",
                    DisplayName = "電流"
                },
            new TelemetryMember {
                    Name = "_102",
                    Type = "int",
                    DisplayName = "功率"
                }};
            var ret = JsonConvert.SerializeObject(thermostat);

            return ret;
        }
    }
}
