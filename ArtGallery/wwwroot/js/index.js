const swiper = new Swiper('.mySwiper', {
    slidesPerView: 3,        // Hiển thị chính xác 3 slides
    spaceBetween: 30,       // Khoảng cách nhỏ giữa các slides
    loop: true,             // Lặp vô tận
    autoplay: {
        delay: 2000,
        disableOnInteraction: false,
    },
    navigation: {
        nextEl: '.swiper-button-next',
        prevEl: '.swiper-button-prev',
    },
    breakpoints: {
        320: {
            slidesPerView: 1,
            spaceBetween: 20
        },
        768: {
            slidesPerView: 2,
            spaceBetween: 30
        },
        1024: {
            slidesPerView: 3,
            spaceBetween: 30
        }
    }
});

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

// Thêm đoạn này vào cuối file index.js để xử lý active state cho các tab
document.addEventListener('DOMContentLoaded', function() {
    // Lấy tất cả các phần tử tag-items
    const tagItems = document.querySelectorAll('.tag-items');
    
    // Thêm sự kiện click cho mỗi tag
    tagItems.forEach(item => {
        item.addEventListener('click', function(e) {
            // Loại bỏ class active khỏi tất cả các tag
            tagItems.forEach(tag => tag.classList.remove('active'));
            
            // Thêm class active cho tag được click
            this.classList.add('active');
            
            // Nếu bạn muốn ngăn chặn chuyển trang mặc định
            // e.preventDefault();
            
            // Code để lọc tranh theo tag có thể được thêm vào đây
        });
    });
});
