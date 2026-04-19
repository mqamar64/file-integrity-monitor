namespace FileIntegrityMonitor.Models
{
    public class ComparisonRow
    {
        public string FilePath { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; 
        public long? OldSize { get; set; }
        public long? NewSize { get; set; }
        public string? OldHash { get; set; }
        public string? NewHash { get; set; }
    }
}