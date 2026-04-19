using FileIntegrityMonitor.Data;
using FileIntegrityMonitor.Models;

namespace FileIntegrityMonitor.Services
{
    public class ScanService
    {
        private readonly HashDb _db;

        public ScanService(HashDb db)
        {
            _db = db;
        }

        public long RunScan(string name, string rootPath, bool isBaseline)
        {
            // Validate required inputs before creating the scan record.
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Scan name is required.");

            if (string.IsNullOrWhiteSpace(rootPath))
                throw new ArgumentException("Root path is required.");

            if (!Directory.Exists(rootPath))
                throw new DirectoryNotFoundException("The selected directory does not exist.");

            // Insert the parent scan first so each discovered file can be tied to this snapshot.
            long scanId = _db.InsertScan(name, rootPath, isBaseline);

            foreach (string filePath in Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    string hash = HashCalculator.ComputeSha512Hash(filePath);
                    long fileSize = new FileInfo(filePath).Length;
                    DateTime lastWriteUtc = File.GetLastWriteTimeUtc(filePath);

                    _db.InsertFile(scanId, filePath, fileSize, hash, lastWriteUtc);
                }
                catch (Exception ex)
                {
                    // Skip unreadable/locked files so one bad file does not abort the entire scan.
                    Console.WriteLine($"[WARN] Skipped file: {filePath} | Reason: {ex.Message}");
                }
            }

            return scanId;
        }

        public List<ScanRecord> GetAllScans() => _db.GetAllScans();
        public List<ScanRecord> GetBaselineScans() => _db.GetBaselineScans();
        public ScanRecord? GetScanById(long scanId) => _db.GetScanById(scanId);
        public List<FileRecord> GetFilesByScanId(long scanId) => _db.GetFilesByScanId(scanId);
        public void DeleteScan(long scanId) => _db.DeleteScan(scanId);
        public ScanRecord? GetLatestBaselineForPath(string rootPath) => _db.GetLatestBaselineForPath(rootPath);
    }
}