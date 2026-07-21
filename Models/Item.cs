using System;

namespace AthenaEcommerce_website.Models;

public class Item
{
public Guid ItemId { get; set; }
public string ImageUrl { get; set; } = string.Empty;
public string ImagePublicId { get; set; } = string.Empty;
public string Name { get; set; } = String.Empty;
public int Price { get; set; }
public int StockAvailable { get; set; }
public int SizesAvailable { get; set; }

}
