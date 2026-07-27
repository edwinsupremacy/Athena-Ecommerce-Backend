using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using AthenaEcommerce_website.Interfaces;
using AthenaEcommerce_website.Models;
using System.IdentityModel.Tokens.Jwt;



namespace AthenaEcommerce_website.Services;

public class TokenService : ITokenService
{
    private readonly UserManager<User> _user;
    private readonly IConfiguration _configuration;
  public TokenService(UserManager<User> user,IConfiguration configuration)
  {
    _user=user;
    _configuration=configuration;
  }

  public async Task<string> CreateToken(User user)
    {
        //  
        List<Claim>claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Email,user.Email!),
            new Claim(JwtRegisteredClaimNames.Sub,user.Id),
        };
        ///signing key
        var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["JWT:SigningKey"])); 
        ///creds
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],
            audience:_configuration["JWT:Audience"],
            claims:claims,
            expires: DateTime.UtcNow.AddHours(5),
             signingCredentials :credentials

        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
