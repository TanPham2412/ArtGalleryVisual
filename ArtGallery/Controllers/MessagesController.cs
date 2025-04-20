using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Models;
using System.Security.Claims;

namespace ArtGallery.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ArtGalleryContext _context;

        public MessagesController(ArtGalleryContext context)
        {
            _context = context;
        }

        // Hiển thị danh sách cuộc trò chuyện
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy danh sách các cuộc trò chuyện
            var conversations = await GetUserConversationsAsync(currentUserId);

            return View(conversations);
        }

        // Hiển thị cuộc trò chuyện cụ thể
        public async Task<IActionResult> Conversation(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy thông tin người dùng đang trò chuyện
            var otherUser = await _context.Users.FindAsync(userId);
            if (otherUser == null)
            {
                return NotFound();
            }

            // Lấy lịch sử tin nhắn giữa hai người
            var messages = await _context.TinNhans
                .Where(m => (m.MaNguoiGui == currentUserId && m.MaNguoiNhan == userId) ||
                           (m.MaNguoiGui == userId && m.MaNguoiNhan == currentUserId))
                .OrderBy(m => m.ThoiGian)
                .ToListAsync();

            // Đánh dấu tin nhắn là đã đọc
            var unreadMessages = messages
                .Where(m => m.MaNguoiNhan == currentUserId && !m.DaDoc)
                .ToList();

            foreach (var message in unreadMessages)
            {
                message.DaDoc = true;
            }

            await _context.SaveChangesAsync();

            ViewData["OtherUser"] = otherUser;
            return View(messages);
        }

        // Gửi tin nhắn mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string receiverId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Nội dung tin nhắn không được để trống");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var message = new TinNhan
            {
                MaNguoiGui = currentUserId,
                MaNguoiNhan = receiverId,
                NoiDung = content,
                ThoiGian = DateTime.Now,
                DaDoc = false
            };

            _context.TinNhans.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Conversation), new { userId = receiverId });
        }

        // API lấy tin nhắn mới
        [HttpGet]
        public async Task<IActionResult> GetNewMessages()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var newMessages = await _context.TinNhans
                .Where(m => m.MaNguoiNhan == currentUserId && !m.DaDoc)
                .Include(m => m.NguoiGui)
                .OrderByDescending(m => m.ThoiGian)
                .Select(m => new {
                    Id = m.MaTinNhan,
                    SenderId = m.MaNguoiGui,
                    SenderName = m.NguoiGui.TenNguoiDung,
                    SenderAvatar = m.NguoiGui.AnhDaiDien,
                    Content = m.NoiDung,
                    Time = m.ThoiGian
                })
                .ToListAsync();

            return Json(new { success = true, messages = newMessages });
        }

        // API lấy danh sách cuộc trò chuyện
        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var conversations = await GetUserConversationsAsync(currentUserId);

            return Json(new { success = true, conversations });
        }

        // Lấy danh sách cuộc trò chuyện của người dùng
        private async Task<List<object>> GetUserConversationsAsync(string userId)
        {
            // Lấy danh sách người dùng đã trò chuyện
            var sentMessages = _context.TinNhans
                .Where(m => m.MaNguoiGui == userId)
                .Select(m => m.MaNguoiNhan);

            var receivedMessages = _context.TinNhans
                .Where(m => m.MaNguoiNhan == userId)
                .Select(m => m.MaNguoiGui);

            var conversationUserIds = await sentMessages
                .Union(receivedMessages)
                .Distinct()
                .ToListAsync();

            var result = new List<object>();

            foreach (var otherUserId in conversationUserIds)
            {
                var otherUser = await _context.Users.FindAsync(otherUserId);

                // Lấy tin nhắn mới nhất
                var lastMessage = await _context.TinNhans
                    .Where(m => (m.MaNguoiGui == userId && m.MaNguoiNhan == otherUserId) ||
                               (m.MaNguoiGui == otherUserId && m.MaNguoiNhan == userId))
                    .OrderByDescending(m => m.ThoiGian)
                    .FirstOrDefaultAsync();

                // Đếm số tin nhắn chưa đọc
                var unreadCount = await _context.TinNhans
                    .CountAsync(m => m.MaNguoiGui == otherUserId &&
                                   m.MaNguoiNhan == userId &&
                                   !m.DaDoc);

                result.Add(new
                {
                    UserId = otherUserId,
                    UserName = otherUser.TenNguoiDung,
                    Avatar = otherUser.AnhDaiDien ?? "/images/authors/default/default-image.png",
                    LastMessage = lastMessage?.NoiDung,
                    LastMessageTime = lastMessage?.ThoiGian,
                    IsRead = lastMessage?.DaDoc ?? true,
                    UnreadCount = unreadCount,
                    IsOnline = false // Cần thêm logic xác định trạng thái online
                });
            }

            // Sắp xếp theo thời gian tin nhắn mới nhất
            return result.OrderByDescending(c => ((dynamic)c).LastMessageTime).ToList();
        }
    }
}