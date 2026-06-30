using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

using Entry.Auth.Requirements;
using Entry.Auth.Services;

namespace Entry.Auth.Requirements
{
  public class SilentRefreshHandler : AuthorizationHandler<SilentRefreshRequirement>
  {
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SilentRefreshHandler(
      IRefreshTokenService refreshTokenService,
      IHttpContextAccessor httpContextAccessor
    )
    {
      _refreshTokenService = refreshTokenService;
      _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
      AuthorizationHandlerContext context,
      SilentRefreshRequirement requirement
    )
    {
      var httpContext = _httpContextAccessor.HttpContext;
      if(httpContext is null)
      {
        context.Fail();
        return;
      }

      var accessToken = httpContext.Request.Cookies["accessToken"];
      var refreshToken = httpContext.Request.Cookies["refreshToken"];

      if(!string.IsNullOrEmpty(accessToken) && !TokenIsExpiredOrNearExpiry(accessToken))
      {
        context.Succeed(requirement);
        return;
      }

      if (!string.IsNullOrEmpty(refreshToken))
      {
        var newToken = await _refreshTokenService.RefreshTokenAsync(refreshToken);
        if(newToken is not null)
        {
          context.Succeed(requirement);
          return;
        }
      }

      context.Fail();
    }

    private static bool TokenIsExpiredOrNearExpiry(string token, int nearExpirySeconds = 60)
    {
      try
      {
        var handler = new JwtSecurityTokenHandler();

        if(!handler.CanReadToken(token))
          return true;

        var jwt = handler.ReadJwtToken(token);
        var expiry = jwt.ValidTo;

        return DateTime.UtcNow >= expiry.AddSeconds(-nearExpirySeconds);
      }
      catch
      {
        return true;
      }
    }
  }
}