/*------------------------------------------------------  -----------------------------------------------------*/
/*--------------------------------------------- SHOPPING CART -------------------------------------------------*/
/*------------------------------------------------------  -----------------------------------------------------*/


/*------------- LOGIN --------------*/
function redirectToLogin() {
    var returnUrl = window.location.pathname; // Lưu URL hiện tại
    window.location.href = '/Identity/Account/Login?returnUrl=' + encodeURIComponent(returnUrl);
}

/*------------------- INDEX ---------------------*/
// Hàm kiểm tra đơn hàng và hiển thị modal xác nhận
function validateOrder(event, paymentMethod) {
    const checkboxes = document.querySelectorAll('.item-checkbox');
    const isChecked = Array.from(checkboxes).some(checkbox => checkbox.checked);

    if (!isChecked) {
        alert("Bạn phải chọn ít nhất một sản phẩm trước khi đặt hàng.");
        event.preventDefault();
        return false; // Ngừng hành động đặt hàng
    }

    // Lấy thông tin các sản phẩm đã chọn
    const selectedItems = [];
    let totalPrice = 0;

    checkboxes.forEach(checkbox => {
        if (checkbox.checked) {
            const itemId = checkbox.getAttribute('data-id');
            const itemName = checkbox.closest('li').querySelector('h5').innerText;
            const itemPrice = parseFloat(checkbox.closest('li').querySelector('.font-weight-bold.text-primary').innerText.replace('đ', '').replace(',', '').trim());
            const itemQuantity = parseInt(checkbox.closest('li').querySelector('input[type="text"]').value);
            const itemTotal = itemPrice * itemQuantity;

            selectedItems.push({ name: itemName, quantity: itemQuantity, price: itemPrice, total: itemTotal });
            totalPrice += itemTotal;
        }
    });

    // Lấy thông tin giao hàng
    const fullName = document.querySelector('input[name="Name"]').value;
    const phoneNumber = document.querySelector('input[name="PhoneNumber"]').value;
    const address = document.querySelector('input[name="ShippingAddress"]').value;

    // Cập nhật nội dung modal với thông tin đã chọn
    const orderItemsList = document.getElementById('order-items-list');
    orderItemsList.innerHTML = '';
    selectedItems.forEach(item => {
        const li = document.createElement('li');
        li.innerHTML = `${item.name} (x${item.quantity}) - ${item.total.toLocaleString()} đ`;
        orderItemsList.appendChild(li);
    });

    document.getElementById('shipping-info').innerText = `Họ và tên: ${fullName}\nSố điện thoại: ${phoneNumber}\nĐịa chỉ giao hàng: ${address}`;
    document.getElementById('final-total-price-modal').innerText = totalPrice.toLocaleString() + ' đ';

    // Hiển thị modal
    $('#orderConfirmationModal').modal('show');
    const modal = document.getElementById('orderConfirmationModal');
    modal.removeAttribute('inert');  // Kích hoạt tiêu điểm trên modal và nội dung của nó

    // Lưu phương thức thanh toán
    document.getElementById('confirmBtn').onclick = function () {
        confirmOrder(paymentMethod);
    };

    // Ngừng hành động mặc định của nút
    event.preventDefault();
}

// Hàm xử lý khi người dùng xác nhận đơn hàng
function confirmOrder(paymentMethod) {
    // Nếu chọn COD, gửi form checkout
    if (paymentMethod === 'cod') {
        document.getElementById('checkoutForm').submit();
    } else if (paymentMethod === 'momo') {
        // Nếu chọn MoMo, gửi form MoMo
        document.getElementById('momoPaymentForm').submit();
    }

    // Đóng modal
    hideModal();
}

// Hàm đóng modal khi bấm nút "Hủy"
function hideModal() {
    const modal = document.getElementById('orderConfirmationModal');
    modal.setAttribute('inert', '');  // Vô hiệu hóa tiêu điểm trên modal và nội dung của nó
    $('#orderConfirmationModal').modal('hide');
}


/*-----------------------------------------------*/
//function validateOrder() {
//    // Lấy tất cả checkbox sản phẩm
//    const checkboxes = document.querySelectorAll('.item-checkbox');

//    // Kiểm tra xem có checkbox nào được chọn không
//    const isChecked = Array.from(checkboxes).some(checkbox => checkbox.checked);

//    if (!isChecked) {
//        // Hiển thị thông báo nếu chưa chọn sản phẩm
//        alert("Bạn phải chọn ít nhất một sản phẩm trước khi đặt hàng.");
//        return false; // Ngăn chặn hành động đặt hàng
//    }

//    // Hiển thị hộp thoại xác nhận đơn hàng
//    const confirmation = confirm("Bạn chắc chắn muốn đặt hàng này?");
//    if (!confirmation) {
//        return false; // Nếu người dùng không xác nhận, ngừng hành động
//    }

//    return true; // Cho phép đặt hàng nếu hợp lệ
//}

/*-----------------------------------------------*/
function updateCart(productId) {
    var quantityInput = document.getElementById('txtQuantity_' + productId);
    var maxQuantity = parseInt(quantityInput.max, 10);  // Lấy số lượng tồn kho tối đa
    var quantity = parseInt(quantityInput.value, 10);

    // Kiểm tra giới hạn số lượng
    if (isNaN(quantity) || quantity < 1) {
        quantity = 1;  // Không cho phép số lượng nhỏ hơn 1
        alert("Số lượng phải lớn hơn hoặc bằng 1.");
    } else if (quantity > maxQuantity) {
        quantity = maxQuantity;  // Không cho phép vượt quá số lượng tồn kho
        alert(`Chỉ còn ${maxQuantity} sản phẩm trong kho!`);
    }

    // Cập nhật giá trị trong ô nhập
    quantityInput.value = quantity;

    // Chuyển hướng cập nhật giỏ hàng
    window.location.href = '/ShoppingCart/UpdateCart?id=' + productId + '&quantity=' + quantity;

    // Cập nhật trạng thái checkbox "Chọn tất cả"
    syncSelectAllCheckbox();
}

function decreaseQuantity(productId) {
    var quantityInput = document.getElementById('txtQuantity_' + productId);
    var currentQuantity = parseInt(quantityInput.value, 10);
    if (currentQuantity > 1) {
        quantityInput.value = currentQuantity - 1;
        updateCart(productId);
    }
}

function increaseQuantity(productId) {
    var quantityInput = document.getElementById('txtQuantity_' + productId);
    var maxQuantity = parseInt(quantityInput.max, 10);
    var currentQuantity = parseInt(quantityInput.value, 10);
    if (currentQuantity < maxQuantity) {
        quantityInput.value = currentQuantity + 1;
        updateCart(productId);
    }
}
/*-----------------------------------------------*/
function toggleSelectAll() {
    const selectAllCheckbox = document.getElementById("select-all");
    const itemCheckboxes = document.querySelectorAll(".item-checkbox");

    itemCheckboxes.forEach((checkbox) => {
        checkbox.checked = selectAllCheckbox.checked; // Cập nhật trạng thái checkbox sản phẩm
    });

    updateTotalPrice(); // Cập nhật giá trị tổng
}

document.addEventListener("DOMContentLoaded", function () {
    const itemCheckboxes = document.querySelectorAll(".item-checkbox");
    const selectAllCheckbox = document.getElementById("select-all");

    // Gắn sự kiện cho từng checkbox sản phẩm
    itemCheckboxes.forEach((checkbox) => {
        checkbox.addEventListener("change", function () {
            const allChecked = Array.from(itemCheckboxes).every(cb => cb.checked);
            selectAllCheckbox.checked = allChecked; // Cập nhật trạng thái "Chọn tất cả"
            updateTotalPrice(); // Cập nhật giá trị tổng khi thay đổi trạng thái checkbox
        });
    });
});

/*-----------------------------------------------*/
/**
 * Hàm tính và cập nhật giá trị tổng
 */
function updateTotalPrice() {
    let totalPrice = 0;

    // Lấy tất cả checkbox sản phẩm
    const itemCheckboxes = document.querySelectorAll(".item-checkbox");

    // Lặp qua các checkbox, kiểm tra trạng thái và tính tổng
    itemCheckboxes.forEach((checkbox) => {
        if (checkbox.checked) {
            const itemRow = checkbox.closest(".list-group-item");
            const priceElement = itemRow.querySelector(".text-danger");
            const price = parseInt(priceElement.textContent.replace(/[^\d]/g, ""), 10);
            totalPrice += price;
        }
    });

    // Hiển thị giá trị tổng ở các phần được yêu cầu
    document.getElementById("total-price").textContent = new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(totalPrice);
    document.getElementById("final-total-price").textContent = new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(totalPrice);

    // Cập nhật giá trị tổng vào input của Momo
    /*document.getElementById("momo-total-price").value = totalPrice;*/
}

/*-----------------------------------------------*/
function updateCheckboxState(checkbox) {
    const itemId = checkbox.getAttribute("data-id");
    const isChecked = checkbox.checked;

    // Lưu trạng thái checkbox vào localStorage
    localStorage.setItem(`cart-item-${itemId}`, isChecked);

    // Cập nhật tổng tiền khi trạng thái checkbox thay đổi
    updateTotalPrice();
}

document.addEventListener("DOMContentLoaded", function () {
    const checkboxes = document.querySelectorAll(".item-checkbox");

    checkboxes.forEach((checkbox) => {
        const itemId = checkbox.getAttribute("data-id");
        const savedState = localStorage.getItem(`cart-item-${itemId}`);

        if (savedState !== null) {
            checkbox.checked = savedState === "true";
        }
    });

    updateTotalPrice();
});

/*-----------------------------------------------*/
function syncSelectAllCheckbox() {
    const itemCheckboxes = document.querySelectorAll(".item-checkbox");
    const selectAllCheckbox = document.getElementById("select-all");

    const allChecked = Array.from(itemCheckboxes).every(checkbox => checkbox.checked);

    selectAllCheckbox.checked = allChecked; // Đồng bộ trạng thái "Chọn tất cả"
}

document.addEventListener("DOMContentLoaded", function () {
    syncSelectAllCheckbox(); // Đồng bộ trạng thái "Chọn tất cả"
});

/*------------------------------------------------  --------------------------------------------------*/
/*------------------------------------------------  --------------------------------------------------*/
