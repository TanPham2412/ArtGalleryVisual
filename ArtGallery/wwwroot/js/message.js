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
});

// Load danh sách cuộc trò chuyện
function loadConversations() {
    $.ajax({
        url: '/Messages/GetConversations',
        type: 'GET',
        success: function(data) {
            renderConversations(data.conversations || []);
        },
        error: function() {
            $('#conversationList').html('<div class="text-center p-3">Không thể tải tin nhắn</div>');
        }
    });
}

// Render danh sách cuộc trò chuyện
function renderConversations(conversations) {
    if (conversations.length === 0) {
        $('#conversationList').html('<div class="text-center p-3">Chưa có cuộc trò chuyện nào</div>');
        return;
    }
    
    let html = '';
    conversations.forEach(function(conv) {
        const lastMessageTime = formatConversationTime(new Date(conv.lastMessageTime));
        const isOnline = conv.isOnline ? '<div class="online-indicator"></div>' : '';
        const unreadBadge = conv.unreadCount > 0 ? '<div class="conversation-unread"></div>' : '';
        
        html += `
        <div class="conversation-item" data-user-id="${conv.userId}" onclick="openConversation('${conv.userId}')">
            <div class="conversation-avatar">
                <img src="${conv.avatar}" alt="${conv.userName}" onerror="this.src='/images/authors/default/default-image.png'">
                ${isOnline}
            </div>
            <div class="conversation-content">
                <div class="conversation-header">
                    <div class="conversation-name">${conv.userName}</div>
                    <div class="conversation-time">${lastMessageTime}</div>
                </div>
                <div class="conversation-message">
                    <div class="conversation-preview">${conv.lastMessage || 'Chưa có tin nhắn'}</div>
                    ${unreadBadge}
                </div>
            </div>
        </div>
        `;
    });
    
    $('#conversationList').html(html);
}

// Mở cuộc trò chuyện
function openConversation(userId) {
    // Đánh dấu cuộc trò chuyện được chọn
    $('.conversation-item').removeClass('active');
    $(`.conversation-item[data-user-id="${userId}"]`).addClass('active');
    
    // Tải thông tin người dùng và tin nhắn mà không hiển thị spinner
    $.ajax({
        url: `/Messages/GetUserInfo/${userId}`,
        type: 'GET',
        success: function(userData) {
            // Sau khi có thông tin người dùng, tải tin nhắn
            loadMessages(userId, userData);
        },
        error: function() {
            $('#messageContent').html('<div class="text-center p-3 mt-5">Không thể tải thông tin người dùng</div>');
        }
    });
}

// Tải tin nhắn
function loadMessages(userId, userData) {
    $.ajax({
        url: `/Messages/GetConversation/${userId}`,
        type: 'GET',
        success: function(messages) {
            renderChat(userData, messages);
        },
        error: function() {
            $('#messageContent').html('<div class="text-center p-3 mt-5">Không thể tải tin nhắn</div>');
        }
    });
}

// Render giao diện chat
function renderChat(user, messages) {
    const isOnline = user.isOnline ? 'online' : '';
    const lastActive = user.isOnline ? 'Đang hoạt động' : `Hoạt động ${formatLastActive(new Date(user.lastActive))}`;
    
    let html = `
    <div class="chat-area">
        <div class="chat-header">
            <div class="chat-user-info">
                <div class="chat-avatar">
                    <img src="${user.avatar}" alt="${user.userName}" onerror="this.src='/images/authors/default/default-image.png'">
                </div>
                <div class="chat-user-details">
                    <div class="chat-username">${user.userName}</div>
                    <div class="chat-status ${isOnline}">${lastActive}</div>
                </div>
            </div>
            <div class="chat-actions">
                <button class="chat-action-btn"><i class="fas fa-phone-alt"></i></button>
                <button class="chat-action-btn"><i class="fas fa-video"></i></button>
                <button class="chat-action-btn"><i class="fas fa-info-circle"></i></button>
            </div>
        </div>
        
        <div class="chat-messages" id="chatMessages">
            ${renderMessages(messages)}
        </div>
        
        <div class="message-input-area">
            <div class="message-input-container">
                <textarea class="message-input" id="messageInput" placeholder="Aa" rows="1"></textarea>
                <div class="message-action-buttons">
                    <div class="message-action"><i class="far fa-smile"></i></div>
                    <div class="message-action"><i class="fas fa-paperclip"></i></div>
                    <div class="message-action"><i class="far fa-image"></i></div>
                </div>
            </div>
            <div class="send-button" onclick="sendMessage('${user.userId}')">
                <i class="far fa-paper-plane"></i>
            </div>
        </div>
    </div>
    `;
    
    $('#messageContent').html(html);
    
    // Scroll đến tin nhắn cuối cùng
    const chatMessages = document.getElementById('chatMessages');
    if (chatMessages) {
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }
    
    // Xử lý nhấn Enter để gửi tin nhắn
    $('#messageInput').on('keydown', function(e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendMessage(user.userId);
        }
    });
    
    // Tự động điều chỉnh chiều cao của textarea
    $('#messageInput').on('input', function() {
        this.style.height = 'auto';
        this.style.height = (this.scrollHeight) + 'px';
    });
}

// Render tin nhắn
function renderMessages(messages) {
    if (!messages || messages.length === 0) {
        return '<div class="text-center p-3">Hãy bắt đầu cuộc trò chuyện</div>';
    }
    
    let html = '';
    let currentDate = '';
    let currentUser = '';
    
    messages.forEach(function(msg, index) {
        const messageDate = formatMessageDate(new Date(msg.thoiGian));
        const isOwn = msg.maNguoiGui === currentUserId; // Biến currentUserId cần được định nghĩa ở đâu đó
        
        // Thêm ngày nếu khác ngày
        if (messageDate !== currentDate) {
            html += `<div class="message-date">${messageDate}</div>`;
            currentDate = messageDate;
            currentUser = ''; // Reset để tin nhắn không bị gộp nhóm khi ngày mới
        }
        
        // Kiểm tra nếu tin nhắn mới từ người gửi khác, bắt đầu nhóm tin nhắn mới
        if (currentUser !== msg.maNguoiGui) {
            // Nếu đã có nhóm trước đó, đóng nhóm đó lại
            if (currentUser !== '') {
                html += `</div>`;
            }
            
            // Bắt đầu nhóm tin nhắn mới
            html += `<div class="message-group ${isOwn ? 'own' : ''}">`;
            currentUser = msg.maNguoiGui;
        }
        
        // Thêm tin nhắn vào nhóm
        html += `
            <div class="message-bubble">${msg.noiDung}</div>
        `;
        
        // Nếu là tin nhắn cuối cùng trong nhóm hoặc cuối danh sách, thêm thời gian
        const nextMsg = messages[index + 1];
        if (!nextMsg || nextMsg.maNguoiGui !== msg.maNguoiGui) {
            html += `<div class="message-time">${formatMessageTime(new Date(msg.thoiGian))}</div>`;
        }
        
        // Nếu là tin nhắn cuối cùng hoặc tin nhắn tiếp theo từ người khác, đóng nhóm
        if (!nextMsg || nextMsg.maNguoiGui !== msg.maNguoiGui) {
            html += `</div>`;
        }
    });
    
    return html;
}

// Gửi tin nhắn
function sendMessage(receiverId) {
    const content = $('#messageInput').val().trim();
    if (!content) return;
    
    // Clear input và reset height
    $('#messageInput').val('').css('height', 'auto');
    
    // Thêm tin nhắn tạm thời vào giao diện
    const tempMessage = {
        noiDung: content,
        thoiGian: new Date(),
        maNguoiGui: currentUserId, // Biến currentUserId cần được định nghĩa ở đâu đó
        daDoc: false
    };
    
    // Thêm tin nhắn mới vào cuối danh sách tin nhắn
    const chatMessages = document.getElementById('chatMessages');
    const tempMessageHtml = `
        <div class="message-group own">
            <div class="message-bubble">${content}</div>
            <div class="message-time">${formatMessageTime(new Date())}</div>
        </div>
    `;
    
    // Nếu không có tin nhắn nào, xóa thông báo "Hãy bắt đầu cuộc trò chuyện"
    if (chatMessages.innerHTML.includes('Hãy bắt đầu cuộc trò chuyện')) {
        chatMessages.innerHTML = '';
    }
    
    chatMessages.innerHTML += tempMessageHtml;
    chatMessages.scrollTop = chatMessages.scrollHeight;
    
    // Gửi tin nhắn lên server
    $.ajax({
        url: '/Messages/SendMessage',
        type: 'POST',
        data: {
            receiverId: receiverId,
            message: content
        },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            // Cập nhật lại danh sách cuộc trò chuyện
            loadConversations();
        },
        error: function() {
            alert('Không thể gửi tin nhắn. Vui lòng thử lại sau.');
        }
    });
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

// Hàm tìm kiếm người dùng
function searchUsers(query) {
    $.ajax({
        url: '/Messages/SearchUsers',
        type: 'GET',
        data: { query: query },
        success: function(data) {
            renderUserResults(data.users || []);
        },
        error: function() {
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
