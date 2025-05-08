document.addEventListener('DOMContentLoaded', function () {
    // Xử lý chuyển đổi tab menu
    const menuItems = document.querySelectorAll('.admin-menu li');
    const sections = document.querySelectorAll('.admin-section');

    // Kiểm tra tham số tab từ URL
    function activateTabFromUrl() {
        const urlParams = new URLSearchParams(window.location.search);
        const tabParam = urlParams.get('tab');
        
        if (tabParam) {
            // Bỏ active ở tất cả menu items và sections
            menuItems.forEach(i => i.classList.remove('active'));
            sections.forEach(s => s.classList.remove('active'));
            
            // Tìm menu item và section tương ứng
            const targetMenuItem = document.querySelector(`.admin-menu li[data-target="${tabParam}"]`);
            const targetSection = document.getElementById(tabParam);
            
            if (targetMenuItem && targetSection) {
                targetMenuItem.classList.add('active');
                targetSection.classList.add('active');
            }
        }
    }
    
    // Kích hoạt tab từ URL khi tải trang
    activateTabFromUrl();

    menuItems.forEach(item => {
        item.addEventListener('click', function () {
            const target = this.getAttribute('data-target');

            // Bỏ active ở tất cả menu items và sections
            menuItems.forEach(i => i.classList.remove('active'));
            sections.forEach(s => s.classList.remove('active'));

            // Thêm active cho item được click và section tương ứng
            this.classList.add('active');
            document.getElementById(target).classList.add('active');
            
            // Cập nhật URL khi chuyển tab (không làm refresh trang)
            const url = new URL(window.location);
            url.searchParams.set('tab', target);
            window.history.pushState({}, '', url);
        });
    });

    // Lấy dữ liệu thống kê dashboard
    fetchDashboardStats();

    // Lấy danh sách người dùng
    fetchUsers();

    // Lấy danh sách đơn đăng ký nghệ sĩ
    fetchArtistApplications();

    // Lấy danh sách tác phẩm
    fetchArtworks();

    // Lấy danh sách đơn hàng
    fetchOrders();

    // Khởi tạo biểu đồ thống kê
    initCharts();

    // Xử lý form cài đặt
    document.getElementById('settings-form').addEventListener('submit', function (e) {
        e.preventDefault();
        saveSettings();
    });

    // Xử lý tìm kiếm
    document.getElementById('user-search').addEventListener('input', debounce(function () {
        searchUsers(this.value);
    }, 300));

    document.getElementById('artwork-search').addEventListener('input', debounce(function () {
        searchArtworks(this.value);
    }, 300));

    document.getElementById('order-search').addEventListener('input', debounce(function () {
        searchOrders(this.value);
    }, 300));
});

// Hàm lấy dữ liệu thống kê dashboard
function fetchDashboardStats() {
    // Gửi yêu cầu AJAX để lấy thống kê
    fetch('/Admin/GetDashboardStats')
        .then(response => response.json())
        .then(data => {
            // Cập nhật số liệu
            document.getElementById('total-users').textContent = data.totalUsers;
            document.getElementById('total-artists').textContent = data.totalArtists;
            document.getElementById('total-artworks').textContent = data.totalArtworks;
            document.getElementById('total-orders').textContent = data.totalOrders;

            // Hiển thị đơn đăng ký nghệ sĩ mới nhất
            const artistAppTable = document.getElementById('artist-applications').getElementsByTagName('tbody')[0];
            artistAppTable.innerHTML = '';

            data.recentArtistApplications.forEach(app => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>${app.artistName}</td>
                    <td>${formatDate(app.applicationDate)}</td>
                    <td>
                        <a href="/Admin/ArtistApproval/${app.userId}" class="btn btn-sm btn-primary">Xem</a>
                    </td>
                `;
                artistAppTable.appendChild(row);
            });

            // Hiển thị tác phẩm mới nhất
            const artworksTable = document.getElementById('recent-artworks').getElementsByTagName('tbody')[0];
            artworksTable.innerHTML = '';

            data.recentArtworks.forEach(artwork => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>
                        <div class="d-flex align-items-center">
                            <div class="artwork-thumbnail me-2">
                                <img src="${artwork.thumbnailUrl}" alt="${artwork.title}">
                            </div>
                            <span>${artwork.title}</span>
                        </div>
                    </td>
                    <td>${artwork.artistName}</td>
                    <td>${formatDate(artwork.createdDate)}</td>
                `;
                artworksTable.appendChild(row);
            });
        })
        .catch(error => console.error('Error fetching dashboard stats:', error));
}

// Hàm lấy danh sách người dùng
function fetchUsers() {
    fetch('/Admin/GetUsers')
        .then(response => response.json())
        .then(data => {
            const usersTable = document.getElementById('users-table').getElementsByTagName('tbody')[0];
            usersTable.innerHTML = '';

            data.forEach(user => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>${user.id}</td>
                    <td>
                        <div class="user-avatar">
                            <img src="${user.avatarPath}" alt="${user.userName}">
                        </div>
                    </td>
                    <td>${user.userName}</td>
                    <td>${user.email}</td>
                    <td>
                        <span class="badge ${user.isAdmin ? 'bg-danger' : user.isArtist ? 'bg-primary' : 'bg-secondary'}">
                            ${user.isAdmin ? 'Admin' : user.isArtist ? 'Nghệ sĩ' : 'Người dùng'}
                        </span>
                    </td>
                    <td>
                        <span class="badge ${user.isActive ? 'bg-success' : 'bg-secondary'}">
                            ${user.isActive ? 'Hoạt động' : 'Đã vô hiệu hóa'}
                        </span>
                    </td>
                    <td>
                        <div class="action-buttons">
                            <button class="btn btn-primary btn-view-user" data-id="${user.id}">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button class="btn ${user.isActive ? 'btn-warning' : 'btn-success'} btn-toggle-user" data-id="${user.id}" data-active="${user.isActive}">
                                <i class="fas ${user.isActive ? 'fa-ban' : 'fa-check'}"></i>
                            </button>
                            <button class="btn btn-danger btn-delete-user" data-id="${user.id}">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    </td>
                `;
                usersTable.appendChild(row);
            });

            // Thêm sự kiện cho các nút
            addUserButtonsEventListeners();
        })
        .catch(error => console.error('Error fetching users:', error));
}

// Hàm lấy danh sách đơn đăng ký nghệ sĩ
function fetchArtistApplications() {
    fetch('/Admin/GetArtistApplications')
        .then(response => response.json())
        .then(data => {
            const artistsTable = document.getElementById('artist-approval-table').getElementsByTagName('tbody')[0];
            artistsTable.innerHTML = '';

            data.forEach(artist => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>${artist.userId}</td>
                    <td>
                        <div class="user-avatar">
                            <img src="${artist.avatarPath}" alt="${artist.artistName}">
                        </div>
                    </td>
                    <td>${artist.artistName}</td>
                    <td>${artist.email}</td>
                    <td>${formatDate(artist.applicationDate)}</td>
                    <td>
                        <div class="action-buttons">
                            <a href="/Admin/ArtistApproval/${artist.userId}" class="btn btn-primary">
                                <i class="fas fa-eye"></i> Xem
                            </a>
                        </div>
                    </td>
                `;
                artistsTable.appendChild(row);
            });
        })
        .catch(error => console.error('Error fetching artist applications:', error));
}

// Hàm lấy danh sách tác phẩm
function fetchArtworks() {
    fetch('/Admin/GetArtworks')
        .then(response => response.json())
        .then(data => {
            const artworksTable = document.getElementById('artworks-table').getElementsByTagName('tbody')[0];
            artworksTable.innerHTML = '';

            data.forEach(artwork => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>${artwork.id}</td>
                    <td>
                        <div class="artwork-thumbnail">
                            <img src="${artwork.thumbnailUrl}" alt="${artwork.title}">
                        </div>
                    </td>
                    <td>${artwork.title}</td>
                    <td>${artwork.artistName}</td>
                    <td>${artwork.category ?? 'Chưa phân loại'}</td>
                    <td>${formatCurrency(artwork.price)}</td>
                    <td>${formatDate(artwork.createdDate)}</td>
                    <td>
                        <div class="action-buttons">
                            <a href="/Artwork/Display/${artwork.id}" class="btn btn-primary btn-sm" target="_blank">
                                <i class="fas fa-eye"></i>
                            </a>
                            <button class="btn btn-danger btn-sm btn-delete-artwork" data-id="${artwork.id}" data-artist-id="${artwork.artistId}">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    </td>
                `;
                artworksTable.appendChild(row);
            });

            // Thêm sự kiện cho các nút
            addArtworkButtonsEventListeners();
        })
        .catch(error => console.error('Error fetching artworks:', error));
}

// Hàm lấy danh sách đơn hàng
function fetchOrders() {
    fetch('/Admin/GetOrders')
        .then(response => response.json())
        .then(data => {
            const ordersTable = document.getElementById('orders-table').getElementsByTagName('tbody')[0];
            ordersTable.innerHTML = '';

            data.forEach(order => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td>${order.orderId}</td>
                    <td>${order.customerName}</td>
                    <td>
                        <div class="d-flex align-items-center">
                            <div class="artwork-thumbnail me-2">
                                <img src="${order.artworkThumbnail}" alt="${order.artworkTitle}">
                            </div>
                            <span>${order.artworkTitle}</span>
                        </div>
                    </td>
                    <td>${formatCurrency(order.totalAmount)}</td>
                    <td>${formatDate(order.orderDate)}</td>
                    <td>
                        <span class="badge ${getBadgeClass(order.status)}">
                            ${order.status}
                        </span>
                    </td>
                    <td>
                        <div class="action-buttons">
                            <button class="btn btn-primary btn-sm btn-view-order" data-id="${order.orderId}">
                                <i class="fas fa-eye"></i>
                            </button>
                            ${order.status !== 'Đã hoàn thành' && order.status !== 'Đã hủy' ?
                        `<div class="dropdown d-inline">
                                    <button class="btn btn-secondary btn-sm dropdown-toggle" type="button" data-bs-toggle="dropdown">
                                        <i class="fas fa-cog"></i>
                                    </button>
                                    <ul class="dropdown-menu">
                                        <li><a class="dropdown-item btn-update-status" data-id="${order.orderId}" data-status="Đã xác nhận" href="#">Xác nhận</a></li>
                                        <li><a class="dropdown-item btn-update-status" data-id="${order.orderId}" data-status="Đã hoàn thành" href="#">Hoàn thành</a></li>
                                        <li><a class="dropdown-item btn-update-status" data-id="${order.orderId}" data-status="Đã hủy" href="#">Hủy</a></li>
                                    </ul>
                                </div>`
                        : ''}
                        </div>
                    </td>
                `;
                ordersTable.appendChild(row);
            });

            // Thêm sự kiện cho các nút
            addOrderButtonsEventListeners();
        })
        .catch(error => console.error('Error fetching orders:', error));
}

// Khởi tạo biểu đồ thống kê
function initCharts() {
    // Biểu đồ doanh thu
    fetch('/Admin/GetRevenueStats')
        .then(response => response.json())
        .then(data => {
            const ctx = document.getElementById('revenue-chart').getContext('2d');
            new Chart(ctx, {
                type: 'line',
                data: {
                    labels: data.labels,
                    datasets: [{
                        label: 'Doanh thu (VND)',
                        data: data.values,
                        backgroundColor: 'rgba(58, 123, 213, 0.1)',
                        borderColor: '#3a7bd5',
                        borderWidth: 2,
                        tension: 0.3,
                        pointBackgroundColor: '#3a7bd5'
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            position: 'top',
                        },
                        title: {
                            display: true,
                            text: 'Doanh thu theo tháng'
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
        });

    // Biểu đồ người dùng
    fetch('/Admin/GetUserStats')
        .then(response => response.json())
        .then(data => {
            const ctx = document.getElementById('users-chart').getContext('2d');
            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: data.labels,
                    datasets: [{
                        label: 'Người dùng mới',
                        data: data.values,
                        backgroundColor: '#3a7bd5'
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            position: 'top',
                        },
                        title: {
                            display: true,
                            text: 'Người dùng đăng ký mới theo tháng'
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
        });

    // Biểu đồ thể loại
    fetch('/Admin/GetCategoryStats')
        .then(response => response.json())
        .then(data => {
            const ctx = document.getElementById('categories-chart').getContext('2d');
            new Chart(ctx, {
                type: 'pie',
                data: {
                    labels: data.labels,
                    datasets: [{
                        data: data.values,
                        backgroundColor: [
                            '#3a7bd5', '#00d2ff', '#2ecc71', '#f39c12',
                            '#e74c3c', '#9b59b6', '#1abc9c', '#34495e'
                        ]
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            position: 'right',
                        },
                        title: {
                            display: true,
                            text: 'Tác phẩm theo thể loại'
                        }
                    }
                }
            });
        });

    // Biểu đồ tác phẩm
    fetch('/Admin/GetArtworkStats')
        .then(response => response.json())
        .then(data => {
            const ctx = document.getElementById('artworks-chart').getContext('2d');
            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: data.labels,
                    datasets: [{
                        label: 'Tác phẩm mới',
                        data: data.values,
                        backgroundColor: '#00d2ff'
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        legend: {
                            position: 'top',
                        },
                        title: {
                            display: true,
                            text: 'Tác phẩm đăng mới theo tháng'
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    }
                }
            });
        });
}

// Thêm sự kiện cho các nút trong bảng người dùng
function addUserButtonsEventListeners() {
    // Nút xem chi tiết người dùng
    document.querySelectorAll('.btn-view-user').forEach(button => {
        button.addEventListener('click', function () {
            const userId = this.getAttribute('data-id');
            window.open(`/User/Profile/${userId}`, '_blank');
        });
    });

    // Nút vô hiệu hóa/kích hoạt người dùng
    document.querySelectorAll('.btn-toggle-user').forEach(button => {
        button.addEventListener('click', function () {
            const userId = this.getAttribute('data-id');
            const isActive = this.getAttribute('data-active') === 'true';

            if (isActive) {
                // Hiển thị modal nhập lý do và thời gian khóa
                document.getElementById('lock-user-id').value = userId;
                
                // Reset form
                document.getElementById('lock-reason').value = '';
                document.getElementById('lock-days').value = '0';
                document.getElementById('lock-hours').value = '0';
                document.getElementById('lock-minutes').value = '0';
                document.getElementById('lock-seconds').value = '0';
                
                // Hiển thị modal (cách sử dụng Bootstrap 5)
                const lockModal = document.getElementById('lockUserModal');
                const bootstrapModal = new bootstrap.Modal(lockModal);
                bootstrapModal.show();
                
                // Đảm bảo các label hiển thị rõ ràng
                setTimeout(() => {
                    const labels = lockModal.querySelectorAll('.form-text');
                    labels.forEach(label => {
                        label.style.visibility = 'visible';
                        label.style.color = '#ccc';
                    });
                }, 100);
            } else {
                // Mở khóa tài khoản
                if (confirm('Bạn có chắc muốn mở khóa tài khoản người dùng này?')) {
                    unlockUser(userId);
                }
            }
        });
    });

    // Xử lý nút xác nhận khóa trong modal
    document.getElementById('confirm-lock-user').addEventListener('click', function() {
        const userId = document.getElementById('lock-user-id').value;
        const reason = document.getElementById('lock-reason').value;
        const days = parseInt(document.getElementById('lock-days').value) || 0;
        const hours = parseInt(document.getElementById('lock-hours').value) || 0;
        const minutes = parseInt(document.getElementById('lock-minutes').value) || 0;
        const seconds = parseInt(document.getElementById('lock-seconds').value) || 0;
        
        // Kiểm tra nếu không có thời gian khóa
        if (days === 0 && hours === 0 && minutes === 0 && seconds === 0) {
            alert('Vui lòng nhập thời gian khóa tài khoản!');
            return;
        }
        
        // Kiểm tra nếu không có lý do
        if (!reason.trim()) {
            alert('Vui lòng nhập lý do khóa tài khoản!');
            return;
        }
        
        // Gửi yêu cầu khóa tài khoản
        lockUser(userId, reason, days, hours, minutes, seconds);
        
        // Đóng modal
        const lockModal = new bootstrap.Modal(document.getElementById('lockUserModal'));
        lockModal.hide();
    });

    // Nút xóa người dùng
    document.querySelectorAll('.btn-delete-user').forEach(button => {
        button.addEventListener('click', function () {
            const userId = this.getAttribute('data-id');

            if (confirm('Bạn có chắc muốn xóa người dùng này? Dữ liệu không thể khôi phục!')) {
                fetch('/Admin/DeleteUser', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify(userId)
                })
                    .then(response => response.json())
                    .then(data => {
                        if (data.success) {
                            alert('Đã xóa người dùng thành công!');
                            fetchUsers(); // Tải lại danh sách
                        } else {
                            alert(`Lỗi: ${data.message}`);
                        }
                    })
                    .catch(error => console.error('Error deleting user:', error));
            }
        });
    });
}

// Hàm khóa tài khoản
function lockUser(userId, reason, days, hours, minutes, seconds) {
    fetch('/Admin/LockUser', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({ 
            userId: userId, 
            reason: reason,
            days: days,
            hours: hours,
            minutes: minutes,
            seconds: seconds
        })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('Đã khóa tài khoản người dùng thành công!');
                fetchUsers(); // Tải lại danh sách
            } else {
                alert(`Lỗi: ${data.message}`);
            }
        })
        .catch(error => console.error('Error locking user:', error));
}

// Hàm mở khóa tài khoản
function unlockUser(userId) {
    fetch('/Admin/UnlockUser', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify(userId)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('Đã mở khóa tài khoản người dùng thành công!');
                fetchUsers(); // Tải lại danh sách
            } else {
                alert(`Lỗi: ${data.message}`);
            }
        })
        .catch(error => console.error('Error unlocking user:', error));
}

// Thêm sự kiện cho các nút trong bảng tác phẩm
function addArtworkButtonsEventListeners() {
    // Nút xem chi tiết tác phẩm
    document.querySelectorAll('.btn-view-artwork').forEach(button => {
        button.addEventListener('click', function() {
            const artworkId = this.getAttribute('data-id');
            window.open(`/Artwork/Display/${artworkId}`, '_blank');
        });
    });

    // Nút xóa tác phẩm
    document.querySelectorAll('.btn-delete-artwork').forEach(button => {
        button.addEventListener('click', function() {
            const artworkId = this.getAttribute('data-id');
            const artistId = this.getAttribute('data-artist-id') || "";
            
            // Hiển thị modal xóa tác phẩm
            document.getElementById('delete-artwork-id').value = artworkId;
            document.getElementById('delete-artist-id').value = artistId;
            document.getElementById('delete-reason').value = '';
            
            // Ẩn thanh tìm kiếm
            document.querySelectorAll('.search-box').forEach(box => {
                box.style.visibility = 'hidden';
            });
            
            // Hiển thị modal
            const deleteModal = document.getElementById('deleteArtworkModal');
            const bootstrapModal = new bootstrap.Modal(deleteModal);
            bootstrapModal.show();
        });
    });
}

// Thêm sự kiện cho các nút trong bảng đơn hàng
function addOrderButtonsEventListeners() {
    // Nút xem chi tiết đơn hàng
    document.querySelectorAll('.btn-view-order').forEach(button => {
        button.addEventListener('click', function () {
            const orderId = this.getAttribute('data-id');
            // Mở modal chi tiết đơn hàng
            fetch(`/Admin/GetOrderDetail/${orderId}`)
                .then(response => response.json())
                .then(data => {
                    // Xử lý hiển thị chi tiết đơn hàng
                    // Sẽ triển khai sau khi có modal
                })
                .catch(error => console.error('Error fetching order details:', error));
        });
    });

    // Nút cập nhật trạng thái đơn hàng
    document.querySelectorAll('.btn-update-status').forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            const orderId = this.getAttribute('data-id');
            const status = this.getAttribute('data-status');

            if (confirm(`Bạn có chắc muốn cập nhật trạng thái đơn hàng thành "${status}"?`)) {
                fetch('/Admin/UpdateOrderStatus', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                    },
                    body: JSON.stringify({ orderId: orderId, status: status })
                })
                    .then(response => response.json())
                    .then(data => {
                        if (data.success) {
                            alert('Đã cập nhật trạng thái đơn hàng thành công!');
                            fetchOrders(); // Tải lại danh sách
                        } else {
                            alert(`Lỗi: ${data.message}`);
                        }
                    })
                    .catch(error => console.error('Error updating order status:', error));
            }
        });
    });
}

// Lưu thiết lập
function saveSettings() {
    const settings = {
        siteName: document.getElementById('site-name').value,
        siteDescription: document.getElementById('site-description').value,
        contactEmail: document.getElementById('contact-email').value,
        commissionRate: document.getElementById('commission-rate').value
    };

    fetch('/Admin/SaveSettings', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify(settings)
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('Đã lưu cài đặt thành công!');
            } else {
                alert(`Lỗi: ${data.message}`);
            }
        })
        .catch(error => console.error('Error saving settings:', error));
}

// Tìm kiếm người dùng
function searchUsers(query) {
    fetch(`/Admin/SearchUsers?q=${encodeURIComponent(query)}`)
        .then(response => response.json())
        .then(data => {
            const usersTable = document.getElementById('users-table').getElementsByTagName('tbody')[0];
            usersTable.innerHTML = '';

            data.forEach(user => {
                // Hiển thị kết quả tìm kiếm - giống code trong fetchUsers
                const row = document.createElement('tr');
                // ... (giống code trong fetchUsers)
                usersTable.appendChild(row);
            });

            // Thêm sự kiện cho các nút
            addUserButtonsEventListeners();
        })
        .catch(error => console.error('Error searching users:', error));
}

// Tìm kiếm tác phẩm
function searchArtworks(query) {
    // Tương tự như searchUsers
}

// Tìm kiếm đơn hàng
function searchOrders(query) {
    // Tương tự như searchUsers
}

// Các hàm tiện ích
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

function getBadgeClass(status) {
    switch (status) {
        case 'Chờ xác nhận':
            return 'bg-warning text-dark';
        case 'Đã xác nhận':
            return 'bg-primary';
        case 'Đã hoàn thành':
            return 'bg-success';
        case 'Đã hủy':
            return 'bg-danger';
        case 'Đã đặt hàng':
            return 'bg-info';
        default:
            return 'bg-secondary';
    }
}

function debounce(func, delay) {
    let timeout;
    return function () {
        const context = this;
        const args = arguments;
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(context, args), delay);
    };
}

// Thêm sự kiện cho nút xác nhận xóa tác phẩm trong document.ready
document.addEventListener('DOMContentLoaded', function() {
    // Các code khác...
    
    // Xử lý nút xác nhận xóa tác phẩm
    document.getElementById('confirm-delete-artwork').addEventListener('click', function() {
        const artworkId = document.getElementById('delete-artwork-id').value;
        const artistId = document.getElementById('delete-artist-id').value;
        const reason = document.getElementById('delete-reason').value.trim() || "Xóa bởi quản trị viên";
        
        fetch('/Admin/DeleteArtwork', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({
                ArtworkId: parseInt(artworkId),
                ArtistId: artistId,
                Reason: reason
            })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('Đã xóa tác phẩm thành công!');
                fetchArtworks(); // Tải lại danh sách
            } else {
                alert(`Lỗi: ${data.message}`);
            }
        })
        .catch(error => console.error('Error deleting artwork:', error));
        
        // Đóng modal
        const deleteModal = bootstrap.Modal.getInstance(document.getElementById('deleteArtworkModal'));
        deleteModal.hide();
    });
});

// Thêm sự kiện ẩn/hiện thanh tìm kiếm khi modal đóng/mở
document.addEventListener('DOMContentLoaded', function() {
    const deleteArtworkModal = document.getElementById('deleteArtworkModal');
    
    // Xử lý sự kiện khi modal ẩn (đóng)
    deleteArtworkModal.addEventListener('hidden.bs.modal', function() {
        // Hiện lại thanh tìm kiếm khi modal đóng
        document.querySelectorAll('.search-box').forEach(box => {
            box.style.visibility = 'visible';
        });
    });
    
    // Xử lý nút xác nhận xóa tác phẩm
    document.getElementById('confirm-delete-artwork').addEventListener('click', function() {
        const artworkId = document.getElementById('delete-artwork-id').value;
        const artistId = document.getElementById('delete-artist-id').value;
        const reason = document.getElementById('delete-reason').value.trim() || "Xóa bởi quản trị viên";
        
        fetch('/Admin/DeleteArtwork', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({
                ArtworkId: parseInt(artworkId),
                ArtistId: artistId,
                Reason: reason
            })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert('Đã xóa tác phẩm thành công!');
                fetchArtworks(); // Tải lại danh sách
            } else {
                alert(`Lỗi: ${data.message}`);
            }
        })
        .catch(error => console.error('Error deleting artwork:', error));
        
        // Đóng modal
        const deleteModal = bootstrap.Modal.getInstance(document.getElementById('deleteArtworkModal'));
        deleteModal.hide();
        
        // Hiện lại thanh tìm kiếm
        document.querySelectorAll('.search-box').forEach(box => {
            box.style.visibility = 'visible';
        });
    });
});