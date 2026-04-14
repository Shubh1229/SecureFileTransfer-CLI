using System.Net.Sockets;
using System.Text;
using SecureFileTransfer.src.data_structures;

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
            Console.WriteLine("Which Peer would you like to connect to?");
            for(int i = 0; i < host.Peers.Length; i++)
            {
                Console.WriteLine($"{i+1}: {host.Peers[i].PeerName}");
            }
            string input = Console.ReadLine() ?? "-1";
            int.TryParse(input, out int index);
            index--;
            if(index < 0 || index > host.Peers.Length)
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
            try
            {
                TcpClient tcpClient = new(peer.IPv4, PORT);
                Console.WriteLine($"Connected to {peer.PeerName} at {peer.IPv4}:{PORT}");
                NetworkStream stream = tcpClient.GetStream();
                string message = "HELLO_FROM_CLIENT";
                byte[] data = Encoding.UTF8.GetBytes(message);

                stream.Write(data, 0, data.Length);
                Console.WriteLine($"Sent: {message}");
            } catch (Exception ex)
            {
                Console.WriteLine($"Could not set up Client Connection: {ex}");
            } finally
            {
                
            }
        }
    }
}