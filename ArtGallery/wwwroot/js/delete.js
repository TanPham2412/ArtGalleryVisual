function confirmDelete(artworkId) {
    if (confirm('Bạn có chắc chắn muốn xóa tranh này không?')) {
        var token = $('input[name="__RequestVerificationToken"]').val();
        if (!token) {
            alert('Lỗi: Không tìm thấy token xác thực. Vui lòng tải lại trang.');
            return;
        }
        
        $.ajax({
            url: '/Artwork/Delete/' + artworkId,
            type: 'POST',
            headers: {
                'RequestVerificationToken': token
            },
            success: function (response) {
                if (response.success) {
                    alert(response.message);
                    window.location.href = response.redirectUrl;
                } else {
                    alert(response.message);
                }
            },
            error: function (xhr, status, error) {
                console.error('Lỗi khi xóa:', xhr.responseText);
                alert('Có lỗi xảy ra khi xóa tranh: ' + error);
            }
        });
    }
}