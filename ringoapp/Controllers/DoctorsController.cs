using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using medicalapp.DTOs;
using medicalapp.Exceptions;
using medicalapp.Services;

namespace medicalapp.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IDoctorService _doctorService;

        public DoctorsController(
            IAppointmentService appointmentService,
            IDoctorService doctorService)
        {
            _appointmentService = appointmentService;
            _doctorService = doctorService;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User is not logged in.");
        }

        public async Task<IActionResult> Dashboard()
        {
            var doctorId = GetCurrentUserId();
            var doctor = await _doctorService.GetDoctorProfileAsync(doctorId);
            if (doctor == null)
            {
                return NotFound("Doctor profile not found.");
            }

            if (!doctor.IsVerified)
            {
                ViewBag.NotVerified = true;
                return View(new List<medicalapp.Models.Appointment>());
            }

            var appointments = await _appointmentService.GetDoctorAppointmentsAsync(doctorId);
            return View(appointments);
        }

        [HttpGet]
        public async Task<IActionResult> Schedule()
        {
            var doctorId = GetCurrentUserId();
            var doctor = await _doctorService.GetDoctorProfileAsync(doctorId);
            if (doctor == null)
            {
                return NotFound("Doctor profile not found.");
            }

            var model = new List<DoctorScheduleDto>();
            for (int i = 0; i < 7; i++)
            {
                var s = doctor.Schedules.FirstOrDefault(x => x.DayOfWeek == i);
                model.Add(new DoctorScheduleDto
                {
                    DayOfWeek = i,
                    StartTime = s?.StartTime ?? new TimeSpan(9, 0, 0),
                    EndTime = s?.EndTime ?? new TimeSpan(17, 0, 0),
                    IsActive = s != null
                });
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Schedule(List<DoctorScheduleDto> model)
        {
            model ??= new List<DoctorScheduleDto>();
            var activeSchedules = model.Where(s => s.IsActive).ToList();

            try
            {
                await _doctorService.UpdateDoctorScheduleAsync(GetCurrentUserId(), activeSchedules);
                TempData["SuccessMessage"] = "Schedule updated successfully!";
                return RedirectToAction(nameof(Dashboard));
            }
            catch (AppException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CompleteAppointment(int appointmentId)
        {
            var appointment = await _appointmentService.GetAppointmentDetailsAsync(appointmentId);
            if (appointment == null)
            {
                return NotFound("Appointment not found.");
            }

            if (appointment.DoctorId != GetCurrentUserId())
            {
                return Forbid();
            }

            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteAppointment(int appointmentId, string medications, string notes)
        {
            if (string.IsNullOrWhiteSpace(medications))
            {
                ModelState.AddModelError(nameof(medications), "Medications list is required.");
                var appointment = await _appointmentService.GetAppointmentDetailsAsync(appointmentId);
                return View(appointment);
            }

            try
            {
                await _appointmentService.CompleteAppointmentAsync(appointmentId, GetCurrentUserId(), medications, notes);
                TempData["SuccessMessage"] = "Appointment completed and prescription issued!";
                return RedirectToAction(nameof(Dashboard));
            }
            catch (AppException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var appointment = await _appointmentService.GetAppointmentDetailsAsync(appointmentId);
                return View(appointment);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmAppointment(int appointmentId)
        {
            try
            {
                await _appointmentService.ConfirmAppointmentAsync(appointmentId, GetCurrentUserId());
                TempData["SuccessMessage"] = "Appointment request confirmed!";
            }
            catch (AppException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectAppointment(int appointmentId)
        {
            try
            {
                await _appointmentService.RejectAppointmentAsync(appointmentId, GetCurrentUserId());
                TempData["SuccessMessage"] = "Appointment request rejected.";
            }
            catch (AppException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsMissed(int appointmentId)
        {
            try
            {
                await _appointmentService.MarkAsMissedAsync(appointmentId, GetCurrentUserId());
                TempData["SuccessMessage"] = "Appointment marked as missed.";
            }
            catch (AppException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            try
            {
                await _appointmentService.CancelAppointmentAsync(appointmentId, GetCurrentUserId(), "Doctor");
                TempData["SuccessMessage"] = "Appointment cancelled successfully.";
            }
            catch (AppException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Dashboard));
        }
    }
}
