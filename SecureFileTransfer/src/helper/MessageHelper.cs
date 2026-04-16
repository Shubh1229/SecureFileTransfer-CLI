using System.Net.Sockets;
using System.Text;
using SecureFileTransfer.src.logging;

namespace SecureFileTransfer.src.helper
{
    public static class MessageHelper
    {
        public static void SendMessage(NetworkStream stream, string json)
        {
            byte[] payload = Encoding.UTF8.GetBytes(json);
            byte[] lengthPrefix = BitConverter.GetBytes(payload.Length);

            DebugLogger.Log($"MessageHelper.SendMessage length={payload.Length}");
            stream.Write(lengthPrefix, 0, lengthPrefix.Length);
            stream.Write(payload, 0, payload.Length);
            stream.Flush();
        }

        public static string? ReadMessage(NetworkStream stream)
        {
            DebugLogger.Log("MessageHelper.ReadMessage started.");

            byte[]? lengthBuffer = ReadExact(stream, 4);
            if (lengthBuffer == null)
            {
                DebugLogger.Log("MessageHelper.ReadMessage failed to read length prefix.");
                return null;
            }

            int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
            DebugLogger.Log($"MessageHelper.ReadMessage payload length={messageLength}");

            if (messageLength <= 0)
            {
                DebugLogger.Log("MessageHelper.ReadMessage got non-positive message length.");
                return null;
            }

            byte[]? payloadBuffer = ReadExact(stream, messageLength);
            if (payloadBuffer == null)
            {
                DebugLogger.Log("MessageHelper.ReadMessage failed to read payload.");
                return null;
            }

            DebugLogger.Log("MessageHelper.ReadMessage completed successfully.");
            return Encoding.UTF8.GetString(payloadBuffer);
        }

        private static byte[]? ReadExact(NetworkStream stream, int length)
        {
            byte[] buffer = new byte[length];
            int totalRead = 0;

            while (totalRead < length)
            {
                int bytesRead = stream.Read(buffer, totalRead, length - totalRead);
                if (bytesRead == 0)
                {
                    DebugLogger.Log($"MessageHelper.ReadExact hit EOF at totalRead={totalRead}, expected={length}");
                    return null;
                }

                totalRead += bytesRead;
            }

            return buffer;
        }
    }
}