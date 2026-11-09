using System.IO;
using System.Text.Json;

namespace MisWpf.Infrastructure
{
    /// <summary>Настройки из appsettings.json (строка подключения и пороги таймеров).</summary>
    public static class AppConfig
    {
        public static string ConnectionString { get; private set; } = "";
        public static int WarnMinutes { get; private set; } = 60;
        public static int CriticalMinutes { get; private set; } = 120;
        public static int QueueDays { get; private set; } = 7;
        public static int RefreshSeconds { get; private set; } = 20;

        public static void Load()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var root = doc.RootElement;

            ConnectionString = root.GetProperty("ConnectionStrings").GetProperty("MisDb").GetString() ?? "";

            if (root.TryGetProperty("Reception", out var r))
            {
                WarnMinutes = ReadInt(r, "WarnMinutes", WarnMinutes);
                CriticalMinutes = ReadInt(r, "CriticalMinutes", CriticalMinutes);
                QueueDays = ReadInt(r, "QueueDays", QueueDays);
                RefreshSeconds = ReadInt(r, "RefreshSeconds", RefreshSeconds);
            }
        }

        private static int ReadInt(JsonElement e, string name, int fallback)
            => e.TryGetProperty(name, out var v) && v.TryGetInt32(out int i) ? i : fallback;
    }
}
