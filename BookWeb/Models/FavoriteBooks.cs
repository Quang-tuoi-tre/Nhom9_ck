﻿namespace BookWeb.Models
{
    public class FavoriteBooks
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
        public DateTime AddedDate { get; set; }
    }
}
