$(document).ready(function () {
    $('#addForm').on('submit', function (e) {
        // Kiểm tra file ảnh
        const imageFile = $('input[name="ImageFile"]')[0].files[0];
        if (!imageFile) {
            e.preventDefault();
            alert('Vui lòng chọn file ảnh');
            return false;
        }
        
        // Kiểm tra tiêu đề
        const title = $('input[name="TieuDe"]').val().trim();
        if (!title) {
            e.preventDefault();
            alert('Vui lòng nhập tiêu đề');
            return false;
        }
        
        // Kiểm tra thể loại
        const categories = $('select[name="SelectedCategories"]').val();
        if (!categories || categories.length === 0) {
            e.preventDefault();
            alert('Vui lòng chọn ít nhất một thể loại');
            return false;
        }
        
        // Kiểm tra giá
        const price = $('input[name="Gia"]').val();
        if (!price || isNaN(price) || Number(price) < 0) {
            e.preventDefault();
            alert('Vui lòng nhập giá hợp lệ');
            return false;
        }
        
        return true;
    });

    // Hiển thị tên file khi chọn ảnh
    $('input[name="ImageFile"]').on('change', function() {
        var fileName = $(this).val().split('\\').pop();
        if (fileName) {
            $(this).next('.custom-file-label').html(fileName);
        }
    });

    // Thêm tag khi click vào tag gợi ý
    $('.clickable-tag').on('click', function() {
        var tagInput = $('input[name="TagsInput"]');
        var currentValue = tagInput.val();
        var newTag = $(this).text();
        
        if (currentValue) {
            tagInput.val(currentValue + ' ' + newTag);
        } else {
            tagInput.val(newTag);
        }
    });

    // Tự động ẩn thông báo sau 3 giây
    setTimeout(function () {
        $('.alert').alert('close');
    }, 3000);
});