using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using medicalapp.Areas.Identity.Data;
using medicalapp.Models;

namespace medicalapp.Data
{
    /// <summary>
    /// Seeds initial database roles, departments, and admin user.
    /// </summary>
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<medicalappContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<medicalappUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Seed Roles
            string[] roleNames = { "Patient", "Doctor", "Admin" };
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed Departments
            if (!context.MedicalDepartments.Any())
            {
                var departments = new MedicalDepartment[]
                {
                    new MedicalDepartment { DepartmentName = "General Medicine" },
                    new MedicalDepartment { DepartmentName = "Cardiology" },
                    new MedicalDepartment { DepartmentName = "Pediatrics" },
                    new MedicalDepartment { DepartmentName = "Dermatology" },
                    new MedicalDepartment { DepartmentName = "Neurology" }
                };
                context.MedicalDepartments.AddRange(departments);
                await context.SaveChangesAsync();
            }

            // Seed Admin User
            var adminEmail = "admin@medicloud.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new medicalappUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };
                var createAdmin = await userManager.CreateAsync(adminUser, "Admin123!");
                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed a Test Doctor (Verified)
            var doctorEmail = "doctor@medicloud.com";
            var doctorUser = await userManager.FindByEmailAsync(doctorEmail);
            if (doctorUser == null)
            {
                doctorUser = new medicalappUser
                {
                    UserName = doctorEmail,
                    Email = doctorEmail,
                    Role = "Doctor",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };
                var createDoctor = await userManager.CreateAsync(doctorUser, "Doctor123!");
                if (createDoctor.Succeeded)
                {
                    await userManager.AddToRoleAsync(doctorUser, "Doctor");

                    var cardiologyDept = context.MedicalDepartments.FirstOrDefault(d => d.DepartmentName == "Cardiology") 
                                         ?? context.MedicalDepartments.First();

                    var doctorProfile = new Doctor
                    {
                        DoctorId = doctorUser.Id,
                        FullName = "John Doe",
                        Specialization = "Cardiologist",
                        ConsultationFee = 150.00m,
                        HospitalName = "MediCloud Heart Center",
                        DepartmentId = cardiologyDept.DepartmentId,
                        IsVerified = true
                    };
                    context.Doctors.Add(doctorProfile);
                    await context.SaveChangesAsync();
                }
            }

            // Seed a Test Patient
            var patientEmail = "patient@medicloud.com";
            var patientUser = await userManager.FindByEmailAsync(patientEmail);
            if (patientUser == null)
            {
                patientUser = new medicalappUser
                {
                    UserName = patientEmail,
                    Email = patientEmail,
                    Role = "Patient",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };
                var createPatient = await userManager.CreateAsync(patientUser, "Patient123!");
                if (createPatient.Succeeded)
                {
                    await userManager.AddToRoleAsync(patientUser, "Patient");

                    var patientProfile = new Patient
                    {
                        PatientId = patientUser.Id,
                        FullName = "Jane Smith",
                        DateOfBirth = new DateTime(1995, 5, 15),
                        Gender = "Female",
                        PhoneNumber = "123-456-7890",
                        MedicalHistoryBaseline = "Asthma diagnosed in childhood."
                    };
                    context.Patients.Add(patientProfile);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
