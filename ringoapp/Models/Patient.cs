using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using medicalapp.Areas.Identity.Data;

namespace medicalapp.Models
{
    /// <summary>
    /// Patient profile details. Linked 1-to-1 with medicalappUser (IdentityUser).
    /// </summary>
    public class Patient
    {
        [Key]
        [ForeignKey("User")]
        [StringLength(450)]
        public string PatientId { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = null!;

        public string? MedicalHistoryBaseline { get; set; }

        // Navigation properties
        public virtual medicalappUser User { get; set; } = null!;
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}
