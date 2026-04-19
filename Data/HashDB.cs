using Microsoft.Data.Sqlite;
using FileIntegrityMonitor.Models;

namespace FileIntegrityMonitor.Data
{
    public class HashDb
    {
        private readonly string _connectionString;

        public HashDb(IWebHostEnvironment env)
        {
            // Store the SQLite database in the app's content root directory.
            string dbPath = Path.Combine(env.ContentRootPath, "hashes.db");
            _connectionString = $"Data Source={dbPath};";

            // Initialize SQLite native dependencies before using the database.
            SQLitePCL.Batteries_V2.Init();
            EnsureSchema();
        }

        private SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private void EnsureSchema()
        {
            using var connection = CreateConnection();
            using var command = connection.CreateCommand();

            command.CommandText =
            @"CREATE TABLE IF NOT EXISTS Scans (
                ScanId INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                RootPath TEXT NOT NULL,
                CreatedTime TEXT NOT NULL,
                IsBaseline INTEGER NOT NULL
            );";
            command.ExecuteNonQuery();

            command.CommandText =
            @"CREATE TABLE IF NOT EXISTS Files (
                ScanId INTEGER NOT NULL,
                FilePath TEXT NOT NULL,
                FileSize INTEGER NOT NULL,
                Hash TEXT NOT NULL,
                LastWriteTime TEXT NOT NULL,
                -- One file path should appear only once per scan snapshot.
                PRIMARY KEY (ScanId, FilePath),
                FOREIGN KEY (ScanId) REFERENCES Scans(ScanId)
            );";
            command.ExecuteNonQuery();
        }

        public long InsertScan(string name, string rootPath, bool isBaseline)
        {
            using var connection = CreateConnection();
            using var command = connection.CreateCommand();

            command.CommandText =
            @"INSERT INTO Scans (Name, RootPath, CreatedTime, IsBaseline)
              VALUES ($name, $rootPath, $createdTime, $isBaseline);
              SELECT last_insert_rowid();";

            command.Parameters.AddWithValue("$name", name);
            command.Parameters.AddWithValue("$rootPath", rootPath);
            command.Parameters.AddWithValue("$createdTime", DateTime.UtcNow.ToString("o"));
            command.Parameters.AddWithValue("$isBaseline", isBaseline ? 1 : 0);

            return (long)command.ExecuteScalar()!;
        }

        public void InsertFile(long scanId, string filePath, long fileSize, string hash, DateTime lastWriteTimeUtc)
        {
            using var connection = CreateConnection();
            using var command = connection.CreateCommand();

            command.CommandText =
            @"INSERT INTO Files (ScanId, FilePath, FileSize, Hash, LastWriteTime)
              VALUES ($scanId, $filePath, $fileSize, $hash, $lastWriteTime);";

            command.Parameters.AddWithValue("$scanId", scanId);
            command.Parameters.AddWithValue("$filePath", filePath);
            command.Parameters.AddWithValue("$fileSize", fileSize);
            command.Parameters.AddWithValue("$hash", hash);
            command.Parameters.AddWithValue("$lastWriteTime", lastWriteTimeUtc.ToString("o"));

            command.ExecuteNonQuery();
        }

        public List<ScanRecord> GetAllScans()
        {
            var scans = new List<ScanRecord>();

            using var connection = CreateConnection();
            using var command = connection.CreateCommand();

            command.CommandText =
            @"SELECT ScanId, Name, RootPath, CreatedTime, IsBaseline
              FROM Scans
              ORDER BY ScanId DESC;";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                scans.Add(new ScanRecord
                {
                    ScanId = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    RootPath = reader.GetString(2),
                    CreatedTime = reader.GetString(3),
                    IsBaseline = reader.GetInt64(4) == 1
                });
            }

            return scans;
        }

        public List<ScanRecord> GetBaselineScans()
        {
            var scans = new List<ScanRecord>();

            using var connection = CreateConnection();
            using var command = connection.CreateCommand();

            command.CommandText =
            @"SELECT ScanId, Name, RootPath, CreatedTime, IsBaseline
              FROM Scans
              WHERE IsBaseline = 1
              ORDER BY ScanId DESC;";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                scans.Add(new ScanRecord
                {
                    ScanId = reader.GetInt64(0),
                    Name = reader.GetString(1),
                    RootPath = reader.GetString(2),
                    CreatedTime = reader.GetString(3),
                    IsBaseline = reader.GetInt64(4) == 1
                });
            }

            return scans;
        }

        public ScanRecord? GetScanById(long scanId)
        {
            using var connection = CreateConnection();
            using var command = connection.CreateCommand();

            command.CommandText =
            @"SELECT ScanId, Name, RootPath, CreatedTime, IsBaseline
              FROM Scans
              WHERE ScanId = $scanId;";

            command.Parameters.AddWithValue("$scanId", scanId);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
                return null;

            return new ScanRecord
            {
                ScanId = reader.GetInt64(0),
                Name = reader.GetString(1),
                RootPath = reader.GetString(2),
                CreatedTime = reader.GetString(3),
                IsBaseline = reader.GetInt64(4) == 1
            };
        }

        public List<FileRecord> GetFilesByScanId(long scanId)
        {
            var files = new List<FileRecord>();

            using var connection = CreateConnection();
            using var command = connection.CreateCommand();

            command.CommandText =
            @"SELECT ScanId, FilePath, FileSize, Hash, LastWriteTime
              FROM Files
              WHERE ScanId = $scanId
              ORDER BY FilePath;";

            command.Parameters.AddWithValue("$scanId", scanId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                files.Add(new FileRecord
                {
                    ScanId = reader.GetInt64(0),
                    FilePath = reader.GetString(1),
                    FileSize = reader.GetInt64(2),
                    Hash = reader.GetString(3),
                    LastWriteTime = reader.GetString(4)
                });
            }

            return files;
        }

        public void DeleteScan(long scanId)
        {
            using var connection = CreateConnection();
            using var transaction = connection.BeginTransaction();

            // Delete child file records first, then the parent scan record,
            // so related data is removed together.
            using var deleteFiles = connection.CreateCommand();
            deleteFiles.CommandText = @"DELETE FROM Files WHERE ScanId = $scanId;";
            deleteFiles.Parameters.AddWithValue("$scanId", scanId);
            deleteFiles.ExecuteNonQuery();

            using var deleteScan = connection.CreateCommand();
            deleteScan.CommandText = @"DELETE FROM Scans WHERE ScanId = $scanId;";
            deleteScan.Parameters.AddWithValue("$scanId", scanId);
            deleteScan.ExecuteNonQuery();

            transaction.Commit();
        }

        public ScanRecord? GetLatestBaselineForPath(string rootPath)
        {
            using var connection = CreateConnection();
            using var command = connection.CreateCommand();

            command.CommandText =
            @"SELECT ScanId, Name, RootPath, CreatedTime, IsBaseline
            FROM Scans
            WHERE IsBaseline = 1 AND RootPath = $rootPath
            ORDER BY ScanId DESC
            LIMIT 1;";

            command.Parameters.AddWithValue("$rootPath", rootPath);

            using var reader = command.ExecuteReader();
            if (!reader.Read())
                return null;

            return new ScanRecord
            {
                ScanId = reader.GetInt64(0),
                Name = reader.GetString(1),
                RootPath = reader.GetString(2),
                CreatedTime = reader.GetString(3),
                IsBaseline = reader.GetInt64(4) == 1
            };
        }
    }
}