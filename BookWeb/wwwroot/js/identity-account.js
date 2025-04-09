

/*----------------------------------ĐĂNG NHẬP (LOGIN) / ĐĂNG KÝ (REGISTER)------------------------------------------------*/
// Thêm sự kiện double-click vào ô input
const inputField = document.getElementById("Input_Email");

inputField.addEventListener("dblclick", function () {
    // Thêm class 'selected' để đổi màu nền thành xám
    this.classList.add("selected");
});

// ----------- Xác nhận trước khi đăng ký ------------------
// Enable/Disable the register button based on checkbox status
const checkbox = document.getElementById('customCheck1');
const registerBtn = document.getElementById('registerBtn');

checkbox.addEventListener('change', function () {
    registerBtn.disabled = !checkbox.checked;
});

