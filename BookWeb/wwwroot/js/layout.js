/*------------------------------------------------------------------------------------------------------------------------------------*/
/*----------------------------------------------------------- LAYOUT -----------------------------------------------------------------*/
/*------------------------------------------------------------------------------------------------------------------------------------*/

/* -------------------------- Search Suggestions ------------------------>*/
$(document).ready(function () {
    const searchInput = $("#search-input");
    $("#search-input").keyup(function () {
        var query = $(this).val();
        if (query !== '') {
            $.ajax({
                url: "/books/SearchSuggestions",
                type: "GET",
                data: { query: query },
                dataType: "json",
                success: function (data) {
                    console.log(data);
                    $("#search-results").empty();
                    $.each(data, function (index, suggestion) {
                        console.log(suggestion);
                        $("#search-results").append(`
            <div class="suggestion" data-name="${suggestion.name}">
                <img src="/images/${suggestion.imageUrl}" alt="${suggestion.name}" style="width:60px; height:76px; margin-right:10px;">
                <div>
                    <span style="font-size:16px;">${suggestion.name}</span><br>
                    <span>${suggestion.price > 0 ? suggestion.price + " VNĐ" : "Miễn phí"}</span>
                </div>
            </div>
        `);
                    });
                    $(".dropdown-content").css("display", "block");
                },
                error: function (xhr, status, error) {
                    console.log("Error:", error);
                }
            });
        } else {
            $("#search-results").empty(); // Xóa gợi ý nếu không có query
            $(".dropdown-content").css("display", "none"); // Ẩn dropdown nếu input rỗng
        }
    });

    // Xử lý khi click vào suggestion
    $(document).on("click", ".suggestion", function () {
        // Lấy tên sách từ thuộc tính data-name của phần tử suggestion
        var selectedSuggestion = $(this).data("name");

        // Gán giá trị tên sách vào ô input
        $("#search-input").val(selectedSuggestion);

        // Xóa gợi ý
        $("#search-results").empty();

        // Ẩn dropdown
        $(".dropdown-content").css("display", "none");
    });

    // Đóng dropdown nếu click ra ngoài
    $(document).click(function (event) {
        if (!$(event.target).closest('.dropdown, #search-results').length) {
            $(".dropdown-content").css("display", "none");
        }
    });
});

/* ------------------------------ 3 gạch ----------------------------->*/
document.addEventListener('DOMContentLoaded', function () {
    const menuButton = document.querySelector('.menu-button');
    const contentPage = document.getElementById('content-page');
    const sidebar = document.querySelector('.iq-sidebar');
    const topsidebar = document.querySelector('.iq-top-navbar');
    const containfluid = document.querySelector('.container-fluid');
    const footer = document.querySelector('.iq-footer');
    const icon = document.querySelector('.chat');
    const footer1 = document.querySelector('.iq-footer1');

    // const cateplus = document.querySelector('.ok');
    //  const cateplus = document.querySelector('.some');

    menuButton.addEventListener('click', () => {
        if (contentPage.style.marginLeft === '0px') {
            contentPage.style.marginLeft = '300px'; // Adjust the value as needed
            sidebar.style.display = 'block';
            topsidebar.style.left = 'auto'; // Reset left position
            topsidebar.style.width = 'calc(100% - 300px)';
            containfluid.style.width = '100%';
            footer.style.marginLeft = '245px';
            icon.style.display = 'none';
            footer1.style.marginLeft = 'auto';
        } else {
            contentPage.style.marginLeft = '0px';
            sidebar.style.display = 'none';
            topsidebar.style.left = '1px';
            topsidebar.style.width = '100%';
            containfluid.style.width = 'auto';
            footer.style.marginLeft = '0px';
            icon.style.display = 'block';
            footer1.style.marginLeft = '320px';
        }
    });
});

/* ------------------------------ + - ----------------------------->*/
$(document).ready(function () {
    $("#plus-icon").click(function () {
        $(".some").addClass("show");  // Show the list with transition
        $(this).hide();               // Hide the plus icon
        $("#minus-icon").show();
        $(".image-carousel").removeClass("with-spacing"); // Gỡ khoảng cách thêm
        // Show the minus icon
    });

    $("#minus-icon").click(function () {
        $(".some").removeClass("show"); // Hide the list with transition
        $(this).hide();                 // Hide the minus icon
        $("#plus-icon").show();
        $(".image-carousel").addClass("with-spacing"); // Thêm khoảng cách thêm
        // Show the plus icon
    });
});

const carouselSlide = document.querySelector('.carousel-slide');

carouselSlide.addEventListener('mouseover', () => {
    carouselSlide.style.animationPlayState = 'paused';
});

carouselSlide.addEventListener('mouseout', () => {
    carouselSlide.style.animationPlayState = 'running';
});

/* -------------------------- mở ra là ko search hay các nút ở layout ko ăn ------------------------>*/
//$.ajax({
//    url: "/books/SearchSuggestions",
//    type: "GET",
//    data: { query: query },
//    dataType: "json",
//    success: function (data) {
//        console.log(data); // In dữ liệu JSON trả về vào Console
//        $("#search-results").empty();
//        $.each(data, function (index, suggestion) {
//            $("#search-results").append(`
//                <div>${suggestion.Name}</div>
//            `);
//        });
//    },
//    error: function (xhr, status, error) {
//        console.error("Error:", error); // Ghi log nếu có lỗi
//    }
//});

/* -------------------------- Old search product ------------------------>*/
 //$(document).ready(function () {
 //    $("#search-input").keyup(function () {
 //        var query = $(this).val();
 //        if (query !== '') {
 //            $.ajax({
 //                url: "/books/SearchSuggestions",
 //                type: "GET",
 //                data: { query: query },
 //                dataType: "json",
 //                success: function (data) {
 //                    $("#search-results").empty(); // Clear previous suggestions
 //                    $.each(data, function (index, suggestion) {
 //                        $("#search-results").append("<div class='suggestion'>" +
 //                            suggestion + "</div>"); // Append new suggestions
 //                    });
 //                    $(".dropdown-content").css("display", "block"); // Show dropdown
 //                },
 //                error: function (xhr, status, error) {
 //                    console.log("Error:", error);
 //                }
 //            });
 //        } else {
 //            $("#search-results").empty(); // Clear suggestions if query is empty
 //            $(".dropdown-content").css("display", "none"); // Hide dropdown if input is empty
 //        }
 //    });
 //    // Handle click on suggestion
 //    $(document).on("click", ".suggestion", function () {
 //        var selectedSuggestion = $(this).text();
 //        $("#search-input").val(selectedSuggestion); // Set search input value to elected suggestion
 //        $("#search-results").empty(); // Clear suggestions
 //        $(".dropdown-content").css("display", "none"); // Hide dropdown
 //    });
 //    // Close dropdown if user clicks outside of it
 //    $(document).click(function (event) {
 //        if (!$(event.target).closest('.dropdown').length) {
 //            $(".dropdown-content").css("display", "none");
 //        }
 //    });
//});

/* ------------------------------  ----------------------------->*/
/* ------------------------------  ----------------------------->*/