namespace SecureFileTransfer.src.data_structures
{
    public class ConnectionLogModel
    {
        public Guid ConnectionId { get; set; } = Guid.NewGuid();
        public required string RemoteComputerName { get; set; }
        public required string RemoteIPv4 { get; set; }
        public string RemoteIPv6 { get; set; } = "";
        public int TotalFilesTransferred { get; set; } = 0;
        public long TotalSizeBytes { get; set; } = 0;
        public DateTime ConnectionDateTimeUtc { get; set; } = DateTime.UtcNow;
        public bool WasSuccessful { get; set; }
        public List<FileTransferLogModel> FileTransfers { get; set; } = new();
    }
}