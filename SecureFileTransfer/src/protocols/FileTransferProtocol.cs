using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using SecureFileTransfer.src.logging;
using SecureFileTransfer.src.security;

namespace SecureFileTransfer.src.protocols
{
    public static class FileTransferProtocol
    {
        private static readonly int BUFFER_SIZE = 8192;
        public static bool Send(NetworkStream stream, string selectedFile, SessionKeyModel sessionKey)
        {

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
                    byte[] encryptedBuffer = EncryptionService.Encrypt(buffer, sessionKey.Key);
                    stream.Write(encryptedBuffer, 0, bytesRead);
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

        public static bool Read(NetworkStream stream, string destinationPath, long fileSizeBytes, SessionKeyModel sessionKey)
        {
            const int BUFFER_SIZE = 8192;

            try
            {
                string? directory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using FileStream fileStream = new(
                    destinationPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None
                );

                byte[] buffer = new byte[BUFFER_SIZE];
                long remaining = fileSizeBytes;
                long totalWritten = 0;

                while (remaining > 0)
                {
                    int bytesToRead = (int)Math.Min(buffer.Length, remaining);
                    int bytesRead = stream.Read(buffer, 0, bytesToRead);

                    if (bytesRead == 0)
                    {
                        DebugLogger.Log($"ReceiveFileBytes hit EOF early. Remaining={remaining}");
                        return false;
                    }

                    byte[] decryptedBuffer = EncryptionService.Decrypt(buffer, sessionKey.Key);

                    fileStream.Write(decryptedBuffer, 0, bytesRead);
                    remaining -= bytesRead;
                    totalWritten += bytesRead;
                    Console.Write($"\rReceiving: {totalWritten}/{fileSizeBytes} bytes");
                }

                fileStream.Flush();
                DebugLogger.Log($"Received {totalWritten} bytes into: {destinationPath}");
                return true;
            }
            catch (Exception ex)
            {
                DebugLogger.LogError($"HostService.ReceiveFileBytes ({destinationPath})", ex);
                return false;
            }
        }
    }
}