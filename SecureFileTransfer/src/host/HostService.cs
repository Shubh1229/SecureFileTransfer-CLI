using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using SecureFileTransfer.src.data_structures;
using SecureFileTransfer.src.helper;
using SecureFileTransfer.src.logging;
using SecureFileTransfer.src.setup;

namespace SecureFileTransfer.src.host
{
    public class HostService
    {
        private readonly int PORT = 5000;

        public void StartHost(HostModel host)
        {
            Console.Clear();

            if (string.IsNullOrWhiteSpace(host.IPv4))
            {
                Console.WriteLine("No valid host IPv4 address found.");
                return;
            }

            IPAddress ipAddress = IPAddress.Parse(host.IPv4);
            TcpListener tcp = new(ipAddress, PORT);
            TransferLogging logger = new();
            ConnectionLogModel? connection = null;

            try
            {
                tcp.Start();
                Console.WriteLine($"Waiting on {host.IPv4}:{PORT}...");

                using TcpClient client = tcp.AcceptTcpClient();
                Console.WriteLine("Client connected!");

                IPEndPoint? remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                if (remoteEndPoint != null)
                {
                    Console.WriteLine($"Remote endpoint: {remoteEndPoint.Address}:{remoteEndPoint.Port}");
                }

                using NetworkStream stream = client.GetStream();

                connection = HostHandshake(host, stream);
                if (connection == null)
                {
                    Console.WriteLine("Connection error...\nPress any key to continue");
                    Console.ReadKey();
                    Console.Clear();
                    return;
                }

                TransferPlanModel? plan = ReadTransferPlan(stream);
                if (plan == null)
                {
                    logger.FinishConnection(connection, false);
                    logger.SaveConnection(connection);
                    return;
                }

                Console.WriteLine($"\nExpecting {plan.FileCount} file(s)...");

                for (int i = 0; i < plan.FileCount; i++)
                {
                    FileInfoModel? incomingFile = ReadFileInfo(stream);
                    if (incomingFile == null)
                    {
                        logger.FinishConnection(connection, false);
                        logger.SaveConnection(connection);
                        return;
                    }

                    logger.AddFileLog(connection, incomingFile.FileName, incomingFile.FileSizeBytes, true);
                }

                logger.FinishConnection(connection, true);
                logger.SaveConnection(connection);
            }
            catch (Exception ex)
            {
                if (connection != null)
                {
                    logger.FinishConnection(connection, false);
                    logger.SaveConnection(connection);
                }

                Console.WriteLine($"Error creating TCP host: {ex.Message}");
            }
            finally
            {
                tcp.Stop();
                Console.WriteLine("Host stopped...\nPress any key to continue");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private ConnectionLogModel? HostHandshake(HostModel host, NetworkStream stream)
        {
            string? messageRead = MessageHelper.ReadMessage(stream);
            if (string.IsNullOrWhiteSpace(messageRead))
            {
                Console.WriteLine("Never received handshake.");
                return null;
            }

            HandshakeModel? receivedHandshake = HandshakeModel.FromJson(messageRead);
            if (receivedHandshake == null)
            {
                Console.WriteLine("Failed to parse handshake.");
                return null;
            }

            bool hasPeer = false;
            foreach (PeersModel peer in host.Peers)
            {
                if (peer.IPv4 == receivedHandshake.SenderIPv4 || peer.PeerName == receivedHandshake.SenderName)
                {
                    hasPeer = true;
                    break;
                }
            }

            if (!hasPeer)
            {
                AddPeer(host, receivedHandshake);
            }

            HandshakeModel handshake = new()
            {
                SenderName = host.HostName,
                SenderIPv4 = host.IPv4,
                SenderIPv6 = host.IPv6
            };

            MessageHelper.SendMessage(stream, handshake.ToJson());

            return new ConnectionLogModel()
            {
                RemoteComputerName = receivedHandshake.SenderName,
                RemoteIPv4 = receivedHandshake.SenderIPv4,
                RemoteIPv6 = receivedHandshake.SenderIPv6
            };
        }

        private void AddPeer(HostModel host, HandshakeModel receivedHandshake)
        {
            var peers = host.Peers.ToList();
            peers.Add(new PeersModel()
            {
                PeerName = receivedHandshake.SenderName,
                IPv4 = receivedHandshake.SenderIPv4,
                IPv6 = receivedHandshake.SenderIPv6
            });

            host.Peers = peers.ToArray();
            HostConfigManager.Save(host);
        }

        private TransferPlanModel? ReadTransferPlan(NetworkStream stream)
        {
            string? json = MessageHelper.ReadMessage(stream);

            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine("No transfer plan received.");
                return null;
            }

            TransferPlanModel? plan = JsonSerializer.Deserialize<TransferPlanModel>(json);
            if (plan == null)
            {
                Console.WriteLine("Failed to deserialize transfer plan.");
                return null;
            }

            return plan;
        }

        private FileInfoModel? ReadFileInfo(NetworkStream stream)
        {
            string? json = MessageHelper.ReadMessage(stream);

            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine("No file info received.");
                return null;
            }

            FileInfoModel? fileInfo = JsonSerializer.Deserialize<FileInfoModel>(json);

            if (fileInfo == null)
            {
                Console.WriteLine("Failed to deserialize file info.");
                return null;
            }

            Console.WriteLine("\nIncoming file info:");
            Console.WriteLine($"Name: {fileInfo.FileName}");
            Console.WriteLine($"Size: {fileInfo.FileSizeBytes} bytes");
            Console.WriteLine($"Suggested Save Name: {fileInfo.SuggestedSaveName}");

            return fileInfo;
        }
    }
}