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
                return defaultPath;
            }

            return File.ReadAllText(PathToConfig).Trim();
        }

        public static void Save(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PathToConfig)!);
            File.WriteAllText(PathToConfig, path);
        }

        public static string GetPath()
        {
            return PathToConfig;
        }
    }
}