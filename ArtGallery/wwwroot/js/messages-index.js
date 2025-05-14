// Đảm bảo Bootstrap modal hoạt động
document.addEventListener('DOMContentLoaded', function() {
    if (typeof bootstrap !== 'undefined') {
        console.log('Bootstrap đã được tải');

        // Kiểm tra nút chọn ảnh
        $(document).on('click', '#openImageSelector', function() {
            console.log('Nút chọn ảnh được nhấn - từ script inline');
            $('#messageImageInput').click();
        });

        // Kiểm tra nút chọn sticker
        $(document).on('click', '#openStickerSelector', function() {
            console.log('Nút chọn sticker được nhấn - từ script inline');
            var stickerModal = new bootstrap.Modal(document.getElementById('stickerModal'));
            stickerModal.show();
        });
    } else {
        console.error('Bootstrap chưa được tải');
        // Tải Bootstrap nếu chưa có
        var script = document.createElement('script');
        script.src = 'https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/js/bootstrap.bundle.min.js';
        document.head.appendChild(script);
    }
});

// Kiểm tra đường dẫn sticker có tồn tại không
function checkStickerPath() {
    const testImg = new Image();
    testImg.onload = function() {
        console.log('Đường dẫn sticker hợp lệ');
    };
    testImg.onerror = function() {
        console.error('Đường dẫn sticker không hợp lệ hoặc thư mục không tồn tại');
    };
    testImg.src = '/images/stickers/daisuhuynh/sticker1.png';
}
checkStickerPath();

// Đơn giản hóa phần tìm kiếm người dùng
document.addEventListener('DOMContentLoaded', function() {
    document.getElementById('recipientInput').addEventListener('input', function() {
        var query = this.value.trim();
        
        if (query.length > 0) {
            document.getElementById('recipientResults').innerHTML = '<div class="text-center p-3">Đang tìm kiếm...</div>';
            
            $.ajax({
                url: '/Messages/SearchUsers',
                type: 'GET',
                data: { query: query },
                success: function(data) {
                    var resultsHtml = '';
                    
                    if (!data.users || data.users.length === 0) {
                        resultsHtml = '<div class="text-center p-3">Không tìm thấy người dùng nào</div>';
                    } else {
                        for (var i = 0; i < data.users.length; i++) {
                            var user = data.users[i];
                            resultsHtml += '<div class="recipient-item" data-user-id="' + user.userId + '">' +
                                '<div class="recipient-avatar">' +
                                '<img src="' + user.avatar + '" alt="" onerror="this.src=\'/images/authors/default/default-image.png\'">' +
                                '</div>' +
                                '<div class="recipient-info">' +
                                '<div class="recipient-name">' + user.userName + '</div>' +
                                '<div class="recipient-username">' + (user.username || user.email || '') + '</div>' +
                                '</div>' +
                                '</div>';
                        }
                    }
                    
                    $('#recipientResults').html(resultsHtml);
                    
                    $('.recipient-item').click(function() {
                        var userId = $(this).data('user-id');
                        $('#newMessageModal').hide();
                        $('#recipientInput').val('');
                        openConversation(userId);
                    });
                },
                error: function() {
                    $('#recipientResults').html('<div class="text-center p-3">Lỗi tìm kiếm người dùng</div>');
                }
            });
        } else {
            $('#recipientResults').html('<div class="text-center p-3">Hãy nhập tên người dùng để tìm kiếm</div>');
        }
    });
    
    // Xử lý sự kiện khi nhấp vào nút bút chì
    document.getElementById('newMessageBtn').addEventListener('click', function() {
        document.getElementById('newMessageModal').style.display = 'flex';
        document.getElementById('recipientInput').focus();
    });
    
    // Đóng form tin nhắn mới khi click vào nút đóng
    document.getElementById('closeNewMessageBtn').addEventListener('click', function() {
        document.getElementById('newMessageModal').style.display = 'none';
        document.getElementById('recipientInput').value = '';
        document.getElementById('recipientResults').innerHTML = 
            '<div class="text-center p-3">Hãy nhập tên người dùng để tìm kiếm</div>';
    });
    
    // Đóng form khi click ra ngoài
    document.getElementById('newMessageModal').addEventListener('click', function(e) {
        if (e.target === this) {
            this.style.display = 'none';
            document.getElementById('recipientInput').value = '';
            document.getElementById('recipientResults').innerHTML = 
                '<div class="text-center p-3">Hãy nhập tên người dùng để tìm kiếm</div>';
        }
    });

    // Lấy ID người dùng từ URL
    const url = window.location.pathname;
    const urlParts = url.split('/');
    // Kiểm tra xem URL có chứa ID người dùng không
    if (urlParts.length > 3) {
        const userId = urlParts[3];
        // Nếu có ID người dùng, tự động mở cuộc trò chuyện
        if (userId) {
            console.log("Mở cuộc trò chuyện với người dùng: " + userId);
            // Gọi hàm openConversation để mở cuộc trò chuyện
            openConversation(userId);
        }
    }
});

// Đảm bảo footer không hiển thị khi trang tin nhắn được tải
$(document).ready(function() {
    $('footer, .footer, .site-footer').remove();
    
    // Kiểm tra URL và mở cuộc trò chuyện nếu cần
    console.log("Index.cshtml script - kiểm tra URL");
    const url = window.location.pathname;
    const urlParts = url.split('/');
    
    // Kiểm tra xem URL có chứa ID người dùng không
    if (urlParts.length > 3) {
        const userId = urlParts[3];
        console.log("Index.cshtml - ID người dùng từ URL:", userId);
        // Nếu có ID người dùng, tự động mở cuộc trò chuyện
        if (userId && typeof openConversation === 'function') {
            console.log("Index.cshtml - Gọi hàm openConversation");
            setTimeout(function() {
                openConversation(userId);
            }, 800);
        } else {
            console.error("Index.cshtml - Không tìm thấy hàm openConversation hoặc không có userId");
        }
    }
});

// Đánh dấu đã đọc tin nhắn
$(document).ready(function() {
    // Đánh dấu đã đọc tất cả tin nhắn khi click vào conversation-item
    $(document).on('click', '.conversation-item', function() {
        const userId = $(this).data('user-id');
        
        // Xóa badge ngay lập tức trên UI
        $(this).find('.unread-badge').remove();
        
        // Gọi API đánh dấu đã đọc
        $.ajax({
            url: '/Messages/MarkConversationAsRead',
            type: 'POST',
            data: { userId: userId },
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                console.log("Đánh dấu đã đọc thành công", response);
                
                // Cập nhật UI một lần nữa để đảm bảo
                loadConversations();
            },
            error: function(err) {
                console.error("Lỗi khi đánh dấu đã đọc:", err);
            }
        });
    });
});

// Xử lý chọn ảnh
$(document).ready(function() {
    // Xóa tất cả các sự kiện click đã đăng ký cho #openImageSelector
    $(document).off('click', '#openImageSelector');
    
    // Đăng ký sự kiện click mới - chỉ thực hiện một lần
    $(document).on('click', '#openImageSelector', function(e) {
        e.preventDefault();
        e.stopPropagation();
        console.log('Nút chọn ảnh được nhấn - đã sửa');
        
        // Chỉ mở hộp thoại chọn ảnh một lần
        $('#messageImageInput').trigger('click');
    });
    
    // Đăng ký sự kiện change cho input file - chỉ một lần
    $(document).off('change', '#messageImageInput');
    $(document).on('change', '#messageImageInput', function() {
        console.log('Đã chọn ảnh, đang xử lý...');
        const fileInput = this;
        
        if (fileInput.files && fileInput.files[0]) {
            const file = fileInput.files[0];
            console.log('File đã chọn:', file.name, file.size);
            
            // Tạo đối tượng FileReader để đọc file
            const reader = new FileReader();
            
            // Xử lý khi đọc file hoàn tất
            reader.onload = function(e) {
                console.log('Đã đọc file thành công, hiển thị preview');
                // Cập nhật preview
                $('#imagePreview').attr('src', e.target.result);
                // Hiển thị container
                $('#imagePreviewContainer').removeClass('d-none');
                // Ẩn sticker nếu có
                $('#stickerPreviewContainer').addClass('d-none');
                $('#messageStickerPath').val('');
                
                // Đảm bảo cuộn xuống để xem ảnh preview
                const chatMessages = document.getElementById('chatMessages');
                if (chatMessages) {
                    chatMessages.scrollTop = chatMessages.scrollHeight;
                }
            };
            
            // Bắt lỗi nếu có
            reader.onerror = function(e) {
                console.error('Lỗi khi đọc file:', e);
                alert('Không thể đọc ảnh đã chọn');
            };
            
            // Đọc file dưới dạng URL
            reader.readAsDataURL(file);
        }
    });
});

// Hàm xóa ảnh đã chọn
function removeMessageImage() {
    console.log('Xóa ảnh đã chọn');
    $('#messageImageInput').val('');
    $('#imagePreviewContainer').addClass('d-none');
    $('#imagePreview').attr('src', '');
}

// Hàm xóa sticker đã chọn
function removeMessageSticker() {
    console.log('Xóa sticker đã chọn');
    $('#stickerPreviewContainer').addClass('d-none');
    $('#messageStickerPath').val('');
    $('#stickerPreview').attr('src', '');
}

// Cập nhật lại hàm renderConversation
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
    
    // Tạo container cho nội dung chat và xóa class mặc định
    let html = `
        <div class="chat-header">
            <div class="chat-user-info">
                <div class="chat-avatar">
                    <img src="${user.avatar}" alt="" onerror="this.src='/images/authors/default/default-image.png'">
                    <span class="status-indicator ${user.isOnline ? 'online' : ''}"></span>
                </div>
                <div class="chat-user">
                    <div class="chat-username">${user.userName}</div>
                </div>
            </div>
            <div class="chat-actions">
                <button type="button" class="btn-icon">
                    <i class="fas fa-info-circle"></i>
                </button>
            </div>
        </div>
        <div class="chat-container">
            <div class="chat-messages" id="chatMessages">`;
    
    // Thêm các tin nhắn
    if (messages && messages.length > 0) {
        let lastDate = '';

        for (const message of messages) {
            // Kiểm tra nếu ngày thay đổi, hiển thị phân cách ngày
            const messageDate = new Date(message.thoiGian).toLocaleDateString('vi-VN');
            if (messageDate !== lastDate) {
                html += `<div class="message-date-separator"><span>${messageDate}</span></div>`;
                lastDate = messageDate;
            }

            // Xác định tin nhắn của mình hay của người khác
            const isMyMessage = message.maNguoiGui === currentUserId;
            const messageClass = isMyMessage ? 'my-message' : 'other-message';

            // Tạo HTML cho tin nhắn
            if (isMyMessage) {
                html += `
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
                html += `<div class="message-text">${message.noiDung}</div>`;
                html += `</div><div class="message-time">${formatTime(message.thoiGian)}</div>
                        </div>
                    </div>`;
            } else {
                html += `
                    <div class="chat-message-item other-message">
                        <div class="message-avatar">
                            <img src="${user.avatar}" alt="" onerror="this.src='/images/authors/default/default-image.png'">
                        </div>
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
                html += `<div class="message-text">${message.noiDung}</div>`;
                html += `</div><div class="message-time">${formatTime(message.thoiGian)}</div>
                        </div>
                    </div>`;
            }
        }
    } else {
        // Không có tin nhắn, hiển thị trạng thái trống
        html += `
            <div class="empty-conversation">
                <div class="empty-icon">
                    <i class="far fa-paper-plane"></i>
                </div>
                <div class="empty-text">Hãy bắt đầu cuộc trò chuyện</div>
            </div>`;
    }
    
    html += `</div>`;
    
    // Thêm khu vực preview ảnh và sticker
    html += `
        <div class="media-preview-area">
            <div id="imagePreviewContainer" class="d-none media-preview-item">
                <img id="imagePreview" src="" alt="Preview" class="preview-img">
                <button type="button" class="btn-remove-preview" onclick="removeMessageImage()">
                    <i class="fas fa-times"></i>
                </button>
            </div>
            
            <div id="stickerPreviewContainer" class="d-none media-preview-item">
                <img id="stickerPreview" src="" alt="Sticker" class="preview-sticker">
                <button type="button" class="btn-remove-preview" onclick="removeMessageSticker()">
                    <i class="fas fa-times"></i>
                </button>
            </div>
        </div>`;
    
    // Thêm phần nhập tin nhắn
    html += `
        <div class="chat-input">
            <div class="input-actions">
                <button type="button" class="btn-icon" id="openImageSelector">
                    <i class="far fa-image"></i>
                </button>
                <button type="button" class="btn-icon" id="openStickerSelector">
                    <i class="far fa-smile"></i>
                </button>
            </div>
            <div class="message-input-wrapper">
                <input type="text" id="messageInput" placeholder="Aa" autocomplete="off">
                <input type="hidden" id="messageImagePath">
                <input type="hidden" id="messageStickerPath">
            </div>
            <button type="button" class="send-btn" id="sendMessageBtn" data-receiver-id="${user.userId}">
                <i class="fas fa-paper-plane"></i>
            </button>
        </div>
    </div>`;
    
    // Cập nhật UI
    $('#messageContent').html(html);
    
    // Cuộn xuống tin nhắn cuối cùng
    const chatMessagesElement = document.getElementById('chatMessages');
    if (chatMessagesElement) {
        chatMessagesElement.scrollTop = chatMessagesElement.scrollHeight;
    }
    
    // Thêm sự kiện gửi tin nhắn
    setupSendMessage(user.userId);
}

// Hàm cập nhật để thêm mới tin nhắn của mình
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
