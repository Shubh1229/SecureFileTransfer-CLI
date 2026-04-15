using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using SecureFileTransfer.src.data_structures;
using SecureFileTransfer.src.helper;
using SecureFileTransfer.src.logging;

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
                ClientHandshake(host, peer, stream);

                SendFileInfo(stream);

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

        private void ClientHandshake(HostModel host, PeersModel peer, NetworkStream stream)
        {
            HandshakeModel handshake = new()
            {
                SenderName = host.HostName,
                SenderIPv4 = host.IPv4,
                SenderIPv6 = host.IPv6
            };

            byte[] msg = Encoding.UTF8.GetBytes(handshake.ToJson());
            stream.Write(msg, 0, msg.Length);

            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            string messageRead = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            HandshakeModel? receivedHandshake = HandshakeModel.FromJson(messageRead);

            if (receivedHandshake == null)
            {
                Console.WriteLine("Never received handshake return.");
                return;
            }

            Console.WriteLine("Handshake completed.");
            Console.WriteLine($"Remote name: {receivedHandshake.SenderName}");
            Console.WriteLine($"Remote IPv4: {receivedHandshake.SenderIPv4}");
        }
        private void SendFileInfo(NetworkStream stream)
        {
            FileBrowserService fileBrowser = new();
            while (true)
            {
                Console.WriteLine("Please select file(s) to send:");
                //string?[] filePathes = 
                break;
            }
            FileInfoModel file = new()
            {
                FileName = "test.txt",
                FileSizeBytes = 1234,
                RelativeSourcePath=Path.Combine(Directory.GetCurrentDirectory(),"test.txt"),
                SuggestedSaveName="received_test.txt"
            };
            string json = JsonSerializer.Serialize<FileInfoModel>(file);
            byte[] data = Encoding.UTF8.GetBytes(json);
            stream.Write(data,0,data.Length);
            Console.WriteLine("Sent File Info:");
            Console.WriteLine($"Name: {file.FileName}");
            Console.WriteLine($"Size: {file.FileSizeBytes}");
        }
    }
}