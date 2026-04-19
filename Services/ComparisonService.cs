using FileIntegrityMonitor.Data;
using FileIntegrityMonitor.Models;

namespace FileIntegrityMonitor.Services
{
    public class ComparisonService
    {
        private readonly HashDb _db;

        public ComparisonService(HashDb db)
        {
            _db = db;
        }

        public CompareViewModel CompareScans(long baselineScanId, long currentScanId)
        {
            var baseline = _db.GetScanById(baselineScanId);
            var current = _db.GetScanById(currentScanId);

            if (baseline == null || current == null)
                throw new Exception("One or both scans could not be found.");

            // Convert both scan file lists into dictionaries so files can be looked up quickly by path.
            var baselineFiles = _db.GetFilesByScanId(baselineScanId)
                .ToDictionary(f => f.FilePath, f => f);

            var currentFiles = _db.GetFilesByScanId(currentScanId)
                .ToDictionary(f => f.FilePath, f => f);


            // Build one combined set of all file paths so we can detect
            // deleted, new, unchanged, and modified files.
            var allPaths = baselineFiles.Keys
                .Union(currentFiles.Keys)
                .OrderBy(p => p)
                .ToList();

            var rows = new List<ComparisonRow>();

            foreach (var path in allPaths)
            {
                bool inBaseline = baselineFiles.ContainsKey(path);
                bool inCurrent = currentFiles.ContainsKey(path);

                if (inBaseline && !inCurrent)
                {
                    // File existed in the baseline but no longer exists in the current scan.
                    var oldFile = baselineFiles[path];
                    rows.Add(new ComparisonRow
                    {
                        FilePath = path,
                        Status = "Deleted",
                        OldSize = oldFile.FileSize,
                        OldHash = oldFile.Hash
                    });
                }
                else if (!inBaseline && inCurrent)
                {
                    // File is present now but did not exist in the baseline.
                    var newFile = currentFiles[path];
                    rows.Add(new ComparisonRow
                    {
                        FilePath = path,
                        Status = "New",
                        NewSize = newFile.FileSize,
                        NewHash = newFile.Hash
                    });
                }
                else
                {
                    // File exists in both scans, so compare hashes to determine whether contents changed.
                    var oldFile = baselineFiles[path];
                    var newFile = currentFiles[path];

                    bool sameHash = oldFile.Hash == newFile.Hash;

                    rows.Add(new ComparisonRow
                    {
                        FilePath = path,
                        Status = sameHash ? "Unchanged" : "Modified",
                        OldSize = oldFile.FileSize,
                        NewSize = newFile.FileSize,
                        OldHash = oldFile.Hash,
                        NewHash = newFile.Hash
                    });
                }
            }

            return new CompareViewModel
            {
                Baseline = baseline,
                CurrentScan = current,
                Rows = rows
            };
        }
    }
}