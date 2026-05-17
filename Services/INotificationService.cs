using FoodDeliveryApp.Models;

namespace FoodDeliveryApp.Services
{
    public interface INotificationService
    {
        Task CreateAsync(
            string userId,
            string title,
            string message,
            string notificationType,
            int? orderId = null);

        Task CreateForAdminsAsync(
            string title,
            string message,
            string notificationType,
            int? orderId = null);

        Task<int> GetUnreadCountAsync(string userId);

        Task<List<Notification>> GetUserNotificationsAsync(string userId);

        Task<bool> MarkAsReadAsync(int notificationId, string userId);

        Task MarkAllAsReadAsync(string userId);
    }
}