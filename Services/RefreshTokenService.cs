using Microsoft.EntityFrameworkCore;

using Entry.Auth.Data;
using Entry.Auth.Models;
using Entry.Auth.Utils;

namespace Entry.Auth.Services
{
  public interface IRefreshTokenService
  {
    Task<string> CreateRefreshTokenAsync(string userId);
    Task<string?> RefreshTokenAsync(string refreshToken);
  }

  public class RefreshTokenService : IRefreshTokenService
  {
    private readonly AppDbContext _db;
    private readonly IJwtService _jwtService;

    public RefreshTokenService(AppDbContext db, IJwtService jwtService)
    {
      _db = db;
      _jwtService = jwtService;
    }

    public async Task<string> CreateRefreshTokenAsync(string userId)
    {
      var token = TokenGenerator.GenerateRandomToken(32);

      var refresh = new RefreshToken
      {
        Token = token,
        UserId = userId,
        ExpiresAt = DateTime.UtcNow.AddDays(30),
        Revoked = false
      };

      _db.RefreshTokens.Add(refresh);
      await _db.SaveChangesAsync();

      return token;
    }

    public async Task<string?> RefreshTokenAsync(string refreshToken)
    {
      var token = await _db.RefreshTokens
        .FirstOrDefaultAsync(x => x.Token == refreshToken);

      if(token == null) return null;
      if(token.Revoked) return null;
      if(token.ExpiresAt < DateTime.UtcNow) return null;

      var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == token.UserId);
      if(user == null) return null;

      token.Revoked = true;
      await _db.SaveChangesAsync();

      var newRefresh = await CreateRefreshTokenAsync(user.Id);

      var newJwt = _jwtService.GenerateToken(user);

      return newJwt;
    }
  }
}