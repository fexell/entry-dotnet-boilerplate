
using Entry.Auth.Models;

namespace Entry.Auth.Services
{
  public interface IJwtService
  {
    JwtTokenResult GenerateToken(AppUser user);
  }
}
