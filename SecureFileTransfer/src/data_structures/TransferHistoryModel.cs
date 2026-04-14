namespace SecureFileTransfer.src.data_structures
{
    public class TransferHistoryModel
    {
        public List<ConnectionLogModel> Connections { get; set; } = new();
    }
}