using System;

namespace AthenaEcommerce_website.Models;

public class Checkout
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string SecondName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int TotalPrice { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string ImagePublicId { get; set; } = string.Empty;

    public int Quantity { get; set; }
    public string OrderReference { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public ModeOfCollection ModeOfCollection { get; set; }= ModeOfCollection.Delivery;

    public string DeliveryLocation { get; set; }=string.Empty;
    public DateTime CreatedAt { get; set; }=DateTime.UtcNow;
}
