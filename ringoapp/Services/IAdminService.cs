using System.Collections.Generic;
using System.Threading.Tasks;
using medicalapp.Models;

namespace medicalapp.Services
{
    public interface IAdminService
    {
        Task<List<Doctor>> GetPendingDoctorsAsync();
        Task VerifyDoctorAsync(string doctorId, bool verify);
        Task<List<MedicalDepartment>> GetDepartmentsAsync();
        Task AddDepartmentAsync(string departmentName);
    }
}
