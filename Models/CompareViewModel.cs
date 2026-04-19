namespace FileIntegrityMonitor.Models
{
    public class CompareViewModel
    {
        public ScanRecord? Baseline { get; set; }
        public ScanRecord? CurrentScan { get; set; }
        public List<ComparisonRow> Rows { get; set; } = new();

        public int NewCount => Rows.Count(r => r.Status == "New");
        public int DeletedCount => Rows.Count(r => r.Status == "Deleted");
        public int ModifiedCount => Rows.Count(r => r.Status == "Modified");
        public int UnchangedCount => Rows.Count(r => r.Status == "Unchanged");
    }
}