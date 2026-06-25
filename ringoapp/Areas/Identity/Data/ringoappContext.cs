using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using medicalapp.Areas.Identity.Data;
using medicalapp.Models;

namespace medicalapp.Data;

public class medicalappContext : IdentityDbContext<medicalappUser>
{
    public medicalappContext(DbContextOptions<medicalappContext> options)
        : base(options)
    {
    }

    // ── MedicalApp Tables ──────────────────────────
    public DbSet<Patient>          Patients          { get; set; } = null!;
    public DbSet<Doctor>           Doctors           { get; set; } = null!;
    public DbSet<DoctorSchedule>   DoctorSchedules   { get; set; } = null!;
    public DbSet<MedicalDepartment> MedicalDepartments { get; set; } = null!;
    public DbSet<Appointment>      Appointments      { get; set; } = null!;
    public DbSet<Document>         Documents         { get; set; } = null!;
    public DbSet<Prescription>     Prescriptions     { get; set; } = null!;
    public DbSet<Notification>     Notifications     { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── Unique Constraints ──────────────────────────────
        builder.Entity<MedicalDepartment>()
            .HasIndex(md => md.DepartmentName)
            .IsUnique();

        // ── 1-to-1: medicalappUser ↔ Patient ─────────────
        builder.Entity<Patient>()
            .HasOne(p => p.User)
            .WithOne(u => u.Patient)
            .HasForeignKey<Patient>(p => p.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── 1-to-1: medicalappUser ↔ Doctor ─────────────
        builder.Entity<Doctor>()
            .HasOne(d => d.User)
            .WithOne(u => u.Doctor)
            .HasForeignKey<Doctor>(d => d.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Appointment: Restrict on Patient / Doctor side ──
        builder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Prescription ────────────────────────────
        builder.Entity<Prescription>()
            .HasOne(pr => pr.Appointment)
            .WithOne(a => a.Prescription)
            .HasForeignKey<Prescription>(pr => pr.AppointmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Prescription>()
            .HasOne(pr => pr.Patient)
            .WithMany(p => p.Prescriptions)
            .HasForeignKey(pr => pr.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Prescription>()
            .HasOne(pr => pr.Doctor)
            .WithMany(d => d.Prescriptions)
            .HasForeignKey(pr => pr.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── Document ──────────────────────────────
        builder.Entity<Document>()
            .HasOne(d => d.User)
            .WithMany(u => u.Documents)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ── DoctorSchedule ──────────────────────────
        builder.Entity<DoctorSchedule>()
            .HasOne(ds => ds.Doctor)
            .WithMany(d => d.Schedules)
            .HasForeignKey(ds => ds.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Notification ──────────────────────────
        builder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
