
// REVIEW IMAGE
document.getElementById('avatar-input').addEventListener('change', function (event) {
    const file = event.target.files[0]; // Lấy file người dùng chọn
    if (file) {
        const reader = new FileReader(); // Dùng FileReader để đọc file
        reader.onload = function (e) {
            document.getElementById('avatar-preview').src = e.target.result; // Cập nhật ảnh preview
        };
        reader.readAsDataURL(file); // Đọc file dưới dạng URL
    }
});

/*----------------------------------THÔNG TIN CÁ NHÂN (INDEX) ------------------------------------------------*/
// LƯU ẢNH
document.getElementById("update-profile-button").addEventListener("click", function () {
    const formData = new FormData(document.getElementById("profile-form"));

    fetch('/Account/UpdateProfile', {
        method: 'POST',
        body: formData
    })

        .then(() => {
            // Hiển thị banner
            showBanner("Hồ sơ của bạn đã được cập nhật thành công!");

            // Ẩn banner sau 3 giây
        })
        .catch(error => {
            alert("Đã xảy ra lỗi: " + error.message);
        });
});


/*----------------------------------THÔNG TIN CÁ NHÂN (INDEX) / ĐỔI MẬT KHÂU (CHANGE PASSWORD)------------------------------------------------*/
// THÔNG BÁO
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
        banner.remove();
    }, 4000);
}
