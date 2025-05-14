$(document).ready(function() {
    // Khai báo biến ở phạm vi toàn cục trong function
    var selectedOrderId = null;
    var selectedOrderStatus = null;
    
    // Xử lý tab trực tiếp
    $('#orderTabs button').on('click', function (e) {
        e.preventDefault();
        $(this).tab('show');
    });
    
    // Xử lý xóa khỏi giỏ hàng
    $('.remove-from-cart').on('click', function() {
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
                                location.reload();
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
    
    // Khởi tạo các tab Bootstrap
    var triggerTabList = [].slice.call(document.querySelectorAll('#orderTabs button'))
    triggerTabList.forEach(function (triggerEl) {
        var tabTrigger = new bootstrap.Tab(triggerEl)
        triggerEl.addEventListener('click', function (event) {
            event.preventDefault()
            tabTrigger.show()
        })
    });
    
    // Lưu tab đang chọn vào local storage
    $('button[data-bs-toggle="tab"]').on('shown.bs.tab', function (e) {
        localStorage.setItem('activeOrderTab', $(e.target).attr('id'));
    });
    
    // Khôi phục tab đã chọn từ local storage
    var activeTab = localStorage.getItem('activeOrderTab');
    if(activeTab){
        $('#' + activeTab).tab('show');
    }

    // Thêm phần xử lý chuột phải cho lịch sử đặt hàng
    $("#history .table tbody tr").on("contextmenu", function(e) {
        e.preventDefault();
        
        // Lấy thông tin đơn hàng từ hàng được click
        var row = $(this);
        selectedOrderId = row.find("td:first").text();
        
        // Lấy chính xác trạng thái từ nội dung của badge
        var statusBadge = row.find("td:eq(5) .badge");
        selectedOrderStatus = statusBadge.text().trim();
        
        // Đánh dấu hàng đang được chọn
        $(".table tr").removeClass("selected");
        row.addClass("selected");
        
        // Ẩn tất cả các nút trước khi hiển thị
        $("#cancelOrder").hide();
        $("#receiveOrder").hide();
        $("#deleteHistory").hide();
        
        // Hiển thị menu ngữ cảnh dựa trên trạng thái đơn hàng
        if (selectedOrderStatus === "Đã xác nhận") {
            // Hiển thị cả nút hủy và nút đã nhận hàng
            $("#cancelOrder").show();
            $("#receiveOrder").show();
        } 
        else if (selectedOrderStatus === "Đã đặt hàng") {
            // Chỉ hiển thị nút hủy đơn hàng
            $("#cancelOrder").show();
        }
        else if (selectedOrderStatus === "Đã hoàn thành" || selectedOrderStatus === "Đã hủy") {
            // Chỉ hiển thị nút xóa lịch sử
            $("#deleteHistory").show();
        }
        
        // Hiển thị context menu tại vị trí chuột
        $("#contextMenu").css({
            top: e.pageY + "px",
            left: e.pageX + "px"
        }).show();
    });
    
    // Thêm phần xử lý chuột phải riêng biệt cho đơn bán hàng
    $("#selling .table tbody tr").on("contextmenu", function(e) {
        e.preventDefault();
        
        // Lấy thông tin đơn hàng từ hàng được click
        var row = $(this);
        selectedOrderId = row.find("td:first").text();
        
        // Lấy chính xác trạng thái
        var statusText = "";
        var statusBadge = row.find("td:eq(6) .badge");
        if (statusBadge.length > 0) {
            statusText = statusBadge.text().trim();
        }
        selectedOrderStatus = statusText;
        
        // Đánh dấu hàng đang được chọn
        $(".table tr").removeClass("selected");
        row.addClass("selected");
        
        // Ẩn tất cả các nút menu ngữ cảnh
        $("#deliverOrder").hide();
        $("#deleteSellerHistory").hide();
        
        // Hiển thị menu ngữ cảnh dựa trên trạng thái đơn hàng
        if (selectedOrderStatus === "Đã xác nhận") {
            // Hiển thị nút giao hàng khi đơn đã xác nhận
            $("#deliverOrder").show();
        }
        else if (selectedOrderStatus === "Đã hoàn thành" || selectedOrderStatus === "Đã hủy") {
            // Hiển thị nút xóa khi đơn đã hoàn thành hoặc đã hủy
            $("#deleteSellerHistory").show();
        }
        
        // Hiển thị context menu tại vị trí chuột
        $("#sellerContextMenu").css({
            top: e.pageY + "px",
            left: e.pageX + "px"
        }).show();
    });
    
    // Đóng context menu khi click ra ngoài
    $(document).on("click", function() {
        $("#contextMenu, #sellerContextMenu").hide();
    });
    
    // Xử lý khi chọn hủy đơn hàng
    $("#cancelOrder").on("click", function() {
        // Lấy phương thức thanh toán từ hàng đang được chọn
        var paymentMethod = $(".table tr.selected").find("td:eq(6) .badge").text().trim();
        var isVnpay = paymentMethod.includes("Chuyển khoản");
        
        if (isVnpay) {
            // Nếu là đơn thanh toán bằng chuyển khoản, hiển thị thông báo không thể hủy
            Swal.fire({
                title: 'Không thể hủy!',
                text: 'Đơn hàng đã được thanh toán bằng chuyển khoản, không thể hủy. Vui lòng liên hệ với người bán để được hỗ trợ.',
                icon: 'warning',
                confirmButtonColor: '#3085d6',
                confirmButtonText: 'Đã hiểu'
            });
            return;
        }
        
        if (selectedOrderStatus === "Đã đặt hàng") {
            // Nếu đơn hàng ở trạng thái "Đã đặt hàng" thì có thể hủy
            Swal.fire({
                title: 'Xác nhận hủy đơn hàng?',
                text: "Bạn có muốn hủy đơn hàng này không?",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Đồng ý hủy',
                cancelButtonText: 'Không hủy'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Gửi ajax request để hủy đơn hàng
                    $.ajax({
                        url: '/Order/UpdateOrderStatus',
                        type: 'POST',
                        data: {
                            orderId: selectedOrderId,
                            status: 'Đã hủy'
                        },
                        success: function(response) {
                            if (response.success) {
                                Swal.fire({
                                    title: 'Hủy thành công!',
                                    text: 'Đơn hàng của bạn đã được hủy',
                                    icon: 'success'
                                }).then(() => {
                                    location.reload();
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
        } 
        else if (selectedOrderStatus === "Đã xác nhận" || selectedOrderStatus === "Đã hoàn thành") {
            // Nếu đơn hàng đã được xác nhận thì không thể hủy
            Swal.fire({
                title: 'Không thể hủy!',
                text: 'Rất tiếc, đơn hàng này đã được xác nhận, bạn vui lòng liên hệ với người bán để được hủy đơn hàng',
                icon: 'warning',
                confirmButtonColor: '#3085d6',
                confirmButtonText: 'Đã hiểu'
            });
        }
        else if (selectedOrderStatus === "Đã hủy") {
            Swal.fire({
                title: 'Thông báo',
                text: 'Đơn hàng này đã được hủy trước đó',
                icon: 'info',
                confirmButtonColor: '#3085d6',
                confirmButtonText: 'Đã hiểu'
            });
        }
    });

    // Xử lý xác nhận đơn hàng
    $(document).on('click', '.btn-xacnhan', function() {
        var orderId = $(this).data('id');

        Swal.fire({
            title: 'Xác nhận đơn hàng?',
            text: "Bạn có chắc muốn xác nhận đơn hàng này?",
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#28a745',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Xác nhận',
            cancelButtonText: 'Không'
        }).then((result) => {
            if (result.isConfirmed) {
                // Gửi ajax request để xác nhận
                $.ajax({
                    url: '/Order/UpdateOrderStatus',
                    type: 'POST',
                    data: {
                        orderId: orderId,
                        status: 'Đã xác nhận'
                    },
                    success: function(response) {
                        if (response.success) {
                            Swal.fire({
                                title: 'Thành công!',
                                text: 'Đã xác nhận đơn hàng',
                                icon: 'success'
                            }).then(() => {
                                location.reload();
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

    // Xử lý hủy đơn hàng
    $(document).on('click', '.btn-huy', function() {
        var orderId = $(this).data('id');

        Swal.fire({
            title: 'Xác nhận hủy đơn hàng?',
            text: "Đơn hàng sẽ bị hủy và không thể khôi phục. Bạn có chắc chắn?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#dc3545',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Đồng ý hủy',
            cancelButtonText: 'Không hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                // Gửi ajax request để hủy
                $.ajax({
                    url: '/Order/UpdateOrderStatus',
                    type: 'POST',
                    data: {
                        orderId: orderId,
                        status: 'Đã hủy'
                    },
                    success: function(response) {
                        if (response.success) {
                            Swal.fire({
                                title: 'Thành công!',
                                text: 'Đã hủy đơn hàng',
                                icon: 'success'
                            }).then(() => {
                                location.reload();
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

    // Thêm xử lý khi click vào nút xóa lịch sử
    $("#deleteHistory").on("click", function() {
        if (selectedOrderStatus === "Đã hoàn thành" || selectedOrderStatus === "Đã hủy") {
            Swal.fire({
                title: 'Xác nhận xóa?',
                text: "Bạn có chắc muốn xóa đơn hàng này khỏi lịch sử?",
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
                        url: '/Order/DeleteHistory',
                        type: 'POST',
                        data: {
                            orderId: selectedOrderId
                        },
                        success: function(response) {
                            if (response.success) {
                                Swal.fire({
                                    title: 'Thành công!',
                                    text: 'Đã xóa đơn hàng khỏi lịch sử',
                                    icon: 'success'
                                }).then(() => {
                                    location.reload();
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
        }
    });

    // Thêm xử lý khi click vào nút xóa lịch sử của người bán
    $("#deleteSellerHistory").on("click", function() {
        if (selectedOrderStatus === "Đã hoàn thành" || selectedOrderStatus === "Đã hủy") {
            Swal.fire({
                title: 'Xác nhận xóa?',
                text: "Bạn có chắc muốn xóa đơn hàng này khỏi danh sách bán hàng? Đơn hàng sẽ không hiển thị trong mục đơn bán hàng nữa.",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: 'Xóa',
                cancelButtonText: 'Hủy'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Gửi ajax request để ẩn
                    $.ajax({
                        url: '/Order/HideSellerHistory',
                        type: 'POST',
                        data: {
                            orderId: selectedOrderId
                        },
                        success: function(response) {
                            if (response.success) {
                                Swal.fire({
                                    title: 'Thành công!',
                                    text: 'Đã xóa đơn hàng khỏi danh sách bán hàng',
                                    icon: 'success'
                                }).then(() => {
                                    location.reload();
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
        }
    });

    // Xử lý nút xóa tất cả lịch sử đặt hàng
    $("#deleteAllHistory").on("click", function() {
        Swal.fire({
            title: 'Xác nhận xóa tất cả?',
            text: "Bạn muốn xóa lịch sử? Chỉ có những giao dịch đã hoàn thành hoặc đã hủy mới có thể xóa.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Xóa tất cả',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                // Gửi ajax request để xóa tất cả
                $.ajax({
                    url: '/Order/DeleteAllHistory',
                    type: 'POST',
                    success: function(response) {
                        if (response.success) {
                            Swal.fire({
                                title: 'Thành công!',
                                text: `Đã xóa ${response.count} đơn hàng khỏi lịch sử`,
                                icon: 'success'
                            }).then(() => {
                                location.reload();
                            });
                        } else {
                            Swal.fire({
                                title: 'Thông báo!',
                                text: response.message,
                                icon: 'info'
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

    // Xử lý nút xóa tất cả lịch sử bán hàng
    $("#deleteAllSellerHistory").on("click", function() {
        Swal.fire({
            title: 'Xác nhận xóa tất cả?',
            text: "Bạn muốn xóa lịch sử? Chỉ có những giao dịch đã hoàn thành hoặc đã hủy mới có thể xóa.",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: 'Xóa tất cả',
            cancelButtonText: 'Hủy'
        }).then((result) => {
            if (result.isConfirmed) {
                // Gửi ajax request để xóa tất cả
                $.ajax({
                    url: '/Order/DeleteAllSellerHistory',
                    type: 'POST',
                    success: function(response) {
                        if (response.success) {
                            Swal.fire({
                                title: 'Thành công!',
                                text: `Đã xóa ${response.count} đơn hàng khỏi lịch sử bán hàng`,
                                icon: 'success'
                            }).then(() => {
                                location.reload();
                            });
                        } else {
                            Swal.fire({
                                title: 'Thông báo!',
                                text: response.message,
                                icon: 'info'
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

    // Thêm xử lý cho nút "Đã nhận hàng" 
    $("#receiveOrder").on("click", function() {
        if (selectedOrderStatus === "Đã xác nhận") {
            Swal.fire({
                title: 'Xác nhận đã nhận hàng?',
                text: "Bạn xác nhận đã nhận được hàng phải không?",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Xác nhận',
                cancelButtonText: 'Hủy'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Gửi ajax request để cập nhật trạng thái
                    $.ajax({
                        url: '/Order/UpdateOrderStatus',
                        type: 'POST',
                        data: {
                            orderId: selectedOrderId,
                            status: 'Đã hoàn thành'
                        },
                        success: function(response) {
                            if (response.success) {
                                Swal.fire({
                                    title: 'Thành công!',
                                    text: 'Đã xác nhận nhận hàng thành công',
                                    icon: 'success'
                                }).then(() => {
                                    location.reload();
                                });
                            } else {
                                Swal.fire({
                                    title: 'Lỗi!',
                                    text: response.message || 'Không thể cập nhật trạng thái đơn hàng',
                                    icon: 'error'
                                });
                            }
                        },
                        error: function(xhr) {
                            Swal.fire({
                                title: 'Lỗi!',
                                text: 'Có lỗi xảy ra, vui lòng thử lại sau.',
                                icon: 'error'
                            });
                        }
                    });
                }
            });
        }
    });

    // Thêm xử lý cho nút "Đã giao hàng" trong menu ngữ cảnh của người bán
    $("#deliverOrder").on("click", function() {
        if (selectedOrderStatus === "Đã xác nhận") {
            Swal.fire({
                title: 'Xác nhận đã giao hàng?',
                text: "Bạn xác nhận đã giao hàng đến người mua?",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#28a745',
                cancelButtonColor: '#6c757d',
                confirmButtonText: 'Xác nhận',
                cancelButtonText: 'Hủy'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Gửi ajax request để cập nhật trạng thái
                    $.ajax({
                        url: '/Order/UpdateOrderStatus',
                        type: 'POST',
                        data: {
                            orderId: selectedOrderId,
                            status: 'Đã hoàn thành'
                        },
                        success: function(response) {
                            if (response.success) {
                                Swal.fire({
                                    title: 'Thành công!',
                                    text: 'Đã cập nhật trạng thái đơn hàng thành Đã hoàn thành',
                                    icon: 'success'
                                }).then(() => {
                                    location.reload();
                                });
                            } else {
                                Swal.fire({
                                    title: 'Lỗi!',
                                    text: response.message || 'Không thể cập nhật trạng thái đơn hàng',
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
        }
    });
}); 