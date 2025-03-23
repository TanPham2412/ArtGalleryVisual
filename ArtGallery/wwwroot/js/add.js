$(document).ready(function () {
    $('#addForm').on('submit', function (e) {
        e.preventDefault();
        var formData = new FormData(this);

        $.ajax({
            url: '@Url.Action("Add", "Home")',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                // Reload trang để hiển thị thông báo
                location.reload();
            },
            error: function (xhr, status, error) {
                console.error('Error:', error);
                alert('Có lỗi xảy ra khi đăng tranh');
            }
        });
    });

    // Tự động ẩn thông báo sau 3 giây
    setTimeout(function () {
        $('.alert').alert('close');
    }, 3000);
});