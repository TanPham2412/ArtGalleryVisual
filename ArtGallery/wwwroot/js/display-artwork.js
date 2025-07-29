// Hàm xử lý yêu thích
function toggleLike(button, artworkId) {
    $.ajax({
        url: '/Artwork/ToggleLike',
        type: 'POST',
        data: { artworkId: artworkId },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
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
        error: function () {
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
        success: function (response) {
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
        error: function () {
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
    const daisuhuynhPath = '/images/stickers/daisuhuynh/';
    const nhisuhuynhPath = '/images/stickers/nhisuhuynh/';
    const tamsuhuynhPath = '/images/stickers/tamsuhuynh/';
    const tusuhuynhPath = '/images/stickers/tusuhuynh/';
    const longtuongPath = '/images/stickers/longtuong/';
    const ngutieumaiPath = '/images/stickers/longtuong/';
    const thuyhanhPath = '/images/stickers/thuyhanh/';
    const vanthuongPath = '/images/stickers/vanthuong/';


    // Tạo 12 stickers mẫu cho mỗi thư mục
    let daisuhuynhHtml = '';
    for (let i = 1; i <= 12; i++) {
        daisuhuynhHtml += `<div class="sticker-item">
            <img src="${daisuhuynhPath}sticker${i}.png" data-path="${daisuhuynhPath}sticker${i}.png" 
                 class="sticker-img" onclick="selectSticker(this)">
        </div>`;
    }
    $('#daisuhuynh-stickers').html(daisuhuynhHtml);

    let nhisuhuynhHtml = '';
    for (let i = 1; i <= 12; i++) {
        nhisuhuynhHtml += `<div class="sticker-item">
            <img src="${nhisuhuynhPath}sticker${i}.png" data-path="${nhisuhuynhPath}sticker${i}.png" 
                 class="sticker-img" onclick="selectSticker(this)">
        </div>`;
    }
    $('#nhisuhuynh-stickers').html(nhisuhuynhHtml);

    let tamsuhuynhHtml = '';
    for (let i = 1; i <= 12; i++) {
        tamsuhuynhHtml += `<div class="sticker-item">
            <img src="${tamsuhuynhPath}sticker${i}.png" data-path="${tamsuhuynhPath}sticker${i}.png" 
                 class="sticker-img" onclick="selectSticker(this)">
        </div>`;
    }
    $('#tamsuhuynh-stickers').html(tamsuhuynhHtml);

    let tusuhuynhHtml = '';
    for (let i = 1; i <= 12; i++) {
        tusuhuynhHtml += `<div class="sticker-item">
            <img src="${tusuhuynhPath}sticker${i}.png" data-path="${tusuhuynhPath}sticker${i}.png" 
                 class="sticker-img" onclick="selectSticker(this)">
        </div>`;
    }
    $('#tusuhuynh-stickers').html(tusuhuynhHtml);

    let longtuongHtml = '';
    for (let i = 1; i <= 12; i++) {
        longtuongHtml += `<div class="sticker-item">
            <img src="${longtuongPath}sticker${i}.png" data-path="${longtuongPath}sticker${i}.png" 
                 class="sticker-img" onclick="selectSticker(this)">
        </div>`;
    }
    $('#longtuong-stickers').html(longtuongHtml);

    let ngutieumaiHtml = '';
    for (let i = 1; i <= 12; i++) {
        ngutieumaiHtml += `<div class="sticker-item">
            <img src="${ngutieumaiPath}sticker${i}.png" data-path="${ngutieumaiPath}sticker${i}.png" 
                 class="sticker-img" onclick="selectSticker(this)">
        </div>`;
    }
    $('#ngutieumai-stickers').html(ngutieumaiHtml);

    let thuyhanhHtml = '';
    for (let i = 1; i <= 12; i++) {
        thuyhanhHtml += `<div class="sticker-item">
            <img src="${thuyhanhPath}sticker${i}.png" data-path="${thuyhanhPath}sticker${i}.png" 
                 class="sticker-img" onclick="selectSticker(this)">
        </div>`;
    }
    $('#thuyhanh-stickers').html(thuyhanhHtml);

    let vanthuongHtml = '';
    for (let i = 1; i <= 12; i++) {
        vanthuongHtml += `<div class="sticker-item">
            <img src="${vanthuongPath}sticker${i}.png" data-path="${vanthuongPath}sticker${i}.png" 
                 class="sticker-img" onclick="selectSticker(this)">
        </div>`;
    }
    $('#vanthuong-stickers').html(vanthuongHtml);

    // Gọi API nếu hard-coded không hoạt động
    $.ajax({
        url: '/Artwork/GetStickers',
        type: 'GET',
        success: function (data) {
            console.log('Stickers loaded:', data);

            if (data.daisuhuynh && data.daisuhuynh.length > 0) {
                let daisuhuynhHtml = '';
                data.daisuhuynh.forEach(function (sticker) {
                    daisuhuynhHtml += `<div class="sticker-item">
                        <img src="${sticker}" data-path="${sticker}" class="sticker-img" onclick="selectSticker(this)">
                    </div>`;
                });
                $('#daisuhuynh-stickers').html(daisuhuynhHtml);
            }

            if (data.nhisuhuynh && data.nhisuhuynh.length > 0) {
                let nhisuhuynhHtml = '';
                data.nhisuhuynh.forEach(function (sticker) {
                    nhisuhuynhHtml += `<div class="sticker-item">
                        <img src="${sticker}" data-path="${sticker}" class="sticker-img" onclick="selectSticker(this)">
                    </div>`;
                });
                $('#nhisuhuynh-stickers').html(nhisuhuynhHtml);
            }

            if (data.tamsuhuynh && data.tamsuhuynh.length > 0) {
                let tamsuhuynhHtml = '';
                data.tamsuhuynh.forEach(function (sticker) {
                    tamsuhuynhHtml += `<div class="sticker-item">
                        <img src="${sticker}" data-path="${sticker}" class="sticker-img" onclick="selectSticker(this)">
                    </div>`;
                });
                $('#tamsuhuynh-stickers').html(tamsuhuynhHtml);
            }

            if (data.tusuhuynh && data.tusuhuynh.length > 0) {
                let tusuhuynhHtml = '';
                data.tusuhuynh.forEach(function (sticker) {
                    tusuhuynhHtml += `<div class="sticker-item">
                        <img src="${sticker}" data-path="${sticker}" class="sticker-img" onclick="selectSticker(this)">
                    </div>`;
                });
                $('#tusuhuynh-stickers').html(tusuhuynhHtml);
            }

            if (data.longtuong && data.longtuong.length > 0) {
                let longtuongHtml = '';
                data.longtuong.forEach(function (sticker) {
                    longtuongHtml += `<div class="sticker-item">
                        <img src="${sticker}" data-path="${sticker}" class="sticker-img" onclick="selectSticker(this)">
                    </div>`;
                });
                $('#longtuong-stickers').html(longtuongHtml);
            }

            if (data.ngutieumai && data.ngutieumai.length > 0) {
                let ngutieumaiHtml = '';
                data.ngutieumai.forEach(function (sticker) {
                    ngutieumaiHtml += `<div class="sticker-item">
                        <img src="${sticker}" data-path="${sticker}" class="sticker-img" onclick="selectSticker(this)">
                    </div>`;
                });
                $('#ngutieumai-stickers').html(ngutieumaiHtml);
            }

            if (data.thuyhanh && data.thuyhanh.length > 0) {
                let thuyhanhHtml = '';
                data.thuyhanh.forEach(function (sticker) {
                    thuyhanhHtml += `<div class="sticker-item">
                        <img src="${sticker}" data-path="${sticker}" class="sticker-img" onclick="selectSticker(this)">
                    </div>`;
                });
                $('#thuyhanh-stickers').html(thuyhanhHtml);
            }

            if (data.vanthuong && data.vanthuong.length > 0) {
                let vanthuongHtml = '';
                data.vanthuong.forEach(function (sticker) {
                    vanthuongHtml += `<div class="sticker-item">
                        <img src="${sticker}" data-path="${sticker}" class="sticker-img" onclick="selectSticker(this)">
                    </div>`;
                });
                $('#vanthuong-stickers').html(vanthuongHtml);
            }

        },
        error: function (err) {
            console.error('Error loading stickers:', err);
        }
    });
}

// Sửa lại hàm chọn sticker để tránh vấn đề với backdrop
function selectSticker(element) {
    console.log('Sticker selected:', $(element).data('path'));
    const stickerPath = $(element).data('path');

    // Lưu dữ liệu trước khi đóng modal
    const targetType = window.currentStickerTarget ? window.currentStickerTarget.type : null;
    const commentId = window.currentStickerTarget ? window.currentStickerTarget.commentId : null;
    
    // Lưu thông tin sticker đã chọn vào biến global
    window.selectedStickerPath = stickerPath;
    
    // Đóng modal theo cách thông thường
    try {
        // Tắt tất cả các modal đang mở
        $('.modal').modal('hide');
        
        // Xóa tất cả backdrop
        setTimeout(function() {
            document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
            document.body.classList.remove('modal-open');
            document.body.style.overflow = '';
            document.body.style.paddingRight = '';
            
            // Áp dụng sticker đã chọn
            applySelectedSticker();
        }, 300);
    } catch (error) {
        console.error("Lỗi khi đóng modal:", error);
        // Xử lý backup nếu phương thức thông thường thất bại
        document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
        document.body.classList.remove('modal-open');
        document.body.style.overflow = '';
        document.body.style.paddingRight = '';
        
        // Áp dụng sticker đã chọn
        applySelectedSticker();
    }
}

// Tách phần xử lý áp dụng sticker thành function riêng
function applySelectedSticker() {
    if (!window.selectedStickerPath) return;
    
    const stickerPath = window.selectedStickerPath;
    const targetType = window.currentStickerTarget ? window.currentStickerTarget.type : null;
    const commentId = window.currentStickerTarget ? window.currentStickerTarget.commentId : null;
    
    if (targetType === 'reply') {
        // Xử lý cho phản hồi mới
        $(`#stickerInputReply-${commentId}`).val(stickerPath);
        $(`#replyStickerPreview-${commentId} .preview-sticker`).attr('src', stickerPath);
        $(`#replyStickerPreview-${commentId}`).removeClass('d-none');
        $(`#replyImagePreview-${commentId}`).addClass('d-none');
    }
    else if (targetType === 'editComment' && window.editCommentState) {
        // Xử lý cho sửa bình luận
        setTimeout(function() {
            $('#editStickerInput').val(stickerPath);
            $('#editStickerPreview').attr('src', stickerPath);
            $('#editStickerPreviewContainer').removeClass('d-none');
            
            // Mở lại modal chỉnh sửa bình luận
            if ($('#editCommentModal').length) {
                $('#editCommentModal').modal('show');
            }
        }, 400);
    }
    else if (targetType === 'editReply' && window.editReplyState) {
        // Xử lý cho sửa phản hồi
        setTimeout(function() {
            $('#editReplyStickerInput').val(stickerPath);
            $('#editReplyStickerPreview').attr('src', stickerPath);
            $('#editReplyStickerPreviewContainer').removeClass('d-none');
            
            // Mở lại modal chỉnh sửa phản hồi
            if ($('#editReplyModal').length) {
                $('#editReplyModal').modal('show');
            }
        }, 400);
    }
    else {
        // Xử lý cho bình luận mới
        $('#stickerInput').val(stickerPath);
        $('#stickerPreview').attr('src', stickerPath);
        $('#stickerPreviewContainer').removeClass('d-none');
        $('#imagePreviewContainer').addClass('d-none');
        $('#imageInput').val('');
    }
    
    // Reset các biến global
    window.currentStickerTarget = null;
    window.selectedStickerPath = null;
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

// Sửa lại hàm xử lý xóa bình luận
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
            success: function (response) {
                if (response.success) {
                    // Hiển thị thông báo nhỏ thay vì SweetAlert
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

                    // Xóa bình luận khỏi DOM với hiệu ứng mờ dần thay vì load lại trang
                    $(`#comment-${commentId}`).fadeOut(300, function () {
                        $(this).remove();

                        // Cập nhật số lượng bình luận trong tiêu đề
                        const commentCount = $('.comment-item').length;
                        $('.comments-list h4').text(`Tất cả bình luận (${commentCount})`);
                    });
                } else {
                    alert(response.message);
                }
            },
            error: function () {
                alert('Có lỗi xảy ra khi xóa bình luận');
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
        success: function (response) {
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
        error: function () {
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

    // Xử lý sticker: nếu sticker input trống và container bị ẩn, có nghĩa là user đã xóa sticker
    const sticker = $('#editStickerInput').val();
    const keepSticker = ($('#editStickerPreviewContainer').hasClass('d-none') === false) &&
        (!sticker && $('#editCommentOriginalSticker').val());

    // Xử lý ảnh: kiểm tra file mới hoặc ảnh gốc (nếu còn hiển thị)
    const imageFile = $('#editImageInput').prop('files')[0];
    const keepOriginalImage = ($('#editImagePreviewContainer').hasClass('d-none') === false) &&
        (!imageFile && $('#editCommentOriginalImage').val());

    // Kiểm tra nếu nội dung trống và không có ảnh, sticker
    if (!editedContent.trim() && !imageFile && !keepOriginalImage && !sticker && !keepSticker) {
        alert('Vui lòng nhập nội dung, chọn ảnh hoặc sticker');
        return;
    }

    // Tạo FormData để gửi cả dữ liệu và file
    const formData = new FormData();
    formData.append('commentId', commentId);
    formData.append('artworkId', artworkId);
    formData.append('editedContent', editedContent);

    // Thêm thông tin về sticker
    if (sticker) {
        formData.append('sticker', sticker);
    } else if (keepSticker) {
        formData.append('sticker', $('#editCommentOriginalSticker').val());
    } else {
        formData.append('sticker', ''); // Sticker bị xóa hoặc không có
    }

    // Thêm ảnh nếu có
    if (imageFile) {
        formData.append('commentImage', imageFile);
    } else if (keepOriginalImage) {
        formData.append('keepOriginalImage', 'true');
    }

    $.ajax({
        url: '/Artwork/EditComment',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                $('#editCommentModal').modal('hide');
                location.reload();
            } else {
                alert(response.message);
            }
        },
        error: function () {
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
    // Lấy thông tin phản hồi từ server trước khi hiển thị modal
    $.ajax({
        url: '/Reply/GetReplyInfo',
        type: 'GET',
        data: { replyId: replyId },
        success: function (response) {
            if (response.success) {
                $('#editReplyId').val(replyId);
                $('#editReplyContent').val(response.content || replyContent);
                $('#editCommentId').val(commentId);
                $('#editArtworkId').val($('#artworkId').val());

                // Lưu thông tin gốc để xử lý khi không có thay đổi
                $('#editReplyOriginalImage').val(response.imagePath || '');
                $('#editReplyOriginalSticker').val(response.sticker || '');

                // Hiển thị ảnh và sticker nếu có
                if (response.imagePath) {
                    $('#editReplyImagePreview').attr('src', response.imagePath);
                    $('#editReplyImagePreviewContainer').removeClass('d-none');
                } else {
                    $('#editReplyImagePreviewContainer').addClass('d-none');
                }

                if (response.sticker) {
                    $('#editReplyStickerPreview').attr('src', response.sticker);
                    $('#editReplyStickerPreviewContainer').removeClass('d-none');
                } else {
                    $('#editReplyStickerPreviewContainer').addClass('d-none');
                }

                // Hiển thị modal
                $('#editReplyModal').modal('show');
            } else {
                alert('Không thể tải thông tin phản hồi');
            }
        },
        error: function () {
            // Fallback nếu không lấy được thông tin chi tiết
            $('#editReplyId').val(replyId);
            $('#editReplyContent').val(replyContent);
            $('#editReplyModal').modal('show');
        }
    });
}

// Thêm hàm xử lý xóa ảnh trong modal sửa phản hồi
$(document).on('click', '#removeEditReplyImage', function () {
    $('#editReplyImageInput').val(''); // Xóa file đã chọn nếu có
    $('#editReplyImagePreviewContainer').addClass('d-none'); // Ẩn container preview
    $('#editReplyOriginalImage').val(''); // Đánh dấu là đã xóa ảnh gốc
});

// Thêm hàm xử lý xóa sticker trong modal sửa phản hồi
$(document).on('click', '#removeEditReplySticker', function () {
    $('#editReplyStickerInput').val(''); // Xóa sticker đã chọn
    $('#editReplyStickerPreviewContainer').addClass('d-none'); // Ẩn container preview
    $('#editReplyOriginalSticker').val(''); // Đánh dấu là đã xóa sticker gốc
});

// Sửa lại hàm saveEditReply để xử lý đúng trường hợp xóa ảnh/sticker
$('#saveEditReply').click(function () {
    const replyId = $('#editReplyId').val();
    const content = $('#editReplyContent').val();
    const commentId = window.editReplyState?.commentId || 0;
    const artworkId = $('#editArtworkId').val();

    // Xử lý sticker: nếu sticker input trống và container bị ẩn, có nghĩa là user đã xóa sticker
    const sticker = $('#editReplyStickerInput').val();
    const keepSticker = ($('#editReplyStickerPreviewContainer').hasClass('d-none') === false) &&
        (!sticker && $('#editReplyOriginalSticker').val());

    // Xử lý ảnh: kiểm tra file mới hoặc ảnh gốc (nếu còn hiển thị)
    const imageFile = $('#editReplyImageInput').prop('files')[0];
    const keepOriginalImage = ($('#editReplyImagePreviewContainer').hasClass('d-none') === false) &&
        (!imageFile && $('#editReplyOriginalImage').val());

    const formData = new FormData();
    formData.append('replyId', replyId);
    formData.append('commentId', commentId);
    formData.append('artworkId', artworkId);
    formData.append('editedContent', content);

    // Thêm thông tin về sticker
    if (sticker) {
        formData.append('sticker', sticker);
    } else if (keepSticker) {
        formData.append('sticker', $('#editReplyOriginalSticker').val());
    } else {
        formData.append('sticker', ''); // Sticker bị xóa hoặc không có
    }

    // Thêm thông tin về ảnh
    if (imageFile) {
        formData.append('replyImage', imageFile);
    } else if (keepOriginalImage) {
        formData.append('keepOriginalImage', 'true');
    }

    // Kiểm tra nếu nội dung trống và không có ảnh, sticker
    if (!content.trim() && !imageFile && !keepOriginalImage && !sticker && !keepSticker) {
        alert('Vui lòng nhập nội dung, chọn ảnh hoặc sticker');
        return;
    }

    $.ajax({
        url: '/Reply/EditReply',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                $('#editReplyModal').modal('hide');
                // Cập nhật UI phản hồi hoặc tải lại trang
                location.reload();
            } else {
                alert('Có lỗi xảy ra: ' + response.message);
            }
        },
        error: function () {
            alert('Có lỗi xảy ra khi kết nối đến máy chủ');
        }
    });
});

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
$(document).ready(function () {
    // Xử lý rating stars
    $('.rating-stars i').on('click', function () {
        const rating = $(this).data('rating');
        $('#selectedRating').val(rating);

        // Reset tất cả các sao về trạng thái chưa chọn
        $('.rating-stars i').removeClass('fas').addClass('far');

        // Thêm class active cho các sao được chọn
        $('.rating-stars i').each(function () {
            if ($(this).data('rating') <= rating) {
                $(this).removeClass('far').addClass('fas');
            }
        });
    });

    // Hiệu ứng hover
    $('.rating-stars i').on('mouseenter', function () {
        const hoverRating = $(this).data('rating');

        // Tô màu các sao khi hover
        $('.rating-stars i').each(function () {
            if ($(this).data('rating') <= hoverRating) {
                $(this).addClass('hover');
            } else {
                $(this).removeClass('hover');
            }
        });
    }).on('mouseleave', function () {
        // Xóa hover khi di chuột ra khỏi
        $('.rating-stars i').removeClass('hover');
    });

    // Sự kiện mở ảnh sản phẩm
    const artworkImage = document.querySelector('.art-image img');
    if (artworkImage) {
        artworkImage.addEventListener('click', function () {
            // Thiết lập src cho ảnh trong modal
            document.getElementById('modalImage').src = this.src;
            // Mở modal
            openArtworkModal();
        });
    }

    // Đóng modal khi click bên ngoài ảnh
    const modal = document.getElementById('artworkModal');
    if (modal) {
        modal.addEventListener('click', function (event) {
            if (event.target === modal) {
                closeArtworkModal();
            }
        });
    }

    // Xử lý sự kiện cho modal ảnh bình luận
    $('#commentImageModal').on('click', function (event) {
        if (event.target === this) {
            closeImageModal();
        }
    });

    // Sử dụng phím Esc để đóng modal
    $(document).keydown(function (event) {
        if (event.keyCode === 27) { // Phím Esc
            closeImageModal();
            closeArtworkModal();
        }
    });

    // Xử lý nút sticker
    $('#openStickerSelector').click(function () {
        // Xóa target trước đó (nếu có)
        window.currentStickerTarget = null;
        console.log('Opening sticker modal for new comment');
        $('#stickerModal').modal('show');
    });

    // Xử lý nút chọn ảnh
    $('#imageInput').change(function () {
        const file = this.files[0];
        console.log('Đã chọn file:', file);
        if (file) {
            const reader = new FileReader();
            reader.onload = function (e) {
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
    $('#removeImage').click(function () {
        $('#imageInput').val('');
        $('#imagePreviewContainer').addClass('d-none');
    });

    // Xóa sticker đã chọn
    $('#removeSticker').click(function () {
        $('#stickerInput').val('');
        $('#stickerPreviewContainer').addClass('d-none');
    });

    // Load stickers khi trang tải xong
    loadStickers();

    // Xử lý ảnh cho phản hồi
    $('.imageInputReply').change(function () {
        const file = this.files[0];
        const commentId = this.id.split('-')[1];

        if (file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                $(`#replyImagePreview-${commentId} .preview-img`).attr('src', e.target.result);
                $(`#replyImagePreview-${commentId}`).removeClass('d-none');
                $(`#replyStickerPreview-${commentId}`).addClass('d-none');
                $(`#stickerInputReply-${commentId}`).val('');
            }
            reader.readAsDataURL(file);
        }
    });

    // Xử lý nút sticker trong modal sửa bình luận
    $('#openEditStickerSelector').click(function () {
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
    $('#editImageInput').change(function () {
        const file = this.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                $('#editImagePreview').attr('src', e.target.result);
                $('#editImagePreviewContainer').removeClass('d-none');

                console.log('Đã hiển thị ảnh preview khi sửa');
            }
            reader.readAsDataURL(file);
        }
    });

    // Xóa ảnh đã chọn trong modal sửa
    $('#removeEditImage').click(function () {
        $('#editImageInput').val('');
        $('#editImagePreviewContainer').addClass('d-none');
        $('#editCommentOriginalImage').val(''); // Đánh dấu xóa ảnh gốc
    });

    // Xóa sticker đã chọn trong modal sửa
    $('#removeEditSticker').click(function () {
        $('#editStickerInput').val('');
        $('#editStickerPreviewContainer').addClass('d-none');
    });

    // Xử lý sự kiện đóng modal sticker
    $('#stickerModal').on('hidden.bs.modal', function () {
        // Nếu đang sửa phản hồi, mở lại modal sửa phản hồi
        if (window.currentStickerTarget) {
            if (window.currentStickerTarget.type === 'editComment' && window.editCommentState) {
                setTimeout(function () {
                    $('#editCommentModal').modal('show');
                }, 500);
            }
            else if (window.currentStickerTarget.type === 'editReply' && window.editReplyState) {
                setTimeout(function () {
                    $('#editReplyModal').modal('show');
                }, 500);
            }
        }
    });

    // Thêm hàm xử lý khi nhấn nút sticker trong modal sửa phản hồi
    $(document).on('click', '#openEditReplySticker', function () {
        // Lưu trạng thái hiện tại của modal sửa phản hồi
        var replyModal = $('#editReplyModal');

        // Lưu nội dung và ID phản hồi trước khi đóng modal
        window.editReplyState = {
            replyId: $('#editReplyId').val(),
            content: $('#editReplyContent').val(),
            originalImage: $('#editReplyOriginalImage').val(),
            originalSticker: $('#editReplyOriginalSticker').val()
        };

        // Đánh dấu trạng thái đang chọn sticker cho modal sửa phản hồi
        window.currentStickerTarget = {
            type: 'editReply'
        };

        // Ẩn modal sửa phản hồi (không đóng hẳn)
        replyModal.modal('hide');

        // Hiển thị modal sticker
        $('#stickerModal').modal('show');
    });

    // Thêm xử lý sự kiện khi upload ảnh trong modal sửa phản hồi
    $('#editReplyImageInput').change(function () {
        const file = this.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                $('#editReplyImagePreview').attr('src', e.target.result);
                $('#editReplyImagePreviewContainer').removeClass('d-none');

                // Không ẩn sticker khi chọn ảnh
                // $('#editReplyStickerPreviewContainer').addClass('d-none');
                // $('#editReplyStickerInput').val('');
            }
            reader.readAsDataURL(file);
        }
    });

    // Xử lý sự kiện nút mở file explorer cho ảnh
    $('#openEditReplyImage').click(function () {
        $('#editReplyImageInput').click();
    });

    // Thêm xử lý nút mở file explorer cho ảnh trong modal sửa bình luận
    $('#openEditImage').click(function () {
        $('#editImageInput').click();
    });

    // Chuyển sự kiện lưu bình luận thành jQuery
    $('#saveEditComment').click(function () {
        saveEditedComment();
    });

    // Xử lý khi modal đóng
    $(document).on('hidden.bs.modal', '.modal', function() {
        // Xóa tất cả backdrop và reset body
        $('.modal-backdrop').remove();
        $('body').removeClass('modal-open');
        $('body').css('padding-right', '');
        $('body').attr('style', '');
    });
    
    // Thêm xử lý khi ấn Esc
    $(document).keydown(function(e) {
        if (e.key === "Escape" && $('.modal-backdrop').length > 0) {
            $('.modal').modal('hide');
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open');
            $('body').css('padding-right', '');
            $('body').attr('style', '');
        }
    });
});

function deleteReply(replyId, artworkId) {
    if (confirm('Bạn có chắc chắn muốn xóa phản hồi này?')) {
        $.ajax({
            url: '/Reply/DeleteReply',
            type: 'POST',
            data: {
                replyId: replyId,
                artworkId: artworkId
            },
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    // Hiển thị thông báo thành công
                    alert(response.message);
                    // Xóa phần tử khỏi DOM hoặc tải lại trang
                    $(`#reply-${replyId}`).fadeOut(300, function () {
                        $(this).remove();
                    });
                } else {
                    // Hiển thị thông báo lỗi
                    alert(response.message);
                }
            },
            error: function () {
                alert('Có lỗi xảy ra khi kết nối đến máy chủ');
            }
        });
    }
}

// Lưu vị trí scroll
function saveScrollPosition() {
    sessionStorage.setItem('scrollPosition', window.scrollY);
}

// Khôi phục vị trí scroll
function restoreScrollPosition() {
    if (sessionStorage.getItem('scrollPosition') !== null) {
        window.scrollTo(0, sessionStorage.getItem('scrollPosition'));
        sessionStorage.removeItem('scrollPosition');
    }
}

// Kiểm tra form trước khi submit
document.addEventListener('DOMContentLoaded', function () {
    // Kiểm tra form bình luận chính
    const commentForm = document.getElementById('commentForm');
    if (commentForm) {
        commentForm.addEventListener('submit', function (e) {
            // Ngăn form submit ngay lập tức để kiểm tra
            e.preventDefault();

            const content = document.getElementById('commentContent').value.trim();
            const imageInput = document.getElementById('imageInput');
            const stickerInput = document.getElementById('stickerInput').value;

            if (content === '' && (!imageInput || !imageInput.files.length) && !stickerInput) {
                alert('Vui lòng nhập nội dung, chọn ảnh hoặc sticker');
                return false;
            }

            // Lưu vị trí scroll
            const scrollPosition = window.scrollY || window.pageYOffset;
            localStorage.setItem('scrollPosition', scrollPosition);

            // Cho phép form submit nếu dữ liệu hợp lệ
            commentForm.submit();
        });
    }

    // Khôi phục vị trí scroll sau khi trang tải lại
    if (localStorage.getItem('scrollPosition')) {
        const savedPosition = localStorage.getItem('scrollPosition');
        window.scrollTo(0, savedPosition);
        localStorage.removeItem('scrollPosition');
    }

    // Xử lý tương tự cho các form phản hồi
    const replyForms = document.querySelectorAll('.reply-form');
    replyForms.forEach(form => {
        form.addEventListener('submit', function (e) {
            e.preventDefault();

            const replyInput = form.querySelector('.reply-input').value.trim();
            const imageInput = form.querySelector('.imageInputReply');
            const stickerInput = form.querySelector('.stickerInputReply').value;

            if (replyInput === '' && (!imageInput || !imageInput.files.length) && !stickerInput) {
                alert('Vui lòng nhập nội dung, chọn ảnh hoặc sticker');
                return false;
            }

            // Lấy dữ liệu từ form
            const formData = new FormData(form);
            const commentId = form.querySelector('input[name="MaBinhLuan"]').value;
            const artworkId = form.querySelector('input[name="MaTranh"]').value;
            
            // Gửi dữ liệu bằng AJAX
            $.ajax({
                url: '/Artwork/AddReply',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function(response) {
                    if (response.success) {
                        // Ẩn form phản hồi sau khi gửi thành công
                        hideReplyForm(commentId);
                        
                        // Xóa nội dung form
                        form.querySelector('.reply-input').value = '';
                        if (imageInput) imageInput.value = '';
                        form.querySelector('.stickerInputReply').value = '';
                        
                        // Ẩn preview nếu có
                        const imagePreview = document.getElementById(`replyImagePreview-${commentId}`);
                        const stickerPreview = document.getElementById(`replyStickerPreview-${commentId}`);
                        if (imagePreview) imagePreview.classList.add('d-none');
                        if (stickerPreview) stickerPreview.classList.add('d-none');
                        
                        // Không cần làm gì thêm vì SignalR sẽ nhận sự kiện và hiển thị phản hồi mới
                    } else {
                        alert('Có lỗi xảy ra khi gửi phản hồi: ' + response.message);
                    }
                },
                error: function() {
                    alert('Có lỗi xảy ra khi gửi phản hồi');
                }
            });
        });
    });

});

// Thêm vào cuối file nếu cần
function showEditReplyModal(replyId, commentId, replyContent) {
    // Thêm data attribute để lưu commentId
    $('#editReplyForm').data('comment-id', commentId);

    $('#editReplyId').val(replyId);
    $('#editReplyContent').val(replyContent);

    const editReplyModal = new bootstrap.Modal(document.getElementById('editReplyModal'));
    editReplyModal.show();
}

// Thêm vào cuối file hoặc trong $(document).ready
$(document).on('hidden.bs.modal', '.modal', function () {
    $('.modal-backdrop').remove();
    $('body').removeClass('modal-open');
    $('body').css('padding-right', '');
});

function closeModalManually() {
    $('#stickerModal').modal('hide');
    
    setTimeout(function() {
        $('.modal-backdrop').remove();
        $('body').removeClass('modal-open');
        $('body').css('padding-right', '');
        $('body').attr('style', '');
    }, 100);
}

// Thêm sự kiện này vào document.ready
$(document).ready(function() {
    // Sửa lại cách xử lý modal
    $(document).on('hide.bs.modal', '.modal', function() {
        // Đánh dấu modal đang đóng để tránh xung đột
        window.modalClosing = true;
        
        setTimeout(function() {
            window.modalClosing = false;
        }, 500);
    });
    
    $(document).on('hidden.bs.modal', '.modal', function() {
        // Đảm bảo xóa mọi backdrop sau khi modal đóng
        document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
        document.body.classList.remove('modal-open');
        document.body.style.overflow = '';
        document.body.style.paddingRight = '';
    });
    
    // Phím tắt Escape để đóng modal và xóa backdrop
    $(document).keydown(function(e) {
        if (e.key === "Escape") {
            $('.modal').modal('hide');
            setTimeout(function() {
                document.querySelectorAll('.modal-backdrop').forEach(el => el.remove());
                document.body.classList.remove('modal-open');
                document.body.style.overflow = '';
                document.body.style.paddingRight = '';
            }, 300);
        }
    });
});