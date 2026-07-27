using System;


namespace AthenaEcommerce_website.Config;

public class MpesaConfig
{
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
    public string Passkey { get; set; } = string.Empty;
    public string BusinessShortCode { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
}