/*-------------------------------------------------- HOME --------------------------------------------------------*/

/*---------------- script.js --------------*/
/* 2 nút qua lại trên panel */
const $next = document.querySelector('.next');
const $prev = document.querySelector('.prev');

// Auto-slide functionality every 2 seconds

// Next and Previous buttons functionality
$next.addEventListener('click', () => {
    const items = document.querySelectorAll('.item');
    document.querySelector('.slide').appendChild(items[0]);
});

$prev.addEventListener('click', () => {
    const items = document.querySelectorAll('.item');
    document.querySelector('.slide').prepend(items[items.length - 1]);
});

// Click handler for specific items to move them to the first position
document.querySelectorAll('.slide .item:nth-child(3), .slide .item:nth-child(4)').forEach(item => {
    item.addEventListener('click', () => {
        const slide = document.querySelector('.slide');
        slide.prepend(item); // Move clicked item to the first position
    });
});


/*-------------------- PANEL --------------------*/
/* Cập nhật phần hiển thị sách */
document.addEventListener('DOMContentLoaded', () => {
    const items = document.querySelectorAll('.item');
    const bookInfo = document.querySelector('.book-info');
    let currentIndex = 0;

    // Hàm cập nhật thông tin sách
    function updateBookInfo(index) {
        const currentItem = items[index];
        const bookTitle = currentItem.getAttribute('data-title');
        const bookPrice = currentItem.getAttribute('data-price');
        const bookImageSrc = currentItem.querySelector('img').src;
        const bookImage = bookInfo.querySelector('.book-image');

        bookInfo.querySelector('#book-title').textContent = bookTitle || 'Tên không xác định';
        bookInfo.querySelector('#book-price').textContent = bookPrice || 'Giá không xác định';

        if (bookImage) {
            bookImage.src = bookImageSrc;
        }
    }

    // Hàm hiển thị item hiện tại
    function showItem(index) {
        items.forEach((item, i) => {
            item.classList.remove('active');
            if (i === index) {
                item.classList.add('active');
                updateBookInfo(i);
            }
        });
    }

    // Tự động chuyển slide
    //let autoSlide = setInterval(() => {
    //    currentIndex = (currentIndex + 1) % items.length;
    //    showItem(currentIndex);
    //}, 2000);

    //document.querySelector('.slide').addEventListener('mouseenter', () => clearInterval(autoSlide));
    //document.querySelector('.slide').addEventListener('mouseleave', () => {
    //    autoSlide = setInterval(() => {
    //        currentIndex = (currentIndex + 1) % items.length;
    //        showItem(currentIndex);
    //    }, 2000);
    //});

    // Xử lý nút Next
    document.querySelector('.next').addEventListener('click', () => {
        currentIndex = (currentIndex + 1) % items.length;
        showItem(currentIndex);
    });

    // Xử lý nút Prev
    document.querySelector('.prev').addEventListener('click', () => {
        currentIndex = (currentIndex - 1 + items.length) % items.length;
        showItem(currentIndex);
    });

    showItem(currentIndex);
});

/*------------------ TOP LIKE / VIEW ------------------------*/
//$(document).ready(function () {
//    // Lắng nghe sự thay đổi của URL
//    $(window).on('popstate', function () {
//        // Cuộn trang đến phần #topFavorites
//        $('html, body').animate({
//            scrollTop: $('#topFavorited').offset().top - 100
//        }, 1000); // 1000ms = 1 giây cuộn trang
//    });

//    // Kiểm tra nếu đang tải lại trang từ nút "Next"
//    if (window.location.href.indexOf("currentGroupTopFavorites") !== -1) {
//        $('html, body').animate({
//            scrollTop: $('#topFavorited').offset().top - 100
//        }, 1000);
//    }
//});

//$(document).ready(function () {
//    // Lắng nghe sự thay đổi của URL
//    $(window).on('popstate', function () {
//        // Cuộn trang đến phần #topFavorites
//        $('html, body').animate({
//            scrollTop: $('#topViewed').offset().top
//        }, 1000); // 1000ms = 1 giây cuộn trang
//    });

//    // Kiểm tra nếu đang tải lại trang từ nút "Next"
//    if (window.location.href.indexOf("currentGroupTopViewed") !== -1) {
//        $('html, body').animate({
//            scrollTop: $('#topViewed').offset().top
//        }, 1000);
//    }
//});

/*------------------ TOP LIKE NO LOADING ------------------------*/
$(document).ready(function () {
    //let currentGroupFavorites = @Model.CurrentGroupFavorites;  // Lấy nhóm hiện tại của sách yêu thích
    //const totalGroupsFavorites = @Model.TotalGroupsFavorites;  // Tổng số nhóm sách yêu thích
    let currentGroupFavorites = $("#topFavorited").data("current-group-favorites");
    const totalGroupsFavorites = $("#topFavorited").data("total-groups-favorites");
    let indexFavorite = $("#topFavorited").data("index-favorite");  // Lấy giá trị indexFavorite ban đầu

    // Hàm tải danh sách sách yêu thích
    function loadFavorites(group) {
        $.ajax({
            url: `/Home/LoadFavorites?currentGroupTopFavorites=${group}`,
            type: "GET",
            success: function (result) {
                $("#topFavorited").html(result);  // Cập nhật nội dung phần sách yêu thích
                currentGroupFavorites = group;  // Cập nhật nhóm hiện tại
                indexFavorite = result.indexFavorite;
                toggleButtons();  // Cập nhật trạng thái các nút chuyển trang
            },
            error: function (xhr, status, error) {
                console.error("Error loading favorites:", error);
            }
        });
    }

    // Cập nhật trạng thái các nút Next và Previous
    function toggleButtons() {
        // Nút Previous sẽ disabled nếu currentGroupFavorites <= 1
        $("#prev-btn-favorites").toggleClass("disabled", currentGroupFavorites <= 1);
        // Nút Next sẽ disabled nếu currentGroupFavorites >= totalGroupsFavorites
        $("#next-btn-favorites").toggleClass("disabled", currentGroupFavorites >= totalGroupsFavorites);
    }

    // Khi nhấn nút Previous
    $("#prev-btn-favorites").click(function () {
        if (currentGroupFavorites > 1) {
            loadFavorites(currentGroupFavorites - 1);  // Tải lại danh sách sách yêu thích nhóm trước đó
        }
    });

    // Khi nhấn nút Next
    $("#next-btn-favorites").click(function () {
        if (currentGroupFavorites < totalGroupsFavorites) {
            loadFavorites(currentGroupFavorites + 1);  // Tải lại danh sách sách yêu thích nhóm tiếp theo
        }
    });

    // Cập nhật trạng thái các nút ngay từ đầu khi trang được tải
    toggleButtons();
});

/*------------------ TOP VIEW NO LOADING ------------------------*/
$(document).ready(function () {
    let currentGroupViewed = $("#topViewed").data("current-group-viewed");
    const totalGroupsViewed = $("#topViewed").data("total-groups-viewed");

    // Hàm tải danh sách sách đã xem
    function loadViewed(group) {
        $.ajax({
            url: `/Home/LoadViewed?currentGroupTopViewed=${group}`,
            type: "GET",
            success: function (result) {
                $("#topViewed").html(result);  // Cập nhật lại nội dung phần sách đã xem
                currentGroupViewed = group;  // Cập nhật nhóm hiện tại
                toggleButtons();  // Cập nhật trạng thái các nút chuyển trang
            },
            error: function (xhr, status, error) {
                console.error("Error loading viewed:", error);
            }
        });
    }

    // Cập nhật trạng thái các nút Next và Previous cho Top Viewed
    function toggleButtons() {
        $("#prev-btn-viewed").toggleClass("disabled", currentGroupViewed <= 1);
        $("#next-btn-viewed").toggleClass("disabled", currentGroupViewed >= totalGroupsViewed);
    }

    // Khi nhấn nút Previous cho Top Viewed
    $("#prev-btn-viewed").click(function () {
        if (currentGroupViewed > 1) {
            loadViewed(currentGroupViewed - 1);
        }
    });

    // Khi nhấn nút Next cho Top Viewed
    $("#next-btn-viewed").click(function () {
        if (currentGroupViewed < totalGroupsViewed) {
            loadViewed(currentGroupViewed + 1);
        }
    });

    // Cập nhật trạng thái các nút ngay từ đầu khi trang được tải
    toggleButtons();
});
