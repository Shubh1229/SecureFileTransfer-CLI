using System.Net.Sockets;
using System.Text.Json;
using SecureFileTransfer.src.data_structures;
using SecureFileTransfer.src.helper;
using SecureFileTransfer.src.logging;
using SecureFileTransfer.src.setup;

namespace SecureFileTransfer.src.client
{
    public class ClientService
    {
        private readonly int PORT = 5000;

        public void StartClient(HostModel host)
        {
            if (host.Peers.Length == 0)
            {
                Console.WriteLine("No peers saved.");
                return;
            }

            Console.WriteLine("Which peer would you like to connect to?");
            for (int i = 0; i < host.Peers.Length; i++)
            {
                Console.WriteLine($"{i + 1}: {host.Peers[i].PeerName} ({host.Peers[i].IPv4})");
            }

            string input = Console.ReadLine() ?? "-1";
            bool parsed = int.TryParse(input, out int index);
            index--;

            if (!parsed || index < 0 || index >= host.Peers.Length)
            {
                Console.WriteLine("Invalid input...");
                return;
            }

            PeersModel peer = host.Peers[index];

            if (string.IsNullOrWhiteSpace(peer.IPv4))
            {
                Console.WriteLine("Selected peer does not have a valid IPv4 address.");
                return;
            }

            TransferLogging logger = new();
            ConnectionLogModel connectionLog = logger.StartConnection(
                peer.PeerName,
                peer.IPv4,
                peer.IPv6
            );

            try
            {
                using TcpClient tcpClient = new(peer.IPv4, PORT);
                Console.WriteLine($"Connected to {peer.PeerName} at {peer.IPv4}:{PORT}");

                using NetworkStream stream = tcpClient.GetStream();

                bool handshakeSuccess = ClientHandshake(host, stream);
                if (!handshakeSuccess)
                {
                    logger.FinishConnection(connectionLog, false);
                    return;
                }

                List<string> selectedFiles = SelectFiles();
                if (selectedFiles.Count == 0)
                {
                    Console.WriteLine("No files selected.");
                    logger.FinishConnection(connectionLog, false);
                    return;
                }

                SendTransferPlan(stream, selectedFiles.Count);

                foreach (string filePath in selectedFiles)
                {
                    SendFileInfo(stream, filePath);

                    FileInfo fileStats = new(filePath);
                    logger.AddFileLog(connectionLog, fileStats.Name, fileStats.Length, true);
                }

                logger.FinishConnection(connectionLog, true);
            }
            catch (Exception ex)
            {
                logger.FinishConnection(connectionLog, false);
                Console.WriteLine($"Could not set up client connection: {ex.Message}");
            }
            finally
            {
                logger.SaveConnection(connectionLog);
                Console.WriteLine("Client stopped...\nPress any key to continue");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private bool ClientHandshake(HostModel host, NetworkStream stream)
        {
            HandshakeModel handshake = new()
            {
                SenderName = host.HostName,
                SenderIPv4 = host.IPv4,
                SenderIPv6 = host.IPv6
            };

            MessageHelper.SendMessage(stream, handshake.ToJson());

            string? messageRead = MessageHelper.ReadMessage(stream);
            if (string.IsNullOrWhiteSpace(messageRead))
            {
                Console.WriteLine("Never received handshake return.");
                return false;
            }

            HandshakeModel? receivedHandshake = HandshakeModel.FromJson(messageRead);
            if (receivedHandshake == null)
            {
                Console.WriteLine("Failed to parse handshake return.");
                return false;
            }

            Console.WriteLine("Handshake completed.");
            Console.WriteLine($"Remote name: {receivedHandshake.SenderName}");
            Console.WriteLine($"Remote IPv4: {receivedHandshake.SenderIPv4}");

            return true;
        }

        private List<string> SelectFiles()
        {
            FileBrowserService fileBrowser = new();
            List<string> selectedFiles = new();

            while (true)
            {
                string startPath = FindFileConfigManager.Load();
                string? selectedFile = fileBrowser.BrowseForFile(startPath);

                if (selectedFile == null)
                {
                    break;
                }

                if (!selectedFiles.Contains(selectedFile))
                {
                    selectedFiles.Add(selectedFile);

                    string? parent = Path.GetDirectoryName(selectedFile);
                    if (!string.IsNullOrWhiteSpace(parent))
                    {
                        FindFileConfigManager.Save(parent);
                    }
                }

                Console.WriteLine($"\nAdded: {selectedFile}");
                Console.Write("Add another file? (y/n): ");
                string answer = (Console.ReadLine() ?? "").Trim().ToLower();

                if (answer != "y")
                {
                    break;
                }
            }

            return selectedFiles;
        }

        private void SendTransferPlan(NetworkStream stream, int fileCount)
        {
            TransferPlanModel plan = new()
            {
                FileCount = fileCount
            };

            string json = JsonSerializer.Serialize(plan);
            MessageHelper.SendMessage(stream, json);

            Console.WriteLine($"Sent transfer plan for {fileCount} file(s).");
        }

        private void SendFileInfo(NetworkStream stream, string selectedFile)
        {
            FileInfo fileStats = new(selectedFile);

            FileInfoModel file = new()
            {
                FileName = fileStats.Name,
                FileSizeBytes = fileStats.Length,
                RelativeSourcePath = selectedFile,
                SuggestedSaveName = fileStats.Name
            };

            string json = JsonSerializer.Serialize(file);
            MessageHelper.SendMessage(stream, json);

            Console.WriteLine("Sent file info:");
            Console.WriteLine($"Name: {file.FileName}");
            Console.WriteLine($"Size: {file.FileSizeBytes}");
        }
    }
}