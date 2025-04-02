$(document).ready(function() {
    console.log("Theloai.js đã được tải");
    
    // Xử lý sự kiện hover cho các sản phẩm
    $('.pic-item').hover(
        function() {
            $(this).find('.artwork-title').css('color', '#0096fa');
        },
        function() {
            $(this).find('.artwork-title').css('color', '#333');
        }
    );
});
