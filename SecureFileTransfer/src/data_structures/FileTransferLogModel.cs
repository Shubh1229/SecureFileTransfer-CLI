namespace SecureFileTransfer.src.data_structures
{
    public class FileTransferLogModel
    {
        public Guid TransferId { get; set; } = Guid.NewGuid();
        public required string FileName { get; set; }
        public required long FileSizeBytes { get; set; }
        public DateTime TransferDateTimeUtc { get; set; } = DateTime.UtcNow;
        public bool WasSuccessful { get; set; }
    }
}