using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

using Entry.Auth.Services;
using Entry.Auth.Models;

namespace Entry.Auth.Middlewares
{
  public class SilentRefreshMiddleware
  {
    public async Task InvokeAsync(HttpContext context)
    {
      var jwtService = context.RequestServices.GetRequiredService<IJwtService>();
      var refreshService = context.RequestServices.GetRequiredService<IRefreshTokenService>();
      var userManager = context.RequestServices.GetRequiredService<UserManager<AppUser>>();

      var accessToken = context.Request.Cookies["accessToken"];
      var refreshToken = context.Request.Cookies["refreshToken"];

      // No tokens → continue
      if (string.IsNullOrEmpty(accessToken) && string.IsNullOrEmpty(refreshToken))
      {
          return;
      }

      var handler = new JwtSecurityTokenHandler();

      // Try validate access token
      try
      {
          var jwt = handler.ReadJwtToken(accessToken);
          var userId = jwt.Subject;

          var user = await userManager.FindByIdAsync(userId);
          if (user != null)
          {
              return;
          }
      }
      catch
      {
          // Access token invalid → try refresh
      }

      // No refresh token → continue
      if (string.IsNullOrEmpty(refreshToken))
      {
          return;
      }

      // Try refresh
      var newTokens = await refreshService.RefreshTokenAsync(refreshToken);

      if (newTokens == null)
      {
          context.Response.Cookies.Delete("accessToken");
          context.Response.Cookies.Delete("refreshToken");
          return;
      }

      // Set new access token
      context.Response.Cookies.Append("accessToken", newTokens.AccessToken, new CookieOptions
      {
          HttpOnly = true,
          Secure = true,
          SameSite = SameSiteMode.Strict
      });

      // Rotate refresh token
      var newRefreshToken = await refreshService.CreateRefreshTokenAsync(
          handler.ReadJwtToken(newTokens.RefreshToken).Subject
      );

      context.Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
      {
          HttpOnly = true,
          Secure = true,
          SameSite = SameSiteMode.Strict
      });
    }
  }
}
