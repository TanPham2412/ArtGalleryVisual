// Hàm xử lý yêu thích
function toggleLike(button, artworkId) {
    $.ajax({
        url: '/Artwork/ToggleLike',
        type: 'POST',
        data: { artworkId: artworkId },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                const icon = button.querySelector('i');
                if (response.liked) {
                    // Đã thích
                    icon.classList.remove('far');
                    icon.classList.add('fas');
                    button.classList.add('active');
                } else {
                    // Đã hủy thích
                    icon.classList.remove('fas');
                    icon.classList.add('far');
                    button.classList.remove('active');
                }
            } else {
                alert(response.message);
            }
        },
        error: function() {
            alert('Có lỗi xảy ra khi thực hiện thao tác');
        }
    });
}

// Hàm xử lý theo dõi
function toggleFollow(event, userId) {
    event.preventDefault();
    var button = event.currentTarget;
    
    $.ajax({
        url: '/User/ToggleFollow',
        type: 'POST',
        data: { followedUserId: userId },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                if (response.isFollowing) {
                    button.classList.add('following');
                } else {
                    button.classList.remove('following');
                }
            } else {
                alert(response.message);
            }
        },
        error: function() {
            alert('Có lỗi xảy ra khi thực hiện thao tác theo dõi');
        }
    });
}

// Hàm xử lý modal ảnh sản phẩm
function openArtworkModal() {
    const modal = document.getElementById('artworkModal');
    if (modal) {
        modal.style.display = 'block';
        document.body.classList.add('modal-open');
    } else {
        console.error('Modal element not found');
    }
}

function closeArtworkModal() {
    document.getElementById('artworkModal').style.display = 'none';
    document.body.classList.remove('modal-open');
}

// Hàm xử lý modal ảnh bình luận
function openImageModal(imageSrc) {
    const modal = document.getElementById('commentImageModal');
    const modalImg = document.getElementById('modalCommentImage');
    
    modalImg.src = imageSrc;
    modal.style.display = 'block';
    document.body.classList.add('modal-open');
}

function closeImageModal() {
    document.getElementById('commentImageModal').style.display = 'none';
    document.body.classList.remove('modal-open');
}

// Hàm xử lý form phản hồi
function toggleReplyForm(commentId) {
    const replyForm = document.getElementById(`replyForm-${commentId}`);
    if (replyForm) {
        replyForm.style.display = 'block';
        // Focus vào ô input
        setTimeout(() => {
            const textarea = replyForm.querySelector('textarea');
            if (textarea) textarea.focus();
        }, 100);
    }
}

function hideReplyForm(commentId) {
    const replyForm = document.getElementById(`replyForm-${commentId}`);
    if (replyForm) {
        replyForm.style.display = 'none';
        // Clear nội dung
        const textarea = replyForm.querySelector('textarea');
        if (textarea) textarea.value = '';
    }
}

// Hàm xử lý sticker
function loadStickers() {
    console.log('Loading stickers...');
    // Cập nhật tên thư mục trong biến path (nếu cần)
    const vanthuongPath = '/images/stickers/vanthuong/';
    const daisuhuynhPath = '/images/stickers/daisuhuynh/';
    
    // Tạo 12 stickers mẫu cho mỗi thư mục
    let vanthuongHtml = '';
    for (let i = 1; i <= 12; i++) {
        vanthuongHtml += `<div class="sticker-item">
            <img src="${vanthuongPath}sticker${i}.png" data-path="${vanthuongPath}sticker${i}.png" 
                 class="sticker-img" onclick="selectSticker(this)">
        </div>`;
    }
    $('#vanthuong-stickers').html(vanthuongHtml);
    
    let daisuhuynhHtml = '';
    for (let i = 1; i <= 12; i++) {
        daisuhuynhHtml += `<div class="sticker-item">
            <img src="${daisuhuynhPath}sticker${i}.png" data-path="${daisuhuynhPath}sticker${i}.png" 
                 class="sticker-img" onclick="selectSticker(this)">
        </div>`;
    }
    $('#daisuhuynh-stickers').html(daisuhuynhHtml);
    
    // Gọi API nếu hard-coded không hoạt động
    $.ajax({
        url: '/Artwork/GetStickers',
        type: 'GET',
        success: function(data) {
            console.log('Stickers loaded:', data);
            if (data.vanthuong && data.vanthuong.length > 0) {
                let vanthuongHtml = '';
                data.vanthuong.forEach(function(sticker) {
                    vanthuongHtml += `<div class="sticker-item">
                        <img src="${sticker}" data-path="${sticker}" class="sticker-img" onclick="selectSticker(this)">
                    </div>`;
                });
                $('#vanthuong-stickers').html(vanthuongHtml);
            }
            
            if (data.daisuhuynh && data.daisuhuynh.length > 0) {
                let daisuhuynhHtml = '';
                data.daisuhuynh.forEach(function(sticker) {
                    daisuhuynhHtml += `<div class="sticker-item">
                        <img src="${sticker}" data-path="${sticker}" class="sticker-img" onclick="selectSticker(this)">
                    </div>`;
                });
                $('#daisuhuynh-stickers').html(daisuhuynhHtml);
            }
        },
        error: function(err) {
            console.error('Error loading stickers:', err);
        }
    });
}

function selectSticker(element) {
    console.log('Sticker selected:', $(element).data('path'));
    const stickerPath = $(element).data('path');
    $('#stickerInput').val(stickerPath);
    $('#stickerPreview').attr('src', stickerPath);
    $('#stickerPreviewContainer').removeClass('d-none');
    $('#imagePreviewContainer').addClass('d-none');
    $('#imageInput').val('');
    $('#stickerModal').modal('hide');
}

// Hàm xử lý xóa tranh
function confirmDelete(artworkId) {
    if (confirm('Bạn có chắc chắn muốn xóa tranh này không?')) {
        $.ajax({
            url: '/Artwork/Delete/' + artworkId,
            type: 'POST',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    alert(response.message);
                    window.location.href = response.redirectUrl;
                } else {
                    alert(response.message);
                }
            },
            error: function () {
                alert('Có lỗi xảy ra khi xóa tranh');
            }
        });
    }
}

// Thêm hàm xử lý xóa bình luận
function deleteComment(commentId, artworkId) {
    if (confirm('Bạn có chắc chắn muốn xóa bình luận này?')) {
        $.ajax({
            url: '/Artwork/DeleteComment',
            type: 'POST',
            data: { 
                commentId: commentId,
                artworkId: artworkId 
            },
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    // Hiển thị thông báo thành công
                    Swal.fire({
                        title: 'Thành công!',
                        text: response.message,
                        icon: 'success'
                    }).then(() => {
                        // Tải lại trang để cập nhật danh sách bình luận
                        location.reload();
                    });
                } else {
                    // Hiển thị thông báo lỗi
                    Swal.fire({
                        title: 'Lỗi!',
                        text: response.message,
                        icon: 'error'
                    });
                }
            },
            error: function() {
                Swal.fire({
                    title: 'Lỗi!',
                    text: 'Có lỗi xảy ra khi xóa bình luận',
                    icon: 'error'
                });
            }
        });
    }
}

// Thêm hàm xử lý ẩn/hiện bình luận
function toggleHideComment(commentId, artworkId) {
    $.ajax({
        url: '/Artwork/ToggleHideComment',
        type: 'POST',
        data: { 
            commentId: commentId,
            artworkId: artworkId 
        },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                // Cập nhật giao diện trực tiếp thay vì tải lại trang
                const commentItem = $(`#comment-${commentId}`);
                if (response.isHidden) {
                    commentItem.addClass('comment-hidden');
                    commentItem.find('.comment-media-container, .comment-text').addClass('d-none');
                    commentItem.find('.hidden-comment-notice').removeClass('d-none');
                    commentItem.find('.btn-toggle-hide i').removeClass('fa-eye-slash').addClass('fa-eye');
                    commentItem.find('.btn-toggle-hide').text(' Hiện');
                } else {
                    commentItem.removeClass('comment-hidden');
                    commentItem.find('.comment-media-container, .comment-text').removeClass('d-none');
                    commentItem.find('.hidden-comment-notice').addClass('d-none');
                    commentItem.find('.btn-toggle-hide i').removeClass('fa-eye').addClass('fa-eye-slash');
                    commentItem.find('.btn-toggle-hide').text(' Ẩn');
                }
                
                // Hiển thị thông báo nhỏ
                const toastHTML = `
                    <div class="toast-container position-fixed bottom-0 end-0 p-3">
                        <div class="toast" role="alert" aria-live="assertive" aria-atomic="true">
                            <div class="toast-header">
                                <strong class="me-auto">Thông báo</strong>
                                <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
                            </div>
                            <div class="toast-body">
                                ${response.message}
                            </div>
                        </div>
                    </div>`;
                
                $('body').append(toastHTML);
                $('.toast').toast('show');
            } else {
                // Hiển thị thông báo lỗi
                Swal.fire({
                    title: 'Lỗi!',
                    text: response.message,
                    icon: 'error'
                });
            }
        },
        error: function() {
            Swal.fire({
                title: 'Lỗi!',
                text: 'Có lỗi xảy ra khi ẩn/hiện bình luận',
                icon: 'error'
            });
        }
    });
}

// Hiển thị modal sửa bình luận
function showEditCommentModal(commentId, commentContent) {
    $('#editCommentId').val(commentId);
    $('#editCommentContent').val(commentContent);
    $('#editCommentModal').modal('show');
}

// Lưu bình luận đã chỉnh sửa
function saveEditedComment() {
    const commentId = $('#editCommentId').val();
    const artworkId = $('#editArtworkId').val();
    const editedContent = $('#editCommentContent').val();
    
    if (!editedContent.trim()) {
        alert('Nội dung bình luận không được để trống');
        return;
    }
    
    $.ajax({
        url: '/Artwork/EditComment',
        type: 'POST',
        data: {
            commentId: commentId,
            artworkId: artworkId,
            editedContent: editedContent
        },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                // Cập nhật nội dung bình luận trên UI
                $(`#comment-${commentId} .comment-text`).text(response.editedContent);
                
                // Nếu comment chưa có trạng thái "đã chỉnh sửa", thêm vào
                if (!$(`#comment-${commentId} .edited-marker`).length) {
                    $(`#comment-${commentId} .commenter-name`).append('<span class="edited-marker">(đã chỉnh sửa)</span>');
                }
                
                // Đóng modal
                $('#editCommentModal').modal('hide');
                
                // Hiển thị thông báo
                const toastHTML = `
                    <div class="toast-container position-fixed bottom-0 end-0 p-3">
                        <div class="toast" role="alert" aria-live="assertive" aria-atomic="true">
                            <div class="toast-header">
                                <strong class="me-auto">Thông báo</strong>
                                <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
                            </div>
                            <div class="toast-body">
                                ${response.message}
                            </div>
                        </div>
                    </div>`;
                
                $('body').append(toastHTML);
                $('.toast').toast('show');
            } else {
                alert(response.message);
            }
        },
        error: function() {
            alert('Có lỗi xảy ra khi sửa bình luận');
        }
    });
}

// Sự kiện khi trang được tải xong
$(document).ready(function() {
    // Xử lý rating stars
    $('.rating-stars i').on('click', function() {
        const rating = $(this).data('rating');
        $('#selectedRating').val(rating);
        
        // Reset tất cả các sao về trạng thái chưa chọn
        $('.rating-stars i').removeClass('fas').addClass('far');
        
        // Thêm class active cho các sao được chọn
        $('.rating-stars i').each(function() {
            if ($(this).data('rating') <= rating) {
                $(this).removeClass('far').addClass('fas');
            }
        });
    });
    
    // Hiệu ứng hover
    $('.rating-stars i').on('mouseenter', function() {
        const hoverRating = $(this).data('rating');
        
        // Tô màu các sao khi hover
        $('.rating-stars i').each(function() {
            if ($(this).data('rating') <= hoverRating) {
                $(this).addClass('hover');
            } else {
                $(this).removeClass('hover');
            }
        });
    }).on('mouseleave', function() {
        // Xóa hover khi di chuột ra khỏi
        $('.rating-stars i').removeClass('hover');
    });

    // Sự kiện mở ảnh sản phẩm
    const artworkImage = document.querySelector('.art-image img');
    if (artworkImage) {
        artworkImage.addEventListener('click', function() {
            // Thiết lập src cho ảnh trong modal
            document.getElementById('modalImage').src = this.src;
            // Mở modal
            openArtworkModal();
        });
    }

    // Đóng modal khi click bên ngoài ảnh
    const modal = document.getElementById('artworkModal');
    if (modal) {
        modal.addEventListener('click', function(event) {
            if (event.target === modal) {
                closeArtworkModal();
            }
        });
    }

    // Xử lý sự kiện cho modal ảnh bình luận
    $('#commentImageModal').on('click', function(event) {
        if (event.target === this) {
            closeImageModal();
        }
    });
    
    // Sử dụng phím Esc để đóng modal
    $(document).keydown(function(event) {
        if (event.keyCode === 27) { // Phím Esc
            closeImageModal();
            closeArtworkModal();
        }
    });

    // Xử lý nút sticker
    $('#openStickerSelector').click(function() {
        console.log('Sticker button clicked');
        // Kiểm tra modal đã được tạo chưa
        if ($('#stickerModal').length) {
            $('#stickerModal').modal('show');
        } else {
            console.error('Sticker modal not found');
        }
    });

    // Xử lý nút chọn ảnh
    $('#imageInput').change(function() {
        const file = this.files[0];
        console.log('Đã chọn file:', file);
        if (file) {
            const reader = new FileReader();
            reader.onload = function(e) {
                $('#imagePreview').attr('src', e.target.result);
                $('#imagePreviewContainer').removeClass('d-none');
                $('#stickerPreviewContainer').addClass('d-none');
                $('#stickerInput').val('');
                console.log('Đã hiển thị ảnh preview');
            }
            reader.readAsDataURL(file);
        }
    });
    
    // Xóa ảnh đã chọn
    $('#removeImage').click(function() {
        $('#imageInput').val('');
        $('#imagePreviewContainer').addClass('d-none');
    });
    
    // Xóa sticker đã chọn
    $('#removeSticker').click(function() {
        $('#stickerInput').val('');
        $('#stickerPreviewContainer').addClass('d-none');
    });

    // Load stickers khi trang tải xong
    loadStickers();
});
