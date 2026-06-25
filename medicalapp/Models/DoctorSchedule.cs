using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medicalapp.Models
{
    /// <summary>
    /// Represents the doctor's weekly schedule (day of week and time range).
    /// </summary>
    public class DoctorSchedule
    {
        [Key]
        public int ScheduleId { get; set; }

        [Required]
        [StringLength(450)]
        public string DoctorId { get; set; } = null!;

        /// <summary>
        /// 0 = Sunday, 1 = Monday, ..., 6 = Saturday
        /// </summary>
        [Required]
        public int DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        // Navigation property
        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; } = null!;
    }
}
