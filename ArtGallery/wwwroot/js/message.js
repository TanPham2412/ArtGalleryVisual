$(document).ready(function() {
    console.log("Document ready");
    
    // Tab chuyển đổi giữa hộp thư và cộng đồng
    $('.tab-button').on('click', function() {
        $('.tab-button').removeClass('active');
        $(this).addClass('active');
    });
    
    // Load danh sách cuộc trò chuyện
    loadConversations();
    
    // Xử lý tìm kiếm
    $('#searchConversation').on('input', function() {
        const query = $(this).val().trim();
        if (query.length > 0) {
            searchConversations(query);
        } else {
            loadConversations();
        }
    });
    
    // Xử lý mở form tin nhắn mới - ĐẶT TRONG document.ready
    $('#newMessageBtn').on('click', function() {
        console.log("Nút bút chì được nhấn");
        $('#newMessageModal').css('display', 'flex');
        $('#recipientInput').focus();
    });
    
    // Kiểm tra phần tử có tồn tại không
    console.log("Nút bút chì có tồn tại:", $('#newMessageBtn').length > 0);
    console.log("Modal có tồn tại:", $('#newMessageModal').length > 0);
    
    // Đóng form tin nhắn mới - ĐẶT TRONG document.ready
    $('#closeNewMessageBtn').on('click', function() {
        $('#newMessageModal').hide();
        $('#recipientInput').val('');
        $('#recipientResults').empty();
    });
    
    // Ẩn form khi click ra ngoài - ĐẶT TRONG document.ready
    $(document).on('click', function(e) {
        if ($(e.target).hasClass('new-message-modal')) {
            $('#newMessageModal').hide();
            $('#recipientInput').val('');
            $('#recipientResults').empty();
        }
    });
    
    // Xử lý tìm kiếm người dùng khi nhập vào ô Đến - ĐẶT TRONG document.ready
    $('#recipientInput').on('input', function() {
        const query = $(this).val().trim();
        if (query.length > 0) {
            searchUsers(query);
        } else {
            $('#recipientResults').empty();
        }
    });

    // Lấy ID người dùng hiện tại
    $.ajax({
        url: '/Messages/GetCurrentUserId',
        type: 'GET',
        success: function(data) {
            currentUserId = data.userId;
        }
    });

    // Thêm sự kiện click cho các cuộc trò chuyện
    $(document).on('click', '.conversation-item', function() {
        const userId = $(this).data('user-id');
        openConversation(userId);
    });
});

// Load danh sách cuộc trò chuyện
function loadConversations() {
    $.ajax({
        url: '/Messages/GetConversations',
        type: 'GET',
        success: function(data) {
            renderConversations(data.conversations);
        },
        error: function() {
            $('#conversationList').html('<div class="text-center p-3">Không thể tải cuộc trò chuyện</div>');
        }
    });
}

// Render danh sách cuộc trò chuyện
function renderConversations(conversations) {
    if (!conversations || conversations.length === 0) {
        $('#conversationList').html('<div class="text-center p-3">Chưa có cuộc trò chuyện nào</div>');
        return;
    }
    
    let html = '';
    for (const conv of conversations) {
        html += `
            <div class="conversation-item" data-user-id="${conv.userId}">
                <div class="conversation-avatar">
                    <img src="${conv.avatar}" alt="" onerror="this.src='/images/authors/default/default-image.png'">
                    <span class="status-indicator ${conv.isOnline ? 'online' : ''}"></span>
                </div>
                <div class="conversation-info">
                    <div class="conversation-header">
                        <span class="conversation-name">${conv.userName}</span>
                        <span class="conversation-time">${formatTime(conv.lastMessageTime)}</span>
                    </div>
                    <div class="conversation-preview">
                        <span class="conversation-last-message">${conv.lastMessage || 'Chưa có tin nhắn'}</span>
                        ${conv.unreadCount > 0 ? `<span class="unread-badge">${conv.unreadCount}</span>` : ''}
                    </div>
                </div>
            </div>
        `;
    }
    
    $('#conversationList').html(html);
}

// Mở cuộc trò chuyện
function openConversation(userId) {
    // Lấy thông tin người dùng
    $.ajax({
        url: '/Messages/GetUserInfo',
        type: 'GET',
        data: { id: userId },
        success: function(userData) {
            // Lấy tin nhắn trong cuộc trò chuyện
            $.ajax({
                url: '/Messages/GetConversation',
                type: 'GET',
                data: { userId: userId },
                success: function(messages) {
                    renderConversation(userData, messages);
                    // Đánh dấu cuộc trò chuyện hiện tại
                    $('.conversation-item').removeClass('active');
                    $(`.conversation-item[data-user-id="${userId}"]`).addClass('active');
                },
                error: function() {
                    $('#messageContent').html('<div class="error-message">Không thể tải tin nhắn</div>');
                }
            });
        },
        error: function() {
            $('#messageContent').html('<div class="error-message">Không thể tải thông tin người dùng</div>');
        }
    });
}

// Hiển thị cuộc trò chuyện và tin nhắn
function renderConversation(user, messages) {
    // Xác định ID người dùng hiện tại
    let currentUserId = '';
    $.ajax({
        url: '/Messages/GetCurrentUserId',
        type: 'GET',
        async: false,
        success: function(data) {
            currentUserId = data.userId;
        }
    });
    
    let html = `
        <div class="chat-header">
            <div class="chat-user-info">
                <div class="chat-avatar">
                    <img src="${user.avatar}" alt="" onerror="this.src='/images/authors/default/default-image.png'">
                    <span class="status-indicator ${user.isOnline ? 'online' : ''}"></span>
                </div>
                <div class="chat-user">
                    <div class="chat-username">${user.userName}</div>
                    <div class="chat-status">${user.isOnline ? 'Đang hoạt động' : 'Hoạt động vừa xong'}</div>
                </div>
            </div>
            <div class="chat-actions">
                <button type="button" class="btn-icon">
                    <i class="fas fa-info-circle"></i>
                </button>
            </div>
        </div>
        <div class="chat-messages" id="chatMessages">`;
    
    // Thêm các tin nhắn
    if (messages && messages.length > 0) {
        let lastDate = '';
        
        for (const message of messages) {
            // Kiểm tra nếu ngày thay đổi, hiển thị phân cách ngày
            const messageDate = new Date(message.thoiGian).toLocaleDateString('vi-VN');
            if (messageDate !== lastDate) {
                html += `<div class="message-date-separator">
                    <span>${messageDate}</span>
                </div>`;
                lastDate = messageDate;
            }
            
            // Xác định tin nhắn của mình hay của người khác
            const isMyMessage = message.maNguoiGui === currentUserId;
            
            html += `
                <div class="chat-message-item ${isMyMessage ? 'my-message' : 'other-message'}">
                    ${!isMyMessage ? `
                    <div class="message-avatar">
                        <img src="${user.avatar}" alt="" onerror="this.src='/images/authors/default/default-image.png'">
                    </div>` : ''}
                    <div class="message-content">
                        <div class="message-bubble">
                            <div class="message-text">${message.noiDung}</div>
                        </div>
                        <div class="message-time">${formatTime(message.thoiGian)}</div>
                    </div>
                </div>
            `;
        }
    } else {
        html += `<div class="empty-conversation">
            <div class="empty-icon">
                <i class="far fa-paper-plane"></i>
            </div>
            <div class="empty-text">Hãy bắt đầu cuộc trò chuyện</div>
        </div>`;
    }
    
    html += `</div>
        <div class="chat-input">
            <div class="input-actions">
                <button type="button" class="btn-icon">
                    <i class="far fa-image"></i>
                </button>
                <button type="button" class="btn-icon">
                    <i class="far fa-smile"></i>
                </button>
            </div>
            <div class="message-input-wrapper">
                <input type="text" id="messageInput" placeholder="Aa" autocomplete="off">
            </div>
            <button type="button" class="send-btn" id="sendMessageBtn" data-receiver-id="${user.userId}">
                <i class="fas fa-paper-plane"></i>
            </button>
        </div>
    `;
    
    $('#messageContent').html(html);
    
    // Cuộn xuống tin nhắn cuối cùng
    const chatMessages = document.getElementById('chatMessages');
    if (chatMessages) {
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }
    
    // Thêm sự kiện gửi tin nhắn
    setupSendMessage(user.userId);
}

// Thiết lập sự kiện gửi tin nhắn
function setupSendMessage(receiverId) {
    // Gửi tin nhắn khi nhấn nút gửi
    $('#sendMessageBtn').click(function() {
        sendMessage(receiverId);
    });
    
    // Gửi tin nhắn khi nhấn Enter
    $('#messageInput').keypress(function(e) {
        if (e.which === 13) {
            sendMessage(receiverId);
            return false;
        }
    });
}

// Hàm gửi tin nhắn
function sendMessage(receiverId) {
    const input = $('#messageInput');
    const message = input.val().trim();
    
    if (message.length === 0) return;
    
    // Xóa nội dung input
    input.val('');
    
    // Lấy token antiforgery
    const token = $('input[name="__RequestVerificationToken"]').val();
    
    // Gửi tin nhắn đến server
    $.ajax({
        url: '/Messages/SendMessage',
        type: 'POST',
        data: {
            receiverId: receiverId,
            message: message,
            __RequestVerificationToken: token
        },
        success: function(data) {
            if (data.success) {
                // Thêm tin nhắn vào danh sách tin nhắn
                appendMyMessage(data.message);
                // Cập nhật danh sách cuộc trò chuyện
                loadConversations();
            }
        },
        error: function() {
            alert('Gửi tin nhắn thất bại');
        }
    });
}

// Thêm tin nhắn của mình vào danh sách tin nhắn
function appendMyMessage(message) {
    const html = `
        <div class="chat-message-item my-message">
            <div class="message-content">
                <div class="message-bubble">
                    <div class="message-text">${message.noiDung}</div>
                </div>
                <div class="message-time">${formatTime(message.thoiGian)}</div>
            </div>
        </div>
    `;
    
    $('#chatMessages').append(html);
    
    // Cuộn xuống tin nhắn cuối cùng
    const chatMessages = document.getElementById('chatMessages');
    if (chatMessages) {
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }
}

// Định dạng thời gian
function formatTime(timeString) {
    if (!timeString) return '';
    
    const date = new Date(timeString);
    const now = new Date();
    const diffDays = Math.floor((now - date) / (1000 * 60 * 60 * 24));
    
    // Nếu là ngày hôm nay, hiển thị giờ:phút
    if (diffDays === 0) {
        const hours = date.getHours().toString().padStart(2, '0');
        const minutes = date.getMinutes().toString().padStart(2, '0');
        return `${hours}:${minutes}`;
    }
    
    // Nếu là ngày hôm qua, hiển thị "Hôm qua"
    if (diffDays === 1) {
        return 'Hôm qua';
    }
    
    // Nếu trong tuần này, hiển thị tên thứ
    if (diffDays < 7) {
        const days = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'];
        return days[date.getDay()];
    }
    
    // Nếu trước đó, hiển thị ngày/tháng
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    return `${day}/${month}`;
}

// Tìm kiếm cuộc trò chuyện
function searchConversations(query) {
    $.ajax({
        url: '/Messages/SearchConversations',
        type: 'GET',
        data: { query: query },
        success: function(data) {
            renderConversations(data.conversations || []);
        }
    });
}

// Định dạng thời gian hiển thị trong danh sách cuộc trò chuyện
function formatConversationTime(date) {
    const now = new Date();
    const diff = Math.floor((now - date) / 1000 / 60); // Số phút
    
    if (diff < 1) return 'Vừa xong';
    if (diff < 60) return `${diff} phút`;
    
    const diffHours = Math.floor(diff / 60);
    if (diffHours < 24) return `${diffHours} giờ`;
    
    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 7) return `${diffDays} ngày`;
    
    // Nếu hơn 1 tuần, hiển thị ngày tháng
    return `${date.getDate()}/${date.getMonth() + 1}`;
}

// Định dạng ngày
function formatMessageDate(date) {
    const now = new Date();
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const messageDay = new Date(date.getFullYear(), date.getMonth(), date.getDate());
    
    const diff = Math.floor((today - messageDay) / (1000 * 60 * 60 * 24));
    
    if (diff === 0) return 'Hôm nay';
    if (diff === 1) return 'Hôm qua';
    if (diff < 7) {
        const days = ['Chủ nhật', 'Thứ hai', 'Thứ ba', 'Thứ tư', 'Thứ năm', 'Thứ sáu', 'Thứ bảy'];
        return days[messageDay.getDay()];
    }
    
    return `${date.getDate()}/${date.getMonth() + 1}/${date.getFullYear()}`;
}

// Định dạng thời gian tin nhắn
function formatMessageTime(date) {
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${hours}:${minutes}`;
}

// Định dạng thời gian hoạt động cuối
function formatLastActive(date) {
    const now = new Date();
    const diff = Math.floor((now - date) / 1000 / 60); // Số phút
    
    if (diff < 1) return 'vừa xong';
    if (diff < 60) return `${diff} phút trước`;
    
    const diffHours = Math.floor(diff / 60);
    if (diffHours < 24) return `${diffHours} giờ trước`;
    
    return `${date.getDate()}/${date.getMonth() + 1}`;
}

// Cần thêm biến lưu ID người dùng hiện tại
let currentUserId = '';

// Biến toàn cục để lưu người nhận tin nhắn đã chọn
let selectedRecipients = [];

// Hàm tìm kiếm người dùng - điều chỉnh để tìm kiếm chính xác hơn
function searchUsers(query) {
    console.log("Tìm kiếm người dùng với từ khóa:", query);
    
    $.ajax({
        url: '/Messages/SearchUsers',
        type: 'GET',
        data: { query: query },
        success: function(data) {
            console.log("Kết quả tìm kiếm:", data);
            renderUserResults(data.users || []);
        },
        error: function(err) {
            console.error("Lỗi tìm kiếm:", err);
            $('#recipientResults').html('<div class="text-center p-3">Không thể tìm kiếm người dùng</div>');
        }
    });
}

// Hiển thị kết quả tìm kiếm người dùng
function renderUserResults(users) {
    if (users.length === 0) {
        $('#recipientResults').html('<div class="text-center p-3">Không tìm thấy người dùng nào</div>');
        return;
    }
    
    let html = '';
    users.forEach(function(user) {
        html += `
        <div class="recipient-item" data-user-id="${user.userId}" data-user-name="${user.userName}">
            <div class="recipient-avatar">
                <img src="${user.avatar}" alt="${user.userName}" onerror="this.src='/images/authors/default/default-image.png'">
            </div>
            <div class="recipient-info">
                <div class="recipient-name">${user.userName}</div>
                <div class="recipient-username">@${user.username || user.userId}</div>
            </div>
        </div>
        `;
    });
    
    $('#recipientResults').html(html);
    
    // Xử lý sự kiện khi chọn người dùng
    $('.recipient-item').on('click', function() {
        const userId = $(this).data('user-id');
        const userName = $(this).data('user-name');
        
        // Mở cuộc trò chuyện với người dùng được chọn
        $('#newMessageModal').hide();
        $('#recipientInput').val('');
        $('#recipientResults').empty();
        
        openConversation(userId);
    });
}

$(function() {
    // Đăng ký lại sự kiện cho nút bút chì sau khi trang đã tải hoàn toàn
    $(document).on('click', '#newMessageBtn', function() {
        console.log("Nút bút chì được nhấn");
        $('#newMessageModal').css('display', 'flex');
        $('#recipientInput').focus();
    });
});
