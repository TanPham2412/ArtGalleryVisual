using Microsoft.AspNetCore.SignalR;
using ArtGallery.Models;
using System.Threading.Tasks;

namespace ArtGallery.Hubs
{
    public class CommentHub : Hub
    {
        /// <summary>
        /// Gửi bình luận mới đến tất cả client đang xem cùng một tác phẩm
        /// </summary>
        /// <param name="artworkId">ID của tác phẩm</param>
        /// <param name="comment">Đối tượng bình luận</param>
        public async Task SendComment(int artworkId, object comment)
        {
            await Clients.Group($"artwork_{artworkId}").SendAsync("ReceiveComment", comment);
        }

        /// <summary>
        /// Gửi phản hồi mới đến tất cả client đang xem cùng một tác phẩm
        /// </summary>
        /// <param name="artworkId">ID của tác phẩm</param>
        /// <param name="commentId">ID của bình luận gốc</param>
        /// <param name="reply">Đối tượng phản hồi</param>
        public async Task SendReply(int artworkId, int commentId, object reply)
        {
            await Clients.Group($"artwork_{artworkId}").SendAsync("ReceiveReply", commentId, reply);
        }

        /// <summary>
        /// Thông báo khi một bình luận bị xóa
        /// </summary>
        /// <param name="artworkId">ID của tác phẩm</param>
        /// <param name="commentId">ID của bình luận bị xóa</param>
        public async Task NotifyCommentDeleted(int artworkId, int commentId)
        {
            await Clients.Group($"artwork_{artworkId}").SendAsync("CommentDeleted", commentId);
        }

        /// <summary>
        /// Thông báo khi một phản hồi bị xóa
        /// </summary>
        /// <param name="artworkId">ID của tác phẩm</param>
        /// <param name="replyId">ID của phản hồi bị xóa</param>
        public async Task NotifyReplyDeleted(int artworkId, int replyId)
        {
            await Clients.Group($"artwork_{artworkId}").SendAsync("ReplyDeleted", replyId);
        }

        /// <summary>
        /// Thông báo khi một bình luận được chỉnh sửa
        /// </summary>
        /// <param name="artworkId">ID của tác phẩm</param>
        /// <param name="commentId">ID của bình luận</param>
        /// <param name="updatedComment">Nội dung bình luận đã cập nhật</param>
        public async Task NotifyCommentEdited(int artworkId, int commentId, object updatedComment)
        {
            await Clients.Group($"artwork_{artworkId}").SendAsync("CommentEdited", commentId, updatedComment);
        }

        /// <summary>
        /// Thông báo khi một phản hồi được chỉnh sửa
        /// </summary>
        /// <param name="artworkId">ID của tác phẩm</param>
        /// <param name="replyId">ID của phản hồi</param>
        /// <param name="updatedReply">Nội dung phản hồi đã cập nhật</param>
        public async Task NotifyReplyEdited(int artworkId, int replyId, object updatedReply)
        {
            await Clients.Group($"artwork_{artworkId}").SendAsync("ReplyEdited", replyId, updatedReply);
        }

        /// <summary>
        /// Thông báo khi trạng thái ẩn/hiện của bình luận thay đổi
        /// </summary>
        /// <param name="artworkId">ID của tác phẩm</param>
        /// <param name="commentId">ID của bình luận</param>
        /// <param name="isHidden">Trạng thái ẩn/hiện mới</param>
        public async Task NotifyCommentVisibilityChanged(int artworkId, int commentId, bool isHidden)
        {
            await Clients.Group($"artwork_{artworkId}").SendAsync("CommentVisibilityChanged", commentId, isHidden);
        }

        /// <summary>
        /// Tham gia vào nhóm của một tác phẩm để nhận thông báo về bình luận
        /// </summary>
        /// <param name="artworkId">ID của tác phẩm</param>
        public async Task JoinArtworkGroup(int artworkId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"artwork_{artworkId}");
        }

        /// <summary>
        /// Rời khỏi nhóm của một tác phẩm
        /// </summary>
        /// <param name="artworkId">ID của tác phẩm</param>
        public async Task LeaveArtworkGroup(int artworkId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"artwork_{artworkId}");
        }
    }
}