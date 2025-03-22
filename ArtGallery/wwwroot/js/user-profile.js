function toggleFollow(event, userId) {
    event.preventDefault();
    
    const button = event.currentTarget;
    const isFollowing = button.classList.contains('following');
    
    // Hiệu ứng tức thì
    if (isFollowing) {
        button.classList.remove('following');
    } else {
        button.classList.add('following');
    }

    // Gọi API
    fetch('/Home/ToggleFollow', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify(userId)
    })
    .then(response => response.json())
    .then(data => {
        if (!data.success) {
            // Đảo ngược trạng thái nếu thất bại
            if (isFollowing) {
                button.classList.add('following');
            } else {
                button.classList.remove('following');
            }
            console.error('Lỗi:', data.message);
        } else {
            // Cập nhật số lượng người theo dõi nếu cần
            updateFollowerCount(data.followerCount);
        }
    })
    .catch(error => {
        console.error('Error:', error);
        // Đảo ngược trạng thái nếu có lỗi
        if (isFollowing) {
            button.classList.add('following');
        } else {
            button.classList.remove('following');
        }
    });
}

function updateFollowerCount(count) {
    // Cập nhật hiển thị số người theo dõi
    const followerCountElements = document.querySelectorAll('.profile-stat-item:last-child');
    if (followerCountElements.length > 0) {
        followerCountElements.forEach(element => {
            element.textContent = `${count} Người theo dõi`;
        });
    }
}