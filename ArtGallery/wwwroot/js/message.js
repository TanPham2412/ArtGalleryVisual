console.log("message.js được tải");

$(document).ready(function() {
    console.log("Document ready trong message.js");
    
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
    
    // THÊM MỚI: Kiểm tra và mở cuộc trò chuyện từ URL
    function checkURLForConversation() {
        console.log("Đang kiểm tra URL cho cuộc trò chuyện");
        const url = window.location.pathname;
        console.log("URL hiện tại:", url);
        const urlParts = url.split('/');
        console.log("Các phần của URL:", urlParts);
        
        // Kiểm tra xem URL có chứa ID người dùng không
        if (urlParts.length > 3 && urlParts[1].toLowerCase() === "messages" && urlParts[2].toLowerCase() === "index") {
            const userId = urlParts[3];
            console.log("ID người dùng từ URL:", userId);
            // Nếu có ID người dùng, tự động mở cuộc trò chuyện
            if (userId) {
                console.log("Đang mở cuộc trò chuyện với người dùng:", userId);
                setTimeout(function() {
                    openConversation(userId);
                }, 500); // Thêm timeout để đảm bảo DOM đã được tải
            }
        }
    }

    // Xử lý click vào nút chọn ảnh
    // $(document).on('click', '#openImageSelector', function () {
    //     console.log('Nút chọn ảnh được nhấn');
    //     document.getElementById('messageImageInput').click();
    // });

    // Xử lý khi chọn ảnh
    // $(document).on('change', '#messageImageInput', function () {
    //     const file = this.files[0];
    //     if (file) {
    //         const reader = new FileReader();
    //         reader.onload = function (e) {
    //             $('#imagePreview').attr('src', e.target.result);
    //             $('#imagePreviewContainer').removeClass('d-none');
    //             $('#stickerPreviewContainer').addClass('d-none');
    //             $('#messageStickerPath').val('');
    //         }
    //         reader.readAsDataURL(file);
    //     }
    // });

    // Xử lý click vào nút chọn sticker
    $(document).on('click', '#openStickerSelector', function () {
        console.log('Nút chọn sticker được nhấn');
        // Tải stickers nếu chưa tải
        if (!window.stickersLoaded) {
            loadStickers();
            window.stickersLoaded = true;
        }
        // Hiển thị modal sticker - Sửa cách gọi modal
        $('#stickerModal').modal('show');
    });
    
    // Gọi hàm kiểm tra URL khi trang đã tải xong
    checkURLForConversation();
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
    console.log("Đang mở cuộc trò chuyện với userId:", userId);
    
    // API đánh dấu tin nhắn đã đọc
    $.ajax({
        url: '/Messages/MarkConversationAsRead',
        type: 'POST',
        data: { userId: userId },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            console.log("Đánh dấu tin nhắn đã đọc:", response);
        }
    });
    
    // Lấy thông tin người dùng và hiển thị cuộc trò chuyện
    $.ajax({
        url: '/Messages/GetUserInfo',
        type: 'GET',
        data: { id: userId },
        success: function(userData) {
            $.ajax({
                url: '/Messages/GetConversation',
                type: 'GET',
                data: { userId: userId },
                success: function(messages) {
                    renderConversation(userData, messages);
                    $('.conversation-item').removeClass('active');
                    $(`.conversation-item[data-user-id="${userId}"]`).addClass('active');
                    
                    // Cập nhật lại danh sách cuộc trò chuyện
                    loadConversations();
                },
                error: function(err) {
                    $('#messageContent').html('<div class="error-message">Không thể tải tin nhắn</div>');
                }
            });
        },
        error: function(err) {
            $('#messageContent').html('<div class="error-message">Không thể tải thông tin người dùng</div>');
        }
    });
}

// Hiển thị cuộc trò chuyện và tin nhắn - sử dụng template từ HTML
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
    
    // Tạo container cho nội dung chat
    let html = document.createElement('div');
    
    // Thêm header chat từ template
    const headerTemplate = document.getElementById('chatHeaderTemplate');
    const headerClone = document.importNode(headerTemplate.content, true);
    // Cập nhật thông tin user
    const headerAvatar = headerClone.querySelector('.chat-avatar img');
    headerAvatar.src = user.avatar;
    const statusIndicator = headerClone.querySelector('.status-indicator');
    if (user.isOnline) {
        statusIndicator.classList.add('online');
    }
    headerClone.querySelector('.chat-username').textContent = user.userName;
    html.appendChild(headerClone);
    
    // Thêm container tin nhắn
    const chatMessages = document.createElement('div');
    chatMessages.className = 'chat-messages';
    chatMessages.id = 'chatMessages';
    
    // Thêm các tin nhắn
    if (messages && messages.length > 0) {
        let lastDate = '';

        for (const message of messages) {
            // Kiểm tra nếu ngày thay đổi, hiển thị phân cách ngày
            const messageDate = new Date(message.thoiGian).toLocaleDateString('vi-VN');
            if (messageDate !== lastDate) {
                const dateSeparator = document.createElement('div');
                dateSeparator.className = 'message-date-separator';
                dateSeparator.innerHTML = `<span>${messageDate}</span>`;
                chatMessages.appendChild(dateSeparator);
                lastDate = messageDate;
            }

            // Xác định tin nhắn của mình hay của người khác
            const isMyMessage = message.maNguoiGui === currentUserId;

            // Tạo tin nhắn từ template
            const messageTemplate = document.getElementById('messageTemplate');
            const messageClone = document.importNode(messageTemplate.content, true);
            const messageItem = messageClone.querySelector('.chat-message-item');

            if (isMyMessage) {
                messageItem.classList.add('my-message');
                // Xóa avatar nếu là tin nhắn của mình
                const avatar = messageClone.querySelector('.message-avatar');
                messageItem.removeChild(avatar);
            } else {
                messageItem.classList.add('other-message');
                // Cập nhật avatar
                const avatar = messageClone.querySelector('.message-avatar img');
                avatar.src = user.avatar;
            }

            // Cập nhật nội dung tin nhắn
            const messageBubble = messageClone.querySelector('.message-bubble');
            const messageText = messageClone.querySelector('.message-text');

            // Thêm sticker nếu có
            if (message.sticker) {
                const stickerElem = document.createElement('div');
                stickerElem.className = 'message-sticker';
                const img = document.createElement('img');
                img.src = message.sticker;
                img.alt = "Sticker";
                stickerElem.appendChild(img);
                messageBubble.appendChild(stickerElem);
            }

            // Thêm ảnh nếu có
            if (message.duongDanAnh) {
                const imageContainer = document.createElement('div');
                imageContainer.className = 'message-image-container';
                const img = document.createElement('img');
                img.src = message.duongDanAnh;
                img.alt = "Ảnh";
                img.className = 'message-image';
                imageContainer.appendChild(img);
                messageBubble.appendChild(imageContainer);
            }

            // Thêm text nếu có
            messageText.textContent = message.noiDung;

            messageClone.querySelector('.message-time').textContent = formatTime(message.thoiGian);

            chatMessages.appendChild(messageClone);
        }
    } else {
        // Không có tin nhắn, hiển thị trạng thái trống
        const emptyTemplate = document.getElementById('emptyConversationTemplate');
        chatMessages.appendChild(document.importNode(emptyTemplate.content, true));
    }
    
    html.appendChild(chatMessages);
    
    // Thêm phần nhập tin nhắn
    const inputTemplate = document.getElementById('chatInputTemplate');
    const inputClone = document.importNode(inputTemplate.content, true);
    const sendButton = inputClone.querySelector('#sendMessageBtn');
    sendButton.setAttribute('data-receiver-id', user.userId);
    html.appendChild(inputClone);
    
    // Cập nhật UI
    $('#messageContent').html(html.innerHTML);
    
    // Cuộn xuống tin nhắn cuối cùng
    const chatMessagesElement = document.getElementById('chatMessages');
    if (chatMessagesElement) {
        chatMessagesElement.scrollTop = chatMessagesElement.scrollHeight;
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

// Sửa hàm sendMessage để xử lý gửi ảnh đúng cách
function sendMessage(receiverId) {
    const input = $('#messageInput');
    const message = input.val().trim();
    const stickerPath = $('#messageStickerPath').val();
    const imageFile = $('#messageImageInput')[0].files[0];

    // Kiểm tra có nội dung tin nhắn hoặc file ảnh hoặc sticker không
    if (message.length === 0 && !stickerPath && !imageFile) return;

    // Xóa nội dung input và preview
    input.val('');
    
    // Lấy token antiforgery
    const token = $('input[name="__RequestVerificationToken"]').val();

    // Tạo FormData để gửi file
    const formData = new FormData();
    formData.append('receiverId', receiverId);
    formData.append('message', message);
    formData.append('__RequestVerificationToken', token);

    if (stickerPath) {
        formData.append('sticker', stickerPath);
    }

    if (imageFile) {
        formData.append('messageImage', imageFile);
        // Đặt lại input file và preview ảnh
        $('#messageImageInput').val('');
        $('#imagePreviewContainer').addClass('d-none');
    } else {
        // Nếu không có file thì vẫn cần xóa preview
        $('#messageStickerPath').val('');
        $('#stickerPreviewContainer').addClass('d-none');
    }

    // Gửi tin nhắn đến server
    $.ajax({
        url: '/Messages/SendMessageWithMedia',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function(data) {
            if (data.success) {
                // Thêm tin nhắn vào danh sách tin nhắn
                appendMyMessage(data.message);
                // Cập nhật danh sách cuộc trò chuyện
                loadConversations();
            }
        },
        error: function(xhr) {
            console.error("Lỗi gửi tin nhắn:", xhr.responseText);
            alert('Gửi tin nhắn thất bại');
        }
    });
}

// Sửa lại hàm hiển thị tin nhắn của mình
function appendMyMessage(message) {
    let html = `
    <div class="chat-message-item my-message">
        <div class="message-content">
            <div class="message-bubble">`;
    
    // Thêm sticker nếu có
    if (message.sticker) {
        html += `<div class="message-sticker"><img src="${message.sticker}" alt="Sticker"></div>`;
    }

    // Thêm ảnh nếu có
    if (message.duongDanAnh) {
        html += `<div class="message-image-container"><img src="${message.duongDanAnh}" alt="Ảnh" class="message-image"></div>`;
    }

    // Thêm text nếu có
    if (message.noiDung) {
        html += `<div class="message-text">${message.noiDung}</div>`;
    }
    
    html += `</div>
            <div class="message-time">${formatTime(message.thoiGian)}</div>
        </div>
    </div>`;
    
    // Thêm vào danh sách tin nhắn
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

// Hàm xóa ảnh đã chọn
function removeMessageImage() {
    $('#imagePreviewContainer').addClass('d-none');
    $('#messageImageInput').val('');
}

// Hàm xóa sticker đã chọn
function removeMessageSticker() {
    $('#stickerPreviewContainer').addClass('d-none');
    $('#messageStickerPath').val('');
}

// Thêm hàm loadStickers tương tự như trong display-artwork.js
function loadStickers() {
    console.log('Loading stickers...');
    // Cập nhật tên thư mục trong biến path
    const daisuhuynhPath = '/images/stickers/daisuhuynh/';
    const nhisuhuynhPath = '/images/stickers/nhisuhuynh/';
    const tamsuhuynhPath = '/images/stickers/tamsuhuynh/';
    const tusuhuynhPath = '/images/stickers/tusuhuynh/';

    // Tạo 12 stickers mẫu cho mỗi thư mục
    let daisuhuynhHtml = '';
    for (let i = 1; i <= 12; i++) {
        daisuhuynhHtml += `<div class="sticker-item">
            <img src="${daisuhuynhPath}sticker${i}.png" data-path="${daisuhuynhPath}sticker${i}.png" 
                 class="sticker-img" onclick="selectMessageSticker(this)">
        </div>`;
    }
    $('#daisuhuynh-stickers').html(daisuhuynhHtml);

    let nhisuhuynhHtml = '';
    for (let i = 1; i <= 12; i++) {
        nhisuhuynhHtml += `<div class="sticker-item">
            <img src="${nhisuhuynhPath}sticker${i}.png" data-path="${nhisuhuynhPath}sticker${i}.png" 
                 class="sticker-img" onclick="selectMessageSticker(this)">
        </div>`;
    }
    $('#nhisuhuynh-stickers').html(nhisuhuynhHtml);

    let tamsuhuynhHtml = '';
    for (let i = 1; i <= 12; i++) {
        tamsuhuynhHtml += `<div class="sticker-item">
            <img src="${tamsuhuynhPath}sticker${i}.png" data-path="${tamsuhuynhPath}sticker${i}.png" 
                 class="sticker-img" onclick="selectMessageSticker(this)">
        </div>`;
    }
    $('#tamsuhuynh-stickers').html(tamsuhuynhHtml);

    let tusuhuynhHtml = '';
    for (let i = 1; i <= 12; i++) {
        tusuhuynhHtml += `<div class="sticker-item">
            <img src="${tusuhuynhPath}sticker${i}.png" data-path="${tusuhuynhPath}sticker${i}.png" 
                 class="sticker-img" onclick="selectMessageSticker(this)">
        </div>`;
    }
    $('#tusuhuynh-stickers').html(tusuhuynhHtml);

    // Gọi API nếu cần
    $.ajax({
        url: '/Artwork/GetStickers',
        type: 'GET',
        success: function (data) {
            console.log('Stickers loaded:', data);
            // Cập nhật UI nếu có dữ liệu từ API
        },
        error: function (err) {
            console.error('Error loading stickers:', err);
        }
    });
}

// Hàm chọn sticker
function selectMessageSticker(element) {
    const stickerPath = $(element).data('path');
    $('#stickerModal').modal('hide');

    $('#stickerPreview').attr('src', stickerPath);
    $('#stickerPreviewContainer').removeClass('d-none');
    $('#imagePreviewContainer').addClass('d-none');
    $('#messageStickerPath').val(stickerPath);
}