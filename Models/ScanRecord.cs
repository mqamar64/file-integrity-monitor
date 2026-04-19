namespace FileIntegrityMonitor.Models
{
    public class ScanRecord
    {
        public long ScanId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RootPath { get; set; } = string.Empty;
        public string CreatedTime { get; set; } = string.Empty;
        public bool IsBaseline { get; set; }
    }
}