using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using medicalapp.Data;
using medicalapp.Exceptions;
using medicalapp.Models;

namespace medicalapp.Services
{
    public class AdminService : IAdminService
    {
        private readonly medicalappContext _context;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public AdminService(
            medicalappContext context,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _context = context;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task<List<Doctor>> GetPendingDoctorsAsync()
        {
            return await _context.Doctors
                .Include(d => d.Department)
                .Include(d => d.User)
                    .ThenInclude(u => u.Documents)
                .Where(d => !d.IsVerified)
                .OrderByDescending(d => d.User.CreatedAt)
                .ToListAsync();
        }

        public async Task VerifyDoctorAsync(string doctorId, bool verify)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

            if (doctor == null)
            {
                throw new AppException("Doctor profile not found.");
            }

            if (verify)
            {
                doctor.IsVerified = true;
                await _context.SaveChangesAsync();

                await _notificationService.CreateNotificationAsync(
                    doctorId,
                    "Your doctor credentials have been approved! You can now accept consultation requests.",
                    "/Doctors/Dashboard"
                );

                if (!string.IsNullOrEmpty(doctor.User?.Email))
                {
                    await _emailService.SendEmailAsync(
                        doctor.User.Email,
                        "Credentials Approved - MediCloud Portal",
                        $"Dear Dr. {doctor.FullName},\n\nWe are pleased to inform you that your medical credentials have been successfully verified and approved.\n\nYou can now set your availability and accept patient bookings in the portal."
                    );
                }
            }
            else
            {
                // If rejected, we can delete the profile or keep it flag-based. Deleting the doctor entity (or disabling) is standard.
                // Let's delete the doctor profile and set the user role back or delete the user.
                // For simplicity, let's remove the doctor profile.
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();

                await _notificationService.CreateNotificationAsync(
                    doctorId,
                    "Your doctor credentials verification failed. Please check with administrator.",
                    "/"
                );

                if (!string.IsNullOrEmpty(doctor.User?.Email))
                {
                    await _emailService.SendEmailAsync(
                        doctor.User.Email,
                        "Credentials Rejected - MediCloud Portal",
                        $"Dear Dr. {doctor.FullName},\n\nUnfortunately, we were unable to verify your medical credentials. Your doctor profile has been rejected.\n\nPlease contact support for more details."
                    );
                }
            }
        }

        public async Task<List<MedicalDepartment>> GetDepartmentsAsync()
        {
            return await _context.MedicalDepartments
                .Include(d => d.Doctors)
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
        }

        public async Task AddDepartmentAsync(string departmentName)
        {
            if (string.IsNullOrWhiteSpace(departmentName))
            {
                throw new AppException("Department name cannot be empty.");
            }

            var exists = await _context.MedicalDepartments
                .AnyAsync(d => d.DepartmentName.ToLower() == departmentName.Trim().ToLower());

            if (exists)
            {
                throw new AppException("A department with this name already exists.");
            }

            var dept = new MedicalDepartment
            {
                DepartmentName = departmentName.Trim()
            };

            _context.MedicalDepartments.Add(dept);
            await _context.SaveChangesAsync();
        }
    }
}
