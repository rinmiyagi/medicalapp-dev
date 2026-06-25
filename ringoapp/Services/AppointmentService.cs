using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using medicalapp.Common;
using medicalapp.Data;
using medicalapp.DTOs;
using medicalapp.Exceptions;
using medicalapp.Models;

namespace medicalapp.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly medicalappContext _context;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;

        public AppointmentService(
            medicalappContext context,
            INotificationService notificationService,
            IEmailService emailService)
        {
            _context = context;
            _notificationService = notificationService;
            _emailService = emailService;
        }

        public async Task<List<Appointment>> GetPatientAppointmentsAsync(string patientId)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .Include(a => a.Prescription)
                .Where(a => a.PatientId == patientId)
                .OrderBy(a => a.ScheduledDateTime)
                .ToListAsync();
        }

        public async Task<List<Appointment>> GetDoctorAppointmentsAsync(string doctorId)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Prescription)
                .Where(a => a.DoctorId == doctorId)
                .OrderBy(a => a.ScheduledDateTime)
                .ToListAsync();
        }

        public async Task<Appointment?> GetAppointmentDetailsAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Department)
                .Include(a => a.Prescription)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        }

        public async Task<Appointment> BookAppointmentAsync(AppointmentBookDto dto, string patientId)
        {
            if (dto.ScheduledDateTime <= DateTime.UtcNow)
            {
                throw new AppException("Appointments must be booked for a future date and time.");
            }

            if (dto.ScheduledDateTime.Minute % 30 != 0 || dto.ScheduledDateTime.Second != 0 || dto.ScheduledDateTime.Millisecond != 0)
            {
                throw new AppException("Appointments can only be booked in 30-minute increments (e.g., :00, :30).");
            }

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Schedules)
                .FirstOrDefaultAsync(d => d.DoctorId == dto.DoctorId);

            if (doctor == null)
            {
                throw new AppException("The selected doctor does not exist.");
            }

            if (!doctor.IsVerified)
            {
                throw new AppException("Cannot book an appointment with a doctor who is not yet verified.");
            }

            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null)
            {
                throw new AppException("Patient profile not found.");
            }

            // Check if selected time is within the doctor's weekly schedule
            var day = (int)dto.ScheduledDateTime.DayOfWeek;
            var time = dto.ScheduledDateTime.TimeOfDay;
            var isWithinSchedule = doctor.Schedules.Any(s =>
                s.DayOfWeek == day &&
                time >= s.StartTime &&
                time < s.EndTime);

            if (!isWithinSchedule)
            {
                throw new AppException("Selected time is outside of the doctor's working schedule.");
            }

            // Check if slot is already booked by another patient
            var isAlreadyBooked = await _context.Appointments.AnyAsync(a =>
                a.DoctorId == dto.DoctorId &&
                a.ScheduledDateTime == dto.ScheduledDateTime &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.Rejected);

            if (isAlreadyBooked)
            {
                throw new AppException("This time slot has already been booked by another patient.");
            }

            // Check if patient already has an appointment at that exact time
            var duplicate = await _context.Appointments
                .AnyAsync(a => a.PatientId == patientId && 
                               a.ScheduledDateTime == dto.ScheduledDateTime && 
                               a.Status != AppointmentStatus.Cancelled &&
                               a.Status != AppointmentStatus.Rejected);

            if (duplicate)
            {
                throw new AppException("You already have an appointment scheduled for this time.");
            }

            var appointment = new Appointment
            {
                PatientId = patientId,
                DoctorId = dto.DoctorId,
                ScheduledDateTime = dto.ScheduledDateTime,
                Status = AppointmentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            // Notify Doctor
            await _notificationService.CreateNotificationAsync(
                doctor.DoctorId, 
                $"New appointment request from {patient.FullName} for {dto.ScheduledDateTime:g}.",
                "/Doctors/Dashboard"
            );

            if (!string.IsNullOrEmpty(doctor.User?.Email))
            {
                await _emailService.SendEmailAsync(
                    doctor.User.Email,
                    "New Appointment Request Received",
                    $"Dear Dr. {doctor.FullName},\n\nYou have received a new consultation request from {patient.FullName} for {dto.ScheduledDateTime:g}.\n\nPlease review it in your portal."
                );
            }

            return appointment;
        }

        public async Task CancelAppointmentAsync(int appointmentId, string userId, string role)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
            {
                throw new AppException("Appointment not found.");
            }

            if (appointment.Status == AppointmentStatus.Completed || 
                appointment.Status == AppointmentStatus.Cancelled ||
                appointment.Status == AppointmentStatus.Rejected)
            {
                throw new AppException($"Cannot cancel appointment that is already {appointment.Status}.");
            }

            // Authorization check
            if (role == "Patient" && appointment.PatientId != userId)
            {
                throw new AppException("You are not authorized to cancel this appointment.");
            }
            if (role == "Doctor" && appointment.DoctorId != userId)
            {
                throw new AppException("You are not authorized to cancel this appointment.");
            }

            appointment.Status = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();

            // Send notification to the other party
            if (role == "Patient")
            {
                await _notificationService.CreateNotificationAsync(
                    appointment.DoctorId,
                    $"Appointment with {appointment.Patient.FullName} for {appointment.ScheduledDateTime:g} has been cancelled by the patient.",
                    "/Doctors/Dashboard"
                );

                if (!string.IsNullOrEmpty(appointment.Doctor.User?.Email))
                {
                    await _emailService.SendEmailAsync(
                        appointment.Doctor.User.Email,
                        "Appointment Cancelled by Patient",
                        $"Dr. {appointment.Doctor.FullName},\n\nThe appointment scheduled with {appointment.Patient.FullName} for {appointment.ScheduledDateTime:g} has been cancelled."
                    );
                }
            }
            else
            {
                await _notificationService.CreateNotificationAsync(
                    appointment.PatientId,
                    $"Your appointment with Dr. {appointment.Doctor.FullName} for {appointment.ScheduledDateTime:g} has been cancelled by the doctor.",
                    "/Patients/Dashboard"
                );

                if (!string.IsNullOrEmpty(appointment.Patient.User?.Email))
                {
                    await _emailService.SendEmailAsync(
                        appointment.Patient.User.Email,
                        "Appointment Cancelled by Doctor",
                        $"Dear {appointment.Patient.FullName},\n\nYour appointment scheduled with Dr. {appointment.Doctor.FullName} for {appointment.ScheduledDateTime:g} has been cancelled."
                    );
                }
            }
        }

        public async Task CompleteAppointmentAsync(int appointmentId, string doctorId, string medications, string notes)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
            {
                throw new AppException("Appointment not found.");
            }

            if (appointment.DoctorId != doctorId)
            {
                throw new AppException("You are not authorized to complete this appointment.");
            }

            if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Confirmed)
            {
                throw new AppException("Can only complete active pending or confirmed appointments.");
            }

            appointment.Status = AppointmentStatus.Completed;

            // Generate prescription
            var prescription = new Prescription
            {
                AppointmentId = appointmentId,
                PatientId = appointment.PatientId,
                DoctorId = doctorId,
                Medications = medications,
                Notes = notes,
                IssuedAt = DateTime.UtcNow
            };

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            // Notify Patient
            await _notificationService.CreateNotificationAsync(
                appointment.PatientId,
                $"Your consultation with Dr. {appointment.Doctor.FullName} is completed. Your prescription has been issued.",
                "/Patients/Prescriptions"
            );

            if (!string.IsNullOrEmpty(appointment.Patient.User?.Email))
            {
                await _emailService.SendEmailAsync(
                    appointment.Patient.User.Email,
                    "Consultation Completed & Prescription Issued",
                    $"Dear {appointment.Patient.FullName},\n\nYour consultation with Dr. {appointment.Doctor.FullName} on {appointment.ScheduledDateTime:g} has been finalized.\n\nYour prescription of {medications} has been issued. You can view details in your portal."
                );
            }
        }

        public async Task ConfirmAppointmentAsync(int appointmentId, string doctorId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.DoctorId == doctorId);

            if (appointment == null)
            {
                throw new AppException("Appointment not found.");
            }

            if (appointment.Status != AppointmentStatus.Pending)
            {
                throw new AppException($"Cannot confirm appointment that is already {appointment.Status}.");
            }

            appointment.Status = AppointmentStatus.Confirmed;
            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                appointment.PatientId,
                $"Your appointment request with Dr. {appointment.Doctor.FullName} for {appointment.ScheduledDateTime:g} has been confirmed.",
                "/Patients/Dashboard"
            );

            if (!string.IsNullOrEmpty(appointment.Patient.User?.Email))
            {
                await _emailService.SendEmailAsync(
                    appointment.Patient.User.Email,
                    "Appointment Confirmed",
                    $"Dear {appointment.Patient.FullName},\n\nYour appointment request with Dr. {appointment.Doctor.FullName} for {appointment.ScheduledDateTime:g} has been confirmed."
                );
            }
        }

        public async Task RejectAppointmentAsync(int appointmentId, string doctorId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.DoctorId == doctorId);

            if (appointment == null)
            {
                throw new AppException("Appointment not found.");
            }

            if (appointment.Status != AppointmentStatus.Pending)
            {
                throw new AppException($"Cannot reject appointment that is already {appointment.Status}.");
            }

            appointment.Status = AppointmentStatus.Rejected;
            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                appointment.PatientId,
                $"Your appointment request with Dr. {appointment.Doctor.FullName} for {appointment.ScheduledDateTime:g} has been rejected.",
                "/Patients/Dashboard"
            );

            if (!string.IsNullOrEmpty(appointment.Patient.User?.Email))
            {
                await _emailService.SendEmailAsync(
                    appointment.Patient.User.Email,
                    "Appointment Rejected",
                    $"Dear {appointment.Patient.FullName},\n\nYour appointment request with Dr. {appointment.Doctor.FullName} for {appointment.ScheduledDateTime:g} has been rejected."
                );
            }
        }

        public async Task MarkAsMissedAsync(int appointmentId, string doctorId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.DoctorId == doctorId);

            if (appointment == null)
            {
                throw new AppException("Appointment not found.");
            }

            if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Confirmed)
            {
                throw new AppException($"Cannot mark appointment as missed if it is already {appointment.Status}.");
            }

            appointment.Status = AppointmentStatus.Missed;
            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                appointment.PatientId,
                $"Your scheduled appointment with Dr. {appointment.Doctor.FullName} on {appointment.ScheduledDateTime:g} was missed and has been closed.",
                "/Patients/Dashboard"
            );

            if (!string.IsNullOrEmpty(appointment.Patient.User?.Email))
            {
                await _emailService.SendEmailAsync(
                    appointment.Patient.User.Email,
                    "Appointment Missed",
                    $"Dear {appointment.Patient.FullName},\n\nYour scheduled appointment with Dr. {appointment.Doctor.FullName} on {appointment.ScheduledDateTime:g} was marked as missed."
                );
            }
        }
    }
}
