$(document).ready(function() {
    // Xử lý sắp xếp bảng
    $('.sortable').click(function() {
        const column = $(this).data('sort');
        const table = $('#artworkTable');
        const rows = table.find('tbody tr').toArray();
        let direction = $(this).hasClass('asc') ? -1 : 1;
        
        // Đảo chiều sắp xếp nếu đã click trước đó
        $(this).toggleClass('asc', direction === 1);
        $(this).toggleClass('desc', direction === -1);
        
        // Reset arrows on other columns
        $('.sortable').not(this).removeClass('asc desc');
        
        // Sắp xếp dựa vào cột được chọn
        rows.sort(function(a, b) {
            let x, y;
            
            if (column === 'tieuDe') {
                x = $(a).find('td').eq(0).text().toLowerCase();
                y = $(b).find('td').eq(0).text().toLowerCase();
                return x > y ? direction : x < y ? -direction : 0;
            } 
            else if (column === 'gia') {
                x = parseFloat($(a).find('td').eq(2).text().replace(/\./g, '').replace(',', '.'));
                y = parseFloat($(b).find('td').eq(2).text().replace(/\./g, '').replace(',', '.'));
            }
            else if (column === 'daBan') {
                x = parseInt($(a).find('td').eq(3).text());
                y = parseInt($(b).find('td').eq(3).text());
            }
            else if (column === 'soLuongTon') {
                x = parseInt($(a).find('td').eq(4).text());
                y = parseInt($(b).find('td').eq(4).text());
            }
            else if (column === 'totalSales') {
                x = parseFloat($(a).find('td').eq(5).text().replace(/\./g, '').replace(',', '.'));
                y = parseFloat($(b).find('td').eq(5).text().replace(/\./g, '').replace(',', '.'));
            }
            else if (column === 'ngayDang') {
                let dateA = $(a).find('td').eq(6).text();
                let dateB = $(b).find('td').eq(6).text();
                x = dateA === '-' ? new Date(0) : new Date(dateA.split('/').reverse().join('-'));
                y = dateB === '-' ? new Date(0) : new Date(dateB.split('/').reverse().join('-'));
                return x > y ? direction : x < y ? -direction : 0;
            }
            
            return x > y ? direction : x < y ? -direction : 0;
        });
        
        // Thay thế các hàng hiện tại bằng hàng đã sắp xếp
        $.each(rows, function(index, row) {
            table.children('tbody').append(row);
        });
    });
    
    // Xử lý tìm kiếm
    $('#searchArtwork').on('keyup', function() {
        const value = $(this).val().toLowerCase();
        $('#artworkTable tbody tr').filter(function() {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
        $('#totalItems').text($('#artworkTable tbody tr:visible').length);
    });

    // Thêm chức năng click vào hàng để mở trang chi tiết
    $('#artworkTable tbody tr').click(function() {
        const artworkId = $(this).data('artwork-id');
        window.location.href = `/Artwork/Display/${artworkId}`;
    });
});
