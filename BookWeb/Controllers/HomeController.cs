using BookWeb.Data;
using BookWeb.Models;
using BookWeb.Repositories;
using BookWeb.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookWeb.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly IBookRepo _bookRepo;
        private readonly ICategoryRepo _categoryRepo;

        public HomeController(ApplicationDbContext context, IBookRepo bookRepo, ICategoryRepo categoryRepo)
        {
            _context = context;
            _bookRepo = bookRepo;
            _categoryRepo = categoryRepo;
        }

        public async Task<IActionResult> Index(int currentGroupTopFavorites = 1, int currentGroupTopViewed = 1, int currentGroupTopSelling = 1)
        {
            // Lấy danh sách sách mới nhất (giới hạn 12 sách)
            var books = await _bookRepo.GetAllAsync();
            books = books.OrderByDescending(b => b.CreatedDate).Take(12);

            // Lấy danh sách danh mục
            var cates = await _categoryRepo.GetAllAsync();

            // Lấy Top sách được yêu thích
            var topFavorites = await _bookRepo.GetTopFavoritedBooksAsync(10);
            int groupSize = 4;
            int totalFavorites = topFavorites.Count();
            int totalGroupsFavorites = (int)Math.Ceiling((double)totalFavorites / groupSize);
            int startIndexFavorites = (currentGroupTopFavorites - 1) * groupSize;
            var currentFavorites = topFavorites.Skip(startIndexFavorites).Take(groupSize).ToList();

            // Lấy Top sách được xem nhiều nhất
            var topViewed = await _bookRepo.GetTopViewedBooksAsync(10);
            int totalViewed = topViewed.Count();
            int totalGroupsViewed = (int)Math.Ceiling((double)totalViewed / groupSize);
            int startIndexViewed = (currentGroupTopViewed - 1) * groupSize;
            var currentViewed = topViewed.Skip(startIndexViewed).Take(groupSize).ToList();

            // Lấy Top sách bán chạy nhất
            var topSelling = await _bookRepo.GetAllAsync();
            topSelling = books.OrderByDescending(b => b.SoldQuantity).Take(12);
            //var topSelling = await _bookRepo.GetTopSellingBooksAsync(12);
            //int totalSelling = topSelling.Count();
            //int totalGroupsSelling = (int)Math.Ceiling((double)totalSelling / groupSize);
            //int startIndexSelling = (currentGroupTopSelling - 1) * groupSize;
            //var currentSelling = topSelling.Skip(startIndexSelling).Take(groupSize).ToList();

            // Tạo ViewModel để truyền dữ liệu
            HomeViewModel home = new HomeViewModel()
            {
                Categories = cates,
                Books = books,
                // Thêm sách thích nhiều
                TopBooksFavorites = currentFavorites,
                TotalGroupsFavorites = totalGroupsFavorites,
                CurrentGroupFavorites = currentGroupTopFavorites,
                StartIndexFavorites = startIndexFavorites,
                // Thêm sách xem nhiều
                TopBooksViewed = currentViewed,
                TotalGroupsViewed = totalGroupsViewed,
                CurrentGroupViewed = currentGroupTopViewed,
                StartIndexViewed = startIndexViewed,
                // Thêm sách bán chạy vào ViewModel
                TopBooksSelling = topSelling
                //TopBooksSelling = currentSelling,  
                //TotalGroupsSelling = totalGroupsSelling,
                //CurrentGroupSelling = currentGroupTopSelling,
                //StartIndexSelling = startIndexSelling
            };

            return View(home);
        }

        public async Task<IActionResult> LoadFavorites(int currentGroupTopFavorites)
        {
            int groupSize = 4;

            var topFavorites = await _bookRepo.GetTopFavoritedBooksAsync(10);
            int totalFavorites = topFavorites.Count();
            int totalGroupsFavorites = (int)Math.Ceiling((double)totalFavorites / groupSize);

            int startIndexFavorites = (currentGroupTopFavorites - 1) * groupSize;
            var currentFavorites = topFavorites.Skip(startIndexFavorites).Take(groupSize).ToList();

            var viewModel = new HomeViewModel
            {
                TopBooksFavorites = currentFavorites,
                CurrentGroupFavorites = currentGroupTopFavorites,
                TotalGroupsFavorites = totalGroupsFavorites,
                StartIndexFavorites = startIndexFavorites
            };

            return PartialView("_TopBooksPartial", viewModel);
        }

        public async Task<IActionResult> LoadViewed(int currentGroupTopViewed)
        {
            int groupSize = 4;

            var topViewed = await _bookRepo.GetTopViewedBooksAsync(10);
            int totalViewed = topViewed.Count();
            int totalGroupsViewed = (int)Math.Ceiling((double)totalViewed / groupSize);

            int startIndexViewed = (currentGroupTopViewed - 1) * groupSize;
            var currentViewed = topViewed.Skip(startIndexViewed).Take(groupSize).ToList();

            var viewModel = new HomeViewModel
            {
                TopBooksViewed = currentViewed,
                CurrentGroupViewed = currentGroupTopViewed,
                TotalGroupsViewed = totalGroupsViewed,
                StartIndexViewed = startIndexViewed
            };

            return PartialView("_PartialView", viewModel);  // Sử dụng chung PartialView
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult TermOfUse()
        {
            return View();
        }
        public IActionResult ContactUs()
        {
            return View();
        }
        public IActionResult AboutUs()
        {
            return View();
        }
        public IActionResult Service()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
