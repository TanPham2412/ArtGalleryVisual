using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ArtGallery.Models;
using System.Security.Claims;

namespace ArtGallery.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ArtGalleryContext _context;
        private readonly UserManager<NguoiDung> _userManager;

        public MessagesController(ArtGalleryContext context, UserManager<NguoiDung> userManager)
        {
            _context = context;
            _userManager = userManager;
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
            var otherUser = await _userManager.FindByIdAsync(userId);
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
        public async Task<IActionResult> SendMessage(string receiverId, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequest("Nội dung tin nhắn không được để trống");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var tinNhan = new TinNhan
            {
                MaNguoiGui = currentUserId,
                MaNguoiNhan = receiverId,
                NoiDung = message,
                ThoiGian = DateTime.Now,
                DaDoc = false
            };

            _context.TinNhans.Add(tinNhan);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = tinNhan });
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

        // API lấy tin nhắn gần đây
        [HttpGet]
        public async Task<IActionResult> GetRecentMessages()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Lấy tin nhắn mới nhất từ các người gửi
            var latestMessages = await _context.TinNhans
                .Where(m => m.MaNguoiNhan == currentUserId)
                .GroupBy(m => m.MaNguoiGui)
                .Select(g => g.OrderByDescending(m => m.ThoiGian).FirstOrDefault())
                .Take(5)
                .ToListAsync();

            var messages = new List<object>();
            foreach (var message in latestMessages)
            {
                var sender = await _userManager.FindByIdAsync(message.MaNguoiGui);
                if (sender != null)
                {
                    messages.Add(new
                    {
                        maTinNhan = message.MaTinNhan,
                        maNguoiGui = message.MaNguoiGui,
                        tenNguoiGui = sender.TenNguoiDung,
                        avatarNguoiGui = sender.AnhDaiDien ?? "/images/authors/default/default-image.png",
                        noiDung = message.NoiDung,
                        thoiGian = message.ThoiGian,
                        daDoc = message.DaDoc
                    });
                }
            }

            // Đếm tổng số tin nhắn chưa đọc
            var unreadCount = await _context.TinNhans
                .CountAsync(m => m.MaNguoiNhan == currentUserId && !m.DaDoc);

            return Json(new { messages, unreadCount });
        }

        // API lấy số lượng tin nhắn chưa đọc
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var count = await _context.TinNhans
                .CountAsync(m => m.MaNguoiNhan == currentUserId && !m.DaDoc);

            return Json(new { count });
        }

        // API lấy tin nhắn trong cuộc trò chuyện
        [HttpGet]
        public async Task<IActionResult> GetConversation(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var messages = await _context.TinNhans
                .Where(m => (m.MaNguoiGui == currentUserId && m.MaNguoiNhan == userId) ||
                          (m.MaNguoiGui == userId && m.MaNguoiNhan == currentUserId))
                .OrderBy(m => m.ThoiGian)
                .Select(m => new {
                    maTinNhan = m.MaTinNhan,
                    maNguoiGui = m.MaNguoiGui,
                    maNguoiNhan = m.MaNguoiNhan,
                    noiDung = m.NoiDung,
                    thoiGian = m.ThoiGian,
                    daDoc = m.DaDoc
                })
                .ToListAsync();

            // Đánh dấu tin nhắn là đã đọc
            var unreadMessages = await _context.TinNhans
                .Where(m => m.MaNguoiGui == userId && m.MaNguoiNhan == currentUserId && !m.DaDoc)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.DaDoc = true;
            }
            await _context.SaveChangesAsync();

            return Json(messages);
        }

        // API lấy ID người dùng hiện tại
        [HttpGet]
        public IActionResult GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Json(new { userId });
        }

        // API tìm kiếm cuộc trò chuyện
        [HttpGet]
        public async Task<IActionResult> SearchConversations(string query)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var users = await _userManager.Users
                .Where(u => u.Id != currentUserId &&
                           (u.TenNguoiDung.Contains(query) || u.UserName.Contains(query)))
                .ToListAsync();

            var conversations = new List<object>();

            foreach (var user in users)
            {
                // Tìm tin nhắn gần nhất giữa 2 người
                var lastMessage = await _context.TinNhans
                    .Where(t => (t.MaNguoiGui == currentUserId && t.MaNguoiNhan == user.Id) ||
                               (t.MaNguoiGui == user.Id && t.MaNguoiNhan == currentUserId))
                    .OrderByDescending(t => t.ThoiGian)
                    .FirstOrDefaultAsync();

                // Đếm tin nhắn chưa đọc
                var unreadCount = await _context.TinNhans
                    .CountAsync(t => t.MaNguoiGui == user.Id &&
                                   t.MaNguoiNhan == currentUserId &&
                                   !t.DaDoc);

                conversations.Add(new
                {
                    userId = user.Id,
                    userName = user.TenNguoiDung,
                    avatar = user.GetAvatarPath(),
                    lastMessage = lastMessage?.NoiDung,
                    lastMessageTime = lastMessage?.ThoiGian,
                    unreadCount,
                    isOnline = false // Cần bổ sung logic xác định trạng thái online
                });
            }

            return Json(new { conversations });
        }

        // API lấy thông tin người dùng
        [HttpGet]
        public async Task<IActionResult> GetUserInfo(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Json(new
            {
                userId = user.Id,
                userName = user.TenNguoiDung,
                avatar = user.GetAvatarPath(),
                isOnline = false, // Cần bổ sung logic xác định trạng thái online
                lastActive = DateTime.Now // Giả sử
            });
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
                var otherUser = await _userManager.FindByIdAsync(otherUserId);
                if (otherUser == null) continue;

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
                    userId = otherUserId,
                    userName = otherUser.TenNguoiDung,
                    avatar = otherUser.GetAvatarPath(),
                    lastMessage = lastMessage?.NoiDung,
                    lastMessageTime = lastMessage?.ThoiGian,
                    isRead = lastMessage?.DaDoc ?? true,
                    unreadCount = unreadCount,
                    isOnline = false // Cần thêm logic xác định trạng thái online
                });
            }

            // Sắp xếp theo thời gian tin nhắn mới nhất
            return result.OrderByDescending(c => ((dynamic)c).lastMessageTime).ToList();
        }
    }
}