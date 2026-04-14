using System.Net;
using System.Net.Sockets;
using System.Text;
using SecureFileTransfer.src.data_structures;

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
            try
            {
                tcp.Start();
                Console.WriteLine($"Waiting on {host.IPv4}:{PORT}...");
                TcpClient client = tcp.AcceptTcpClient();
                Console.WriteLine("Client Connected!");
                IPEndPoint? remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                if (remoteEndPoint != null)
                {
                    Console.WriteLine($"Remote endpoint: {remoteEndPoint.Address}:{remoteEndPoint.Port}");
                }
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"Received: {message}");

            } catch (Exception ex)
            {
                Console.WriteLine($"Error Creating TCP Host: {ex}");
            } finally
            {
                tcp.Stop();
                Console.WriteLine("Host Stopped...\nPress any key to continue");
                Console.ReadKey();
                Console.Clear();
            }
        }
    }
}