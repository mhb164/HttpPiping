using HttpPiping.Setting;
using System.Text.Json;

namespace HttpPiping.App
{
    public static class HttpPipingSettingIO
    {
        public static HttpPipingSetting LoadJson(string filename)
        {
            try
            {
                if (File.Exists(filename))
                    return JsonSerializer.Deserialize<HttpPipingSetting>(File.ReadAllText(filename));
            }
            catch { }

            var setting = new HttpPipingSetting()
            {
                Pipes = new List<PipeSetting>()
                    {
                        new PipeSetting()
                        {
                            Id = 1,
                            Type = PipeType.WebSocket,
                            Address = "localhost:5159",
                            IsSecure = false,
                        },
                    },
                FlowRules = new List<FlowRule>()
                {
                    new FlowRule() { HostPattern ="*.google.com", PipeId = 1 },
                }
            };

            SaveJson(filename, setting);
            return setting;

        }

        private static void SaveJson(string filename, HttpPipingSetting setting)
        {
            File.WriteAllText(filename, JsonSerializer.Serialize(setting, new JsonSerializerOptions() { WriteIndented = true }));
        }
    }
}