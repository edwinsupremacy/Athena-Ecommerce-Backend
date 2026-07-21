namespace AthenaEcommerce_website.Models;

public enum OrderStatus
{
    Pending,
    Paid,
    Failed,
    Cancelled
}

public enum ModeOfCollection
{
    Delivery,
    Pickup
}