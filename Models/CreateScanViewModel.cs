using System.ComponentModel.DataAnnotations;

namespace FileIntegrityMonitor.Models
{
    public class CreateScanViewModel
    {
        [Required]
        [Display(Name = "Scan Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Folder Path")]
        public string RootPath { get; set; } = string.Empty;
    }
}