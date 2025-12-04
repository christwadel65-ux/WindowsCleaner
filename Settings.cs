using System;
using System.IO;
using System.Text.Json;

namespace WindowsCleaner
{
    public class AppSettings
    {
        public string? ReportSortColumn { get; set; }
        public string? ReportSortDirection { get; set; } // "ASC" or "DESC"
    }

    public static class SettingsManager
    {
        private static readonly string _dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WindowsCleaner");
        private static readonly string _file = Path.Combine(_dir, "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (!Directory.Exists(_dir)) Directory.CreateDirectory(_dir);
                if (!File.Exists(_file)) return new AppSettings();
                var txt = File.ReadAllText(_file);
                return JsonSerializer.Deserialize<AppSettings>(txt) ?? new AppSettings();
            }
            catch { return new AppSettings(); }
        }

        public static void Save(AppSettings s)
        {
            try
            {
                if (!Directory.Exists(_dir)) Directory.CreateDirectory(_dir);
                var txt = JsonSerializer.Serialize(s, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_file, txt);
            }
            catch { }
        }
    }
}
