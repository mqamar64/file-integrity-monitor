using System.Security.Cryptography;
using System.Text;

namespace FileIntegrityMonitor.Services
{
    public static class HashCalculator
    {
        public static string ComputeSha512Hash(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found.", filePath);

            // Open the file as a stream so the hash can be computed directly from file contents.
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var sha512 = SHA512.Create();

            byte[] hashBytes = sha512.ComputeHash(fileStream);
            var sb = new StringBuilder(hashBytes.Length * 2);

            foreach (byte b in hashBytes)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
    }
}