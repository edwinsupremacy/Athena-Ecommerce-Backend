using System;
using Microsoft.AspNetCore.Identity;

namespace AthenaEcommerce_website.Models;

public class User : IdentityUser

{
  public String FirstName { get; set; }= String.Empty;
  public String SecondName { get; set; }= String.Empty;

}
