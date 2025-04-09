

/*---------------------------------- ĐỔI MẬT KHÂU (CHANGE PASSWORD)------------------------------------------------*/
//ẨN HIỆN MẬT KHẨU
// JavaScript to toggle password visibility
document.getElementById("toggleOldPassword").addEventListener("click", function () {
    const oldPasswordField = document.getElementById("OldPassword");
    const type = oldPasswordField.type === "password" ? "text" : "password";
    oldPasswordField.type = type;
});

document.getElementById("toggleNewPassword").addEventListener("click", function () {
    const newPasswordField = document.getElementById("NewPassword");
    const type = newPasswordField.type === "password" ? "text" : "password";
    newPasswordField.type = type;
});

document.getElementById("toggleConfirmPassword").addEventListener("click", function () {
    const confirmPasswordField = document.getElementById("ConfirmPassword");
    const type = confirmPasswordField.type === "password" ? "text" : "password";
    confirmPasswordField.type = type;
});
