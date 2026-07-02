using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

using Entry.Auth.Models;

namespace Entry.Auth.Services
{
  public class JwtService : IJwtService
  {
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
      _config = config;
    }

    public JwtTokenResult GenerateToken(AppUser user)
    {
      var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
      );

      var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

      var claims = new List<Claim>
      {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
        new Claim("username", user.UserName ?? "")
      };

      var expires = DateTime.UtcNow.AddHours(1);

      var token = new JwtSecurityToken(
        issuer: _config["Jwt:Issuer"],
        audience: _config["Jwt:Audience"],
        claims: claims,
        expires: expires,
        signingCredentials: creds
      );

      var jwt = new JwtSecurityTokenHandler().WriteToken(token);

      return new JwtTokenResult
      {
        Token = jwt,
        ExpiresAt = expires,
        ExpiresInSeconds = (int)(expires - DateTime.UtcNow).TotalSeconds
      };
    }
  }

  public class JwtTokenResult
  {
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int ExpiresInSeconds { get; set; }
  }
}
