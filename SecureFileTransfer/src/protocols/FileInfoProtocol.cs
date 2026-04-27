using System.Net.Sockets;
using System.Text.Json;
using SecureFileTransfer.src.data_structures;
using SecureFileTransfer.src.helper;
using SecureFileTransfer.src.logging;
using SecureFileTransfer.src.security;

namespace SecureFileTransfer.src.protocols
{
    public static class FileInfoProtocol
    {
        public static void Send(
            NetworkStream stream,
            string selectedFile,
            SessionKeyModel sessionKey)
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

            MessageHelper.SendEncryptedMessage(stream, json, sessionKey);

            DebugLogger.Log($"Sent encrypted file info: {file.FileName}, {file.FileSizeBytes} bytes");

            Console.WriteLine("Sent file info:");
            Console.WriteLine($"Name: {file.FileName}");
            Console.WriteLine($"Size: {file.FileSizeBytes}");
        }

        public static FileInfoModel? Read(
            NetworkStream stream,
            SessionKeyModel sessionKey)
        {
            DebugLogger.Log("Host waiting for encrypted file info.");

            string? json = MessageHelper.ReadEncryptedMessage(stream, sessionKey);

            if (string.IsNullOrWhiteSpace(json))
            {
                DebugLogger.Log("No encrypted file info received.");
                Console.WriteLine("No file info received.");
                return null;
            }

            FileInfoModel? fileInfo = JsonSerializer.Deserialize<FileInfoModel>(json);

            if (fileInfo == null)
            {
                DebugLogger.Log("Failed to deserialize encrypted file info.");
                Console.WriteLine("Failed to deserialize file info.");
                return null;
            }

            DebugLogger.Log($"Received encrypted file info: {fileInfo.FileName}, {fileInfo.FileSizeBytes} bytes");

            Console.WriteLine("\nIncoming file info:");
            Console.WriteLine($"Name: {fileInfo.FileName}");
            Console.WriteLine($"Size: {fileInfo.FileSizeBytes} bytes");
            Console.WriteLine($"Suggested Save Name: {fileInfo.SuggestedSaveName}");

            return fileInfo;
        }
    }
}