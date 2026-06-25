using System;

namespace medicalapp.DTOs
{
    public class DoctorScheduleDto
    {
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }
    }
}
