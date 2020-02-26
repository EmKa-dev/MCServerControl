using System.IO;
using System.Text.Json;

namespace McServerControlAPI.Models
{
    public static class ConfigReader
    {
        private const string ConfigFilePath = @".\Content\Config.json";

        public static string GetConfigProperty(string property)
        {
            using (FileStream fs = new FileStream(ConfigFilePath, FileMode.Open, FileAccess.Read))
            {
                using (JsonDocument doc = JsonDocument.Parse(fs))
                {

                    var value = doc.RootElement.GetProperty(property).GetString();

                    return value;
                }
            }

        }
    }
}
