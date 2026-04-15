using Godot;
using System;
using System.IO;
using System.Text;


namespace SRC.Logger
{
    /// <summary>
    /// 自定义日志器，将日志信息进行保存
    /// </summary>
    public static class Logger
    {
        // 最低日志信息急别，忽略 Debug 信息
        public static Level MinLevel = Level.Debug;
        // 日志信息级别
        public enum Level{ Debug, Info, Warning, Error }

        private static string _logFilePath;  // 日志文件路径
        private static FileLogWritter _logWritter;  // 日志写入器

        static Logger()
        {
            // 确定路径
            string exeDir = Path.GetDirectoryName(OS.GetExecutablePath());
            string logDir = Path.Combine(exeDir, ".logs");
            string fileName = $"app_{DateTime.Now:yyyy-MM-dd}.log";
            try
            {
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);
                _logFilePath = Path.Combine(logDir, fileName);
            }
            catch
            {
                // 如果没权限（比如 Program Files），就回退到 user://
                string userDir = ProjectSettings.GlobalizePath("user://logs");
                if (!Directory.Exists(userDir))
                    Directory.CreateDirectory(userDir);
                _logFilePath = Path.Combine(userDir, fileName);
                GD.PrintErr("无法在程序目录创建日志，已改用 user://logs");
            }

            _logWritter = new FileLogWritter(_logFilePath);
            Debug($"日志文件路径为：{_logFilePath}");
            // 程序退出时确保关闭写入器，写入所有日志
            AppDomain.CurrentDomain.ProcessExit += (s, e) => _logWritter.Close();
        }

        // 公共 API
        public static void Debug(string message, object context = null) => LogMessage(Level.Debug, message, context);
        public static void Info(string message, object context = null) => LogMessage(Level.Info, message, context);
        public static void Warn(string message, object context = null) => LogMessage(Level.Warning, message, context);
        public static void Error(string message, object context = null) => LogMessage(Level.Error, message, context);

        private static void LogMessage(Level level, string msg, object context)
        {
            string module = context?.GetType().Name ?? "Global";
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string formatted = $"[{timestamp}] [{level}] [{module}] {msg}";

            switch (level)
            {
                case Level.Warning: GD.PrintRich($"[color=yellow]{formatted}[/color]"); break;
                case Level.Error: GD.PrintRich($"[color=red]{formatted}[/color]"); break;
                default: GD.Print(formatted); break;
            }
            // 只将Info，Warn，Error信息写入日志
            if (level > MinLevel)
                _logWritter?.WriteLine(formatted);
            return;
        }
    }
}
