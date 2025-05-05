function openEditProfileModal() {
    const modal = document.getElementById('editProfileModal');
    if (modal) {
        modal.style.display = 'block';
        document.body.classList.add('modal-open');
        
        // Khởi tạo các select và giá trị đã lưu
        initializeSelects();

        // Khôi phục giá trị cho tất cả privacy settings
        const genderPrivacy = document.querySelector('select[name="HienThiGioiTinh"]');
        const locationPrivacy = document.querySelector('select[name="HienThiDiaChi"]');
        const birthYearPrivacy = document.querySelector('select[name="HienThiNamSinh"]');
        const birthdayPrivacy = document.querySelector('select[name="HienThiNgaySinh"]');

        // Khôi phục privacy settings
        if (genderPrivacy) {
            const savedGenderPrivacy = genderPrivacy.getAttribute('data-current-value');
            if (savedGenderPrivacy) {
                genderPrivacy.value = savedGenderPrivacy;
            }
        }

        if (locationPrivacy) {
            const savedLocationPrivacy = locationPrivacy.getAttribute('data-current-value');
            if (savedLocationPrivacy) {
                locationPrivacy.value = savedLocationPrivacy;
            }
        }

        // Privacy cho năm sinh
        if (birthYearPrivacy) {
            const savedBirthYearPrivacy = birthYearPrivacy.getAttribute('data-current-value');
            if (savedBirthYearPrivacy) {
                birthYearPrivacy.value = savedBirthYearPrivacy;
            }
        }

        // Privacy cho ngày và tháng sinh
        if (birthdayPrivacy) {
            const savedBirthdayPrivacy = birthdayPrivacy.getAttribute('data-current-value');
            if (savedBirthdayPrivacy) {
                birthdayPrivacy.value = savedBirthdayPrivacy;
            }
        }
        
        // Khôi phục location đã chọn
        const locationSelect = document.getElementById('locationSelect');
        const currentLocation = document.getElementById('currentLocation')?.value;  
        console.log('Current Location:', currentLocation);
        
        if (locationSelect && currentLocation) {
            setTimeout(() => {
                locationSelect.value = currentLocation;
                console.log('Location set to:', locationSelect.value);
            }, 100);
        }

        // Khôi phục ngày sinh
        const currentBirthDate = document.getElementById('currentBirthDate')?.value;
        console.log('Current Birth Date:', currentBirthDate);
        
        if (currentBirthDate) {
            const [year, month, day] = currentBirthDate.split('-');
            
            // Set giá trị cho các select ngày sinh
            const yearSelect = document.querySelector('select[name="BirthYear"]');
            const monthSelect = document.querySelector('select[name="BirthMonth"]');
            const daySelect = document.querySelector('select[name="BirthDay"]');

            if (yearSelect) yearSelect.value = year;
            if (monthSelect) monthSelect.value = parseInt(month);
            if (daySelect) {
                setTimeout(() => {
                    daySelect.value = parseInt(day);
                }, 100);
            }
        }
    } else {
        console.error('Modal element not found');
    }
}

function closeEditProfileModal() {
    document.getElementById('editProfileModal').style.display = 'none';
    document.body.classList.remove('modal-open');
}

function addMediaField(existingMedia = null) {
    const container = document.getElementById('socialMediaContainer');
    const mediaItem = document.createElement('div');
    mediaItem.className = 'social-media-item';
    
    const select = document.createElement('select');
    select.name = 'LoaiMedia[]';
    select.className = 'form-select';
    
    const options = [
        { value: 'X', text: 'X' },
        { value: 'Facebook', text: 'Facebook' },
        { value: 'Instagram', text: 'Instagram' },
        { value: 'Tiktok', text: 'Tiktok' },
        { value: 'Website', text: 'Website' }
    ];
    
    options.forEach(opt => {
        const option = document.createElement('option');
        option.value = opt.value;
        option.textContent = opt.text;
        if (existingMedia && existingMedia.loaiMedia === opt.value) {
            option.selected = true;
        }
        select.appendChild(option);
    });

    const input = document.createElement('input');
    input.type = 'text';
    input.name = 'DuongDan[]';
    input.placeholder = 'ID';
    input.className = 'form-control';
    if (existingMedia) {
        input.value = existingMedia.duongDan;
    }

    const removeBtn = document.createElement('button');
    removeBtn.type = 'button';
    removeBtn.className = 'remove-media';
    removeBtn.textContent = '×';
    removeBtn.onclick = function() {
        container.removeChild(mediaItem);
    };

    mediaItem.appendChild(select);
    mediaItem.appendChild(input);
    mediaItem.appendChild(removeBtn);
    container.appendChild(mediaItem);
}

function removeMedia(button) {
    button.parentElement.remove();
}

document.addEventListener('DOMContentLoaded', function () {
    // Lưu giá trị hiện tại cho các select
    const selects = ['DiaChi', 'HienThiGioiTinh', 'HienThiDiaChi', 'HienThiNamSinh', 'HienThiNgaySinh'];
    selects.forEach(name => {
        const select = document.querySelector(`select[name="${name}"]`);
        if (select) {
            select.setAttribute('data-current-value', select.value);
        }
    });

    // Lưu giá trị hiện tại cho gender
    const genderInput = document.querySelector('input[name="GioiTinh"]:checked');
    if (genderInput) {
        genderInput.setAttribute('data-current-value', genderInput.value);
    }

    // Thêm event listener cho nút chỉnh sửa hồ sơ
    const editProfileBtn = document.querySelector('.edit-profile-btn');
    if (editProfileBtn) {
        editProfileBtn.onclick = function() {
            openEditProfileModal();
        };
    }

    // Xóa nút Theo dõi bên trái nếu nó được tạo động bởi JavaScript
    const leftFollowButtons = document.querySelectorAll('.profile-header button:not(.follow-button-primary):not(.share-profile-btn):not(.edit-profile-btn)');
    leftFollowButtons.forEach(button => {
        if (button.textContent.includes('Theo dõi')) {
            button.style.display = 'none';
        }
    });

    // Sửa tiêu đề modal khi mở
    const openEditProfileModal = window.openEditProfileModal;
    window.openEditProfileModal = function() {
        openEditProfileModal();
        // Đổi tiêu đề modal sang tiếng việt
        const modalTitle = document.querySelector('.modal-header h3');
        if (modalTitle) {
            modalTitle.textContent = 'Chỉnh sửa hồ sơ';
        }
    };
    
    // Cập nhật label ô input cho dropdown địa điểm
    initializeSelects();

    // Thêm sự kiện cho nút Thêm trong phần nổi bật
    const addFeaturedBtn = document.querySelector('.add-featured');
    if (addFeaturedBtn) {
        addFeaturedBtn.addEventListener('click', function() {
            openAddFeaturedModal();
        });
    }
    
    // Debug để xem liệu sự kiện click có hoạt động không
    console.log('Added event listener to featured button:', addFeaturedBtn);
});

function initializeSelects() {
    // Khởi tạo year select
    const yearSelect = document.querySelector('select[name="BirthYear"]');
    if (yearSelect) {
        yearSelect.innerHTML = '<option value="">--</option>';
        const currentYear = new Date().getFullYear();
        for (let year = currentYear; year >= 1900; year--) {
            const option = document.createElement('option');
            option.value = year;
            option.textContent = year;
            yearSelect.appendChild(option);
        }
    }

    // Khởi tạo month select
    const monthSelect = document.querySelector('select[name="BirthMonth"]');
    if (monthSelect) {
        monthSelect.innerHTML = '<option value="">--</option>';
        for (let month = 1; month <= 12; month++) {
            const option = document.createElement('option');
            option.value = month;
            option.textContent = month;
            monthSelect.appendChild(option);
        }
    }

    // Khởi tạo day select
    const daySelect = document.querySelector('select[name="BirthDay"]');
    if (daySelect) {
        daySelect.innerHTML = '<option value="">--</option>';
        for (let day = 1; day <= 31; day++) {
            const option = document.createElement('option');
            option.value = day;
            option.textContent = day;
            daySelect.appendChild(option);
        }
    }

    // Lấy ngày sinh hiện tại từ hidden input
    const currentBirthDate = document.getElementById('currentBirthDate')?.value;
    console.log('Current Birth Date:', currentBirthDate);
    
    if (currentBirthDate) {
        const [year, month, day] = currentBirthDate.split('-');
        
        // Set giá trị cho các select
        if (yearSelect) yearSelect.value = year;
        if (monthSelect) monthSelect.value = parseInt(month);
        if (daySelect) daySelect.value = parseInt(day);
    }

    // Khôi phục privacy settings cho ngày sinh
    const birthYearPrivacy = document.querySelector('select[name="HienThiNamSinh"]');
    const birthdayPrivacy = document.querySelector('select[name="HienThiNgaySinh"]');

    // Lấy giá trị privacy từ data attributes
    const savedBirthPrivacy = birthYearPrivacy?.getAttribute('data-current-value');
    console.log('Saved Birth Privacy:', savedBirthPrivacy);

    // Set giá trị privacy
    if (birthYearPrivacy && savedBirthPrivacy) {
        birthYearPrivacy.value = savedBirthPrivacy;
    }
    if (birthdayPrivacy && savedBirthPrivacy) {
        birthdayPrivacy.value = savedBirthPrivacy;
    }

    // Initialize location select
    const locationSelect = document.getElementById('locationSelect');
    const currentLocation = document.getElementById('currentLocation').value;

    if (locationSelect) {
        locationSelect.innerHTML = '<option value="">--</option>';

        const countries = [
            "Afghanistan", "Albania", "Algeria", "Andorra", "Angola", "Argentina",
            "Armenia", "Australia", "Austria", "Azerbaijan", "Bahamas", "Bahrain",
            "Bangladesh", "Belarus", "Belgium", "Belize", "Benin", "Bhutan",
            "Bolivia", "Brazil", "Brunei", "Bulgaria", "Cambodia", "Cameroon",
            "Canada", "Chile", "China", "Colombia", "Congo", "Costa Rica",
            "Croatia", "Cuba", "Cyprus", "Czech Republic", "Denmark", "Ecuador",
            "Egypt", "Estonia", "Ethiopia", "Finland", "France", "Georgia",
            "Germany", "Ghana", "Greece", "Guatemala", "Haiti", "Honduras",
            "Hungary", "Iceland", "India", "Indonesia", "Iran", "Iraq", "Ireland",
            "Israel", "Italy", "Jamaica", "Japan", "Jordan", "Kazakhstan", "Kenya",
            "Kuwait", "Laos", "Latvia", "Lebanon", "Libya", "Lithuania", "Malaysia",
            "Mexico", "Mongolia", "Morocco", "Myanmar", "Nepal", "Netherlands",
            "New Zealand", "Nigeria", "North Korea", "Norway", "Pakistan", "Panama",
            "Paraguay", "Peru", "Philippines", "Poland", "Portugal", "Qatar",
            "Romania", "Russia", "Saudi Arabia", "Serbia", "Singapore", "Slovakia",
            "Somalia", "South Africa", "South Korea", "Spain", "Sri Lanka",
            "Sweden", "Switzerland", "Syria", "Taiwan", "Thailand", "Turkey",
            "Ukraine", "United Arab Emirates", "United Kingdom", "United States",
            "Uruguay", "Uzbekistan", "Venezuela", "Vietnam", "Yemen", "Zimbabwe"
        ];

        countries.forEach(country => {
            const option = document.createElement('option');
            option.value = country;
            option.textContent = country;
            if (country === currentLocation) {
                option.selected = true;
            }
            locationSelect.appendChild(option);
        });
    }
}

function updateDaySelect() {
    const monthSelect = document.querySelector('select[name="BirthMonth"]');
    const yearSelect = document.querySelector('select[name="BirthYear"]');
    const daySelect = document.querySelector('select[name="BirthDay"]');

    if (!monthSelect || !yearSelect || !daySelect) return;

    const month = parseInt(monthSelect.value) || 1;
    const year = parseInt(yearSelect.value) || 2000;

    daySelect.innerHTML = '<option value="">--</option>';

    let daysInMonth;
    if ([4, 6, 9, 11].includes(month)) {
        daysInMonth = 30;
    } else if (month === 2) {
        const isLeapYear = (year % 4 === 0 && year % 100 !== 0) || (year % 400 === 0);
        daysInMonth = isLeapYear ? 29 : 28;
    } else {
        daysInMonth = 31;
    }

    for (let day = 1; day <= daysInMonth; day++) {
        const option = document.createElement('option');
        option.value = day;
        option.textContent = day;
        daySelect.appendChild(option);
    }
}

// Handle profile image preview
document.getElementById('profileImage').addEventListener('change', function(e) {
    const file = e.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function(e) {
            document.getElementById('profilePreview').src = e.target.result;
        }
        reader.readAsDataURL(file);
    }
});

// Xử lý preview cover image
document.getElementById('coverImage').addEventListener('change', function(e) {
    const file = e.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function(e) {
            const coverPreview = document.getElementById('coverPreview');
            coverPreview.src = e.target.result;
            coverPreview.style.display = 'block';
        }
        reader.readAsDataURL(file);
    }
});

// Thêm xử lý cho việc click vào ảnh đại diện
document.querySelector('.profile-image-container').addEventListener('click', function() {
    document.getElementById('profileImage').click();
});

// Thêm xử lý cho nút edit
document.querySelector('.edit-icon').addEventListener('click', function(e) {
    e.stopPropagation(); // Ngăn sự kiện click lan ra profile-image-container
    document.getElementById('profileImage').click();
});

// Character count for nickname
document.getElementById('nickname').addEventListener('input', function() {
    const maxLength = this.getAttribute('maxlength');
    const currentLength = this.value.length;
    document.querySelector('.char-count').textContent = `${currentLength}/${maxLength}`;
});

// Thêm hàm kiểm tra ngày sinh
function validateBirthDate() {
    const yearSelect = document.querySelector('select[name="BirthYear"]');
    const monthSelect = document.querySelector('select[name="BirthMonth"]');
    const daySelect = document.querySelector('select[name="BirthDay"]');

    const year = yearSelect.value;
    const month = monthSelect.value;
    const day = daySelect.value;

    // Nếu cả 3 trường đều trống, cho phép submit
    if (!year && !month && !day) {
        return true;
    }

    // Nếu có year nhưng không có month hoặc day
    if (year && (!month || !day)) {
        alert('Vui lòng chọn đầy đủ tháng và ngày');
        return false;
    }

    // Nếu có month hoặc day nhưng không có year
    if ((month || day) && !year) {
        alert('Vui lòng chọn năm sinh');
        return false;
    }

    return true;
}

// Sửa lại phần xử lý submit form
document.getElementById('editProfileForm').addEventListener('submit', async function(e) {
    e.preventDefault();
    
    // Kiểm tra ngày sinh trước khi submit
    if (!validateBirthDate()) {
        return;
    }

    const formData = new FormData(this);
    
    // Xử lý ngày sinh trước khi gửi
    const year = formData.get('BirthYear');
    const month = formData.get('BirthMonth');
    const day = formData.get('BirthDay');

    // Nếu có đầy đủ thông tin ngày sinh
    if (year && month && day) {
        // Định dạng ngày sinh theo yyyy-MM-dd
        const birthDate = `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`;
        formData.set('NgaySinh', birthDate);
    } else {
        // Nếu không có thông tin ngày sinh, gửi null
        formData.set('NgaySinh', '');
    }

    const coverImageInput = document.getElementById('coverImage');
    
    if (coverImageInput.files.length > 0) {
        formData.append('coverImage', coverImageInput.files[0]);
    }
    
    try {
        const response = await fetch(this.action, {
            method: 'POST',
            body: formData,
            headers: {
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            }
        });
        
        const result = await response.json();
        
        if (result.success) {
            window.location.reload();
        } else {
            alert(result.message || 'Có lỗi xảy ra khi cập nhật thông tin');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('Có lỗi xảy ra khi cập nhật thông tin');
    }
});

function openViewAllInfoModal() {
    const modal = document.getElementById('viewAllInfoModal');
    if (modal) {
        modal.style.display = 'block';
        document.body.classList.add('modal-open');
    } else {
        console.error('Modal element not found');
    }
}

function closeViewAllInfoModal() {
    document.getElementById('viewAllInfoModal').style.display = 'none';
    document.body.classList.remove('modal-open');
}

// Cập nhật sự kiện cho nút "Xem tất cả"
document.addEventListener('DOMContentLoaded', function() {
    // Các sự kiện đã có...
    
    // Thêm sự kiện cho nút xem tất cả thông tin
    const seeAllMediaBtn = document.querySelector('.see-all-media');
    if (seeAllMediaBtn) {
        seeAllMediaBtn.onclick = function(e) {
            e.preventDefault();
            openViewAllInfoModal();
        };
    }
});

// Thêm hàm để mở cuộc trò chuyện với người dùng
function openConversationWithUser(userId) {
    // Ngăn chặn event mặc định
    event.preventDefault();
    
    // Chuyển hướng đến trang tin nhắn với ID người dùng
    window.location.href = '/Messages/Index/' + userId;
}

document.addEventListener('DOMContentLoaded', function() {
    // Code hiện tại...
    
    // Thêm xử lý cho nút nhắn tin trong View All Info Modal
    const messageButtonInModal = document.querySelector('.user-info-modal .message-button-primary');
    if (messageButtonInModal) {
        messageButtonInModal.addEventListener('click', function(e) {
            e.preventDefault();
            const userId = this.getAttribute('data-user-id');
            openConversationWithUser(userId);
        });
    }
});

// Mở modal thêm tác phẩm nổi bật
window.openAddFeaturedModal = function() {
    console.log('Opening add featured modal');
    const modal = document.getElementById('addFeaturedModal');
    if (modal) {
        modal.style.display = 'block';
        document.body.classList.add('modal-open');
        loadUserArtworks();
    } else {
        console.error('Modal not found: addFeaturedModal');
    }
}

// Định nghĩa rõ hàm closeAddFeaturedModal ở phạm vi toàn cục
window.closeAddFeaturedModal = function() {
    const modal = document.getElementById('addFeaturedModal');
    if (modal) {
        modal.style.display = 'none';
        document.body.classList.remove('modal-open');
    }
}

// Thêm toggleFeaturedArtwork ở phạm vi toàn cục
window.toggleFeaturedArtwork = function(artworkId, element) {
    const isCurrentlyFeatured = element.classList.contains('is-featured');
    const shouldBeFeatured = !isCurrentlyFeatured;
    
    console.log(`Toggling featured status for artwork ${artworkId} to ${shouldBeFeatured}`);
    
    fetch('/User/ToggleFeaturedArtwork', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
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
                
                // Thêm vào grid
                const featuredGrid = document.getElementById('featured-grid');
                const addButton = featuredGrid.querySelector('.add-featured');
                
                const newFeaturedItem = document.createElement('div');
                newFeaturedItem.className = 'featured-item';
                newFeaturedItem.dataset.id = artworkId;
                
                newFeaturedItem.innerHTML = `
                    <a href="/Artwork/Display/${artworkId}">
                        <img src="${data.artwork.duongDanAnh}" alt="${data.artwork.tieuDe}">
                        <div class="featured-item-title">${data.artwork.tieuDe}</div>
                    </a>
                    <button class="remove-featured" onclick="removeFeatured(${artworkId})">
                        <i class="fas fa-times"></i>
                    </button>
                `;
                
                if (addButton) {
                    featuredGrid.insertBefore(newFeaturedItem, addButton);
                } else {
                    featuredGrid.appendChild(newFeaturedItem);
                }
            } else {
                element.classList.remove('is-featured');
                element.querySelector('.overlay-indicator').innerHTML = '<i class="fas fa-plus-circle"></i> Thêm';
                
                // Xóa khỏi grid
                const featuredItem = document.querySelector(`.featured-item[data-id="${artworkId}"]`);
                if (featuredItem) {
                    featuredItem.remove();
                }
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

window.removeFeatured = function(artworkId) {
    event.preventDefault();
    event.stopPropagation();
    
    if(confirm('Bạn có chắc muốn xóa tác phẩm này khỏi mục nổi bật?')) {
        toggleFeaturedArtwork(artworkId, document.querySelector(`.artwork-select-item[data-id="${artworkId}"]`));
    }
}

// Tải danh sách tác phẩm của người dùng
function loadUserArtworks() {
    const userId = document.querySelector('meta[name="user-id"]')?.content;
    console.log('Loading artworks for user:', userId);
    
    if (!userId) {
        console.error('Không tìm thấy ID người dùng');
        alert('Không thể xác định ID người dùng');
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
            console.log('Received data:', data);
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
        
        artworkElement.addEventListener('click', () => toggleFeaturedArtwork(artwork.maTranh, artworkElement));
        
        container.appendChild(artworkElement);
    });
    
    // Thêm tính năng tìm kiếm
    document.getElementById('artworkSearch').addEventListener('input', function() {
        const searchTerm = this.value.toLowerCase();
        document.querySelectorAll('.artwork-select-item').forEach(item => {
            const title = item.querySelector('.artwork-title').textContent.toLowerCase();
            item.style.display = title.includes(searchTerm) ? 'block' : 'none';
        });
    });
}
// Thêm chức năng đóng modal vào nút đóng
document.addEventListener('DOMContentLoaded', function() {
    // Đóng modal khi nhấn nút close
    const closeButtons = document.querySelectorAll('#addFeaturedModal .close');
    closeButtons.forEach(button => {
        button.onclick = function() {
            document.getElementById('addFeaturedModal').style.display = 'none';
            document.body.classList.remove('modal-open');
        };
    });
});
