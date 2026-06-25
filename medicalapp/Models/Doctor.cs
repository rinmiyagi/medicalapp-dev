using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using medicalapp.Areas.Identity.Data;

namespace medicalapp.Models
{
    /// <summary>
    /// Doctor profile details. Linked 1-to-1 with medicalappUser (IdentityUser).
    /// </summary>
    public class Doctor
    {
        [Key]
        [ForeignKey("User")]
        [StringLength(450)]
        public string DoctorId { get; set; } = null!;

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Specialization { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ConsultationFee { get; set; }

        [Required]
        [StringLength(100)]
        public string HospitalName { get; set; } = null!;

        public bool IsVerified { get; set; } = false;

        // Navigation properties
        public virtual medicalappUser User { get; set; } = null!;

        [ForeignKey("DepartmentId")]
        public virtual MedicalDepartment Department { get; set; } = null!;

        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public virtual ICollection<DoctorSchedule> Schedules { get; set; } = new List<DoctorSchedule>();
    }
}
