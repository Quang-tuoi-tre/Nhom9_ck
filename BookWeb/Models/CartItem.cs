﻿namespace BookWeb.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Image { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int StockQuantity { get; set; }
        public bool IsSelected { get; set; }
    }
}
