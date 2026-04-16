using System.Net.Sockets;
using System.Text;

namespace SecureFileTransfer.src.helper
{
    public static class MessageHelper
    {
        public static void SendMessage(NetworkStream stream, string json)
        {
            byte[] payload = Encoding.UTF8.GetBytes(json);
            byte[] lengthPrefix = BitConverter.GetBytes(payload.Length);
            Console.WriteLine("Sending Handshake - MessageHelper.SendMessage");
            stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            stream.Write(payload, 0, payload.Length);
        }

        public static string? ReadMessage(NetworkStream stream)
        {
            byte[] lengthBuffer = new byte[4];
            Console.WriteLine("Reading handshake - MessageHelper.SendMessage");
            int read = stream.Read(lengthBuffer, 0, 4);

            if (read != 4)
            {
                return null;
            }

            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
            byte[] payloadBuffer = new byte[messageLength];

            int totalRead = 0;
            while (totalRead < messageLength)
            {
                Console.WriteLine($"Total Read: {totalRead}, Message Length: {messageLength}");
                int bytesRead = stream.Read(payloadBuffer, totalRead, messageLength - totalRead);
                if (bytesRead == 0)
                {
                    return null;
                }

                totalRead += bytesRead;
            }

            return Encoding.UTF8.GetString(payloadBuffer);
        }
    }
}