namespace SecureFileTransfer.src.logging
{
    public static class DebugLogger
    {
        private static readonly string PathToLog =
            Path.Combine(Directory.GetCurrentDirectory(), "data", ".data", "debug.log");

        public static void Log(string message)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PathToLog)!);

            string line = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC] {message}";
            File.AppendAllText(PathToLog, line + Environment.NewLine);
        }

        public static void LogError(string context, Exception ex)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PathToLog)!);

            string line =
                $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC] ERROR in {context}: {ex.Message}{Environment.NewLine}{ex}";

            File.AppendAllText(PathToLog, line + Environment.NewLine + Environment.NewLine);
        }

        public static string GetPath()
        {
            return PathToLog;
        }
        public static void Separator(string title = "")
        {
            Log(string.IsNullOrWhiteSpace(title)
                ? "--------------------------------------------------"
                : $"-------------------- {title} --------------------");
        }
    }
}