using System;

namespace AthenaEcommerce_website.Models;

public class Order
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string SecondName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
public string? MpesaReceiptNumber { get; set; }
    public string? CheckoutRequestId { get; set; }
    public int Quantity { get; set; }
    public string OrderReference { get; set; } = string.Empty;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public ModeOfCollection ModeOfCollection { get; set; } = ModeOfCollection.Delivery;

    public string DeliveryLocation { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
