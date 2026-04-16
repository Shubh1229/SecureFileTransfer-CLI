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
            DebugLogger.Separator("HOST SESSION START");
            DebugLogger.Log("Entered HostService.StartHost");

            Console.Clear();

            if (string.IsNullOrWhiteSpace(host.IPv4))
            {
                DebugLogger.Log("Host IPv4 was empty. Exiting host start.");
                Console.WriteLine("No valid host IPv4 address found.");
                return;
            }

            string currentDownloadPath = DownloadConfigManager.Load();
            DebugLogger.Log($"Loaded current download path: {currentDownloadPath}");

            FileBrowserService downloadDirectoryBrowser = new();
            string? selectedDownloadPath = downloadDirectoryBrowser.BrowseForDirectory(currentDownloadPath);

            if (selectedDownloadPath == null)
            {
                selectedDownloadPath = currentDownloadPath;
                DebugLogger.Log("Download directory selection cancelled. Keeping existing path.");
            }

            DownloadConfigManager.Save(selectedDownloadPath);
            DebugLogger.Log($"Using download path: {selectedDownloadPath}");

            Console.WriteLine($"Files will be saved to: {selectedDownloadPath}");

            IPAddress ipAddress = IPAddress.Parse(host.IPv4);
            TcpListener tcp = new(ipAddress, PORT);
            TransferLogging logger = new();
            ConnectionLogModel? connection = null;

            try
            {
                tcp.Start();
                DebugLogger.Log($"TCP listener started on {host.IPv4}:{PORT}");
                Console.WriteLine($"Waiting on {host.IPv4}:{PORT}...");

                using TcpClient client = tcp.AcceptTcpClient();
                DebugLogger.Log("Client accepted by host.");
                Console.WriteLine("Client connected!");

                IPEndPoint? remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                if (remoteEndPoint != null)
                {
                    DebugLogger.Log($"Remote endpoint: {remoteEndPoint.Address}:{remoteEndPoint.Port}");
                    Console.WriteLine($"Remote endpoint: {remoteEndPoint.Address}:{remoteEndPoint.Port}");
                }

                using NetworkStream stream = client.GetStream();

                DebugLogger.Log("Starting host handshake.");
                connection = HostHandshake(host, stream);
                if (connection == null)
                {
                    DebugLogger.Log("Host handshake returned null.");
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
                    DebugLogger.Log("Transfer plan was null. Ending host session.");
                    return;
                }

                DebugLogger.Log($"Expecting {plan.FileCount} file(s).");
                Console.WriteLine($"\nExpecting {plan.FileCount} file(s)...");

                for (int i = 0; i < plan.FileCount; i++)
                {
                    DebugLogger.Log($"Reading file info #{i + 1} of {plan.FileCount}");
                    FileInfoModel? incomingFile = ReadFileInfo(stream);
                    if (incomingFile == null)
                    {
                        logger.FinishConnection(connection, false);
                        logger.SaveConnection(connection);
                        DebugLogger.Log("Incoming file info was null. Ending host session.");
                        return;
                    }

                    logger.AddFileLog(connection, incomingFile.FileName, incomingFile.FileSizeBytes, true);

                    string filePath = Path.Combine(selectedDownloadPath, incomingFile.SuggestedSaveName);
                    DebugLogger.Log($"Prepared destination path for incoming file: {filePath}");
                }

                logger.FinishConnection(connection, true);
                logger.SaveConnection(connection);
                DebugLogger.Log("Host session completed successfully.");
            }
            catch (Exception ex)
            {
                if (connection != null)
                {
                    logger.FinishConnection(connection, false);
                    logger.SaveConnection(connection);
                }

                DebugLogger.LogError("HostService.StartHost", ex);
                Console.WriteLine($"Error creating TCP host: {ex.Message}");
            }
            finally
            {
                tcp.Stop();
                DebugLogger.Log("TCP listener stopped.");
                Console.WriteLine("Host stopped...\nPress any key to continue");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private ConnectionLogModel? HostHandshake(HostModel host, NetworkStream stream)
        {
            DebugLogger.Log("Host waiting for handshake message.");
            string? messageRead = MessageHelper.ReadMessage(stream);

            if (string.IsNullOrWhiteSpace(messageRead))
            {
                DebugLogger.Log("Host never received handshake.");
                Console.WriteLine("Never received handshake.");
                return null;
            }

            HandshakeModel? receivedHandshake = HandshakeModel.FromJson(messageRead);
            if (receivedHandshake == null)
            {
                DebugLogger.Log("Host failed to parse handshake.");
                Console.WriteLine("Failed to parse handshake.");
                return null;
            }

            DebugLogger.Log($"Received handshake from {receivedHandshake.SenderName} ({receivedHandshake.SenderIPv4})");

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
                DebugLogger.Log("Peer not found in host config. Adding peer.");
                AddPeer(host, receivedHandshake);
            }

            HandshakeModel handshake = new()
            {
                SenderName = host.HostName,
                SenderIPv4 = host.IPv4,
                SenderIPv6 = host.IPv6
            };

            DebugLogger.Log("Host sending handshake response.");
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
            DebugLogger.Log($"Peer added and host config saved: {receivedHandshake.SenderName} ({receivedHandshake.SenderIPv4})");
        }

        private TransferPlanModel? ReadTransferPlan(NetworkStream stream)
        {
            DebugLogger.Log("Host waiting for transfer plan.");
            string? json = MessageHelper.ReadMessage(stream);

            if (string.IsNullOrWhiteSpace(json))
            {
                DebugLogger.Log("No transfer plan received.");
                Console.WriteLine("No transfer plan received.");
                return null;
            }

            TransferPlanModel? plan = JsonSerializer.Deserialize<TransferPlanModel>(json);
            if (plan == null)
            {
                DebugLogger.Log("Failed to deserialize transfer plan.");
                Console.WriteLine("Failed to deserialize transfer plan.");
                return null;
            }

            DebugLogger.Log($"Transfer plan deserialized successfully. FileCount={plan.FileCount}");
            return plan;
        }

        private FileInfoModel? ReadFileInfo(NetworkStream stream)
        {
            DebugLogger.Log("Host waiting for file info.");
            string? json = MessageHelper.ReadMessage(stream);

            if (string.IsNullOrWhiteSpace(json))
            {
                DebugLogger.Log("No file info received.");
                Console.WriteLine("No file info received.");
                return null;
            }

            FileInfoModel? fileInfo = JsonSerializer.Deserialize<FileInfoModel>(json);

            if (fileInfo == null)
            {
                DebugLogger.Log("Failed to deserialize file info.");
                Console.WriteLine("Failed to deserialize file info.");
                return null;
            }

            DebugLogger.Log($"Received file info: {fileInfo.FileName}, {fileInfo.FileSizeBytes} bytes");
            Console.WriteLine("\nIncoming file info:");
            Console.WriteLine($"Name: {fileInfo.FileName}");
            Console.WriteLine($"Size: {fileInfo.FileSizeBytes} bytes");
            Console.WriteLine($"Suggested Save Name: {fileInfo.SuggestedSaveName}");

            return fileInfo;
        }
    }
}