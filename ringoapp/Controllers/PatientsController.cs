using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using medicalapp.DTOs;
using medicalapp.Exceptions;
using medicalapp.Services;

namespace medicalapp.Controllers
{
    [Authorize(Roles = "Patient")]
    public class PatientsController : Controller
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IDoctorService _doctorService;
        private readonly IAdminService _adminService;

        public PatientsController(
            IAppointmentService appointmentService,
            IDoctorService doctorService,
            IAdminService adminService)
        {
            _appointmentService = appointmentService;
            _doctorService = doctorService;
            _adminService = adminService;
        }

        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User is not logged in.");
        }

        public async Task<IActionResult> Dashboard()
        {
            var patientId = GetCurrentUserId();
            var appointments = await _appointmentService.GetPatientAppointmentsAsync(patientId);
            return View(appointments);
        }

        public async Task<IActionResult> SearchDoctors(int? departmentId, string? query)
        {
            var doctors = await _doctorService.GetDoctorsAsync(departmentId, query);
            ViewBag.Departments = await _adminService.GetDepartmentsAsync();
            ViewBag.SelectedDepartmentId = departmentId;
            ViewBag.SearchQuery = query;
            return View(doctors);
        }

        [HttpGet]
        public async Task<IActionResult> BookAppointment(string doctorId)
        {
            var doctor = await _doctorService.GetDoctorProfileAsync(doctorId);
            if (doctor == null || !doctor.IsVerified)
            {
                return NotFound("Doctor not found or not verified.");
            }

            ViewBag.Doctor = doctor;
            var model = new AppointmentBookDto
            {
                DoctorId = doctorId,
                ScheduledDateTime = DateTime.Today.AddDays(1).AddHours(9) // default tomorrow 9 AM
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookAppointment(AppointmentBookDto model)
        {
            if (!ModelState.IsValid)
            {
                var doctor = await _doctorService.GetDoctorProfileAsync(model.DoctorId);
                ViewBag.Doctor = doctor;
                return View(model);
            }

            try
            {
                await _appointmentService.BookAppointmentAsync(model, GetCurrentUserId());
                TempData["SuccessMessage"] = "Appointment booked successfully!";
                return RedirectToAction(nameof(Dashboard));
            }
            catch (AppException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var doctor = await _doctorService.GetDoctorProfileAsync(model.DoctorId);
                ViewBag.Doctor = doctor;
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelAppointment(int appointmentId)
        {
            try
            {
                await _appointmentService.CancelAppointmentAsync(appointmentId, GetCurrentUserId(), "Patient");
                TempData["SuccessMessage"] = "Appointment cancelled successfully.";
            }
            catch (AppException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> Prescriptions()
        {
            var patientId = GetCurrentUserId();
            var appointments = await _appointmentService.GetPatientAppointmentsAsync(patientId);
            var completedWithPrescriptions = appointments
                .Where(a => a.Status == Common.AppointmentStatus.Completed && a.Prescription != null)
                .ToList();

            return View(completedWithPrescriptions);
        }

        [HttpGet]
        public async Task<IActionResult> PrintPrescription(int appointmentId)
        {
            var appointment = await _appointmentService.GetAppointmentDetailsAsync(appointmentId);
            if (appointment == null || appointment.PatientId != GetCurrentUserId() || appointment.Prescription == null)
            {
                return NotFound("Prescription not found.");
            }

            return View(appointment);
        }
    }
}
