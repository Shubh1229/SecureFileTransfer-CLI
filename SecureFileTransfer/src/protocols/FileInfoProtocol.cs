using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using SecureFileTransfer.src.data_structures;
using SecureFileTransfer.src.helper;
using SecureFileTransfer.src.logging;

namespace SecureFileTransfer.src.protocols
{
    public class FileInfoProtocol
    {
        public static void Send(NetworkStream stream, string selectedFile)
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

        public static FileInfoModel? Read(NetworkStream stream)
        {
            DebugLogger.Log("Host waiting for file info.");
            string? json = MessageHelper.ReadMessage(stream);

            if (string.IsNullOrWhiteSpace(json))
            {
                DebugLogger.Log("No file info received.");
                Console.WriteLine("No file info received.");
                return null;
            }

            FileInfoModel? fileInfo = JsonSerializer.Deserialize<FileInfoModel>(json);

            if (fileInfo == null)
            {
                DebugLogger.Log("Failed to deserialize file info.");
                Console.WriteLine("Failed to deserialize file info.");
                return null;
            }

            DebugLogger.Log($"Received file info: {fileInfo.FileName}, {fileInfo.FileSizeBytes} bytes");
            Console.WriteLine("\nIncoming file info:");
            Console.WriteLine($"Name: {fileInfo.FileName}");
            Console.WriteLine($"Size: {fileInfo.FileSizeBytes} bytes");
            Console.WriteLine($"Suggested Save Name: {fileInfo.SuggestedSaveName}");

            return fileInfo;
        }
    }
}