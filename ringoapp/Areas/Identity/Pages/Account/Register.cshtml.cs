// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using medicalapp.Areas.Identity.Data;
using medicalapp.Data;
using medicalapp.Models;

namespace medicalapp.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<medicalappUser> _signInManager;
        private readonly UserManager<medicalappUser> _userManager;
        private readonly IUserStore<medicalappUser> _userStore;
        private readonly IUserEmailStore<medicalappUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly medicalappContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RegisterModel(
            UserManager<medicalappUser> userManager,
            IUserStore<medicalappUser> userStore,
            SignInManager<medicalappUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            medicalappContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public SelectList Departments { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required(ErrorMessage = "Full name is required.")]
            [StringLength(100, ErrorMessage = "Full name must be under 100 characters.")]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Please select a role.")]
            [Display(Name = "Register As")]
            public string Role { get; set; } // "Patient" or "Doctor"

            [Display(Name = "Date of Birth (Patients only)")]
            [DataType(DataType.Date)]
            public DateTime? DateOfBirth { get; set; }

            [Display(Name = "Gender (Patients only)")]
            [StringLength(10)]
            public string InputGender { get; set; }

            [Display(Name = "Phone Number (Patients only)")]
            [StringLength(20)]
            public string PhoneNumber { get; set; }

            // Doctor fields
            [Display(Name = "Medical Department (Doctors only)")]
            public int? DepartmentId { get; set; }

            [StringLength(100)]
            [Display(Name = "Specialization / Field (Doctors only)")]
            public string Specialization { get; set; }

            [Range(0, 100000, ErrorMessage = "Fee must be a positive number.")]
            [Display(Name = "Consultation Fee (Doctors only)")]
            public decimal? ConsultationFee { get; set; }

            [StringLength(150)]
            [Display(Name = "Hospital / Clinic Name (Doctors only)")]
            public string HospitalName { get; set; }

            [Display(Name = "Medical License Document (Doctors only)")]
            public IFormFile LicenseImage { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            await LoadDepartmentsAsync();
        }

        private async Task LoadDepartmentsAsync()
        {
            var depts = await _context.MedicalDepartments
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();
            Departments = new SelectList(depts, "DepartmentId", "DepartmentName");
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Custom conditional validation
            if (Input.Role == "Patient")
            {
                if (string.IsNullOrWhiteSpace(Input.InputGender))
                {
                    ModelState.AddModelError("Input.InputGender", "Gender is required for patients.");
                }
                if (!Input.DateOfBirth.HasValue)
                {
                    ModelState.AddModelError("Input.DateOfBirth", "Date of Birth is required for patients.");
                }
            }
            else if (Input.Role == "Doctor")
            {
                if (!Input.DepartmentId.HasValue)
                {
                    ModelState.AddModelError("Input.DepartmentId", "Department is required for doctors.");
                }
                if (string.IsNullOrWhiteSpace(Input.Specialization))
                {
                    ModelState.AddModelError("Input.Specialization", "Specialization is required for doctors.");
                }
                if (!Input.ConsultationFee.HasValue)
                {
                    ModelState.AddModelError("Input.ConsultationFee", "Consultation Fee is required for doctors.");
                }
                if (string.IsNullOrWhiteSpace(Input.HospitalName))
                {
                    ModelState.AddModelError("Input.HospitalName", "Hospital Name is required for doctors.");
                }
                if (Input.LicenseImage == null || Input.LicenseImage.Length == 0)
                {
                    ModelState.AddModelError("Input.LicenseImage", "Medical license image/PDF is required.");
                }
                else
                {
                    // File size validation (max 10 MB)
                    if (Input.LicenseImage.Length > 10 * 1024 * 1024)
                    {
                        ModelState.AddModelError("Input.LicenseImage", "License document must be smaller than 10MB.");
                    }
                    // File extension validation
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                    var ext = Path.GetExtension(Input.LicenseImage.FileName).ToLowerInvariant();
                    if (!allowedExtensions.Contains(ext))
                    {
                        ModelState.AddModelError("Input.LicenseImage", "Only JPG, PNG, and PDF files are allowed.");
                    }
                    // MIME type validation
                    var allowedMimeTypes = new[] { "image/jpeg", "image/png", "application/pdf" };
                    if (!allowedMimeTypes.Contains(Input.LicenseImage.ContentType))
                    {
                        ModelState.AddModelError("Input.LicenseImage", "Invalid file format.");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                var user = CreateUser();
                user.Role = Input.Role;
                user.IsActive = true;
                user.CreatedAt = DateTime.UtcNow;
                user.EmailConfirmed = true;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                using var transaction = await _context.Database.BeginTransactionAsync();
                string savedFilePath = null;
                try
                {
                    var result = await _userManager.CreateAsync(user, Input.Password);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User created a new account with password.");

                        // Assign role
                        await _userManager.AddToRoleAsync(user, Input.Role);

                        // Create patient or doctor profile
                        if (Input.Role == "Patient")
                        {
                            var patient = new Patient
                            {
                                PatientId = user.Id,
                                FullName = Input.FullName,
                                DateOfBirth = Input.DateOfBirth ?? DateTime.Today.AddYears(-20),
                                Gender = Input.InputGender ?? "Other",
                                PhoneNumber = Input.PhoneNumber ?? "",
                                MedicalHistoryBaseline = ""
                            };
                            _context.Patients.Add(patient);
                        }
                        else if (Input.Role == "Doctor")
                        {
                            var doctor = new Doctor
                            {
                                DoctorId = user.Id,
                                FullName = Input.FullName,
                                Specialization = Input.Specialization,
                                ConsultationFee = Input.ConsultationFee.Value,
                                HospitalName = Input.HospitalName,
                                DepartmentId = Input.DepartmentId.Value,
                                IsVerified = false
                            };
                            _context.Doctors.Add(doctor);

                            // Save file
                            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "licenses");
                            if (!Directory.Exists(uploadsFolder))
                            {
                                Directory.CreateDirectory(uploadsFolder);
                            }

                            var ext = Path.GetExtension(Input.LicenseImage.FileName).ToLowerInvariant();
                            var uniqueFileName = $"license_{user.Id}{ext}";
                            savedFilePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var stream = new FileStream(savedFilePath, FileMode.Create))
                            {
                                await Input.LicenseImage.CopyToAsync(stream);
                            }

                            var fileUrl = $"/uploads/licenses/{uniqueFileName}";
                            var document = new Document
                            {
                                UserId = user.Id,
                                DocumentType = "MedicalLicense",
                                S3ObjectKey = $"licenses/{uniqueFileName}",
                                FileUrl = fileUrl,
                                UploadedAt = DateTime.UtcNow
                            };
                            _context.Documents.Add(document);
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        if (Input.Role == "Doctor")
                        {
                            TempData["StatusMessage"] = "Registration submitted! Admin approval is required before you can log in.";
                            return RedirectToPage("Login");
                        }

                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                        }
                        else
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    if (savedFilePath != null && System.IO.File.Exists(savedFilePath))
                    {
                        System.IO.File.Delete(savedFilePath);
                    }
                    _logger.LogError(ex, "Error occurred during doctor registration.");
                    ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                }
            }

            // If we got this far, something failed, redisplay form
            await LoadDepartmentsAsync();
            return Page();
        }

        private medicalappUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<medicalappUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(medicalappUser)}'. " +
                    $"Ensure that '{nameof(medicalappUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<medicalappUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<medicalappUser>)_userStore;
        }
    }
}
