
namespace SecureFileTransfer.src.data_structures
{
    public class FileInfoModel
    {
        public string Type { get; set; } = "file_info";
        public string FileName { get; set; } = "";
        public long FileSizeBytes { get; set; }
        public string RelativeSourcePath { get; set; } = "";
        public string SuggestedSaveName { get; set; } = "";
    }
}