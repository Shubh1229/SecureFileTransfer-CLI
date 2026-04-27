

using System.Net.Sockets;
using System.Text.Json;
using SecureFileTransfer.src.data_structures;
using SecureFileTransfer.src.helper;
using SecureFileTransfer.src.logging;

namespace SecureFileTransfer.src.protocols
{
    public static class TransferPlanProtocol
    {
        public static void Send(NetworkStream stream, int fileCount)
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


        public static TransferPlanModel? Read(NetworkStream stream)
        {
            DebugLogger.Log("Host waiting for transfer plan.");
            string? json = MessageHelper.ReadMessage(stream);

            if (string.IsNullOrWhiteSpace(json))
            {
                DebugLogger.Log("No transfer plan received.");
                Console.WriteLine("No transfer plan received.");
                return null;
            }

            TransferPlanModel? plan = JsonSerializer.Deserialize<TransferPlanModel>(json);
            if (plan == null)
            {
                DebugLogger.Log("Failed to deserialize transfer plan.");
                Console.WriteLine("Failed to deserialize transfer plan.");
                return null;
            }

            DebugLogger.Log($"Transfer plan deserialized successfully. FileCount={plan.FileCount}");
            return plan;
        }
    }
}