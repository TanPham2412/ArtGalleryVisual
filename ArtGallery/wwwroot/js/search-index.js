document.addEventListener('DOMContentLoaded', function () {
    // Xử lý nút search option
    const searchOptionBtn = document.querySelector('.search-option-btn');
    if (searchOptionBtn) {
        searchOptionBtn.addEventListener('click', function () {
            alert('Tính năng đang phát triển!');
        });
    }

    // Đảm bảo scroll container hoạt động tốt
    const tag = document.querySelector('.tag');
    if (tag) {
        // Tìm tab active
        const activeTag = tag.querySelector('.tag-items.active');
        if (activeTag) {
            // Scroll đến tab active
            setTimeout(() => {
                activeTag.scrollIntoView({ behavior: 'smooth', block: 'nearest', inline: 'center' });
            }, 100);
        }
    }
});

// Hàm toggle Follow
function toggleFollow(button, artistId) {
    const isFollowing = button.classList.contains('following');
    
    $.ajax({
        url: '/Search/ToggleFollow',
        type: 'POST',
        data: { artistId: artistId },
        headers: {
            'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                if (response.following) {
                    button.textContent = "Following";
                    button.classList.add("following");
                } else {
                    button.textContent = "Follow";
                    button.classList.remove("following");
                }
            } else {
                // Nếu cần đăng nhập
                if (response.message.includes("đăng nhập")) {
                    window.location.href = '/Identity/Account/Login';
                } else {
                    alert(response.message);
                }
            }
        },
        error: function () {
            alert('Có lỗi xảy ra khi thực hiện thao tác theo dõi');
        }
    });
}

// Hàm toggle Like
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
                    icon.classList.remove('far');
                    icon.classList.add('fas');
                    
                    // Cập nhật số lượng like nếu có hiển thị
                    const likeCountElem = button.closest('.artwork-overlay').querySelector('.like-count-badge span');
                    if (likeCountElem) {
                        likeCountElem.textContent = parseInt(likeCountElem.textContent) + 1;
                    }
                } else {
                    icon.classList.remove('fas');
                    icon.classList.add('far');
                    
                    // Cập nhật số lượng like nếu có hiển thị
                    const likeCountElem = button.closest('.artwork-overlay').querySelector('.like-count-badge span');
                    if (likeCountElem) {
                        const currentCount = parseInt(likeCountElem.textContent);
                        likeCountElem.textContent = currentCount > 0 ? currentCount - 1 : 0;
                    }
                }
            } else {
                // Nếu cần đăng nhập
                if (response.message && response.message.includes("đăng nhập")) {
                    window.location.href = '/Identity/Account/Login';
                } else {
                    alert(response.message || 'Có lỗi xảy ra');
                }
            }
        },
        error: function () {
            alert('Có lỗi xảy ra khi thực hiện thao tác like');
        }
    });
}