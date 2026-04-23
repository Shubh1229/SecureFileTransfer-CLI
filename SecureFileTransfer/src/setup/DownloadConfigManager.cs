using SecureFileTransfer.src.logging;

namespace SecureFileTransfer.src.setup
{
    public static class DownloadConfigManager
    {
        private static readonly string PathToConfig = AppPaths.DownloadPathConfig;

        public static string Load()
        {
            AppPaths.EnsureAppDirectoryExists();

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
            AppPaths.EnsureAppDirectoryExists();
            File.WriteAllText(PathToConfig, path);
            DebugLogger.Log($"DownloadConfigManager saved path: {path}");
        }

        public static string GetPath()
        {
            return PathToConfig;
        }
    }
}