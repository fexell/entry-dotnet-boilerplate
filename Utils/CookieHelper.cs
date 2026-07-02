using Microsoft.AspNetCore.Http;

namespace Entry.Auth.Utils
{
  public static class CookieHelper
  {
    public static void Set(HttpResponse response, string key, string value, TimeSpan? maxAge = null, CookieOptions? options = null)
    {
      var opts = options ?? new CookieOptions
      {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        MaxAge = maxAge
      };

      response.Cookies.Append(key, value, opts);
    }

    public static string? Get(HttpRequest request, string key)
    {
      return request.Cookies.TryGetValue(key, out var value) ? value : null;
    }

    public static void Delete(HttpResponse response, string key)
    {
      response.Cookies.Delete(key);
    }
  }
}
