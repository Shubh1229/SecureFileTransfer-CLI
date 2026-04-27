namespace SecureFileTransfer.src.setup
{
    public static class AppPaths
    {
        public static string AppDataDirectory
        {
            get
            {

                if (OperatingSystem.IsWindows())
                {
                    return Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "SecureFileTransfer"
                    );
                }

                // macOS + Linux
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".securefiletransfer"
                );
            }
        }

        public static string HostConfigPath =>
            Path.Combine(AppDataDirectory, "host.yaml");

        public static string TransferLogsPath =>
            Path.Combine(AppDataDirectory, "transfer_logs.yaml");

        public static string DebugLogPath =>
            Path.Combine(AppDataDirectory, "debug.log");

        public static string DownloadPathConfig =>
            Path.Combine(AppDataDirectory, "download_path.txt");

        public static string FindFilePathConfig =>
            Path.Combine(AppDataDirectory, "find_file_path.txt");

        public static void EnsureAppDirectoryExists()
        {
            Directory.CreateDirectory(AppDataDirectory);
        }
    }
}