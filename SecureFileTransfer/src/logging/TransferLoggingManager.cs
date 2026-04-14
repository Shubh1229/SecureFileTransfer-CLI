using SecureFileTransfer.src.data_structures;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SecureFileTransfer.src.logging
{
    public static class TransferLoggingManager
    {
        private static readonly string PathToConfig =
            Path.Combine(Directory.GetCurrentDirectory(), "data", ".data", "transfer_logs.yaml");

        public static TransferHistoryModel Load()
        {
            string yaml = File.ReadAllText(PathToConfig);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            TransferHistoryModel logs = deserializer.Deserialize<TransferHistoryModel>(yaml);

            return logs;
        }

        public static void Save(TransferHistoryModel logs)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            string yaml = serializer.Serialize(logs);

            Directory.CreateDirectory(Path.GetDirectoryName(PathToConfig)!);
            File.WriteAllText(PathToConfig, yaml);
        }

        public static string GetPath()
        {
            return PathToConfig;
        }
    }
}