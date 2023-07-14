using Newtonsoft.Json;

namespace Gemnet.Settings
{
    public class Settings
    {
        public class SData
        {

            public String ipAddress { get; set; }
            public UInt16 Port { get; set; }

            public UInt16 P2PPort { get; set; }

            public string DBConnectionString { get; set; }
        }

        public static SData ImportSettings()
        {
            var envSettings = Environment.GetEnvironmentVariables();

            return new SData()
            { 
                ipAddress = envSettings["ipAddress"] as string,
                Port = UInt16.Parse(envSettings["Port"] as string),
                P2PPort = UInt16.Parse(envSettings["P2PPort"] as string),
                DBConnectionString = envSettings["DBConnectionString"] as string,
            };
        }

        public static SData ImportSettings(string path)
            => JsonConvert.DeserializeObject<SData>(File.ReadAllText(path));

        public static void ExportSettings(SData data, string path)
            => File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented));
    }
}
