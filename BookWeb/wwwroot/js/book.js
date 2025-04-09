/*--------------------------------------------------  --------------------------------------------------------*/
/*------------------------------------------------ BOOK --------------------------------------------------------*/
/*--------------------------------------------------  --------------------------------------------------------*/


/*------------------------------------ LOGIN ----------------------------------------*/
function redirectToLogin() {
    var returnUrl = window.location.pathname; // Lưu URL hiện tại
    window.location.href = '/Identity/Account/Login?returnUrl=' + encodeURIComponent(returnUrl);
}

/*------------------------------------- AUDIO --------------------------------------------*/
document.getElementById('play-audio').addEventListener('click', function () {
    const audioPlayer = document.getElementById('audio-player');
    audioPlayer.style.display = 'flex';
});

document.getElementById('close-player').addEventListener('click', function () {
    const audioPlayer = document.getElementById('audio-player');
    audioPlayer.style.display = 'none';
    const audio = document.getElementById('audio');
    audio.pause();
});

/*----------------------------------------- BOOKMARK & NOTE --------------------------------------------------*/
// Hàm xử lý khi nhấn vào "Đọc ngay"
function showBookDetail() {
    var pdfBookmarkSection = document.getElementById("pdfBookmarkSection");
    var readButton = document.getElementById("read-button");

    // Kiểm tra xem file PDF đang hiển thị hay không
    if (pdfBookmarkSection.style.display === "none") {
        // Hiển thị file PDF
        pdfBookmarkSection.style.display = "block";

        // Tải PDF nếu chưa được tải
        if (!pdfBookmarkSection.hasChildNodes()) {
            loadPDF(pdfUrl); // Hàm loadPDF đã được định nghĩa trước đó
        }

        // Đổi nút thành "Ẩn"
        readButton.textContent = "Ẩn";
    } else {
        // Ẩn file PDF
        pdfBookmarkSection.style.display = "none";

        // Đổi nút thành "Đọc ngay"
        readButton.textContent = "Đọc ngay";
    }
}
// Khai báo các biến
let pdfDoc = null;
let currentPage = 1;
let totalPages = 0;
let isRendering = false;
let pageNumPending = null;
const bookId = '@Model.Id'; // Mã sách

// Đường dẫn đến PDF của bạn
const pdfUrl = '@Model.pdf';

// Tải và render PDF
function loadPDF(pdfUrl) {
    // Tải PDF từ URL
    pdfjsLib.getDocument(pdfUrl).promise.then(function (pdf) {
        pdfDoc = pdf;
        totalPages = pdf.numPages;
        renderPage(currentPage);
        loadBookmarks(); // Tải danh sách dấu trang đã lưu
    }).catch(function (error) {
        console.error('Error loading PDF: ', error);
    });
}

// Render trang PDF
function renderPage(pageNum) {
    if (isRendering) {
        pageNumPending = pageNum;
        return;
    }
    isRendering = true;

    pdfDoc.getPage(pageNum).then(function (page) {
        const canvas = document.createElement('canvas');
        const context = canvas.getContext('2d');
        const scale = 1.5;
        const viewport = page.getViewport({ scale: scale });

        canvas.height = viewport.height;
        canvas.width = viewport.width;

        page.render({
            canvasContext: context,
            viewport: viewport
        }).promise.then(function () {
            document.getElementById('pdfContainer').innerHTML = '';
            document.getElementById('pdfContainer').appendChild(canvas);

            // Cập nhật số trang hiện tại
            document.getElementById('currentPage').value = currentPage;
            // Cập nhật tổng số trang
            document.getElementById('totalPages').textContent = totalPages;

            isRendering = false;
            if (pageNumPending !== null) {
                renderPage(pageNumPending);
                pageNumPending = null;
            }
        }).catch(function (error) {
            console.error("Lỗi khi render trang PDF: ", error);
        });
    }).catch(function (error) {
        console.error("Lỗi khi tải trang từ PDF: ", error);
    });
}

// Chuyển đến trang tiếp theo
function nextPage() {
    if (currentPage < totalPages) {
        currentPage++;
        renderPage(currentPage);
    }
}

// Chuyển đến trang trước
function prevPage() {
    if (currentPage > 1) {
        currentPage--;
        renderPage(currentPage);
    }
}

// Lưu dấu trang
function saveBookmark(event) {
    event.preventDefault(); // Ngừng hành động mặc định của form

    const pageNumber = document.getElementById('currentPage').value;
    const bookId = document.querySelector('input[name="bookId"]').value;
    const note = document.getElementById('bookmarkNote').value; // Lấy ghi chú từ form
    // Lấy số lượng dấu trang hiện tại
    const bookmarkList = document.getElementById('bookmarkList').getElementsByTagName('li');

    if (bookmarkList.length >= 10) {
        alert('Bạn đã đánh dấu tối đa 10 trang. Không thể thêm dấu trang mới.');
        return; // Dừng nếu đã đạt giới hạn
    }
    // Kiểm tra nếu ghi chú vượt quá 10 ký tự
    if (note.length > 50) {
        alert("Ghi chú không được vượt quá 50 ký tự.");
        return;
    }

    // Gửi yêu cầu AJAX để lưu dấu trang
    fetch('/Bookmark/SetBookmark', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            bookId: bookId,
            pageNumber: pageNumber,
            note: note // Gửi ghi chú
        })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert(data.message); // Hiển thị thông báo thành công
                loadBookmarks(); // Tải lại danh sách dấu trang
            } else {
                alert(data.message); // Thông báo dấu trang đã tồn tại
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Có lỗi xảy ra.');
        });
}

// Hàm để tải danh sách dấu trang từ backend
function loadBookmarks() {
    const bookId = document.querySelector('input[name="bookId"]').value;

    fetch(`/Bookmark/GetBookmarks?bookId=${bookId}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                const bookmarkList = data.bookmarks;
                const bookmarkContainer = document.getElementById('bookmarkList');
                bookmarkContainer.innerHTML = ''; // Xóa danh sách cũ

                // Hiển thị danh sách dấu trang
                bookmarkList.forEach(bookmark => {
                    const listItem = document.createElement('li');
                    listItem.classList.add('list-group-item', 'd-flex', 'justify-content-between', 'align-items-center', 'bookmark-item');
                    listItem.innerHTML = `
                                        <span>Trang ${bookmark.pageNumber}</span>
                                        <span class="note">${bookmark.note || 'Không có ghi chú'}</span>
                                        <button class="btn btn-danger btn-sm" onclick="deleteBookmark(${bookmark.pageNumber})">
                                            <i class="fas fa-trash"></i> Xóa
                                        </button>
                                    `;
                    listItem.setAttribute('data-page', bookmark.pageNumber);

                    // Thêm sự kiện click vào mỗi dấu trang
                    listItem.addEventListener('click', function () {
                        goToPage(bookmark.pageNumber, bookmark.note); // Chuyển đến trang khi click vào dấu trang
                    });

                    bookmarkContainer.appendChild(listItem);
                });

                // Cập nhật thông tin số dấu trang
                const bookmarkCount = bookmarkList.length;
                const totalBookmarks = 10; // Giới hạn tối đa là 10
                document.getElementById('bookmarkCount').textContent = `${bookmarkCount}/${totalBookmarks}`;
            } else {
                alert('Không thể tải dấu trang.');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Có lỗi xảy ra.');
        });
}

// Hàm chuyển đến trang khi click vào dấu trang
function goToPage(pageNumber, note) {
    currentPage = pageNumber;  // Cập nhật trang hiện tại
    renderPage(currentPage);   // Render lại trang PDF

    // Cập nhật form ghi chú
    document.getElementById('currentPage').value = pageNumber;
    document.getElementById('bookmarkNote').value = note; // Hiển thị ghi chú
}

// Hàm để xóa dấu trang
function deleteBookmark(pageNumber) {
    const bookId = document.querySelector('input[name="bookId"]').value;

    fetch(`/Bookmark/DeleteBookmark?bookId=${bookId}&pageNumber=${pageNumber}`, {
        method: 'POST',
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                alert(data.message); // Thông báo thành công
                loadBookmarks(); // Tải lại danh sách dấu trang sau khi xóa
            } else {
                alert('Có lỗi xảy ra khi xóa dấu trang.');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Có lỗi xảy ra khi xóa dấu trang.');
        });
}

// Tải PDF ban đầu
loadPDF(pdfUrl);

// Điều khiển chuyển trang
document.getElementById('nextButton').addEventListener('click', nextPage);
document.getElementById('prevButton').addEventListener('click', prevPage);

/*------------------------------------------- RATING ----------------------------------------*/
// Hiển thị thông báo đánh giá
function showRatingMessage(value) {
    const messages = ["Dở tệ", "Không hài lòng", "Bình thường", "Hài lòng", "Tuyệt vời"];
    const stars = document.querySelectorAll(".star");

    // Cập nhật thông báo
    document.getElementById("rating-message").innerText = messages[value - 1];

    // Tô màu hover cho các ngôi sao
    stars.forEach((star, index) => {
        if (index < value) {
            star.classList.add("hovered");
        } else {
            star.classList.remove("hovered");
        }
    });
}

// Reset trạng thái hover khi chuột rời khỏi khu vực ngôi sao
function resetHover() {
    const stars = document.querySelectorAll(".star");

    // Xóa màu hover
    stars.forEach((star) => {
        star.classList.remove("hovered");
    });

    // Xóa thông báo
    document.getElementById("rating-message").innerText = "";
}

// Gửi đánh giá bằng cách cập nhật giá trị sao đã chọn
function submitRating(value) {
    // Cập nhật giá trị cho input hidden
    document.getElementById("star-value").value = value;

    // Tô màu cố định cho các ngôi sao đã chọn
    const stars = document.querySelectorAll(".star");
    stars.forEach((star, index) => {
        if (index < value) {
            star.classList.add("selected");
        } else {
            star.classList.remove("selected");
        }
    });
}

// Hiển thị banner thông báo thành công

// Kiểm tra trước khi gửi form
document.querySelector("#rating-form").addEventListener("submit", function (e) {
    const starValue = document.getElementById("star-value").value;
    if (starValue === "0" || starValue === "") {
        e.preventDefault(); // Ngăn gửi form nếu chưa chọn sao
        showBanner("Bạn cần chọn ít nhất 1 sao trước khi gửi đánh giá.");
    } else {
        showBanner(`Cảm ơn bạn đã chọn ${starValue} sao cho TVQ Books.`);
    }
});

/*------------------------------------------- COMMENT --------------------------------------*/
function timeAgo(dateString) {
    const date = new Date(dateString);
    if (isNaN(date.getTime())) {  // Kiểm tra giá trị thời gian hợp lệ
        return "Thời gian không hợp lệ";
    }

    const now = new Date();
    const difference = Math.floor((now - date) / 1000);

    if (difference < 60) return `${difference} giây trước`;
    if (difference < 3600) return `${Math.floor(difference / 60)} phút trước`;
    if (difference < 86400) return `${Math.floor(difference / 3600)} giờ trước`;
    if (difference < 604800) return `${Math.floor(difference / 86400)} ngày trước`;
    if (difference < 2592000) return `${Math.floor(difference / 604800)} tuần trước`;
    if (difference < 31536000) return `${Math.floor(difference / 2592000)} tháng trước`;

    return `${Math.floor(difference / 31536000)} năm trước`;
}

document.querySelectorAll('.comment-time').forEach(el => {
    const time = el.getAttribute('data-time');
    el.textContent = timeAgo(time);
});

// Thay đổi nội dung thời gian động
document.addEventListener('DOMContentLoaded', function () {
    const replyButtons = document.querySelectorAll('.reply-button');

    // Kiểm tra nếu có replyButton
    if (replyButtons.length > 0) {
        replyButtons.forEach(button => {
            button.addEventListener('click', function () {
                const commentId = button.getAttribute('data-comment-id');
                const replyForm = document.getElementById('reply-form-' + commentId);
                // Toggle the visibility of the reply form
                if (replyForm.style.display === 'none' || replyForm.style.display === '') {
                    replyForm.style.display = 'block';
                } else {
                    replyForm.style.display = 'none';
                }
            });
        });
    }
});

document.addEventListener('DOMContentLoaded', function () {
    const textarea = document.getElementById("commentContent");
    const submitBtn = document.getElementById("submitBtn");

    if (textarea && submitBtn) {  // Kiểm tra sự tồn tại của textarea và submitBtn
        textarea.addEventListener("input", function () {
            if (textarea.value.trim() !== "") {
                submitBtn.disabled = false;
            } else {
                submitBtn.disabled = true;
            }
        });
    }
});

function updateCartOnBuyNow(event, productId) {
    event.preventDefault();  // Ngăn chặn hành động mặc định của thẻ <a>

    // Thực hiện thêm sản phẩm vào giỏ hàng
    console.log("Thêm sản phẩm vào giỏ hàng với ID: " + productId);
    // Bạn có thể thêm logic xử lý giỏ hàng ở đây
}

/*---------------------------------------- CART ----------------------------------------*/
function updateCartOnBuyNow(event, productId) {
    event.preventDefault(); // Ngăn chặn hành động mặc định của thẻ a

    // Thực hiện gọi AJAX để thêm sản phẩm vào giỏ hàng
    fetch(`/ShoppingCart/AddToCart?id=${productId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Không thể thêm sản phẩm vào giỏ hàng!');
            }
            return response.text();
        })

        .then(() => {
            // Sau khi thêm sản phẩm thành công, chuyển hướng đến trang giỏ hàng
            window.location.href = '/ShoppingCart/Index';
        })
        .catch(error => {
            alert(error.message);
        });
}

function addToCartWithBanner(productId) {
    // Gửi yêu cầu AJAX thêm sản phẩm vào giỏ hàng
    fetch(`/ShoppingCart/AddToCart?id=${productId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Không thể thêm sản phẩm vào giỏ hàng!');
            }
            return response.text();
        })

        .then(() => {
            // Hiển thị banner thông báo
            showBanner("Sản phẩm đã được thêm vào giỏ hàng");

            // Cập nhật giỏ hàng nếu cần (tùy bạn có muốn tải lại giỏ hàng không)
        })
        .catch(error => {
            alert(error.message);
        });
}

function showBanner(message) {
    // Tạo banner
    const banner = document.createElement("div");
    banner.id = "success-banner";
    banner.style.position = "fixed";
    banner.style.top = "20%";
    banner.style.left = "50%";
    banner.style.transform = "translate(-50%, -50%)";
    banner.style.backgroundColor = "#28a745";
    banner.style.color = "white";
    banner.style.padding = "20px";
    banner.style.borderRadius = "8px";
    banner.style.boxShadow = "0 4px 8px rgba(0, 0, 0, 0.2)";
    banner.style.zIndex = "1000";
    banner.innerHTML = `
                <i class="fa fa-check-circle" style="margin-right: 8px;"></i>${message}
            `;

    // Thêm banner vào DOM
    document.body.appendChild(banner);

    // Tự động ẩn sau 3 giây
    setTimeout(() => {
        window.location.reload();
        banner.remove();
    }, 2000);
}

/*--------------------------------------------------  --------------------------------------------------------*/
/*--------------------------------------------------  --------------------------------------------------------*/