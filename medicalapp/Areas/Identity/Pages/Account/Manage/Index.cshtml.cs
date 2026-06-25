// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using medicalapp.Areas.Identity.Data;
using medicalapp.Data;
using medicalapp.Models;

namespace medicalapp.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<medicalappUser> _userManager;
        private readonly SignInManager<medicalappUser> _signInManager;
        private readonly medicalappContext _context;

        public IndexModel(
            UserManager<medicalappUser> userManager,
            SignInManager<medicalappUser> signInManager,
            medicalappContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Required(ErrorMessage = "Full Name is required.")]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            // Patient fields
            [Display(Name = "Date of Birth")]
            [DataType(DataType.Date)]
            public DateTime? DateOfBirth { get; set; }

            [Display(Name = "Gender")]
            public string Gender { get; set; }

            [Display(Name = "Medical History Baseline")]
            [DataType(DataType.MultilineText)]
            public string MedicalHistoryBaseline { get; set; }

            // Doctor fields
            [Display(Name = "Specialization")]
            public string Specialization { get; set; }

            [Display(Name = "Consultation Fee")]
            public decimal? ConsultationFee { get; set; }

            [Display(Name = "Hospital Name")]
            public string HospitalName { get; set; }
        }

        private async Task LoadAsync(medicalappUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            var input = new InputModel
            {
                PhoneNumber = phoneNumber
            };

            if (user.Role == "Patient")
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == user.Id);
                if (patient != null)
                {
                    input.FullName = patient.FullName;
                    input.DateOfBirth = patient.DateOfBirth;
                    input.Gender = patient.Gender;
                    input.MedicalHistoryBaseline = patient.MedicalHistoryBaseline;
                }
            }
            else if (user.Role == "Doctor")
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == user.Id);
                if (doctor != null)
                {
                    input.FullName = doctor.FullName;
                    input.Specialization = doctor.Specialization;
                    input.ConsultationFee = doctor.ConsultationFee;
                    input.HospitalName = doctor.HospitalName;
                }
            }
            else
            {
                input.FullName = "Administrator";
            }

            Input = input;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            if (user.Role == "Patient")
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == user.Id);
                if (patient == null)
                {
                    patient = new Patient { PatientId = user.Id };
                    _context.Patients.Add(patient);
                }
                patient.FullName = Input.FullName;
                patient.DateOfBirth = Input.DateOfBirth ?? DateTime.Today.AddYears(-20);
                patient.Gender = Input.Gender ?? "Other";
                patient.PhoneNumber = Input.PhoneNumber ?? "";
                patient.MedicalHistoryBaseline = Input.MedicalHistoryBaseline;
            }
            else if (user.Role == "Doctor")
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == user.Id);
                if (doctor == null)
                {
                    var dept = await _context.MedicalDepartments.FirstOrDefaultAsync();
                    if (dept == null)
                    {
                        dept = new MedicalDepartment { DepartmentName = "General Medicine" };
                        _context.MedicalDepartments.Add(dept);
                        await _context.SaveChangesAsync();
                    }

                    doctor = new Doctor { DoctorId = user.Id, DepartmentId = dept.DepartmentId };
                    _context.Doctors.Add(doctor);
                }
                doctor.FullName = Input.FullName;
                doctor.Specialization = Input.Specialization ?? "";
                doctor.ConsultationFee = Input.ConsultationFee ?? 0;
                doctor.HospitalName = Input.HospitalName ?? "";
            }

            await _context.SaveChangesAsync();

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
