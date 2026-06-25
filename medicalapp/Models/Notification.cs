using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using medicalapp.Areas.Identity.Data;

namespace medicalapp.Models
{
    /// <summary>
    /// Manages notifications sent to users (e.g., appointment confirmations, cancellations).
    /// </summary>
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = null!;

        [Required]
        public string Message { get; set; } = null!;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(250)]
        public string? TargetUrl { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual medicalappUser User { get; set; } = null!;
    }
}
