using System.ComponentModel.DataAnnotations;

namespace medicalapp.Models
{
    /// <summary>
    /// Master data for medical departments.
    /// </summary>
    public class MedicalDepartment
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string DepartmentName { get; set; } = null!;

        // Navigation property
        public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
