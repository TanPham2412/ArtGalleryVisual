document.addEventListener('DOMContentLoaded', function () {
    // Xử lý nút search option (nếu cần thêm chức năng)
    const searchOptionBtn = document.querySelector('.search-option-btn');
    if (searchOptionBtn) {
        searchOptionBtn.addEventListener('click', function () {
            // Có thể thêm chức năng hiển thị popup tùy chọn tìm kiếm nâng cao
            alert('Tính năng đang phát triển!');
        });
    }

    // Đảm bảo scroll container hoạt động tốt
    const categoriesTab = document.querySelector('.categories-tabs');
    if (categoriesTab) {
        // Tìm tab active
        const activeTab = categoriesTab.querySelector('.category-tab.active');
        if (activeTab) {
            // Scroll đến tab active
            setTimeout(() => {
                activeTab.scrollIntoView({ behavior: 'smooth', block: 'nearest', inline: 'center' });
            }, 100);
        }
    }
});