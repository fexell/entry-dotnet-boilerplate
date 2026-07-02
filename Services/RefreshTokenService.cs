using Microsoft.EntityFrameworkCore;

using Entry.Auth.Data;
using Entry.Auth.Models;
using Entry.Auth.Utils;

namespace Entry.Auth.Services
{
  public class RefreshTokenService : IRefreshTokenService
  {
    private readonly AppDbContext _db;
    private readonly IJwtService _jwtService;

    public RefreshTokenService(AppDbContext db, IJwtService jwtService)
    {
      _db = db;
      _jwtService = jwtService;
    }

    // ------------------------------------------------------
    // CREATE REFRESH TOKEN
    // ------------------------------------------------------

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

    // ------------------------------------------------------
    // REFRESH TOKEN (ROTATION)
    // ------------------------------------------------------

    public async Task<TokenPair?> RefreshTokenAsync(string refreshToken)
    {
      var token = await _db.RefreshTokens
        .FirstOrDefaultAsync(x => x.Token == refreshToken);

      if (token == null || token.Revoked || token.ExpiresAt < DateTime.UtcNow)
        return null;

      var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == token.UserId);
      if (user == null)
        return null;

      // revoke old refresh token
      token.Revoked = true;
      await _db.SaveChangesAsync();

      // create new refresh token
      var newRefresh = await CreateRefreshTokenAsync(user.Id);

      // create new access token
      var jwt = _jwtService.GenerateToken(user);

      return new TokenPair
      {
        AccessToken = jwt.Token,
        RefreshToken = newRefresh,
        ExpiresInSeconds = jwt.ExpiresInSeconds
      };
    }

    // ------------------------------------------------------
    // REVOKE
    // ------------------------------------------------------

    public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
    {
      var token = await _db.RefreshTokens
        .FirstOrDefaultAsync(x => x.Token == refreshToken);

      if (token == null) return false;

      token.Revoked = true;
      await _db.SaveChangesAsync();
      return true;
    }

    public async Task<bool> RevokeAllUserTokensAsync(string userId)
    {
      var tokens = await _db.RefreshTokens
        .Where(x => x.UserId == userId && !x.Revoked)
        .ToListAsync();

      foreach (var t in tokens)
        t.Revoked = true;

      await _db.SaveChangesAsync();
      return true;
    }
  }
}
