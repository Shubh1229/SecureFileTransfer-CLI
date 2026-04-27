using SecureFileTransfer.src.logging;

namespace SecureFileTransfer.src.setup
{
    public static class FindFileConfigManager
    {
        private static readonly string PathToConfig = AppPaths.FindFilePathConfig;

        public static string Load()
        {
            AppPaths.EnsureAppDirectoryExists();

            if (!File.Exists(PathToConfig))
            {
                string defaultPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Desktop"
                );

                Save(defaultPath);
                DebugLogger.Log($"FindFileConfigManager created default browse path: {defaultPath}");
                return defaultPath;
            }

            string path = File.ReadAllText(PathToConfig).Trim();
            DebugLogger.Log($"FindFileConfigManager loaded path: {path}");
            return path;
        }

        public static void Save(string path)
        {
            AppPaths.EnsureAppDirectoryExists();
            File.WriteAllText(PathToConfig, path);
            DebugLogger.Log($"FindFileConfigManager saved path: {path}");
        }

        public static string GetPath()
        {
            return PathToConfig;
        }
    }
}