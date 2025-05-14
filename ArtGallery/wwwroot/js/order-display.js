$(document).ready(function() {
    const price = parseFloat(artworkPrice);
    const maxQuantity = parseInt(maxStock);
    const quantityInput = $('#quantity');
    const totalPrice = $('#totalPrice');
    const summaryQuantity = $('#summaryQuantity');
    const summaryTotal = $('#summaryTotal');
    
    function updateTotal() {
        const quantity = parseInt(quantityInput.val());
        const total = price * quantity;
        
        totalPrice.text(formatCurrency(total) + ' VND');
        summaryQuantity.text(quantity);
        summaryTotal.text(formatCurrency(total) + ' VND');
        
        $('#totalAmount').val(total);
    }
    
    function formatCurrency(amount) {
        return new Intl.NumberFormat('vi-VN').format(amount);
    }

    // Khi submit form VNPay, cập nhật số tiền động
    $('#vnpayForm').on('submit', function () {
        var quantity = parseInt($('#quantity').val());
        var total = quantity * price;
        $('#vnpayAmount').val(total);
    });

    $('#decrease').click(function() {
        let value = parseInt(quantityInput.val());
        if (value > 1) {
            quantityInput.val(value - 1);
            updateTotal();
        }
    });
    
    $('#increase').click(function() {
        let value = parseInt(quantityInput.val());
        if (value < maxQuantity) {
            quantityInput.val(value + 1);
            updateTotal();
        }
    });
    
    quantityInput.on('input change', function() {
        let value = parseInt($(this).val()) || 1;
        
        if (value < 1) {
            $(this).val(1);
            value = 1;
        } else if (value > maxQuantity) {
            $(this).val(maxQuantity);
            value = maxQuantity;
        }
        
        updateTotal();
    });
    
    updateTotal();

    // Cập nhật số tiền trong modal khi thay đổi số lượng
    function updateTotalAmount() {
        var soLuong = parseInt($("#quantity").val());
        var tongTien = soLuong * price;
        
        // Định dạng tiền tệ
        var formattedAmount = formatCurrency(tongTien);
        
        // Cập nhật hiển thị
        $("#qrAmountText, #codAmountText").text(formattedAmount);
        $(".thanh-tien").text(formattedAmount);
    }
    
    // Xử lý khi nhấn nút Xóa khỏi giỏ hàng
    $("#btnRemoveFromCart").on('click', function() {
        var orderToRemove = $(this).data('order-id');
        
        Swal.fire({
            title: 'Xác nhận xóa?',
            text: "Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Xóa',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                // Gửi ajax request để xóa
                $.ajax({
                    url: '/Order/RemoveFromCart',
                    type: 'POST',
                    data: {
                        orderId: orderToRemove
                    },
                    success: function(response) {
                        if (response.success) {
                            Swal.fire({
                                title: 'Thành công!',
                                text: 'Đã xóa sản phẩm khỏi giỏ hàng',
                                icon: 'success'
                            }).then(() => {
                                // Reload trang để cập nhật giao diện
                                window.location.href = '/Order/Display/' + artworkId;
                            });
                        } else {
                            Swal.fire({
                                title: 'Lỗi!',
                                text: response.message,
                                icon: 'error'
                            });
                        }
                    },
                    error: function() {
                        Swal.fire({
                            title: 'Lỗi!',
                            text: 'Có lỗi xảy ra, vui lòng thử lại sau.',
                            icon: 'error'
                        });
                    }
                });
            }
        });
    });
    
    // Xử lý khi nhấn nút Thêm vào giỏ hàng
    $("#btnThemGioHang").on('click', function() {
        var soLuong = parseInt($("#quantity").val());
        var tongTien = soLuong * price;
        
        // Thêm vào giỏ hàng với trạng thái "Chờ xác nhận"
        $.ajax({
            url: '/Order/AddToCart',
            type: 'POST',
            data: {
                maTranh: artworkId,
                soLuong: soLuong,
                tongTien: tongTien,
                trangThai: "Chờ xác nhận",
                phuongThucThanhToan: "chưa xác định"
            },
            success: function(response) {
                if (response.success) {
                    // Đổi text của nút
                    $("#btnThemGioHang").html('<i class="fas fa-check me-2"></i>Đã thêm vào giỏ hàng');
                    $("#btnThemGioHang").removeClass('btn-success').addClass('btn-secondary');
                    $("#btnThemGioHang").prop('disabled', true);
                    
                    // Hiển thị thông báo
                    Swal.fire({
                        title: 'Thành công!',
                        text: 'Đã thêm vào giỏ hàng. Xem lịch sử đặt hàng?',
                        icon: 'success',
                        showCancelButton: true,
                        confirmButtonText: 'Xem lịch sử',
                        cancelButtonText: 'Tiếp tục mua sắm'
                    }).then((result) => {
                        if (result.isConfirmed) {
                            window.location.href = '/Order/History';
                        }
                    });
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: response.message,
                        icon: 'error'
                    });
                }
            },
            error: function() {
                Swal.fire({
                    title: 'Lỗi!',
                    text: 'Có lỗi xảy ra, vui lòng thử lại sau.',
                    icon: 'error'
                });
            }
        });
    });
    
    // Xử lý khi nhấn nút Xác nhận đặt hàng
    $("#btnXacNhanDatHang").on('click', function(e) {
        e.preventDefault();
        updateTotalAmount();
        var paymentModal = new bootstrap.Modal(document.getElementById('paymentModal'));
        paymentModal.show();
    });
    
    // Xử lý thanh toán QR thành công
    $("#btnSimulateQRPayment").click(function() {
        // Lưu đơn hàng với trạng thái "Đã đặt hàng" thay vì "Chờ xác nhận"
        saveOrder("Đã đặt hàng", "chuyển khoản");
    });

    // Xử lý thanh toán VNPAY
    $("#btnConfirmVNPAY").click(function() {
        // Không gọi ajax mà chỉ chuẩn bị form để submit
        var soLuong = parseInt($("#quantity").val());
        var tongTien = soLuong * price;

        // Cập nhật giá trị vào form
        $('#vnpayAmount').val(tongTien);
        
        // Thêm thông tin cần thiết vào form để xử lý sau khi thanh toán
        if (!$('#artwork-id').length) {
            $('<input>').attr({
                type: 'hidden',
                id: 'artwork-id',
                name: 'MaTranh',
                value: artworkId
            }).appendTo('#vnpayForm');
        }
        
        if (!$('#quantity-input').length) {
            $('<input>').attr({
                type: 'hidden',
                id: 'quantity-input',
                name: 'SoLuong',
                value: soLuong
            }).appendTo('#vnpayForm');
        }
        
        if (isInCart && orderId) {
            if (!$('#order-id-input').length) {
                $('<input>').attr({
                    type: 'hidden',
                    id: 'order-id-input',
                    name: 'OrderId',
                    value: orderId
                }).appendTo('#vnpayForm');
            }
        }
    });

    // Xử lý thanh toán COD
    $("#btnConfirmCOD").click(function() {
        var soLuong = parseInt($("#quantity").val());
        var tongTien = soLuong * price;
        
        // Gửi Ajax request để lưu đơn hàng với phương thức COD
        $.ajax({
            url: '/Order/PlaceOrder',
            type: 'POST',
            data: {
                maTranh: artworkId,
                soLuong: soLuong,
                tongTien: tongTien,
                trangThai: "Đã đặt hàng",
                phuongThucThanhToan: "thanh toán khi nhận hàng",
                orderId: isInCart ? orderId : null
            },
            success: function(response) {
                if (response.success) {
                    // Thông báo thành công rồi chuyển hướng
                    Swal.fire({
                        title: 'Đặt hàng thành công!',
                        text: 'Đơn hàng của bạn đã được xác nhận.',
                        icon: 'success',
                        confirmButtonText: 'Xem chi tiết'
                    }).then(() => {
                        window.location.href = '/Order/History';
                    });
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: response.message || 'Có lỗi xảy ra khi đặt hàng.',
                        icon: 'error'
                    });
                }
            },
            error: function() {
                Swal.fire({
                    title: 'Lỗi!',
                    text: 'Có lỗi xảy ra, vui lòng thử lại sau.',
                    icon: 'error'
                });
            }
        });
    });
    
    // Hàm lưu đơn hàng
    function saveOrder(trangThai, phuongThuc) {
        var soLuong = parseInt($("#quantity").val());
        var tongTien = soLuong * price;
        
        // Gửi Ajax request để lưu/cập nhật đơn hàng
        $.ajax({
            url: '/Order/PlaceOrder',
            type: 'POST',
            data: {
                maTranh: artworkId,
                soLuong: soLuong,
                tongTien: tongTien,
                trangThai: trangThai,
                phuongThucThanhToan: phuongThuc,
                orderId: isInCart ? orderId : null
            },
            success: function(response) {
                if (response.success) {
                    // Chuyển đến trang đặt hàng thành công
                    window.location.href = '/Order/OrderSuccess';
                } else {
                    Swal.fire({
                        title: 'Lỗi!',
                        text: response.message,
                        icon: 'error'
                    });
                }
            },
            error: function() {
                Swal.fire({
                    title: 'Lỗi!',
                    text: 'Có lỗi xảy ra, vui lòng thử lại sau.',
                    icon: 'error'
                });
            }
        });
    }

    // Xử lý sự kiện khi người dùng chọn đánh giá sao
    $('.rating-stars i').on('click', function() {
        const rating = $(this).data('rating');
        $('#selectedRating').val(rating);

        // Cập nhật hiển thị sao
        $('.rating-stars i').removeClass('fas').addClass('far');
        $('.rating-stars i').each(function() {
            if ($(this).data('rating') <= rating) {
                $(this).removeClass('far').addClass('fas');
            }
        });
    });

    // Hiển thị sao khi hover
    $('.rating-stars i').on('mouseenter', function() {
        const rating = $(this).data('rating');

        $('.rating-stars i').each(function() {
            if ($(this).data('rating') <= rating) {
                $(this).addClass('hover');
            }
        });
    }).on('mouseleave', function() {
        $('.rating-stars i').removeClass('hover');
    });
});
