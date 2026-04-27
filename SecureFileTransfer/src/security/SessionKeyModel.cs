namespace SecureFileTransfer.src.security
{
    public class SessionKeyModel
    {
        public byte[] Key { get; set; } = Array.Empty<byte>();

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public bool IsEstablished => Key.Length > 0;
    }
}