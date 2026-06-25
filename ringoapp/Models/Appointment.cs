using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medicalapp.Models
{
    /// <summary>
    /// Represents a consultation appointment between a patient and a doctor.
    /// </summary>
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }

        [Required]
        [StringLength(450)]
        public string PatientId { get; set; } = null!;

        [Required]
        [StringLength(450)]
        public string DoctorId { get; set; } = null!;

        [Required]
        public DateTime ScheduledDateTime { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // 'Pending', 'Confirmed', 'Completed', 'Cancelled', 'Missed'

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; } = null!;

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; } = null!;

        public virtual Prescription? Prescription { get; set; }
    }
}
