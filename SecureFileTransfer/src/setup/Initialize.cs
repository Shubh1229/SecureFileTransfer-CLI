using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using SecureFileTransfer.src.data_structures;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SecureFileTransfer.src.setup
{
    public class Initialize
    {
        public Initialize()
        {
            string path = Path.Combine("data", ".data", "find_file_path.txt");
            string findFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Desktop"
            );

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            if (!File.Exists(path))
            {
                File.WriteAllText(path, findFilePath);
            }
            string fullHostName = Dns.GetHostName();
            string hostName = "";
            foreach(char c in fullHostName)
            {
                if(c == '.') break;
                hostName += c;
            }
            Console.WriteLine($"Host Name: {hostName}");

            string IPv4_address = "", IPv6_address = "";

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                if (ni.Name.StartsWith("utun") || ni.Name.StartsWith("awdl"))
                    continue;

                var properties = ni.GetIPProperties();

                foreach (UnicastIPAddressInformation addr in properties.UnicastAddresses)
                {
                    IPAddress ip = addr.Address;

                    if (IPAddress.IsLoopback(ip))
                        continue;

                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        Console.WriteLine($"Usable IPv4 ({ni.Name}): {ip}");
                        IPv4_address = ip.ToString();
                    }
                    else if (ip.AddressFamily == AddressFamily.InterNetworkV6 && !ip.IsIPv6LinkLocal)
                    {
                        Console.WriteLine($"Usable IPv6 ({ni.Name}): {ip}");
                        IPv6_address = ip.ToString();
                    }
                }
            }
            HostModel host = new()
            {
                HostName = hostName,
                FullHostName = fullHostName,
                IPv4 = IPv4_address,
                IPv6 = IPv6_address,
                Peers = Array.Empty<PeersModel>()
            };
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            string yaml = serializer.Serialize(host);

            // Path to hidden file
            path = Path.Combine("data", ".data", "host.yaml");

            // Ensure directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            // Write file
            if(!File.Exists(path)) File.WriteAllText(path, yaml);

            TransferHistoryModel logs = new();

            yaml = serializer.Serialize(logs);

            path = Path.Combine("data", ".data", "transfer_logs.yaml");

            if(!File.Exists(path)) File.WriteAllText(path, yaml);

            path = Path.Combine("data", ".data", "download_path.txt");
            string downloadpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"Downloads");
            if(!File.Exists(path)) File.WriteAllText(path, downloadpath);
        }
    }
}