namespace FileIntegrityMonitor.Models
{
    public class FileRecord
    {
        public long ScanId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string Hash { get; set; } = string.Empty;
        public string LastWriteTime { get; set; } = string.Empty;
    }
}