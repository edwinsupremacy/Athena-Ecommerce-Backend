using AthenaEcommerce_website.Data;
using AthenaEcommerce_website.Models;
using Microsoft.EntityFrameworkCore;

namespace AthenaEcommerce_website.Services;

public class PendingOrderCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private static readonly TimeSpan PendingTimeout = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(2);

    public PendingOrderCleanupService(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var cutoff = DateTime.UtcNow - PendingTimeout;

            var staleOrders = await context.Order
                .Include(o => o.OrderItems)
                .Where(o => o.Status == OrderStatus.Pending && o.CreatedAt < cutoff)
                .ToListAsync(stoppingToken);

            foreach (var order in staleOrders)
            {
                foreach (var orderItem in order.OrderItems)
                {
                    var itemSize = await context.ItemSize
                        .FirstOrDefaultAsync(
                            s => s.ItemId == orderItem.ItemId && s.Size == orderItem.Size,
                            stoppingToken);

                    if (itemSize != null)
                        itemSize.StockAvailable += orderItem.Quantity;
                }

                order.Status = OrderStatus.Failed;
            }

            if (staleOrders.Count > 0)
            {
                await context.SaveChangesAsync(stoppingToken);
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }
}