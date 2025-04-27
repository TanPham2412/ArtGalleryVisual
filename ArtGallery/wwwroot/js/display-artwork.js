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
    
    // Đóng modal sticker trước
    $('#stickerModal').modal('hide');
    
    // Kiểm tra target hiện tại
    if (window.currentStickerTarget) {
        if (window.currentStickerTarget.type === 'reply') {
            const commentId = window.currentStickerTarget.commentId;
            
            // Xử lý cho phản hồi
            $(`#stickerInputReply-${commentId}`).val(stickerPath);
            $(`#replyStickerPreview-${commentId} .preview-sticker`).attr('src', stickerPath);
            $(`#replyStickerPreview-${commentId}`).removeClass('d-none');
            $(`#replyImagePreview-${commentId}`).addClass('d-none');
        } 
        else if (window.currentStickerTarget.type === 'editComment') {
            // Xử lý cho sửa bình luận
            setTimeout(function() {
                // Mở lại modal sửa bình luận
                $('#editCommentModal').modal('show');
                
                // Khôi phục nội dung trước đó
                if (window.editCommentState) {
                    $('#editCommentId').val(window.editCommentState.commentId);
                    $('#editCommentContent').val(window.editCommentState.content);
                    $('#editCommentOriginalImage').val(window.editCommentState.originalImage);
                    $('#editCommentOriginalSticker').val(window.editCommentState.originalSticker);
                }
                
                // Cập nhật sticker mới nhưng không ảnh hưởng đến ảnh
                $('#editStickerInput').val(stickerPath);
                $('#editStickerPreview').attr('src', stickerPath);
                $('#editStickerPreviewContainer').removeClass('d-none');
            }, 500);
        }
        
        // Reset target sau khi đã chọn
        window.currentStickerTarget = null;
    } else {
        // Xử lý cho bình luận mới
        $('#stickerInput').val(stickerPath);
        $('#stickerPreview').attr('src', stickerPath);
        $('#stickerPreviewContainer').removeClass('d-none');
        
        // Trong trường hợp bình luận mới, vẫn giữ lại hành vi ẩn ảnh
        $('#imagePreviewContainer').addClass('d-none');
        $('#imageInput').val('');
    }
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

// Hiển thị modal sửa bình luận với sticker và ảnh
function showEditCommentModal(commentId, commentContent) {
    const comment = $(`#comment-${commentId}`);
    $('#editCommentId').val(commentId);
    $('#editCommentContent').val(commentContent);
    
    // Lấy thông tin sticker hiện tại nếu có
    const sticker = comment.find('.comment-sticker').attr('src');
    if (sticker) {
        $('#editStickerInput').val(sticker);
        $('#editStickerPreview').attr('src', sticker);
        $('#editStickerPreviewContainer').removeClass('d-none');
        $('#editCommentOriginalSticker').val(sticker);
    } else {
        $('#editStickerPreviewContainer').addClass('d-none');
        $('#editStickerInput').val('');
        $('#editCommentOriginalSticker').val('');
    }
    
    // Lấy thông tin ảnh hiện tại nếu có
    const image = comment.find('.comment-image').attr('src');
    if (image) {
        $('#editImagePreview').attr('src', image);
        $('#editImagePreviewContainer').removeClass('d-none');
        $('#editCommentOriginalImage').val(image);
    } else {
        $('#editImagePreviewContainer').addClass('d-none');
        $('#editCommentOriginalImage').val('');
    }
    
    // Hiển thị modal
    $('#editCommentModal').modal('show');
}

// Lưu bình luận đã chỉnh sửa bao gồm sticker và ảnh
function saveEditedComment() {
    const commentId = $('#editCommentId').val();
    const artworkId = $('#editArtworkId').val();
    const editedContent = $('#editCommentContent').val();
    const stickerPath = $('#editStickerInput').val();
    
    if (!editedContent.trim() && !stickerPath && !hasEditImageSelected()) {
        alert('Vui lòng nhập nội dung, chọn ảnh hoặc sticker');
        return;
    }
    
    // Tạo FormData để gửi cả dữ liệu và file
    const formData = new FormData();
    formData.append('commentId', commentId);
    formData.append('artworkId', artworkId);
    formData.append('editedContent', editedContent);
    formData.append('sticker', stickerPath);
    
    // Thêm ảnh nếu có
    const imageInput = document.getElementById('editImageInput');
    if (imageInput.files.length > 0) {
        formData.append('commentImage', imageInput.files[0]);
    }
    
    // Thêm thông tin có cần giữ lại ảnh cũ không
    formData.append('keepOriginalImage', !hasEditImageSelected() && $('#editCommentOriginalImage').val() ? 'true' : 'false');
    
    $.ajax({
        url: '/Artwork/EditComment',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                // Cập nhật giao diện
                location.reload(); // Cách đơn giản nhất là tải lại trang
                
                // Hoặc cập nhật từng thành phần trên UI (phức tạp hơn)
                /*
                $(`#comment-${commentId} .comment-text`).text(response.editedContent);
                
                // Cập nhật sticker nếu có
                if (response.sticker) {
                    if ($(`#comment-${commentId} .sticker-container`).length == 0) {
                        $(`#comment-${commentId} .comment-media-container`).prepend('<div class="sticker-container"><img class="comment-sticker"></div>');
                    }
                    $(`#comment-${commentId} .comment-sticker`).attr('src', response.sticker);
                    $(`#comment-${commentId} .sticker-container`).removeClass('d-none');
                } else {
                    $(`#comment-${commentId} .sticker-container`).addClass('d-none');
                }
                
                // Cập nhật ảnh nếu có
                if (response.imagePath) {
                    if ($(`#comment-${commentId} .comment-image-container`).length == 0) {
                        $(`#comment-${commentId} .comment-media-container`).append('<div class="comment-image-container"><img class="comment-image zoomable-image"></div>');
                    }
                    $(`#comment-${commentId} .comment-image`).attr('src', response.imagePath);
                    $(`#comment-${commentId} .comment-image-container`).removeClass('d-none');
                } else {
                    $(`#comment-${commentId} .comment-image-container`).addClass('d-none');
                }
                */
                
                // Đóng modal
                $('#editCommentModal').modal('hide');
            } else {
                alert(response.message);
            }
        },
        error: function() {
            alert('Có lỗi xảy ra khi sửa bình luận');
        }
    });
}

// Kiểm tra xem có đang chọn ảnh mới không
function hasEditImageSelected() {
    return $('#editImageInput').get(0).files.length > 0;
}

// Hiển thị modal sửa phản hồi
function showEditReplyModal(replyId, commentId, replyContent) {
    $('#editReplyId').val(replyId);
    $('#editReplyCommentId').val(commentId);
    $('#editReplyContent').val(replyContent);
    $('#editReplyModal').modal('show');
}

// Lưu phản hồi đã chỉnh sửa
function saveEditedReply() {
    const replyId = $('#editReplyId').val();
    const commentId = $('#editReplyCommentId').val();
    const artworkId = $('#editReplyArtworkId').val();
    const editedContent = $('#editReplyContent').val();
    
    if (!editedContent.trim()) {
        alert('Nội dung phản hồi không được để trống');
        return;
    }
    
    $.ajax({
        url: '/Artwork/EditReply',
        type: 'POST',
        data: {
            replyId: replyId,
            commentId: commentId,
            artworkId: artworkId,
            editedContent: editedContent
        },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                // Cập nhật nội dung phản hồi trên UI
                $(`#reply-${replyId} .reply-text`).text(response.editedContent);
                
                // Nếu phản hồi chưa có trạng thái "đã chỉnh sửa", thêm vào
                if (!$(`#reply-${replyId} .edited-marker`).length) {
                    $(`#reply-${replyId} .reply-username`).append('<span class="edited-marker">(đã chỉnh sửa)</span>');
                }
                
                // Đóng modal
                $('#editReplyModal').modal('hide');
                
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
            alert('Có lỗi xảy ra khi sửa phản hồi');
        }
    });
}

// Xóa phản hồi
function deleteReply(replyId, artworkId) {
    if (confirm('Bạn có chắc chắn muốn xóa phản hồi này?')) {
        $.ajax({
            url: '/Artwork/DeleteReply',
            type: 'POST',
            data: {
                replyId: replyId,
                artworkId: artworkId
            },
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    // Xóa phản hồi khỏi UI
                    $(`#reply-${replyId}`).fadeOut(300, function() {
                        $(this).remove();
                    });
                    
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
                alert('Có lỗi xảy ra khi xóa phản hồi');
            }
        });
    }
}

// Thêm vào phần có sẵn để hỗ trợ sticker và ảnh trong phản hồi
function openReplyImageSelector(commentId) {
    $(`#imageInputReply-${commentId}`).click();
}

function openReplyStickerSelector(commentId) {
    // Lưu thông tin rằng đang chọn sticker cho phản hồi, không phải bình luận mới
    window.currentStickerTarget = {
        type: 'reply',
        commentId: commentId
    };
    $('#stickerModal').modal('show');
}

function removeReplyImage(commentId) {
    $(`#imageInputReply-${commentId}`).val('');
    $(`#replyImagePreview-${commentId}`).addClass('d-none');
}

function removeReplySticker(commentId) {
    $(`#stickerInputReply-${commentId}`).val('');
    $(`#replyStickerPreview-${commentId}`).addClass('d-none');
}

// Thêm các event handlers vào document.ready
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
        // Xóa target trước đó (nếu có)
        window.currentStickerTarget = null;
        console.log('Opening sticker modal for new comment');
        $('#stickerModal').modal('show');
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

    // Xử lý ảnh cho phản hồi
    $('.imageInputReply').change(function() {
        const file = this.files[0];
        const commentId = this.id.split('-')[1];
        
        if (file) {
            const reader = new FileReader();
            reader.onload = function(e) {
                $(`#replyImagePreview-${commentId} .preview-img`).attr('src', e.target.result);
                $(`#replyImagePreview-${commentId}`).removeClass('d-none');
                $(`#replyStickerPreview-${commentId}`).addClass('d-none');
                $(`#stickerInputReply-${commentId}`).val('');
            }
            reader.readAsDataURL(file);
        }
    });

    // Xử lý nút sticker trong modal sửa bình luận
    $('#openEditStickerSelector').click(function() {
        // Lưu trạng thái hiện tại của modal sửa bình luận
        var commentModal = $('#editCommentModal');
        
        // Lưu nội dung và ID bình luận trước khi đóng modal
        window.editCommentState = {
            commentId: $('#editCommentId').val(),
            content: $('#editCommentContent').val(),
            originalImage: $('#editCommentOriginalImage').val(),
            originalSticker: $('#editCommentOriginalSticker').val()
        };
        
        // Đánh dấu trạng thái đang chọn sticker cho modal sửa
        window.currentStickerTarget = {
            type: 'editComment'
        };
        
        // Ẩn modal sửa bình luận (không đóng hẳn)
        commentModal.modal('hide');
        
        // Hiển thị modal sticker
        $('#stickerModal').modal('show');
    });
    
    // Xử lý nút chọn ảnh trong modal sửa bình luận
    $('#editImageInput').change(function() {
        const file = this.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function(e) {
                $('#editImagePreview').attr('src', e.target.result);
                $('#editImagePreviewContainer').removeClass('d-none');
                
                console.log('Đã hiển thị ảnh preview khi sửa');
            }
            reader.readAsDataURL(file);
        }
    });
    
    // Xóa ảnh đã chọn trong modal sửa
    $('#removeEditImage').click(function() {
        $('#editImageInput').val('');
        $('#editImagePreviewContainer').addClass('d-none');
        $('#editCommentOriginalImage').val(''); // Đánh dấu xóa ảnh gốc
    });
    
    // Xóa sticker đã chọn trong modal sửa
    $('#removeEditSticker').click(function() {
        $('#editStickerInput').val('');
        $('#editStickerPreviewContainer').addClass('d-none');
    });

    // Xử lý sự kiện đóng modal sticker
    $('#stickerModal').on('hidden.bs.modal', function () {
        // Nếu đang sửa bình luận, mở lại modal sửa bình luận
        if (window.currentStickerTarget && window.currentStickerTarget.type === 'editComment' && window.editCommentState) {
            setTimeout(function() {
                $('#editCommentModal').modal('show');
            }, 500);
        }
    });
});
