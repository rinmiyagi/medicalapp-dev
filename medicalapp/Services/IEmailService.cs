using System.Threading.Tasks;

namespace medicalapp.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string message);
    }
}
