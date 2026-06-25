using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace medicalapp.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string toEmail, string subject, string message)
        {
            // Log to console for local development
            _logger.LogInformation($"[EMAIL SENT] To: {toEmail} | Subject: {subject} | Body: {message}");
            return Task.CompletedTask;
        }
    }
}
