$(document).ready(function() {
    // Script cho dropdown user menu
    $('#userDropdown').on('click', function(e) {
        e.stopPropagation();
        $('#userDropdownMenu').toggle();
    });
    
    // Ẩn dropdown khi click ra ngoài
    $(document).on('click', function(e) {
        if (!$('#userDropdown').is(e.target) && 
            !$('#userDropdownMenu').is(e.target) && 
            $('#userDropdownMenu').has(e.target).length === 0) {
            $('#userDropdownMenu').hide();
        }
    });
    
    // Script cho dropdown category menu
    $('#categoryDropdownBtn').on('click', function(e) {
        e.stopPropagation();
        $('#categoryDropdownMenu').toggle();
    });
    
    // Ẩn dropdown category khi click ra ngoài
    $(document).on('click', function(e) {
        if (!$('#categoryDropdownBtn').is(e.target) && 
            !$('#categoryDropdownMenu').is(e.target) && 
            $('#categoryDropdownMenu').has(e.target).length === 0) {
            $('#categoryDropdownMenu').hide();
        }
    });
    
    console.log("Navigation.js đã được tải");
    
    const searchInput = $('#searchInput');
    const searchResults = $('#searchResults');
    let timer;
    
    // Bắt sự kiện input để tìm kiếm khi người dùng gõ
    searchInput.on('input', function() {
        clearTimeout(timer);
        const query = $(this).val().trim();
        console.log("Đang nhập tìm kiếm:", query);
        
        if (query.length < 1) {
            searchResults.hide();
            return;
        }
        
        // Delay đợi người dùng gõ xong
        timer = setTimeout(function() {
            performSearch(query);
        }, 300);
    });
    
    // Thêm xử lý nhấn Enter để chuyển đến trang kết quả tìm kiếm
    searchInput.on('keydown', function(e) {
        console.log('Key pressed:', e.key);
        if (e.key === 'Enter') {
            e.preventDefault();
            const query = $(this).val().trim();
            console.log('Search query:', query);

            if (query) {
                // Kiểm tra nếu query bắt đầu bằng # hoặc có chứa #tag
                if (query.startsWith('#') || query.match(/#\w+/)) {
                    // Xử lý tìm kiếm theo tag
                    const tagQuery = query.startsWith('#') ? query.substring(1) : query.match(/#(\w+)/)[1];
                    window.location.href = '/Search/Index?tag=' + encodeURIComponent(tagQuery);
                } else {
                    // Tìm kiếm bình thường
                    window.location.href = '/Search/Index?q=' + encodeURIComponent(query);
                }
            }
        }
    });
    
    // Hàm thực hiện tìm kiếm
    function performSearch(query) {
        console.log("Gọi API tìm kiếm với từ khóa:", query);
        
        $.ajax({
            url: '/Search/SearchAll',
            type: 'GET',
            data: { query: query },
            success: function(data) {
                console.log("Kết quả tìm kiếm:", data);
                
                if (data && (data.users?.length > 0 || data.artworks?.length > 0)) {
                    renderSearchResults(query, data.users || [], data.artworks || []);
                    searchResults.show();
                    
                    // Quan trọng: Ngăn mở lightbox sau khi tìm kiếm
                    $('.search-results a').click(function(e) {
                        e.stopPropagation();
                    });
                } else {
                    searchResults.html('<div class="p-3 text-center">Không tìm thấy kết quả</div>');
                    searchResults.show();
                }
            },
            error: function(xhr, status, error) {
                console.error("Lỗi khi tìm kiếm:", error);
                searchResults.html('<div class="p-3 text-center text-danger">Đã xảy ra lỗi khi tìm kiếm</div>');
                searchResults.show();
            }
        });
    }
    
    // Hàm render kết quả tìm kiếm
    function renderSearchResults(query, users, artworks) {
        let html = '';
        
        // Hiển thị thông báo có phải bạn muốn tìm
        html += `<div class="search-heading">Có phải bạn muốn tìm</div>`;
        
        // Hiển thị tranh (sản phẩm) nếu có, giới hạn số lượng 
        if (artworks && artworks.length > 0) {
            const maxArtworksToShow = 3;
            const artworksToDisplay = artworks.slice(0, maxArtworksToShow);
            
            artworksToDisplay.forEach(item => {
                // Kiểm tra dữ liệu hợp lệ trước khi hiển thị
                const title = item.tieuDe || 'Chưa có tiêu đề';
                const artist = item.nguoiDung || 'Không xác định';
                const price = item.gia ? new Intl.NumberFormat('vi-VN').format(item.gia) + 'đ' : 'Liên hệ';
                const imagePath = item.duongDanAnh || '/images/default.png';
                
                html += `
                <a href="/Artwork/Display/${item.maTranh}" class="search-result-item prevent-lightbox">
                    <div class="search-result-image">
                        <img src="${imagePath}" alt="${title}" class="search-thumbnail" onerror="this.src='/images/default.png'">
                    </div>
                    <div class="search-result-info">
                        <div class="search-result-title">${title}</div>
                        <div class="search-result-artist">${artist}</div>
                        <div class="search-result-price">${price}</div>
                    </div>
                </a>
                `;
            });
            
            // Hiển thị thêm nút "Xem thêm" nếu có nhiều kết quả
            if (artworks.length > maxArtworksToShow) {
                html += `<a href="/Artwork/Products?searchString=${encodeURIComponent(query)}" class="search-view-more">Xem thêm ${artworks.length - maxArtworksToShow} sản phẩm</a>`;
            }
        }
        
        // Hiển thị người dùng, giới hạn số lượng
        if (users && users.length > 0) {
            const maxUsersToShow = 2;
            const usersToDisplay = users.slice(0, maxUsersToShow);
            
            if (artworks && artworks.length > 0) {
                html += '<div class="search-divider"></div>';
            }
            
            usersToDisplay.forEach(user => {
                // Kiểm tra dữ liệu hợp lệ trước khi hiển thị
                const username = user.tenNguoiDung || 'Người dùng';
                const userLoginName = user.tenDangNhap || 'user';
                const avatarPath = user.anhDaiDien || '/images/authors/default/default-image.png';
                
                html += `
                <a href="/User/Profile/${user.id}" class="search-result-item prevent-lightbox">
                    <div class="search-result-avatar">
                        <img src="${avatarPath}" alt="${username}" class="search-avatar" onerror="this.src='/images/authors/default/default-image.png'">
                    </div>
                    <div class="search-result-info">
                        <div class="search-result-name">${username}</div>
                        <div class="search-result-username">@${userLoginName}</div>
                    </div>
                </a>
                `;
            });
        }
        
        if (users?.length === 0 && artworks?.length === 0) {
            html = '<div class="search-no-results">Không tìm thấy kết quả</div>';
        }
        
        searchResults.html(html);
        
        // Ngăn không cho mở lightbox khi click vào kết quả tìm kiếm
        $('.prevent-lightbox').on('click', function(e) {
            e.stopPropagation();
        });
    }
    
    // Ẩn kết quả khi click bên ngoài
    $(document).on('click', function(e) {
        if (!searchInput.is(e.target) && !searchResults.is(e.target) && searchResults.has(e.target).length === 0) {
            searchResults.hide();
        }
    });

    // Cải thiện trải nghiệm tìm kiếm và nút
    $('.search-box input').on('focus', function() {
        $(this).parent().addClass('search-focus');
    }).on('blur', function() {
        $(this).parent().removeClass('search-focus');
    });
    
    // Hiệu ứng hover cho nút
    $('.btn-post-artwork').hover(
        function() {
            $(this).css('transform', 'translateY(-1px)');
        },
        function() {
            $(this).css('transform', 'translateY(0)');
        }
    );

    // Code xử lý dropdown thông báo
    $('#notificationDropdown').on('click', function(e) {
        e.stopPropagation();
        $('#notificationDropdownMenu').toggle();
        
        // Đánh dấu đã xem khi mở dropdown
        if ($('#notificationDropdownMenu').is(':visible')) {
            markNotificationsAsViewed();
        }
    });
    
    // Ẩn dropdown thông báo khi click ra ngoài
    $(document).on('click', function(e) {
        if (!$('#notificationDropdown').is(e.target) && 
            !$('#notificationDropdownMenu').is(e.target) && 
            $('#notificationDropdownMenu').has(e.target).length === 0) {
            $('#notificationDropdownMenu').hide();
        }
    });
    
    // Tab thông báo
    $('.notification-tab').on('click', function() {
        $('.notification-tab').removeClass('active');
        $(this).addClass('active');
        
        const tab = $(this).data('tab');
        if (tab === 'all') {
            $('.notification-item').show();
        } else if (tab === 'unread') {
            $('.notification-item').hide();
            $('.notification-item.unread').show();
        }
    });
    
    // Đánh dấu tất cả là đã đọc
    $('#markAllAsRead').on('click', function(e) {
        e.preventDefault();
        e.stopPropagation();
        
        $.ajax({
            url: '/Notification/MarkAllAsRead',
            type: 'POST',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function(response) {
                if (response.success) {
                    $('.notification-item').removeClass('unread');
                    $('.notification-status').remove();
                    $('.notification-badge').remove();
                }
            }
        });
    });
    
    // Đánh dấu thông báo là đã xem khi mở dropdown
    function markNotificationsAsViewed() {
        $.ajax({
            url: '/Notification/MarkAsViewed',
            type: 'POST',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            }
        });
    }
    
    // Đánh dấu thông báo đã đọc khi click vào thông báo
    $('.notification-item').on('click', function() {
        const notificationId = $(this).data('id');
        
        $.ajax({
            url: '/Notification/MarkAsRead',
            type: 'POST',
            data: { id: notificationId },
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            }
        });
        
        $(this).removeClass('unread');
        $(this).find('.notification-status').remove();
    });
});

// Giữ hàm toggleFollow ở bên ngoài document.ready
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

document.addEventListener('DOMContentLoaded', function () {
    // Fix navbar height issues
    const navbar = document.querySelector('.navbar');
    if (navbar) {
        navbar.style.height = 'auto';
        navbar.style.minHeight = '56px';
    }
});

// Thêm hàm xử lý tìm kiếm theo tag
function performSearchByTag(tag) {
    console.log("Tìm kiếm theo tag:", tag);
    
    $.ajax({
        url: '/Search/SearchByTag',
        type: 'GET',
        data: { tag: tag },
        success: function(data) {
            console.log("Kết quả tìm kiếm theo tag:", data);
            
            if (data && data.artworks?.length > 0) {
                renderSearchResults('', [], data.artworks);
                searchResults.show();
            } else {
                searchResults.html('<div class="p-3 text-center">Không tìm thấy tác phẩm nào với tag này</div>');
                searchResults.show();
            }
        },
        error: function(xhr, status, error) {
            console.error("Lỗi khi tìm kiếm theo tag:", error);
            searchResults.html('<div class="p-3 text-center text-danger">Đã xảy ra lỗi khi tìm kiếm</div>');
            searchResults.show();
        }
    });
}