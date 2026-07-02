

using Entry.Auth.Models;

namespace Entry.Auth.Services
{
  public interface IRefreshTokenService
  {
    Task<string> CreateRefreshTokenAsync(string userId);
    Task<TokenPair?> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    Task<bool> RevokeAllUserTokensAsync(string userId);
  }
}