document.addEventListener("DOMContentLoaded", function () {
    // Xử lý slideshow
    const slides = document.querySelectorAll('.slide');
    let currentSlide = 0;
    function nextSlide() {
        // Ẩn slide hiện tại
        slides[currentSlide].classList.remove('active');

        // Chuyển đến slide tiếp theo
        currentSlide = (currentSlide + 1) % slides.length;

        // Hiển thị slide mới
        slides[currentSlide].classList.add('active');
    }

    // Chuyển slide mỗi 5 giây
    setInterval(nextSlide, 5000);
});

function togglePassword(button) {
    const passwordInput = button.previousElementSibling;
    const icon = button.querySelector('i');

    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
        icon.classList.remove('fa-eye');
        icon.classList.add('fa-eye-slash');
    } else {
        passwordInput.type = 'password';
        icon.classList.remove('fa-eye-slash');
        icon.classList.add('fa-eye');
    }
}
