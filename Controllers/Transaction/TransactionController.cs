using AthenaEcommerce_website.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AthenaEcommerce_website.Controllers.Transaction
{
    [Route("transaction")]
    [ApiController]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _context.Order
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            if (orders.Count == 0)
            {
                return NotFound("No orders found");
            }

            return Ok(orders);
        }

         [HttpGet("get-order/{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await _context.Order
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            return Ok(order);
        }

          [HttpGet("get-by-reference/{orderReference}")]
        public async Task<IActionResult> GetOrderByReference(string orderReference)
        {
            var order = await _context.Order
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .FirstOrDefaultAsync(o => o.OrderReference == orderReference);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            return Ok(order);
        }


    }
}
