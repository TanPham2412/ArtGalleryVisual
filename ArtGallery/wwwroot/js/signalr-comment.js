// Kết nối SignalR cho bình luận và phản hồi theo thời gian thực

// Biến lưu trữ kết nối SignalR
let commentConnection;

// Khởi tạo kết nối SignalR
function initializeCommentHub(artworkId) {
    console.log('Bắt đầu khởi tạo kết nối SignalR với artworkId:', artworkId);
    
    // Tạo kết nối đến CommentHub
    commentConnection = new signalR.HubConnectionBuilder()
        .withUrl('/commentHub')
        .withAutomaticReconnect([0, 2000, 5000, 10000, 15000, 30000]) // Cấu hình thời gian thử lại kết nối
        .configureLogging(signalR.LogLevel.Information) // Thay đổi mức log để dễ theo dõi
        .build();
    
    console.log('Đã tạo đối tượng kết nối SignalR:', commentConnection);

    // Xử lý sự kiện nhận bình luận mới
    commentConnection.on('ReceiveComment', function (comment) {
        console.log('%c[SignalR] ReceiveComment được gọi', 'background: #4CAF50; color: white; padding: 2px 5px; border-radius: 3px;');
        console.log('Dữ liệu bình luận nhận được:', comment);
        
        try {
            // Kiểm tra dữ liệu bình luận
            if (!comment || !comment.id) {
                console.error('Dữ liệu bình luận không hợp lệ:', comment);
                return;
            }
            
            // Kiểm tra xem bình luận đã tồn tại chưa
            const existingComment = document.getElementById(`comment-${comment.id}`);
            if (existingComment) {
                console.log(`Bình luận với ID ${comment.id} đã tồn tại, không thêm lại`);
                return;
            }
            
            // Tạo HTML cho bình luận mới
            console.log('Tạo HTML cho bình luận mới');
            const commentHtml = createCommentHtml(comment);
            console.log('HTML đã tạo:', commentHtml.substring(0, 100) + '...');
            
            // Tìm container comments-list
            console.log('Tìm container .comments-list');
            const commentsContainer = document.querySelector('.comments-list');
            console.log('Container tìm thấy:', commentsContainer);
            
            if (commentsContainer) {
                // Sử dụng DOM API thay vì jQuery
                console.log('Thêm bình luận vào DOM');
                const tempDiv = document.createElement('div');
                tempDiv.innerHTML = commentHtml.trim();
                const commentElement = tempDiv.firstElementChild;
                
                if (!commentElement) {
                    console.error('Không thể tạo phần tử bình luận từ HTML');
                    console.log('HTML gốc:', commentHtml);
                    return;
                }
                
                // Thêm vào đầu danh sách
                if (commentsContainer.firstChild) {
                    commentsContainer.insertBefore(commentElement, commentsContainer.firstChild);
                    console.log('Đã thêm bình luận vào đầu danh sách');
                } else {
                    commentsContainer.appendChild(commentElement);
                    console.log('Đã thêm bình luận vào container rỗng');
                }
                
                console.log('%c Đã thêm bình luận mới vào DOM với ID: ' + comment.id, 'background: #2196F3; color: white; padding: 2px 5px; border-radius: 3px;');
                 
                // Làm nổi bật bình luận mới
                commentElement.classList.add('new-comment-highlight');
                // Cuộn đến bình luận mới
                commentElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
                setTimeout(() => {
                    commentElement.classList.remove('new-comment-highlight');
                }, 3000);
            } else {
                console.error('Không tìm thấy container .comments-list');
                // Tìm kiếm các container khác có thể chứa bình luận
                const possibleContainers = document.querySelectorAll('.comments-list, #comments-list, .comment-list, #comment-list, .comments');
                console.log('Các container có thể chứa bình luận:', possibleContainers);
                
                if (possibleContainers.length > 0) {
                    console.log('Thử thêm vào container đầu tiên tìm thấy:', possibleContainers[0]);
                    const tempDiv = document.createElement('div');
                    tempDiv.innerHTML = commentHtml.trim();
                    const commentElement = tempDiv.firstElementChild;
                    
                    if (commentElement) {
                        possibleContainers[0].insertBefore(commentElement, possibleContainers[0].firstChild);
                        console.log('Đã thêm bình luận vào container thay thế:', possibleContainers[0]);
                        
                        // Làm nổi bật bình luận mới
                        commentElement.classList.add('new-comment-highlight');
                        // Cuộn đến bình luận mới
                        commentElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
                        setTimeout(() => {
                            commentElement.classList.remove('new-comment-highlight');
                        }, 3000);
                    }
                }
            }
            
            // Cập nhật số lượng bình luận
            updateCommentCount(1);
        } catch (error) {
            console.error('Lỗi khi xử lý bình luận mới:', error);
            console.error('Stack trace:', error.stack);
        }
    });

    // Xử lý sự kiện nhận phản hồi mới
    commentConnection.on('ReceiveReply', function (commentId, reply) {
        console.log('%c[SignalR] ReceiveReply được gọi', 'background: #4CAF50; color: white; padding: 2px 5px; border-radius: 3px;');
        console.log('Nhận phản hồi mới cho bình luận ID:', commentId);
        console.log('Dữ liệu phản hồi:', reply);
        
        try {
            // Kiểm tra dữ liệu phản hồi
            if (!reply || !reply.id || !commentId) {
                console.error('Dữ liệu phản hồi không hợp lệ:', reply);
                return;
            }
            
            // Kiểm tra xem phản hồi đã tồn tại chưa
            const existingReply = document.getElementById(`reply-${reply.id}`);
            if (existingReply) {
                console.log(`Phản hồi với ID ${reply.id} đã tồn tại, không thêm lại`);
                return;
            }
            
            // Tạo HTML cho phản hồi mới
            console.log('Tạo HTML cho phản hồi mới');
            const replyHtml = createReplyHtml(reply);
            console.log('HTML đã tạo:', replyHtml.substring(0, 100) + '...');
            
            // Tìm container chứa phản hồi
            console.log(`Tìm container replies-container-${commentId}`);
            const repliesContainer = document.getElementById(`replies-container-${commentId}`);
            console.log('Container tìm thấy:', repliesContainer);
            
            if (repliesContainer) {
                // Sử dụng DOM API thay vì jQuery
                console.log('Thêm phản hồi vào DOM');
                const tempDiv = document.createElement('div');
                tempDiv.innerHTML = replyHtml.trim();
                const replyElement = tempDiv.firstElementChild;
                
                if (!replyElement) {
                    console.error('Không thể tạo phần tử phản hồi từ HTML');
                    console.log('HTML gốc:', replyHtml);
                    return;
                }
                
                // Kiểm tra xem avatar có được tạo đúng không
                const avatarImg = replyElement.querySelector('.reply-user-avatar');
                console.log('Avatar img element:', avatarImg);
                if (avatarImg) {
                    console.log('Avatar src:', avatarImg.src);
                    console.log('Avatar alt:', avatarImg.alt);
                    
                    // Ngăn chặn chớp nháy bằng cách thiết lập style trực tiếp
                    avatarImg.style.transition = 'none';
                    avatarImg.style.animation = 'none';
                    
                    // Xử lý lỗi khi tải avatar
                    avatarImg.onerror = function() {
                        console.error('Lỗi khi tải avatar, sử dụng ảnh mặc định');
                        this.src = '/images/default-avatar.png';
                    };
                } else {
                    console.error('Không tìm thấy avatar img element');
                }
                
                // Thêm vào cuối danh sách phản hồi
                repliesContainer.appendChild(replyElement);
                console.log('Đã thêm phản hồi mới vào DOM với ID:', reply.id);
                
                // Làm nổi bật phản hồi mới
                replyElement.classList.add('new-reply-highlight');
                setTimeout(() => {
                    replyElement.classList.remove('new-reply-highlight');
                }, 3000);
                
                // Cuộn đến phản hồi mới
                replyElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
                
                // Cập nhật số lượng phản hồi
                updateReplyCount(commentId, 1);
                
                // Hiển thị container phản hồi nếu đang ẩn
                repliesContainer.style.display = 'block';
            } else {
                console.error(`Không tìm thấy container replies-container-${commentId}`);
                // Tìm kiếm bình luận để thêm container phản hồi nếu cần
                const commentElement = document.getElementById(`comment-${commentId}`);
                if (commentElement) {
                    console.log(`Tìm thấy bình luận với ID ${commentId}, tạo container phản hồi mới`);
                    const newRepliesContainer = document.createElement('div');
                    newRepliesContainer.id = `replies-container-${commentId}`;
                    newRepliesContainer.className = 'replies-container';
                    commentElement.appendChild(newRepliesContainer);
                    
                    // Thêm phản hồi vào container mới
                    const tempDiv = document.createElement('div');
                    tempDiv.innerHTML = replyHtml.trim();
                    const replyElement = tempDiv.firstElementChild;
                    
                    if (replyElement) {
                        newRepliesContainer.appendChild(replyElement);
                        console.log('Đã thêm phản hồi vào container mới tạo');
                        
                        // Làm nổi bật phản hồi mới
                        replyElement.classList.add('new-reply-highlight');
                        // Cuộn đến phản hồi mới
                        replyElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
                        setTimeout(() => {
                            replyElement.classList.remove('new-reply-highlight');
                        }, 3000);
                        
                        // Cập nhật số lượng phản hồi
                        updateReplyCount(commentId, 1);
                    }
                } else {
                    console.error(`Không tìm thấy bình luận với ID ${commentId}`);
                }
            }
        } catch (error) {
            console.error('Lỗi khi xử lý phản hồi mới:', error);
            console.error('Stack trace:', error.stack);
        }
    });

    // Xử lý sự kiện bình luận bị xóa
    commentConnection.on('CommentDeleted', function (commentId) {
        // Xóa bình luận khỏi DOM
        $(`#comment-${commentId}`).fadeOut(300, function () {
            $(this).remove();
            // Cập nhật số lượng bình luận
            updateCommentCount(-1);
        });
    });

    // Xử lý sự kiện phản hồi bị xóa
    commentConnection.on('ReplyDeleted', function (replyId, commentId) {
        // Xóa phản hồi khỏi DOM
        $(`#reply-${replyId}`).fadeOut(300, function () {
            $(this).remove();
            // Cập nhật số lượng phản hồi
            updateReplyCount(commentId, -1);
        });
    });

    // Xử lý sự kiện bình luận được chỉnh sửa
    commentConnection.on('CommentEdited', function (commentId, updatedComment) {
        // Cập nhật nội dung bình luận
        $(`#comment-content-${commentId}`).html(updatedComment.content);
        
        // Cập nhật ảnh nếu có
        if (updatedComment.imagePath) {
            if ($(`#comment-image-${commentId}`).length) {
                $(`#comment-image-${commentId}`).attr('src', updatedComment.imagePath);
                $(`#comment-image-container-${commentId}`).removeClass('d-none');
            } else {
                const imageHtml = `<div id="comment-image-container-${commentId}" class="comment-image-container">
                    <img id="comment-image-${commentId}" src="${updatedComment.imagePath}" class="img-fluid rounded zoomable-image" alt="Comment image" onclick="openImageModal('${updatedComment.imagePath}')">
                </div>`;
                $(`#comment-content-${commentId}`).after(imageHtml);
            }
        } else {
            $(`#comment-image-container-${commentId}`).addClass('d-none');
        }
        
        // Cập nhật sticker nếu có
        if (updatedComment.sticker) {
            if ($(`#comment-sticker-${commentId}`).length) {
                $(`#comment-sticker-${commentId}`).attr('src', updatedComment.sticker);
                $(`#comment-sticker-container-${commentId}`).removeClass('d-none');
            } else {
                const stickerHtml = `<div id="comment-sticker-container-${commentId}" class="sticker-container">
                    <img id="comment-sticker-${commentId}" src="${updatedComment.sticker}" class="comment-sticker" alt="Sticker">
                </div>`;
                $(`#comment-content-${commentId}`).after(stickerHtml);
            }
        } else {
            $(`#comment-sticker-container-${commentId}`).addClass('d-none');
        }
        
        // Hiển thị trạng thái đã chỉnh sửa
        $(`#comment-edited-${commentId}`).removeClass('d-none');
    });

    // Xử lý sự kiện phản hồi được chỉnh sửa
    commentConnection.on('ReplyEdited', function (replyId, updatedReply) {
        // Cập nhật nội dung phản hồi
        $(`#reply-content-${replyId}`).html(updatedReply.content);
        
        // Cập nhật ảnh nếu có
        if (updatedReply.imagePath) {
            if ($(`#reply-image-${replyId}`).length) {
                $(`#reply-image-${replyId}`).attr('src', updatedReply.imagePath);
                $(`#reply-image-container-${replyId}`).removeClass('d-none');
            } else {
                const imageHtml = `<div id="reply-image-container-${replyId}" class="reply-image-container">
                    <img id="reply-image-${replyId}" src="${updatedReply.imagePath}" class="reply-image zoomable-image" alt="Reply image" onclick="openImageModal('${updatedReply.imagePath}')">
                </div>`;
                $(`#reply-content-${replyId}`).after(imageHtml);
            }
        } else {
            $(`#reply-image-container-${replyId}`).addClass('d-none');
        }
        
        // Cập nhật sticker nếu có
        if (updatedReply.sticker) {
            if ($(`#reply-sticker-${replyId}`).length) {
                $(`#reply-sticker-${replyId}`).attr('src', updatedReply.sticker);
                $(`#reply-sticker-container-${replyId}`).removeClass('d-none');
            } else {
                const stickerHtml = `<div id="reply-sticker-container-${replyId}" class="sticker-container">
                    <img id="reply-sticker-${replyId}" src="${updatedReply.sticker}" class="reply-sticker" alt="Sticker">
                </div>`;
                $(`#reply-content-${replyId}`).after(stickerHtml);
            }
        } else {
            $(`#reply-sticker-container-${replyId}`).addClass('d-none');
        }
        
        // Hiển thị trạng thái đã chỉnh sửa
        $(`#reply-edited-${replyId}`).removeClass('d-none');
    });

    // Xử lý sự kiện kết nối/ngắt kết nối
    commentConnection.onclose(error => {
        console.log('%c[SignalR] Kết nối đã đóng', 'background: #F44336; color: white; padding: 2px 5px; border-radius: 3px;');
        if (error) {
            console.error('Lỗi kết nối:', error);
        }
        
        // Kiểm tra xem trang có đang hiển thị không
        if (!document.hidden) {
            console.log('Trang đang hiển thị, thử kết nối lại sau 5 giây...');
            setTimeout(() => {
                console.log('Đang thử kết nối lại...');
                commentConnection.start()
                    .then(() => {
                        console.log('%c[SignalR] Kết nối lại thành công', 'background: #4CAF50; color: white; padding: 2px 5px; border-radius: 3px;');
                        // Tham gia lại nhóm artwork
                        const artworkIdElement = document.getElementById('artwork-id');
                        if (artworkIdElement) {
                            const artworkId = parseInt(artworkIdElement.value);
                            if (!isNaN(artworkId) && artworkId > 0) {
                                joinArtworkGroup(artworkId);
                            }
                        }
                    })
                    .catch(err => {
                        console.error('Không thể kết nối lại:', err);
                    });
            }, 5000);
        }
    });
    
    // Bắt đầu kết nối
    console.log('%c[SignalR] Bắt đầu kết nối...', 'background: #FF9800; color: white; padding: 2px 5px; border-radius: 3px;');
    commentConnection.start()
        .then(function () {
            console.log('%c[SignalR] Kết nối thành công!', 'background: #4CAF50; color: white; padding: 2px 5px; border-radius: 3px;');
            // Tham gia nhóm của tác phẩm
            console.log('Tham gia nhóm artwork_' + artworkId);
            
            // Đảm bảo rằng chúng ta đã tham gia nhóm trước khi tiếp tục
            return joinArtworkGroup(artworkId);
        })
        .then(function() {
            // Kiểm tra kết nối đã được thiết lập
            console.log('Trạng thái kết nối sau khi tham gia nhóm:', commentConnection.state);
            
            // Kiểm tra xem các sự kiện đã được đăng ký chưa
            const events = commentConnection.methods || {};
            console.log('Các sự kiện đã đăng ký:', Object.keys(events));
            
            // Kiểm tra xem container comments-list có tồn tại không
            const commentsContainer = document.querySelector('.comments-list');
            console.log('Container comments-list:', commentsContainer ? 'Tìm thấy' : 'Không tìm thấy');
            
            // Thiết lập kiểm tra kết nối định kỳ
            setInterval(() => {
                if (commentConnection && commentConnection.state !== 'Connected') {
                    console.log('%c[SignalR] Kiểm tra định kỳ: Kết nối không hoạt động, trạng thái:', 'background: #FF9800; color: white;', commentConnection.state);
                    commentConnection.start()
                        .then(() => {
                            console.log('%c[SignalR] Kết nối lại thành công', 'background: #4CAF50; color: white;');
                            // Tham gia lại nhóm artwork
                            const artworkId = parseInt(document.getElementById('artwork-id')?.value);
                            if (!isNaN(artworkId) && artworkId > 0) {
                                joinArtworkGroup(artworkId);
                            }
                        })
                        .catch(err => {
                            console.error('Không thể kết nối lại:', err);
                        });
                } else {
                    console.log('%c[SignalR] Kiểm tra định kỳ: Kết nối OK', 'color: #4CAF50;');
                }
            }, 30000); // Kiểm tra mỗi 30 giây
            
            // Thêm một kiểm tra kết nối định kỳ
            setInterval(function() {
                if (commentConnection && commentConnection.state === 'Connected') {
                    console.log('Kết nối SignalR vẫn hoạt động');
                } else {
                    console.warn('Kết nối SignalR không hoạt động, trạng thái:', commentConnection ? commentConnection.state : 'không có kết nối');
                    // Thử kết nối lại
                    if (commentConnection && commentConnection.state !== 'Connecting') {
                        console.log('Đang thử kết nối lại...');
                        commentConnection.start()
                            .then(() => joinArtworkGroup(artworkId))
                            .catch(err => console.error('Lỗi khi kết nối lại:', err));
                    }
                }
            }, 30000); // Kiểm tra mỗi 30 giây
        })
        .catch(function (err) {
            console.error('Lỗi kết nối SignalR:', err.toString());
            
            // Thử kết nối lại sau 5 giây
            setTimeout(function() {
                console.log('Đang thử kết nối lại sau lỗi...');
                initializeCommentHub(artworkId);
            }, 5000);
        });
}

// Tham gia nhóm của tác phẩm
function joinArtworkGroup(artworkId) {
    console.log('Đang gọi JoinArtworkGroup với artworkId:', artworkId);
    
    // Trả về Promise để có thể sử dụng trong chuỗi .then()
    return new Promise((resolve, reject) => {
        if (!commentConnection) {
            console.error('Không thể tham gia nhóm: Kết nối không tồn tại');
            reject(new Error('Kết nối không tồn tại'));
            return;
        }
        
        if (commentConnection.state !== 'Connected') {
            console.warn('Kết nối không ở trạng thái Connected, hiện tại là:', commentConnection.state);
            console.log('Đang thử kết nối trước khi tham gia nhóm...');
            
            commentConnection.start().then(() => {
                console.log('Đã kết nối, bây giờ tham gia nhóm');
                return commentConnection.invoke('JoinArtworkGroup', artworkId);
            }).then(() => {
                console.log('Đã tham gia nhóm artwork_' + artworkId + ' thành công');
                resolve();
            }).catch(err => {
                console.error('Lỗi khi kết nối và tham gia nhóm:', err.toString());
                reject(err);
            });
        } else {
            commentConnection.invoke('JoinArtworkGroup', artworkId)
                .then(() => {
                    console.log('Đã tham gia nhóm artwork_' + artworkId + ' thành công');
                    resolve();
                })
                .catch(err => {
                    console.error('Lỗi khi tham gia nhóm:', err.toString());
                    reject(err);
                });
        }
    });
}

// Rời khỏi nhóm của tác phẩm
function leaveArtworkGroup(artworkId) {
    console.log('Đang gọi LeaveArtworkGroup với artworkId:', artworkId);
    commentConnection.invoke('LeaveArtworkGroup', artworkId).catch(function (err) {
        console.error('Lỗi khi rời khỏi nhóm:', err.toString());
    });
}

// Tạo HTML cho bình luận mới
function createCommentHtml(comment) {
    // Tạo HTML cho bình luận dựa trên cấu trúc trong Display.cshtml
    let ratingHtml = '';
    if (comment.rating > 0) {
        ratingHtml = '<div class="comment-rating">';
        for (let i = 1; i <= 5; i++) {
            ratingHtml += `<i class="${i <= comment.rating ? 'fas fa-star' : 'far fa-star'}"></i>`;
        }
        ratingHtml += '</div>';
    }

    let imageHtml = '';
    if (comment.imagePath) {
        imageHtml = `
            <div class="comment-image-container">
                <img src="${comment.imagePath}" alt="Comment image" class="img-fluid rounded zoomable-image" 
                     onclick="openImageModal('${comment.imagePath}')" />
            </div>
        `;
    }

    let stickerHtml = '';
    if (comment.sticker) {
        stickerHtml = `
            <div class="sticker-container">
                <img src="${comment.sticker}" alt="Sticker" class="comment-sticker" />
            </div>
        `;
    }

    // Tạo HTML cho bình luận
    return `
        <div id="comment-${comment.id}" class="comment-item ${comment.isHidden ? 'comment-hidden' : ''}">
            <div class="comment-header">
                <div class="d-flex align-items-center">
                    <img src="${comment.userAvatar}" alt="${comment.userName}" class="commenter-avatar" />
                    <div class="commenter-info">
                        <h5 class="commenter-name">
                            ${comment.userName}
                            ${comment.isEdited ? '<span class="edited-marker">(đã chỉnh sửa)</span>' : ''}
                        </h5>
                        <div class="comment-date">${formatDate(comment.date)}</div>
                    </div>
                </div>
                ${ratingHtml}
            </div>
            <div class="comment-content">
                <p id="comment-content-${comment.id}">${comment.content}</p>
                ${imageHtml}
                ${stickerHtml}
            </div>
            <div class="comment-actions">
                <button class="btn-reply" onclick="showReplyForm(${comment.id})">
                    <i class="fas fa-reply me-1"></i>Trả lời
                </button>
                <span class="reply-count" id="reply-count-${comment.id}">0</span>
            </div>
            <div class="replies-container" id="replies-container-${comment.id}">
                <!-- Replies will be added here -->
            </div>
            <div class="reply-form-container" id="reply-form-${comment.id}" style="display: none;">
                <!-- Reply form will be added here -->
            </div>
        </div>
    `;
}

// Tạo HTML cho phản hồi mới
function createReplyHtml(reply) {
    let imageHtml = '';
    if (reply.imagePath) {
        imageHtml = `
        <div id="reply-image-container-${reply.id}" class="reply-image-container">
            <img id="reply-image-${reply.id}" src="${reply.imagePath}" class="reply-image zoomable-image" alt="Reply image" onclick="openImageModal('${reply.imagePath}')">
        </div>`;
    }
    
    let stickerHtml = '';
    if (reply.sticker) {
        stickerHtml = `
        <div id="reply-sticker-container-${reply.id}" class="sticker-container">
            <img id="reply-sticker-${reply.id}" src="${reply.sticker}" class="reply-sticker" alt="Sticker">
        </div>`;
    }
    
    let editedHtml = reply.isEdited ? '<span id="reply-edited-${reply.id}" class="edited-marker">(đã chỉnh sửa)</span>' : '<span id="reply-edited-${reply.id}" class="edited-marker d-none">(đã chỉnh sửa)</span>';
    
    // Tạo HTML cho các nút hành động (chỉnh sửa, xóa)
    // Lưu ý: Cần kiểm tra quyền người dùng ở phía server
    const actionButtons = `
    <div class="reply-admin-actions mt-1">
        <button class="btn-edit-reply" onclick="editReply(${reply.id}, ${reply.commentId})">
            <i class="fas fa-edit me-1"></i>Sửa
        </button>
        <button class="btn-delete-reply" onclick="deleteReply(${reply.id}, ${reply.artworkId})">
            <i class="fas fa-trash-alt me-1"></i>Xóa
        </button>
    </div>`;
    
    // Đảm bảo có đường dẫn avatar hợp lệ
    const avatarSrc = reply.userAvatar && reply.userAvatar.trim() !== '' ? reply.userAvatar : '/images/default-avatar.png';
    
    return `
    <div id="reply-${reply.id}" class="reply-item">
        <div class="d-flex align-items-start">
            <div class="reply-avatar">
                <img src="${avatarSrc}" alt="${reply.userName}" class="reply-user-avatar" style="transition: none; animation: none;" onerror="this.src='/images/default-avatar.png'; console.log('Avatar fallback loaded');" />
            </div>
            <div class="reply-content">
                <div class="reply-bubble">
                    <div class="reply-media-container">
                        ${stickerHtml}
                        ${imageHtml}
                    </div>
                    <h6 class="reply-username">
                        ${reply.userName}
                        ${editedHtml}
                    </h6>
                    <p class="reply-text mb-0">${reply.content}</p>
                    ${actionButtons}
                </div>
                <div class="reply-meta">
                    <span class="reply-time">${formatDate(reply.date)}</span>
                </div>
            </div>
        </div>
    </div>`;
}

// Định dạng ngày giờ
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleString('vi-VN');
}

// Cập nhật số lượng bình luận
function updateCommentCount(change) {
    const countElement = document.getElementById('comment-count');
    if (countElement) {
        let count = parseInt(countElement.textContent) || 0;
        count += change;
        countElement.textContent = count;
    }
}

// Cập nhật số lượng phản hồi
function updateReplyCount(commentId, change) {
    const countElement = document.getElementById(`reply-count-${commentId}`);
    if (countElement) {
        let count = parseInt(countElement.textContent) || 0;
        count += change;
        countElement.textContent = count;
    }
}

// Khởi tạo khi trang được tải
document.addEventListener('DOMContentLoaded', function () {
    console.log('DOMContentLoaded - Đang khởi tạo SignalR');
    
    // Lấy ID của tác phẩm từ URL hoặc từ một thuộc tính data trên trang
    const artworkIdElement = document.getElementById('artwork-id');
    console.log('DOMContentLoaded - Artwork ID element:', artworkIdElement);
    
    if (artworkIdElement) {
        const artworkId = parseInt(artworkIdElement.value);
        console.log('DOMContentLoaded - Artwork ID value:', artworkId);
        
        if (!isNaN(artworkId) && artworkId > 0) {
            console.log('DOMContentLoaded - Artwork ID hợp lệ, đang khởi tạo kết nối');
            // Đảm bảo rằng kết nối cũ được đóng trước khi tạo kết nối mới
            if (commentConnection) {
                commentConnection.stop().then(function() {
                    console.log('Đã đóng kết nối cũ');
                    initializeCommentHub(artworkId);
                }).catch(function(err) {
                    console.error('Lỗi khi đóng kết nối cũ:', err);
                    // Vẫn tiếp tục khởi tạo kết nối mới
                    initializeCommentHub(artworkId);
                });
            } else {
                console.log('Không có kết nối cũ, tạo kết nối mới');
                initializeCommentHub(artworkId);
            }
        } else {
            console.error('Giá trị artwork-id không hợp lệ:', artworkIdElement.value);
        }
    } else {
        console.error('Không tìm thấy phần tử artwork-id');
    }
});

// Thêm một hàm kiểm tra kết nối
function checkConnectionState() {
    if (commentConnection) {
        console.log('Trạng thái kết nối hiện tại:', commentConnection.state);
        return commentConnection.state;
    }
    return 'disconnected';
}

// Hàm xử lý sự kiện chỉnh sửa phản hồi
function editReply(replyId, commentId) {
    console.log('Chỉnh sửa phản hồi:', replyId, 'của bình luận:', commentId);
    
    // Lấy nội dung hiện tại của phản hồi
    const replyContent = document.querySelector(`#reply-${replyId} .reply-text`).textContent;
    
    // Lấy ID tác phẩm từ input hidden
    const artworkId = document.getElementById('artwork-id').value;
    
    // Gọi hàm hiển thị modal chỉnh sửa phản hồi
    showEditReplyModal(replyId, commentId, replyContent);
}

// Hàm hiển thị modal chỉnh sửa phản hồi
function showEditReplyModal(replyId, commentId, replyContent) {
    // Lấy thông tin phản hồi từ server trước khi hiển thị modal
    $.ajax({
        url: '/Reply/GetReplyInfo',
        type: 'GET',
        data: { replyId: replyId },
        success: function (response) {
            if (response.success) {
                $('#editReplyId').val(replyId);
                $('#editReplyContent').val(response.content || replyContent);
                $('#editCommentId').val(commentId);
                $('#editArtworkId').val($('#artwork-id').val());

                // Lưu thông tin gốc để xử lý khi không có thay đổi
                $('#editReplyOriginalImage').val(response.imagePath || '');
                $('#editReplyOriginalSticker').val(response.sticker || '');

                // Hiển thị ảnh và sticker nếu có
                if (response.imagePath) {
                    $('#editReplyImagePreview').attr('src', response.imagePath);
                    $('#editReplyImagePreviewContainer').removeClass('d-none');
                } else {
                    $('#editReplyImagePreviewContainer').addClass('d-none');
                }

                if (response.sticker) {
                    $('#editReplyStickerPreview').attr('src', response.sticker);
                    $('#editReplyStickerPreviewContainer').removeClass('d-none');
                } else {
                    $('#editReplyStickerPreviewContainer').addClass('d-none');
                }

                // Hiển thị modal
                $('#editReplyModal').modal('show');
            } else {
                alert('Không thể tải thông tin phản hồi');
            }
        },
        error: function () {
            // Fallback nếu không lấy được thông tin chi tiết
            $('#editReplyId').val(replyId);
            $('#editReplyContent').val(replyContent);
            $('#editCommentId').val(commentId);
            $('#editArtworkId').val($('#artwork-id').val());
            $('#editReplyModal').modal('show');
        }
    });
}

// Đóng kết nối khi rời khỏi trang
window.addEventListener('beforeunload', function () {
    if (commentConnection) {
        commentConnection.stop();
    }
});