using System.Collections.Generic;
using System.Threading.Tasks;
using medicalapp.Models;

namespace medicalapp.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, string message, string? targetUrl = null);
        Task<List<Notification>> GetNotificationsAsync(string userId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
    }
}
