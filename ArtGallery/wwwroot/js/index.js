
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

function toggleLike(button) {
    // Toggle class active
    button.classList.toggle('active');

    // Thay đổi icon
    const icon = button.querySelector('i');
    if (button.classList.contains('active')) {
        icon.classList.remove('far');
        icon.classList.add('fas');
    } else {
        icon.classList.remove('fas');
        icon.classList.add('far');
    }
}
