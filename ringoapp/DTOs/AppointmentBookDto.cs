using System;
using System.ComponentModel.DataAnnotations;

namespace medicalapp.DTOs
{
    public class AppointmentBookDto
    {
        [Required]
        public string DoctorId { get; set; } = null!;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ScheduledDateTime { get; set; }
    }
}
