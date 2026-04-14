using SecureFileTransfer.src.data_structures;

namespace SecureFileTransfer.src.logging
{
    public class TransferLogging
    {
        public ConnectionLogModel StartConnection(string remoteComputerName, string remoteIPv4, string remoteIPv6 = "")
        {
            return new ConnectionLogModel
            {
                RemoteComputerName = remoteComputerName,
                RemoteIPv4 = remoteIPv4,
                RemoteIPv6 = remoteIPv6,
                WasSuccessful = false
            };
        }

        public void AddFileLog(ConnectionLogModel connection, string fileName, long fileSizeBytes, bool wasSuccessful)
        {
            var fileLog = new FileTransferLogModel
            {
                FileName = fileName,
                FileSizeBytes = fileSizeBytes,
                WasSuccessful = wasSuccessful
            };

            connection.FileTransfers.Add(fileLog);
            connection.TotalFilesTransferred++;
            connection.TotalSizeBytes += fileSizeBytes;
        }

        public void FinishConnection(ConnectionLogModel connection, bool wasSuccessful)
        {
            connection.WasSuccessful = wasSuccessful;
        }

        public void SaveConnection(ConnectionLogModel connection)
        {
            TransferHistoryModel history = TransferLoggingManager.Load();
            history.Connections.Add(connection);
            TransferLoggingManager.Save(history);
        }
        public void CompleteAndSaveConnection(ConnectionLogModel connection, bool wasSuccessful)
        {
            connection.WasSuccessful = wasSuccessful;
            SaveConnection(connection);
        }
    }
}