using AthenaEcommerce_website.Data;
using AthenaEcommerce_website.DTOs.AdminDto;
using AthenaEcommerce_website.Interfaces;
using AthenaEcommerce_website.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AthenaEcommerce_website.Controllers.Auth
{
    [Route("api/auth")]
    [ApiController]
    public class Auth : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _token;
        public Auth(ApplicationDbContext context, SignInManager<User> signInManager, UserManager<User> userManager, ITokenService token)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _token = token;
        }

        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin(RegisterAdminDto registerAdminDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
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
            if (!result.Succeeded) return BadRequest("Could not create account.Try Again");


            await _userManager.AddToRoleAsync(user, "Admin");

            var token = await _token.CreateToken(user);
            return Ok
            (
               new ResponseAdminDto
               {
                   Token = token,
                   FirstName = user.FirstName,
                   SecondName = user.SecondName,
                   UserName = user.UserName,
                   Email = user.Email!,
                   PhoneNumber = user.PhoneNumber!,
                   Role = "Admin"
               }
            );

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
        //////post shoes
        /// update shoes
        /// delete shoes
        ///get all shoes
        /// get shoes by id
        /// search shoes by name
        /// get shoes by category-color
        /// get shoes by price-range
        /// get shoes by gender
        /// get shoes by size
        /// post transaction
        /// get transaction by id
        /// get all transactions
    }


}
