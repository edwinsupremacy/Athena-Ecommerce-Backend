using AthenaEcommerce_website.Data;
using AthenaEcommerce_website.DTOs.AdminDto;
using AthenaEcommerce_website.DTOs.ItemDto;
using AthenaEcommerce_website.Interfaces;
using AthenaEcommerce_website.Models;
// using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace AthenaEcommerce_website.Controllers.Auth
{
    [Route("api/products")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("get-item/{id}")]
        public async Task<IActionResult> GetItem(Guid id)
        {
            var existingItem = await _context.Item
                .Include(i => i.ItemSizes)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (existingItem == null)
            {
                return NotFound("Item not found");
            }
            return Ok(existingItem);
        }




        [HttpGet("search-item")]
        public async Task<IActionResult> SearchItem(
            [FromQuery] string name,
            [FromQuery] PriceRange? priceRange,
            [FromQuery] Color? color,
            [FromQuery] ShoeType? shoeType,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Search term is required");
            }

            var query = ApplyCommonFilters(
                _context.Item.Where(i => i.Name.Contains(name)),
                priceRange, color, shoeType, minPrice, maxPrice);

            var items = await query
                .Select(i => new ItemResponseDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Price = i.Price,
                    Sizes = i.ItemSizes.Select(s => new ItemSizeDto
                    {
                        Size = s.Size,
                        StockAvailable = s.StockAvailable,
                    }).ToList(),

                    ImageUrl = i.ImageUrl,
                })
                .ToListAsync();

            if (items.Count == 0)
            {
                return NotFound("No items found");
            }

            return Ok(items);
        }

        private IQueryable<Item> ApplyCommonFilters(
      IQueryable<Item> query,
      PriceRange? priceRange,
      Color? color,
      ShoeType? shoeType,
      decimal? minPrice,
      decimal? maxPrice)
        {
            if (priceRange.HasValue)
                query = query.Where(i => i.PriceRange == priceRange.Value);

            if (color.HasValue)
                query = query.Where(i => i.Color == color.Value);

            if (shoeType.HasValue)
                query = query.Where(i => i.ShoeType == shoeType.Value);

            if (minPrice.HasValue)
                query = query.Where(i => i.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(i => i.Price <= maxPrice.Value);

            return query;
        }


        [HttpGet("get-items")]
        public async Task<IActionResult> GetItems(
   [FromQuery] PriceRange? priceRange,
   [FromQuery] Color? color,
   [FromQuery] ShoeType? shoeType,
   [FromQuery] decimal? minPrice,
   [FromQuery] decimal? maxPrice)
        {
            var query = ApplyCommonFilters(_context.Item.AsQueryable(), priceRange, color, shoeType, minPrice, maxPrice);

            var items = await query
                .Select(i => new ItemResponseDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Price = i.Price,
                    Sizes = i.ItemSizes.Select(s => new ItemSizeDto
                    {
                        Size = s.Size,
                        StockAvailable = s.StockAvailable,
                    }).ToList(),

                    ImageUrl = i.ImageUrl,
                })
                .ToListAsync();

            if (items.Count == 0)
                return NotFound("No items found");

            return Ok(items);
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetItemsByCategory(
            Category category,
            [FromQuery] PriceRange? priceRange,
            [FromQuery] Color? color,
            [FromQuery] ShoeType? shoeType,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            var query = ApplyCommonFilters(
                _context.Item.Where(i => i.Category == category),
                priceRange, color, shoeType, minPrice, maxPrice);

            var items = await query
                .Select(i => new ItemResponseDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Price = i.Price,
                    Sizes = i.ItemSizes.Select(s => new ItemSizeDto
                    {
                        Size = s.Size,
                        StockAvailable = s.StockAvailable,
                    }).ToList(),

                    ImageUrl = i.ImageUrl,
                })
                .ToListAsync();

            if (items.Count == 0)
                return NotFound("No items found");

            return Ok(items);
        }
 

    }
}
