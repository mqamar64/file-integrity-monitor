namespace FileIntegrityMonitor.Models
{
    public class ScanSummaryViewModel
    {
        public int TotalScans { get; set; }
        public int TotalBaselines { get; set; }
        public int TotalRegularScans { get; set; }
        public List<ScanRecord> RecentScans { get; set; } = new();
    }
}