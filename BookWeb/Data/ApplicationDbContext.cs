using BookWeb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> 
    { 
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { 
        }
            public DbSet<Book> Books { get; set; }
            public DbSet<Category> Categories { get; set; }
            public DbSet<Author> Authors { get; set; }

        public DbSet<FavoriteBooks> FavoriteBooks { get; set; }
        public DbSet<Comment> Comments { get; set; }        
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<CartItem> CartItem { get; set; } = default!;
        public DbSet<MomoInfoModel> MomoInfoModels { get; set; }
    }
}
