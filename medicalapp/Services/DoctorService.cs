using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using medicalapp.Data;
using medicalapp.DTOs;
using medicalapp.Exceptions;
using medicalapp.Models;

namespace medicalapp.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly medicalappContext _context;

        public DoctorService(medicalappContext context)
        {
            _context = context;
        }

        public async Task<List<Doctor>> GetDoctorsAsync(int? departmentId, string? query)
        {
            var doctorsQuery = _context.Doctors
                .Include(d => d.Department)
                .Where(d => d.IsVerified);

            if (departmentId.HasValue)
            {
                doctorsQuery = doctorsQuery.Where(d => d.DepartmentId == departmentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim().ToLower();
                doctorsQuery = doctorsQuery.Where(d => 
                    d.FullName.ToLower().Contains(query) || 
                    d.Specialization.ToLower().Contains(query) || 
                    d.HospitalName.ToLower().Contains(query)
                );
            }

            return await doctorsQuery.OrderBy(d => d.FullName).ToListAsync();
        }

        public async Task<Doctor?> GetDoctorProfileAsync(string doctorId)
        {
            return await _context.Doctors
                .Include(d => d.Department)
                .Include(d => d.Schedules)
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId);
        }

        public async Task UpdateDoctorScheduleAsync(string doctorId, List<DoctorScheduleDto> schedules)
        {
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null)
            {
                throw new AppException("Doctor profile not found.");
            }

            // Remove existing schedules
            var existingSchedules = _context.DoctorSchedules.Where(ds => ds.DoctorId == doctorId);
            _context.DoctorSchedules.RemoveRange(existingSchedules);

            // Add new schedules
            foreach (var s in schedules)
            {
                if (s.DayOfWeek < 0 || s.DayOfWeek > 6)
                {
                    throw new AppException("Day of week must be between 0 (Sunday) and 6 (Saturday).");
                }
                if (s.StartTime >= s.EndTime)
                {
                    throw new AppException("Start time must be earlier than end time.");
                }

                var schedule = new DoctorSchedule
                {
                    DoctorId = doctorId,
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                };
                _context.DoctorSchedules.Add(schedule);
            }

            await _context.SaveChangesAsync();
        }
    }
}
