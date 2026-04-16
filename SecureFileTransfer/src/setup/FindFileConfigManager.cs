using SecureFileTransfer.src.logging;

namespace SecureFileTransfer.src.setup
{
    public static class FindFileConfigManager
    {
        private static readonly string PathToConfig =
            Path.Combine(Directory.GetCurrentDirectory(), "data", ".data", "find_file_path.txt");

        public static string Load()
        {
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
            Directory.CreateDirectory(Path.GetDirectoryName(PathToConfig)!);
            File.WriteAllText(PathToConfig, path);
            DebugLogger.Log($"FindFileConfigManager saved path: {path}");
        }

        public static string GetPath()
        {
            return PathToConfig;
        }
    }
}