using System;

namespace AthenaEcommerce_website.Models;

public class Item
{
    public Guid Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string ImagePublicId { get; set; } = string.Empty;
    public string Name { get; set; } = String.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
 
    public Category Category { get; set; }

    public PriceRange PriceRange { get; set; }
    public Color Color { get; set; }
    public ShoeType ShoeType { get; set; }

    public ICollection<ItemSize> ItemSizes { get; set; } = new List<ItemSize>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public bool IsDeleted { get; set; } = false;
}


