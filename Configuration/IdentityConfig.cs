using Microsoft.AspNetCore.Identity;

namespace Entry.Auth.Configuration
{
  public static class IdentityConfig
  {
    public static void ConfigureIdentityOptions(IdentityOptions options)
    {
      options.Password.RequireDigit = false;
      options.Password.RequireLowercase = false;
      options.Password.RequireUppercase = false;
      options.Password.RequireNonAlphanumeric = false;
      options.Password.RequiredLength = 6;

      options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
      options.Lockout.MaxFailedAccessAttempts = 5;
      options.Lockout.AllowedForNewUsers = true;

      options.User.RequireUniqueEmail = true;

      options.SignIn.RequireConfirmedEmail = true;
      options.SignIn.RequireConfirmedPhoneNumber = false;
    }
  }
}