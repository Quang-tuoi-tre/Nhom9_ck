/*------------------------------------------------------  -----------------------------------------------------*/
/*------------------------------------------------- BOOKS MANAGE ----------------------------------------------*/
/*------------------------------------------------------  -----------------------------------------------------*/


/*-------- INDEX / DETAIL ---------*/
// Khi nút "THÊM" được nhấn
$('#addModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget);
    var bookId = button.data('id');

    var modal = $(this);
    modal.find('#addBookId').val(bookId);
});

// Khi nút "Cập nhật" được nhấn
$('#updateModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget); // Lấy thông tin từ button đã nhấn
    var bookId = button.data('id');
    var stockQuantity = button.data('stock');
    var bookName = button.data('name');

    // Cập nhật thông tin trong Modal
    var modal = $(this);
    modal.find('#bookId').val(bookId);
    modal.find('#bookStock').val(stockQuantity);
});

/*------ THÊM / SỬA --------*/
// ẢNH
function previewImage(event) {
    var input = event.target;
    var file = input.files[0];
    var reader = new FileReader();

    reader.onload = function (e) {
        var imagePreview = document.getElementById('imagePreview');
        imagePreview.src = e.target.result;
        imagePreview.style.display = 'block'; // Hiển thị hình ảnh preview
    };

    if (file) {
        reader.readAsDataURL(file); // Đọc file dưới dạng base64 và hiển thị
    }
}

//AUDIO
function previewAudio(event) {
    var input = event.target;
    var file = input.files[0];
    var reader = new FileReader();

    reader.onload = function (e) {
        var audioPreview = document.getElementById('audioPreview');
        audioPreview.src = e.target.result; // Đặt nguồn cho file audio mới
        audioPreview.style.display = 'block'; // Hiển thị file audio preview
    };

    if (file) {
        reader.readAsDataURL(file); // Đọc file audio dưới dạng base64 và hiển thị
    }
}

// PRICE
document.getElementById('priceInput').addEventListener('input', function () {
    const price = this.value.replace(/[^0-9]/g, ''); // Loại bỏ ký tự không phải số
    if (price) {
        const formatted = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(price);
        document.getElementById('formattedPrice').innerText = formatted;
    } else {
        document.getElementById('formattedPrice').innerText = '';
    }
});

/*------------------------------------------------  --------------------------------------------------*/
/*------------------------------------------------  --------------------------------------------------*/