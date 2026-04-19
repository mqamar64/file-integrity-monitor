namespace FileIntegrityMonitor.Models
{
    public class ScanDetailsViewModel
    {
        public ScanRecord? Scan { get; set; }
        public List<FileRecord> Files { get; set; } = new();

        public ScanRecord? BaselineScan { get; set; }
        public List<ComparisonRow> ComparisonRows { get; set; } = new();

        public bool HasComparison => ComparisonRows.Any();

        public int NewCount => ComparisonRows.Count(r => r.Status == "New");
        public int DeletedCount => ComparisonRows.Count(r => r.Status == "Deleted");
        public int ModifiedCount => ComparisonRows.Count(r => r.Status == "Modified");
        public int UnchangedCount => ComparisonRows.Count(r => r.Status == "Unchanged");
    }
}