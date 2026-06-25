using System.Collections.Generic;
using System.Threading.Tasks;
using medicalapp.DTOs;
using medicalapp.Models;

namespace medicalapp.Services
{
    public interface IDoctorService
    {
        Task<List<Doctor>> GetDoctorsAsync(int? departmentId, string? query);
        Task<Doctor?> GetDoctorProfileAsync(string doctorId);
        Task UpdateDoctorScheduleAsync(string doctorId, List<DoctorScheduleDto> schedules);
    }
}
