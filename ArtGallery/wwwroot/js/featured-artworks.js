// Xử lý các chức năng liên quan đến tác phẩm nổi bật

// Đơn giản hóa hàm mở modal tác phẩm nổi bật
function showFeaturedArtworksModal() {
    const modal = document.getElementById('addFeaturedModal');
    if (modal) {
        modal.style.display = 'block';
        document.body.classList.add('modal-open');
        loadUserArtworks();
    }
}

// Hàm đóng modal tác phẩm nổi bật
function closeAddFeaturedModal() {
    const modal = document.getElementById('addFeaturedModal');
    if (modal) {
        modal.style.display = 'none';
        document.body.classList.remove('modal-open');
    }
}

// Hàm tải sản phẩm của người dùng
function loadUserArtworks() {
    const userId = document.querySelector('meta[name="user-id"]')?.content;
    
    if (!userId) {
        console.error('Không tìm thấy ID người dùng');
        return;
    }
    
    // Hiển thị loading trong modal
    const container = document.getElementById('artwork-selection-grid');
    if (container) {
        container.innerHTML = '<div class="loading">Đang tải tác phẩm...</div>';
    }
    
    // Gọi API để lấy danh sách tác phẩm
    fetch('/User/GetUserArtworks?userId=' + userId)
        .then(response => response.json())
        .then(data => {
            displayArtworksForSelection(data);
        })
        .catch(error => {
            console.error('Lỗi khi tải danh sách tác phẩm:', error);
            if (container) {
                container.innerHTML = '<div class="error">Có lỗi xảy ra khi tải tác phẩm. Vui lòng thử lại sau.</div>';
            }
        });
}

// Hiển thị danh sách tác phẩm để chọn
function displayArtworksForSelection(artworks) {
    const container = document.getElementById('artwork-selection-grid');
    
    if (!container) return;
    
    container.innerHTML = '';
    
    if (!artworks || artworks.length === 0) {
        container.innerHTML = '<div class="no-artworks">Bạn chưa có tác phẩm nào</div>';
        return;
    }
    
    artworks.forEach(artwork => {
        const isFeatured = artwork.isFeatured;
        
        const artworkElement = document.createElement('div');
        artworkElement.className = `artwork-select-item ${isFeatured ? 'is-featured' : ''}`;
        artworkElement.dataset.id = artwork.maTranh;
        
        artworkElement.innerHTML = `
            <div class="artwork-image-container">
                <img src="${artwork.duongDanAnh}" alt="${artwork.tieuDe}">
                <div class="artwork-select-overlay">
                    <div class="overlay-indicator">
                        ${isFeatured ? '<i class="fas fa-check-circle"></i> Đã thêm' : '<i class="fas fa-plus-circle"></i> Thêm'}
                    </div>
                </div>
            </div>
            <div class="artwork-title">${artwork.tieuDe}</div>
        `;
        
        artworkElement.addEventListener('click', function() {
            toggleFeaturedArtwork(artwork.maTranh, this);
        });
        
        container.appendChild(artworkElement);
    });
    
    // Thêm tính năng tìm kiếm
    const searchInput = document.getElementById('artworkSearch');
    if (searchInput) {
        searchInput.addEventListener('input', function() {
            const searchTerm = this.value.toLowerCase();
            document.querySelectorAll('.artwork-select-item').forEach(item => {
                const title = item.querySelector('.artwork-title').textContent.toLowerCase();
                item.style.display = title.includes(searchTerm) ? 'block' : 'none';
            });
        });
    }
}

// Toggle featured artwork
function toggleFeaturedArtwork(artworkId, element) {
    const isCurrentlyFeatured = element.classList.contains('is-featured');
    const shouldBeFeatured = !isCurrentlyFeatured;
    
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    
    fetch('/User/ToggleFeaturedArtwork', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({ 
            artworkId: artworkId, 
            isFeatured: shouldBeFeatured 
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            if (shouldBeFeatured) {
                element.classList.add('is-featured');
                element.querySelector('.overlay-indicator').innerHTML = '<i class="fas fa-check-circle"></i> Đã thêm';
                
                // Reload trang để hiển thị sản phẩm mới thêm
                window.location.reload();
            } else {
                element.classList.remove('is-featured');
                element.querySelector('.overlay-indicator').innerHTML = '<i class="fas fa-plus-circle"></i> Thêm';
                
                // Reload trang để cập nhật
                window.location.reload();
            }
        } else {
            alert(data.message || 'Có lỗi khi cập nhật tác phẩm nổi bật');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Có lỗi xảy ra khi cập nhật tác phẩm nổi bật');
    });
}

// Xóa tác phẩm khỏi nổi bật
function removeFeatured(artworkId) {
    event.preventDefault();
    event.stopPropagation();
    
    if(confirm('Bạn có chắc muốn xóa tác phẩm này khỏi mục nổi bật?')) {
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
        
        fetch('/User/ToggleFeaturedArtwork', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify({ 
                artworkId: artworkId, 
                isFeatured: false
            })
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                window.location.reload();
            } else {
                alert(data.message || 'Có lỗi khi xóa tác phẩm nổi bật');
            }
        });
    }
}

// Gán sự kiện khi DOM đã tải xong
document.addEventListener('DOMContentLoaded', function() {
    // Gán sự kiện đóng modal
    const closeButtons = document.querySelectorAll('#addFeaturedModal .close');
    closeButtons.forEach(button => {
        button.onclick = closeAddFeaturedModal;
    });
}); 