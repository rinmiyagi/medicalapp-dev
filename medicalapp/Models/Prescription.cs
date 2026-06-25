using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medicalapp.Models
{
    /// <summary>
    /// Prescription issued by the doctor after consultation completion.
    /// </summary>
    public class Prescription
    {
        [Key]
        public int PrescriptionId { get; set; }

        [Required]
        public int AppointmentId { get; set; }

        [Required]
        [StringLength(450)]
        public string PatientId { get; set; } = null!;

        [Required]
        [StringLength(450)]
        public string DoctorId { get; set; } = null!;

        [Required]
        public string Medications { get; set; } = null!;

        public string? Notes { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("AppointmentId")]
        public virtual Appointment Appointment { get; set; } = null!;

        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; } = null!;

        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; } = null!;
    }
}
