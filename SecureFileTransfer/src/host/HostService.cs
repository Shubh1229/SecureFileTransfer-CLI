using System.Net;
using System.Net.Sockets;
using System.Text;
using SecureFileTransfer.src.data_structures;
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

                connection = hostHandshake(host, stream);
                if (connection == null)
                {
                    Console.WriteLine("Connection error...\nPress any key to continue");
                    Console.ReadKey();
                    Console.Clear();
                    return;
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

        private ConnectionLogModel? hostHandshake(HostModel host, NetworkStream stream)
        {
            HandshakeModel handshake = new()
            {
                SenderName = host.HostName,
                SenderIPv4 = host.IPv4,
                SenderIPv6 = host.IPv6
            };

            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);

            string messageRead = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            HandshakeModel? receivedHandshake = HandshakeModel.FromJson(messageRead);

            if (receivedHandshake == null)
            {
                Console.WriteLine("Never received handshake.");
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
                addPeer(host, receivedHandshake);
            }

            byte[] msg = Encoding.UTF8.GetBytes(handshake.ToJson());
            stream.Write(msg, 0, msg.Length);

            return new ConnectionLogModel()
            {
                RemoteComputerName = receivedHandshake.SenderName,
                RemoteIPv4 = receivedHandshake.SenderIPv4,
                RemoteIPv6 = receivedHandshake.SenderIPv6
            };
        }

        private void addPeer(HostModel host, HandshakeModel receivedHandshake)
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
    }
}