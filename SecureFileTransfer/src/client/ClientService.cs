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
            DebugLogger.Separator("CLIENT SESSION START");
            DebugLogger.Log("Entered ClientService.StartClient");

            if (host.Peers.Length == 0)
            {
                DebugLogger.Log("No peers saved. Exiting client start.");
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
                DebugLogger.Log($"Invalid peer selection input: '{input}'");
                Console.WriteLine("Invalid input...");
                return;
            }

            PeersModel peer = host.Peers[index];
            DebugLogger.Log($"Selected peer: {peer.PeerName} ({peer.IPv4})");

            if (string.IsNullOrWhiteSpace(peer.IPv4))
            {
                DebugLogger.Log("Selected peer did not have a valid IPv4 address.");
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
                DebugLogger.Log($"Attempting TCP connection to {peer.IPv4}:{PORT}");
                using TcpClient tcpClient = new(peer.IPv4, PORT);
                Console.WriteLine($"Connected to {peer.PeerName} at {peer.IPv4}:{PORT}");
                DebugLogger.Log("TCP connection established.");

                using NetworkStream stream = tcpClient.GetStream();

                DebugLogger.Log("Starting client handshake.");
                bool handshakeSuccess = ClientHandshake(host, stream);
                if (!handshakeSuccess)
                {
                    logger.FinishConnection(connectionLog, false);
                    DebugLogger.Log("Client handshake failed.");
                    Console.WriteLine("Handshake failed...");
                    return;
                }

                DebugLogger.Log("Selecting files.");
                List<string> selectedFiles = SelectFiles();
                if (selectedFiles.Count == 0)
                {
                    Console.WriteLine("No files selected.");
                    logger.FinishConnection(connectionLog, false);
                    DebugLogger.Log("No files selected.");
                    return;
                }

                DebugLogger.Log($"Selected {selectedFiles.Count} file(s). Sending transfer plan.");
                SendTransferPlan(stream, selectedFiles.Count);

                foreach (string filePath in selectedFiles)
                {
                    DebugLogger.Log($"Sending file info for: {filePath}");
                    SendFileInfo(stream, filePath);

                    DebugLogger.Log($"Starting file byte transfer for: {filePath}");
                    bool fileSent = SendFileBytes(stream, filePath);

                    FileInfo fileStats = new(filePath);
                    DebugLogger.Log($"Logged file metadata: {fileStats.Name} ({fileStats.Length} bytes)");

                    if (!fileSent)
                    {
                        DebugLogger.Log($"File byte transfer failed for: {filePath}");
                        logger.FinishConnection(connectionLog, false);
                        return;
                    }
                    logger.AddFileLog(connectionLog, fileStats.Name, fileStats.Length, true);
                    DebugLogger.Log($"Completed file byte transfer for: {filePath}");
                }

                logger.FinishConnection(connectionLog, true);
                DebugLogger.Log("Client session completed successfully.");
            }
            catch (Exception ex)
            {
                logger.FinishConnection(connectionLog, false);
                DebugLogger.LogError("ClientService.StartClient", ex);
                Console.WriteLine($"Could not set up client connection: {ex.Message}");
            }
            finally
            {
                logger.SaveConnection(connectionLog);
                DebugLogger.Log("Saved client connection log.");
                Console.WriteLine("Client stopped...\nPress any key to continue");
                Console.ReadKey();
                Console.Clear();
            }
        }

        private bool SendFileBytes(NetworkStream stream, string selectedFile)
        {
            const int BUFFER_SIZE = 8192;

            try
            {
                using FileStream fileStream = new(
                    selectedFile,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read
                );

                byte[] buffer = new byte[BUFFER_SIZE];
                int bytesRead;
                long totalSent = 0;

                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, bytesRead);
                    totalSent += bytesRead;
                }

                stream.Flush();

                DebugLogger.Log($"Sent {totalSent} bytes for file: {selectedFile}");
                return true;
            }
            catch (Exception ex)
            {
                DebugLogger.LogError($"ClientService.SendFileBytes ({selectedFile})", ex);
                return false;
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

            DebugLogger.Log("Client sending handshake message.");
            MessageHelper.SendMessage(stream, handshake.ToJson());

            string? messageRead = MessageHelper.ReadMessage(stream);
            if (string.IsNullOrWhiteSpace(messageRead))
            {
                DebugLogger.Log("Client never received handshake return.");
                Console.WriteLine("Never received handshake return.");
                return false;
            }

            HandshakeModel? receivedHandshake = HandshakeModel.FromJson(messageRead);
            if (receivedHandshake == null)
            {
                DebugLogger.Log("Client failed to parse handshake return.");
                Console.WriteLine("Failed to parse handshake return.");
                return false;
            }

            DebugLogger.Log($"Handshake completed with {receivedHandshake.SenderName} ({receivedHandshake.SenderIPv4})");
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
                DebugLogger.Log($"Opening file browser at: {startPath}");

                string? selectedFile = fileBrowser.BrowseForFile(startPath);

                if (selectedFile == null)
                {
                    DebugLogger.Log("File browser returned null/cancelled.");
                    break;
                }

                if (!selectedFiles.Contains(selectedFile))
                {
                    selectedFiles.Add(selectedFile);
                    DebugLogger.Log($"Added file to send list: {selectedFile}");

                    string? parent = Path.GetDirectoryName(selectedFile);
                    if (!string.IsNullOrWhiteSpace(parent))
                    {
                        FindFileConfigManager.Save(parent);
                        DebugLogger.Log($"Saved last browse path: {parent}");
                    }
                }

                Console.WriteLine($"\nAdded: {selectedFile}");
                Console.Write("Add another file? (y/n): ");
                string answer = (Console.ReadLine() ?? "").Trim().ToLower();

                if (answer != "y")
                {
                    DebugLogger.Log("User chose not to add more files.");
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

            DebugLogger.Log($"Sent transfer plan for {fileCount} file(s).");
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

            DebugLogger.Log($"Sent file info: {file.FileName}, {file.FileSizeBytes} bytes");
            Console.WriteLine("Sent file info:");
            Console.WriteLine($"Name: {file.FileName}");
            Console.WriteLine($"Size: {file.FileSizeBytes}");
        }
    }
}