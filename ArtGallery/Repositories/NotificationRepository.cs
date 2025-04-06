using ArtGallery.Models;
using ArtGallery.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ArtGallery.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ArtGalleryContext _context;

        public NotificationRepository(ArtGalleryContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateNotification(string receiverId, string senderId, string title, string content, string url, string notificationType, string imageUrl = null)
        {
            try
            {
                var notification = new ThongBao
                {
                    MaNguoiNhan = receiverId,
                    MaNguoiGui = senderId,
                    TieuDe = title,
                    NoiDung = content,
                    URL = url,
                    LoaiThongBao = notificationType,
                    DuongDanAnh = imageUrl,
                    DaDoc = false,
                    ThoiGian = DateTime.Now
                };

                _context.ThongBaos.Add(notification);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreateSystemNotification(string receiverId, string title, string content, string url, string notificationType, string imageUrl = null)
        {
            return await CreateNotification(receiverId, null, title, content, url, notificationType, imageUrl);
        }

        public async Task<int> GetUnreadNotificationCount(string userId)
        {
            return await _context.ThongBaos
                .CountAsync(t => t.MaNguoiNhan == userId && t.DaDoc != true);
        }
    }
}
