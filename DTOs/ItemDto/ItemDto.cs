using System;
using AthenaEcommerce_website.Models;

namespace AthenaEcommerce_website.DTOs.ItemDto;

public class ItemSizeDto
{
    public int Size { get; set; }
    public int StockAvailable { get; set; }
}

public class PostItemDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public List<ItemSizeDto> Sizes { get; set; } = new();
    public IFormFile Image { get; set; } = null!;
    public Category Category { get; set; }
    public PriceRange PriceRange { get; set; }
    public Color Color { get; set; }
    public ShoeType ShoeType { get; set; }
}

public class UpdateItemDto
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public List<ItemSizeDto>? Sizes { get; set; }
    public IFormFile? Image { get; set; }
    public Category? Category { get; set; }
    public PriceRange? PriceRange { get; set; }
    public Color? Color { get; set; }
    public ShoeType? ShoeType { get; set; }
}

public class ItemResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public List<ItemSizeDto> Sizes { get; set; } = new();
    public string ImageUrl { get; set; } = string.Empty;
}
