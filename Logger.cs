using System;
using System.IO;
using System.Text;

namespace WindowsCleaner
{
    public enum LogLevel { Debug, Info, Warning, Error }

    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        private static string _logFile = Path.Combine(_logDir, "windows-cleaner.log");

        public static event Action<DateTime, LogLevel, string>? OnLog;

        public static void Init(string? logDirectory = null)
        {
            if (!string.IsNullOrEmpty(logDirectory))
            {
                _logDir = logDirectory;
                _logFile = Path.Combine(_logDir, "windows-cleaner.log");
            }

            try
            {
                Directory.CreateDirectory(_logDir);
            }
            catch
            {
                // ignore
            }
        }

        public static void Log(LogLevel level, string message)
        {
            var ts = DateTime.Now;
            try
            {
                var line = $"[{ts:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                lock (_lock)
                {
                    File.AppendAllText(_logFile, line + Environment.NewLine, Encoding.UTF8);
                }
                OnLog?.Invoke(ts, level, message);
            }
            catch
            {
                // ignore logging failures
            }
        }

        public static void Clear()
        {
            try
            {
                lock (_lock)
                {
                    if (File.Exists(_logFile)) File.Delete(_logFile);
                }
                OnLog?.Invoke(DateTime.Now, LogLevel.Info, "Logs effacés");
            }
            catch
            {
            }
        }

        public static string? Export(string destinationPath)
        {
            try
            {
                if (!File.Exists(_logFile)) return null;
                File.Copy(_logFile, destinationPath, true);
                return destinationPath;
            }
            catch
            {
                return null;
            }
        }
    }
}
