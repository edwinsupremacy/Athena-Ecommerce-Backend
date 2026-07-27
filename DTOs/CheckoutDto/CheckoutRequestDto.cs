using System;
using AthenaEcommerce_website.Models;

namespace AthenaEcommerce_website.DTOs.CheckoutDto;

public class CheckoutItemDto
{
    public Guid ItemId { get; set; }
    public int Size { get; set; }
    public int Quantity { get; set; }
}

public class CheckoutRequestDto
{
    public string FirstName { get; set; } = string.Empty;
    public string SecondName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public ModeOfCollection ModeOfCollection { get; set; }
    public string DeliveryLocation { get; set; } = string.Empty;

    public List<CheckoutItemDto> Items { get; set; } = new List<CheckoutItemDto>();
}


public class MpesaStkPushResponse
{
    public string MerchantRequestID { get; set; } = string.Empty;
    public string CheckoutRequestID { get; set; } = string.Empty;
    public string ResponseCode { get; set; } = string.Empty;
    public string ResponseDescription { get; set; } = string.Empty;
    public string CustomerMessage { get; set; } = string.Empty;
}


public class MpesaStkQueryResponse
{
    public string ResponseCode { get; set; } = string.Empty;
    public string ResponseDescription { get; set; } = string.Empty;
    public string MerchantRequestID { get; set; } = string.Empty;
    public string CheckoutRequestID { get; set; } = string.Empty;
    public string ResultCode { get; set; } = string.Empty;
    public string ResultDesc { get; set; } = string.Empty;
}