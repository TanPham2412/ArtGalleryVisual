document.getElementById('userDropdown')?.addEventListener('click', function (e) {
    e.preventDefault();
    document.getElementById('userDropdownMenu').classList.toggle('show');
});

document.addEventListener('click', function (e) {
    if (!e.target.closest('.user-dropdown')) {
        document.getElementById('userDropdownMenu')?.classList.remove('show');
    }
});

let searchTimeout;
const searchInput = document.getElementById('searchInput');
const searchResults = document.getElementById('searchResults');

if (searchInput && searchResults) {
    searchInput.addEventListener('input', function () {
        clearTimeout(searchTimeout);
        const query = this.value.trim();

        if (query.length === 0) {
            searchResults.style.display = 'none';
            return;
        }

        searchTimeout = setTimeout(() => {
            fetch(`/Home/SearchUsers?query=${encodeURIComponent(query)}`)
                .then(response => response.json())
                .then(users => {
                    if (users.length > 0) {
                        searchResults.innerHTML = users.map(user => {
                            const avatarPath = user.anhDaiDien
                                ? `/images/authors/avatars/${user.tenDangNhap}/${user.anhDaiDien}`
                                : '/images/authors/default/default-image.png';

                            return `
                            <div class="search-result-item">
                                <a href="/User/Profile/${user.maNguoiDung}" class="d-flex align-items-center text-decoration-none flex-grow-1">
                                    <div class="search-result-avatar">
                                        <img src="${avatarPath}"
                                             alt="${user.tenNguoiDung}"
                                             onerror="this.src='/images/authors/default/default-image.png'"
                                             style="width: 40px; height: 40px; border-radius: 50%; object-fit: cover;">
                                    </div>
                                    <div class="search-result-user-info ms-2">
                                        <div class="search-result-name">${user.tenNguoiDung || 'Người dùng'}</div>
                                        <div class="search-result-username text-muted">${user.tenDangNhap || ''}</div>
                                    </div>
                                </a>
                                <button class="follow-button ${user.daTheoDoi ? 'following' : ''}"
                                        onclick="toggleFollow(event, '${user.maNguoiDung}')">
                                    ${user.daTheoDoi ? 'Đang theo dõi' : 'Theo dõi'}
                                </button>
                            </div>`;
                        }).join('');
                        searchResults.style.display = 'block';
                    } else {
                        searchResults.innerHTML = '<div class="p-3">Không tìm thấy kết quả</div>';
                        searchResults.style.display = 'block';
                    }
                })
                .catch(error => {
                    console.error('Lỗi tìm kiếm:', error);
                    searchResults.innerHTML = '<div class="p-3">Có lỗi xảy ra khi tìm kiếm</div>';
                    searchResults.style.display = 'block';
                });
        }, 300);
    });

    // Đóng kết quả tìm kiếm khi click ra ngoài
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.search-box')) {
            searchResults.style.display = 'none';
        }
    });
}

function toggleFollow(event, userId) {
    event.preventDefault();
    event.stopPropagation();

    // Nếu chưa đăng nhập, thông báo
    if (!document.querySelector('.user-dropdown')) {
        alert('Bạn cần đăng nhập để thực hiện chức năng này');
        return;
    }

    const button = event.target;
    const isFollowing = button.classList.contains('following');

    // Hiệu ứng tức thì trước khi gọi API
    if (isFollowing) {
        button.classList.remove('following');
        button.textContent = 'Theo dõi';
    } else {
        button.classList.add('following');
        button.textContent = 'Đang theo dõi';
    }

    fetch('/Home/ToggleFollow', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify(userId)
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Lỗi kết nối');
            }
            return response.json();
        })
        .then(data => {
            if (!data.success) {
                // Nếu thất bại, đảo ngược trạng thái nút
                if (isFollowing) {
                    button.classList.add('following');
                    button.textContent = 'Đang theo dõi';
                } else {
                    button.classList.remove('following');
                    button.textContent = 'Theo dõi';
                }
                console.error('Lỗi:', data.message);
                alert(data.message);
            }
        })
        .catch(error => {
            console.error('Lỗi:', error);
            // Đảo ngược trạng thái nút khi có lỗi
            if (isFollowing) {
                button.classList.add('following');
                button.textContent = 'Đang theo dõi';
            } else {
                button.classList.remove('following');
                button.textContent = 'Theo dõi';
            }
            alert('Có lỗi xảy ra khi thực hiện theo dõi');
        });
}