using System.Text;
using System.Text.Json;
using AthenaEcommerce_website.Config;
using AthenaEcommerce_website.DTOs.CheckoutDto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AthenaEcommerce_website.Services
{
    [Route("api/[controller]")]
    [ApiController]
    public class MpesaService : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly MpesaConfig _config;
        public MpesaService(HttpClient httpClient, IOptions<MpesaConfig> mpesaConfig)
        {
            _httpClient = httpClient;
            _config = mpesaConfig.Value;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_config.ConsumerKey}:{_config.ConsumerSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_config.BaseUrl}/oauth/v1/generate?grant_type=client_credentials");

            request.Headers.Add("Authorization", $"Basic {credentials}");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<JsonElement>(json);

            return data.GetProperty("access_token").GetString()!;
        }

        public string NormalizePhoneNumber(string phone)
        {
            phone = phone.Trim();
            if (phone.StartsWith("+")) phone = phone.Substring(1);
            if (phone.StartsWith("0")) phone = "254" + phone.Substring(1);
            return phone;
        }

        public async Task<MpesaStkPushResponse> InitiateStkPushAsync(string phoneNumber, decimal amount, string orderReference)
        {
            var accessToken = await GetAccessTokenAsync();
            var normalizedPhone = NormalizePhoneNumber(phoneNumber);

            var eatZone = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
            var eatNow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, eatZone);
            var timestamp = eatNow.ToString("yyyyMMddHHmmss");

            var password = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_config.BusinessShortCode}{_config.Passkey}{timestamp}"));

            var roundedAmount = (int)Math.Round(amount, MidpointRounding.AwayFromZero);

            var payload = new
            {
                BusinessShortCode = _config.BusinessShortCode,
                Password = password,
                Timestamp = timestamp,
                TransactionType = "CustomerPayBillOnline",
                Amount = roundedAmount,
                PartyA = normalizedPhone,
                PartyB = _config.BusinessShortCode,
                PhoneNumber = normalizedPhone,
                CallBackURL = _config.CallbackUrl,
                AccountReference = orderReference,
                TransactionDesc = "Order Payment"
            };

            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{_config.BaseUrl}/mpesa/stkpush/v1/processrequest");

            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"STK Push failed: {responseJson}");
            }

            return JsonSerializer.Deserialize<MpesaStkPushResponse>(responseJson)!;
        }


    }
}
