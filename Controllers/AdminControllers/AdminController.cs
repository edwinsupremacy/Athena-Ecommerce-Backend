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



namespace AthenaEcommerce_website.Controllers.AdminControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _token;
        private readonly Cloudinary _cloudinary;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, SignInManager<User> signInManager, UserManager<User> userManager, ITokenService token, Cloudinary cloudinary, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _token = token;
            _cloudinary = cloudinary;
            _roleManager = roleManager;
        }



        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin(RegisterAdminDto registerAdminDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var adminCount = await _userManager.Users.CountAsync();
            if (adminCount >= 2)
            {
                return BadRequest("Maximum number of admin accounts reached.");
            }

            var existingEmail = await _userManager.FindByEmailAsync(registerAdminDto.Email);
            if (existingEmail != null)
            {
                return BadRequest("A user with this email already exists.");
            }

            var existingUserName = await _userManager.FindByNameAsync(registerAdminDto.UserName);
            if (existingUserName != null)
            {
                return BadRequest("A user with this username already exists.");
            }

            if (registerAdminDto.Password != registerAdminDto.ConfirmPassword)
            {
                return BadRequest();
            }

            var user = new User
            {
                FirstName = registerAdminDto.FirstName,
                SecondName = registerAdminDto.SecondName,
                UserName = registerAdminDto.UserName,
                Email = registerAdminDto.Email,
                PhoneNumber = registerAdminDto.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(user, registerAdminDto.Password);
            if (!result.Succeeded) return BadRequest("Could not create account. Try Again");

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole("Admin"));
                if (!roleResult.Succeeded) return BadRequest("Could not create the Admin role.");
            }

            var addToRoleResult = await _userManager.AddToRoleAsync(user, "Admin");
            if (!addToRoleResult.Succeeded) return BadRequest("Could not assign the Admin role.");

            var token = await _token.CreateToken(user);
            return Ok(
                new ResponseAdminDto
                {
                    Token = token,
                    FirstName = user.FirstName,
                    SecondName = user.SecondName,
                    UserName = user.UserName,
                    Email = user.Email!,
                    PhoneNumber = user.PhoneNumber!,

                }
            );
        }
        [Authorize]
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto deleteAccountDto)
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var passwordValid = await _signInManager.CheckPasswordSignInAsync(user, deleteAccountDto.Password, false);

            if (!passwordValid.Succeeded)
            {
                return BadRequest("Incorrect password");
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest("Could not delete account. Try again.");
            }

            return Ok("Account deleted successfully");
        }

        [HttpPost("login-admin")]
        public async Task<IActionResult> LoginAdmin(LoginAdminDto loginAdminDto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest("Wrong format");
            }
            User? user;

            if (loginAdminDto.UserNameOrEmailorPhone.Contains('@'))
            {
                user = await _userManager.FindByEmailAsync(loginAdminDto.UserNameOrEmailorPhone);
            }
            else
            {
                user = await _userManager.FindByNameAsync(loginAdminDto.UserNameOrEmailorPhone);
            }

            if (user == null) return BadRequest("Email/Username or Password is incorrect");

            var password = await _signInManager.CheckPasswordSignInAsync(user, loginAdminDto.Password, false);
            if (!password.Succeeded)
            {
                return BadRequest("Wrong credentials");
            }
            var role = await _userManager.IsInRoleAsync(user, "Admin");

            if (role == false)
            {
                return BadRequest("You are not an Admin");
            }

            var token = await _token.CreateToken(user);

            return Ok(new ResponseLoginDto
            {
                Token = token,
                FirstName = user.FirstName,
                SecondName = user.SecondName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber!,
                Role = "Admin"
            });


        }






        /////////////////////////////////////////

        [HttpPost("post-item")]
        [Authorize]
        public async Task<IActionResult> PostItem([FromForm] PostItemDto postItemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Response");
            }

            var uploadResult = new ImageUploadParams
            {
                File = new FileDescription(postItemDto.Image.FileName, postItemDto.Image.OpenReadStream()),
            };

            var uploadedImage = await _cloudinary.UploadAsync(uploadResult);

            if (uploadedImage.Error != null)
            {
                return BadRequest(uploadedImage.Error.Message);
            }

            var newItem = new Item
            {
                Name = postItemDto.Name,
                Price = postItemDto.Price,
                ImageUrl = uploadedImage.SecureUrl.ToString(),
                ImagePublicId = uploadedImage.PublicId,
                Category = postItemDto.Category,
                PriceRange = postItemDto.PriceRange,
                Color = postItemDto.Color,
                ShoeType = postItemDto.ShoeType,
                ItemSizes = postItemDto.Sizes.Select(s => new ItemSize
                {
                    Size = s.Size,
                    StockAvailable = s.StockAvailable,
                }).ToList(),
            };

            _context.Item.Add(newItem);
            await _context.SaveChangesAsync();

            return Ok(newItem);
        }


        [HttpPut("update-item/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateItem(Guid id, [FromForm] UpdateItemDto updateItemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid Response");
            }

            var existingItem = await _context.Item.FindAsync(id);

            if (existingItem == null)
            {
                return NotFound("Item not found");
            }

            existingItem.Name = updateItemDto.Name ?? existingItem.Name;
            existingItem.Price = updateItemDto.Price ?? existingItem.Price;
            existingItem.Category = updateItemDto.Category ?? existingItem.Category;
            existingItem.PriceRange = updateItemDto.PriceRange ?? existingItem.PriceRange;
            existingItem.Color = updateItemDto.Color ?? existingItem.Color;
            existingItem.ShoeType = updateItemDto.ShoeType ?? existingItem.ShoeType;
            if (updateItemDto.Sizes != null)
            {
                var existingSizes = await _context.ItemSize.Where(s => s.ItemId == id).ToListAsync();
                _context.ItemSize.RemoveRange(existingSizes);

                foreach (var s in updateItemDto.Sizes)
                {
                    _context.ItemSize.Add(new ItemSize
                    {
                        ItemId = id,
                        Size = s.Size,
                        StockAvailable = s.StockAvailable,
                    });
                }
            }

            if (updateItemDto.Image != null)
            {
                if (!string.IsNullOrEmpty(existingItem.ImagePublicId))
                {
                    await _cloudinary.DestroyAsync(new DeletionParams(existingItem.ImagePublicId));
                }

                var uploadResult = new ImageUploadParams
                {
                    File = new FileDescription(updateItemDto.Image.FileName, updateItemDto.Image.OpenReadStream()),
                };

                var uploadedImage = await _cloudinary.UploadAsync(uploadResult);

                if (uploadedImage.Error != null)
                {
                    return BadRequest(uploadedImage.Error.Message);
                }

                existingItem.ImageUrl = uploadedImage.SecureUrl.ToString();
                existingItem.ImagePublicId = uploadedImage.PublicId;
            }
            await _context.SaveChangesAsync();

            return Ok(existingItem);

        }
        [HttpDelete("delete-item/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            var existingItem = await _context.Item.FindAsync(id);
            if (existingItem == null)
            {
                return NotFound("Item not found");
            }
            _context.Item.Remove(existingItem);
            await _context.SaveChangesAsync();
            return Ok("Item deleted successfully");
        }

    }
}
