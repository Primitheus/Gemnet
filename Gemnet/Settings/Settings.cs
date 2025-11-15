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
            public string RC4Key { get; set; }
            public bool UseEncryption { get; set; }
            public int? MaxConnections { get; set; } = 1000; // Default to 1000 connections
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
                UseEncryption = bool.Parse(envSettings["UseEncryption"] as string),
                RC4Key = envSettings["RC4Key"] as string,
                MaxConnections = int.TryParse(envSettings["MaxConnections"] as string, out var maxConn) ? maxConn : 1000,
            };
        }

        public static SData ImportSettings(string path)
            => JsonConvert.DeserializeObject<SData>(File.ReadAllText(path));

        public static void ExportSettings(SData data, string path)
            => File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented));
    }
}
