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

// Script để thêm tag khi click vào recommended tag
document.addEventListener('DOMContentLoaded', function () {
    const tagsInput = document.querySelector('input[name="TagsInput"]');
    const clickableTags = document.querySelectorAll('.clickable-tag');

    clickableTags.forEach(tag => {
        tag.addEventListener('click', function () {
            const tagText = this.textContent.trim();

            // Thêm tag vào input nếu chưa có
            if (tagsInput.value.indexOf(tagText) === -1) {
                if (tagsInput.value.trim() !== '') {
                    tagsInput.value += ' ' + tagText;
                } else {
                    tagsInput.value = tagText;
                }
            }
        });
    });
});