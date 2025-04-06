namespace ArtGallery.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<bool> CreateNotification(string receiverId, string senderId, string title, string content, string url, string notificationType, string imageUrl = null);
        Task<bool> CreateSystemNotification(string receiverId, string title, string content, string url, string notificationType, string imageUrl = null);
        Task<int> GetUnreadNotificationCount(string userId);
    }
}
