using System;
using AthenaEcommerce_website.Models;

namespace AthenaEcommerce_website.Interfaces;

public interface ITokenService
{
public Task<string> CreateToken(User user);
}
