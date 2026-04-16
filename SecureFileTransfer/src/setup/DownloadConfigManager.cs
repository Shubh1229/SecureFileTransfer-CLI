using SecureFileTransfer.src.logging;

namespace SecureFileTransfer.src.setup
{
    public static class DownloadConfigManager
    {
        private static readonly string PathToConfig =
            Path.Combine(Directory.GetCurrentDirectory(), "data", ".data", "download_path.txt");

        public static string Load()
        {
            if (!File.Exists(PathToConfig))
            {
                string defaultPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads"
                );

                Save(defaultPath);
                DebugLogger.Log($"DownloadConfigManager created default download path: {defaultPath}");
                return defaultPath;
            }

            string path = File.ReadAllText(PathToConfig).Trim();
            DebugLogger.Log($"DownloadConfigManager loaded path: {path}");
            return path;
        }

        public static void Save(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PathToConfig)!);
            File.WriteAllText(PathToConfig, path);
            DebugLogger.Log($"DownloadConfigManager saved path: {path}");
        }

        public static string GetPath()
        {
            return PathToConfig;
        }
    }
}