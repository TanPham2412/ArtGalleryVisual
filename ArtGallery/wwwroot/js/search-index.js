document.addEventListener('DOMContentLoaded', function () {
    // Xử lý nút search option
    const searchOptionBtn = document.querySelector('.search-option-btn');
    if (searchOptionBtn) {
        searchOptionBtn.addEventListener('click', function () {
            alert('Tính năng đang phát triển!');
        });
    }

    // Đảm bảo scroll container hoạt động tốt
    const searchTag = document.querySelector('.search-tag');
    if (searchTag) {
        // Tìm tab active
        const activeTag = searchTag.querySelector('.search-tag-items.active');
        if (activeTag) {
            // Scroll đến tab active
            setTimeout(() => {
                activeTag.scrollIntoView({ behavior: 'smooth', block: 'nearest', inline: 'center' });
            }, 100);
        }
    }

    // Thêm event listener cho nút theo dõi 
    document.querySelectorAll('.follow-button-primary').forEach(button => {
        button.addEventListener('click', function (event) {
            event.preventDefault();
            const artistId = this.getAttribute('data-artist-id');
            if (artistId) {
                toggleFollow(event, artistId);
            }
        });
    });
});

// Biến để theo dõi trạng thái request đang xử lý
const pendingRequests = {};

// Hàm toggle Follow cập nhật để xử lý nhiều lần nhấn liên tiếp
function toggleFollow(event, artistId) {
    event.preventDefault();

    const button = event.currentTarget;
    const isFollowing = button.classList.contains('following');

    // Nếu đang có request cho artist này, không làm gì cả
    if (pendingRequests[artistId]) return;

    // Đánh dấu đang có request cho artist này
    pendingRequests[artistId] = true;

    // Hiệu ứng tức thì nhưng không block nút
    if (isFollowing) {
        button.classList.remove('following');
    } else {
        button.classList.add('following');
    }

    fetch('/Search/ToggleFollow', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: 'artistId=' + encodeURIComponent(artistId)
    })
        .then(response => response.json())
        .then(data => {
            // Giải phóng lock cho artist này
            delete pendingRequests[artistId];

            if (!data.success) {
                // Đảo ngược trạng thái nếu thất bại
                if (isFollowing) {
                    button.classList.add('following');
                } else {
                    button.classList.remove('following');
                }

                if (data.message && data.message.includes("đăng nhập")) {
                    window.location.href = '/Identity/Account/Login';
                } else {
                    console.error('Lỗi:', data.message);
                }
            }
        })
        .catch(error => {
            // Giải phóng lock cho artist này trong trường hợp lỗi
            delete pendingRequests[artistId];

            console.error('Lỗi khi gọi API:', error);
            // Đảo ngược trạng thái nếu có lỗi
            if (isFollowing) {
                button.classList.add('following');
            } else {
                button.classList.remove('following');
            }
        });
}

// Thay thế toàn bộ hàm toggleLike bằng version đơn giản hơn này
function toggleLike(button, artworkId) {
    // Chặn hành vi mặc định của button
    event.stopPropagation();
    
    // Thay đổi UI ngay lập tức
    const icon = button.querySelector('i');
    if (icon.classList.contains('far')) {
        icon.classList.remove('far');
        icon.classList.add('fas');
        button.classList.add('active');
    } else {
        icon.classList.remove('fas');
        icon.classList.add('far');
        button.classList.remove('active');
    }
    
    // Thêm hiệu ứng animation
    icon.style.animation = 'none';
    setTimeout(() => {
        icon.style.animation = '';
    }, 10);
    
    // Gửi request đến server 
    fetch('/Artwork/ToggleLike', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: 'artworkId=' + artworkId
    })
    .then(response => response.json())
    .then(data => {
        console.log('Like toggled successfully', data);
    })
    .catch(error => {
        console.error('Error toggling like:', error);
    });
}

// Hàm cập nhật tất cả các nút like cho cùng một ảnh
function updateAllLikeButtons(artworkId, isLiked) {
    // Tìm tất cả các nút like có data-artwork-id = artworkId
    const allLikeButtons = document.querySelectorAll(`[data-artwork-id="${artworkId}"]`);

    allLikeButtons.forEach(btn => {
        const icon = btn.querySelector('i');

        if (isLiked) {
            icon.classList.remove('far');
            icon.classList.add('fas');
            btn.classList.add('active');
        } else {
            icon.classList.remove('fas');
            icon.classList.add('far');
            btn.classList.remove('active');
        }
    });
}

// Khi trang tải, đảm bảo trạng thái like được cập nhật chính xác
document.addEventListener('DOMContentLoaded', function () {
    // Đảm bảo jQuery được load trước khi sử dụng
    if (typeof $ === 'undefined') {
        console.error('jQuery chưa được load!');
    } else {
        console.log('jQuery đã sẵn sàng');
    }
});

// Thêm đoạn code này để giữ tag khi chuyển trang
$(document).ready(function() {
    // Lấy tag từ URL hiện tại nếu có
    const urlParams = new URLSearchParams(window.location.search);
    const tagParam = urlParams.get('tag');
    
    // Đảm bảo tất cả các link thể loại đều giữ tag
    if (tagParam) {
        $('a.search-tag-items').each(function() {
            let href = $(this).attr('href');
            if (href.indexOf('tag=') === -1) {
                href += (href.indexOf('?') !== -1) ? '&' : '?';
                href += 'tag=' + encodeURIComponent(tagParam);
                $(this).attr('href', href);
            }
        });
    }
});