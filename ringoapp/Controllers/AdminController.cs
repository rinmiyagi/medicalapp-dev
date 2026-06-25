using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using medicalapp.Exceptions;
using medicalapp.Services;

namespace medicalapp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var pendingDoctors = await _adminService.GetPendingDoctorsAsync();
            return View(pendingDoctors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyDoctor(string doctorId, bool verify)
        {
            try
            {
                await _adminService.VerifyDoctorAsync(doctorId, verify);
                TempData["SuccessMessage"] = verify 
                    ? "Doctor credentials approved successfully!" 
                    : "Doctor profile rejected and removed.";
            }
            catch (AppException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> Departments()
        {
            var departments = await _adminService.GetDepartmentsAsync();
            return View(departments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDepartment(string departmentName)
        {
            if (string.IsNullOrWhiteSpace(departmentName))
            {
                TempData["ErrorMessage"] = "Department name cannot be empty.";
                return RedirectToAction(nameof(Departments));
            }

            try
            {
                await _adminService.AddDepartmentAsync(departmentName);
                TempData["SuccessMessage"] = "Medical department added successfully!";
            }
            catch (AppException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Departments));
        }
    }
}
