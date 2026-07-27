using System.Text.Json;

namespace AthenaEcommerce_website.DTOs.Callbacks;


public class MpesaCallbackDto
{
    public MpesaCallbackBody Body { get; set; } = new();
}

public class MpesaCallbackBody
{
    public MpesaStkCallback StkCallback { get; set; } = new();
}

public class MpesaStkCallback
{
    public string MerchantRequestID { get; set; } = string.Empty;
    public string CheckoutRequestID { get; set; } = string.Empty;
    public int ResultCode { get; set; }
    public string ResultDesc { get; set; } = string.Empty;
    public CallbackMetadata? CallbackMetadata { get; set; }
}

public class CallbackMetadata
{
    public List<CallbackMetadataItem> Item { get; set; } = new();
}

public class CallbackMetadataItem
{
    public string Name { get; set; } = string.Empty;
    public JsonElement Value { get; set; }
}
