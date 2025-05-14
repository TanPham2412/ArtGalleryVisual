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

// Debug cho tag container
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM loaded');
    const tagContainer = document.querySelector('.tag-container');
    if (tagContainer) {
        console.log('Tag container exists');
        console.log('Visibility:', window.getComputedStyle(tagContainer).display);
    } else {
        console.log('Tag container not found');
    }
    
    // Xử lý modal khóa tài khoản
    if (document.getElementById('lockoutModal')) {
        // Đăng xuất người dùng trước khi hiển thị modal
        fetch('/Account/Logout', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            }
        }).then(() => {
            // Hiển thị modal khi trang tải xong
            var lockoutModal = new bootstrap.Modal(document.getElementById('lockoutModal'));
            lockoutModal.show();
        });
        
        // Đếm ngược thời gian
        var endTimeStr = document.getElementById('lockoutModal').getAttribute('data-lockout-end-time');
        if (endTimeStr) {
            var endTime = new Date(endTimeStr).getTime();
            
            var countdownTimer = setInterval(function() {
                var now = new Date().getTime();
                var distance = endTime - now;
                
                if (distance <= 0) {
                    clearInterval(countdownTimer);
                    document.getElementById('lockout-countdown').innerHTML = "Hết hạn. Vui lòng làm mới trang để đăng nhập lại.";
                    return;
                }
                
                var days = Math.floor(distance / (1000 * 60 * 60 * 24));
                var hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
                var minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
                var seconds = Math.floor((distance % (1000 * 60)) / 1000);
                
                document.getElementById('lockout-countdown').innerHTML = 
                    (days > 0 ? '<span style="color: #ff4444;">' + days + '</span> ngày ' : '') + 
                    '<span style="color: #ff4444;">' + hours + '</span> giờ ' +
                    '<span style="color: #ff4444;">' + minutes + '</span> phút ' +
                    '<span style="color: #ff4444;">' + seconds + '</span> giây';
            }, 1000);
        }
    }
});
