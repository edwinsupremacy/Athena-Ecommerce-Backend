using System;

namespace AthenaEcommerce_website.Models;

public class ItemSize
{
    public Guid Id { get; set; }
    public int Size { get; set; }
    public int StockAvailable { get; set; }
    public Guid ItemId { get; set; }
    public Item? Item { get; set; }
}