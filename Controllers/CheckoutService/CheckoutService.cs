using AthenaEcommerce_website.Data;
using AthenaEcommerce_website.DTOs.Callbacks;
using AthenaEcommerce_website.DTOs.CheckoutDto;
using AthenaEcommerce_website.Models;
using AthenaEcommerce_website.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AthenaEcommerce_website.Controllers.CheckoutService
{
    [Route("transaction")]
    [ApiController]
    public class CheckoutService : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly MpesaService _mpesaService;

        public CheckoutService(ApplicationDbContext context, MpesaService mpesaService)
        {
            _context = context;
            _mpesaService = mpesaService;
        }


        [HttpPost("checkout-items")]
        public async Task<IActionResult> CheckoutItems([FromBody] CheckoutRequestDto checkoutRequestDto)
        {
            if (checkoutRequestDto == null || checkoutRequestDto.Items == null || checkoutRequestDto.Items.Count == 0)
            {
                return BadRequest("Invalid checkout request.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest("Please provide valid checkout items");
            }

            var order = new Order
            {
                FirstName = checkoutRequestDto.FirstName,
                SecondName = checkoutRequestDto.SecondName,
                PhoneNumber = checkoutRequestDto.PhoneNumber,
                Email = checkoutRequestDto.Email,
                ModeOfCollection = checkoutRequestDto.ModeOfCollection,
                DeliveryLocation = checkoutRequestDto.DeliveryLocation,
                OrderReference = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                Status = OrderStatus.Pending,
            };

            decimal totalPrice = 0;

            foreach (var checkoutItem in checkoutRequestDto.Items)
            {
                var item = await _context.Item.FindAsync(checkoutItem.ItemId);

                if (item == null)
                {
                    return BadRequest($"Item {checkoutItem.ItemId} not found.");
                }

                var itemSize = await _context.ItemSize
                    .FirstOrDefaultAsync(s => s.ItemId == checkoutItem.ItemId && s.Size == checkoutItem.Size);

                if (itemSize == null)
                {
                    return BadRequest($"Size {checkoutItem.Size} not available for {item.Name}.");
                }

                if (itemSize.StockAvailable < checkoutItem.Quantity)
                {
                    return BadRequest($"Not enough stock for {item.Name} in size {checkoutItem.Size}.");
                }

                var orderItem = new OrderItem
                {
                    ItemId = item.Id,
                    Size = checkoutItem.Size,
                    Quantity = checkoutItem.Quantity,
                    PriceAtPurchase = item.Price,
                };

                order.OrderItems.Add(orderItem);

                itemSize.StockAvailable -= checkoutItem.Quantity;

                totalPrice += item.Price * checkoutItem.Quantity;
            }

            order.TotalPrice = totalPrice;

            _context.Order.Add(order);
            await _context.SaveChangesAsync();

            /////mpesa stk push




            MpesaStkPushResponse stkResponse;
            try
            {
                stkResponse = await _mpesaService.InitiateStkPushAsync(
                checkoutRequestDto.PhoneNumber, totalPrice, order.OrderReference);
            }
            catch (Exception ex)
            {
                foreach (var orderItem in order.OrderItems)
                {
                    var itemSize = await _context.ItemSize
                        .FirstOrDefaultAsync(s => s.ItemId == orderItem.ItemId && s.Size == orderItem.Size);

                    if (itemSize != null) itemSize.StockAvailable += orderItem.Quantity;
                }

                order.Status = OrderStatus.Failed;
                await _context.SaveChangesAsync();

                return StatusCode(502, $"Payment initiation failed: {ex.Message}");
            }

            order.CheckoutRequestId = stkResponse.CheckoutRequestID;
            await _context.SaveChangesAsync();


            return Ok(new
            {
                order.OrderReference,
                TotalPrice = totalPrice,
                stkResponse.CustomerMessage
            });
        }

        [HttpPost("mpesa-callback")]
public async Task<IActionResult> MpesaCallback([FromBody] MpesaCallbackDto callback)
{
    var stkCallback = callback.Body.StkCallback;

    var order = await _context.Order
        .Include(o => o.OrderItems)
        .FirstOrDefaultAsync(o => o.CheckoutRequestId == stkCallback.CheckoutRequestID);

    if (order == null)
        return Ok(new { ResultCode = 0, ResultDesc = "Accepted" });

    if (order.Status != OrderStatus.Pending)
        return Ok(new { ResultCode = 0, ResultDesc = "Accepted" });

    // Don't trust the callback body directly — verify with Safaricom ourselves
    MpesaStkQueryResponse verified;
    try
    {
        verified = await _mpesaService.QueryStkStatusAsync(stkCallback.CheckoutRequestID);
    }
    catch (Exception)
    {
        // Could not verify right now — leave order Pending, let the cleanup service
        // or a retry handle it later, rather than trusting the unverified callback.
        return Ok(new { ResultCode = 0, ResultDesc = "Accepted" });
    }

    if (verified.ResultCode == "0")
    {
        var receiptItem = stkCallback.CallbackMetadata?.Item
            .FirstOrDefault(i => i.Name == "MpesaReceiptNumber");

        var amountItem = stkCallback.CallbackMetadata?.Item
            .FirstOrDefault(i => i.Name == "Amount");
        var paidAmount = amountItem?.Value.GetDecimal() ?? 0;

        if (paidAmount < order.TotalPrice)
        {
            await RestoreStockAsync(order);
            order.Status = OrderStatus.Failed;
        }
        else
        {
            order.MpesaReceiptNumber = receiptItem?.Value.GetString();
            order.Status = OrderStatus.Paid;
        }
    }
    else
    {
        await RestoreStockAsync(order);
        order.Status = OrderStatus.Failed;
    }

    await _context.SaveChangesAsync();

    return Ok(new { ResultCode = 0, ResultDesc = "Accepted" });
}

private async Task RestoreStockAsync(Order order)
{
    foreach (var orderItem in order.OrderItems)
    {
        var itemSize = await _context.ItemSize
            .FirstOrDefaultAsync(s => s.ItemId == orderItem.ItemId && s.Size == orderItem.Size);

        if (itemSize != null) itemSize.StockAvailable += orderItem.Quantity;
    }
}
    }

}
