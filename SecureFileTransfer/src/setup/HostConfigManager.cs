using SecureFileTransfer.src.data_structures;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SecureFileTransfer.src.setup
{
    public static class HostConfigManager
    {
        private static readonly string PathToConfig =
            Path.Combine(Directory.GetCurrentDirectory(), "data", ".data", "host.yaml");

        public static HostModel Load()
        {
            string yaml = File.ReadAllText(PathToConfig);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            HostModel host = deserializer.Deserialize<HostModel>(yaml);

            host.Peers ??= Array.Empty<PeersModel>();
            host.IPv6 ??= "";

            return host;
        }

        public static void Save(HostModel host)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            string yaml = serializer.Serialize(host);

            Directory.CreateDirectory(Path.GetDirectoryName(PathToConfig)!);
            File.WriteAllText(PathToConfig, yaml);
        }

        public static string GetPath()
        {
            return PathToConfig;
        }
    }
}