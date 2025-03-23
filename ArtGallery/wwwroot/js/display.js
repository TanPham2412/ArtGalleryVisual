function confirmDelete(artworkId) {
    if (confirm('Bạn có chắc chắn muốn xóa tranh này không?')) {
        $.ajax({
            url: '/Artwork/Delete/' + artworkId,
            type: 'POST',
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                if (response.success) {
                    alert(response.message);
                    window.location.href = response.redirectUrl;
                } else {
                    alert(response.message);
                }
            },
            error: function () {
                alert('Có lỗi xảy ra khi xóa tranh');
            }
        });
    }
}