using SecureFileTransfer.src.data_structures;
using SecureFileTransfer.src.setup;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SecureFileTransfer.src.logging
{
    public static class TransferLoggingManager
    {
        private static readonly string PathToConfig = AppPaths.TransferLogsPath;

        public static TransferHistoryModel Load()
        {
            AppPaths.EnsureAppDirectoryExists();

            if (!File.Exists(PathToConfig))
            {
                Save(new TransferHistoryModel());
            }

            string yaml = File.ReadAllText(PathToConfig);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            TransferHistoryModel? logs = deserializer.Deserialize<TransferHistoryModel>(yaml);

            return logs ?? new TransferHistoryModel();
        }

        public static void Save(TransferHistoryModel logs)
        {
            AppPaths.EnsureAppDirectoryExists();

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            string yaml = serializer.Serialize(logs);

            File.WriteAllText(PathToConfig, yaml);
        }

        public static string GetPath()
        {
            return PathToConfig;
        }
    }
}