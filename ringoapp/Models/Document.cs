using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using medicalapp.Areas.Identity.Data;

namespace medicalapp.Models
{
    /// <summary>
    /// Manages uploaded documents such as medical licenses.
    /// </summary>
    public class Document
    {
        [Key]
        public int DocumentId { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = null!;

        /// <summary>
        /// File storage path (Local: wwwroot/uploads/... / Task#2: S3 Object Key)
        /// </summary>
        [Required]
        public string S3ObjectKey { get; set; } = null!;

        [Required]
        public string FileUrl { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string DocumentType { get; set; } = null!; // e.g., "MedicalLicense"

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual medicalappUser User { get; set; } = null!;
    }
}
