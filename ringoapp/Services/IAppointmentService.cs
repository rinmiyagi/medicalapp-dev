using System.Collections.Generic;
using System.Threading.Tasks;
using medicalapp.DTOs;
using medicalapp.Models;

namespace medicalapp.Services
{
    public interface IAppointmentService
    {
        Task<List<Appointment>> GetPatientAppointmentsAsync(string patientId);
        Task<List<Appointment>> GetDoctorAppointmentsAsync(string doctorId);
        Task<Appointment> BookAppointmentAsync(AppointmentBookDto dto, string patientId);
        Task CancelAppointmentAsync(int appointmentId, string userId, string role);
        Task CompleteAppointmentAsync(int appointmentId, string doctorId, string medications, string notes);
        Task ConfirmAppointmentAsync(int appointmentId, string doctorId);
        Task RejectAppointmentAsync(int appointmentId, string doctorId);
        Task MarkAsMissedAsync(int appointmentId, string doctorId);
        Task<Appointment?> GetAppointmentDetailsAsync(int appointmentId);
    }
}
