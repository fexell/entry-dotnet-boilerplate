using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;

using Entry.Auth.Configuration;
using Entry.Auth.Data;
using Entry.Auth.Models;
using Entry.Auth.Services;
using Entry.Auth.Requirements;

namespace Entry.Auth.Extensions
{
  public static class ServiceCollectionExtensions
  {
    public static IServiceCollection AddAppDbContext(
      this IServiceCollection services,
      IConfiguration config
    )
    {
      services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(config.GetConnectionString("Default")));

      return services;
    }

    public static IServiceCollection AddAppIdentity(
      this IServiceCollection services
    )
    {
      services.AddIdentity<AppUser, IdentityRole>(options =>
      {
        IdentityConfig.ConfigureIdentityOptions(options);
      })
      .AddEntityFrameworkStores<AppDbContext>()
      .AddDefaultTokenProviders();

      return services;
    }

    public static IServiceCollection AddJwtAuthentication(
      this IServiceCollection services,
      IConfiguration config
    )
    {
      var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);

      services.AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
      })
      .AddJwtBearer(options =>
      {
        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateIssuerSigningKey= true,
          ValidIssuer = config["Jwt:Issuer"],
          ValidAudience = config["Jwt:Audience"],
          IssuerSigningKey = new SymmetricSecurityKey(key)
        };
      });

      return services;
    }

    public static IServiceCollection AddAppServices(
      this IServiceCollection services
    )
    {
      services.AddScoped<IJwtService, JwtService>();
      services.AddScoped<IRefreshTokenService, RefreshTokenService>();

      return services;
    }

    public static IServiceCollection AddAppAuthorization(
      this IServiceCollection services
    )
    {
      services.AddHttpContextAccessor();
      services.AddScoped<IAuthorizationHandler, SilentRefreshHandler>();

      services.AddAuthorization(options =>
      {
        options.AddPolicy("SilentRefresh", policy =>
        {
          policy.Requirements.Add(new SilentRefreshRequirement());
        });
      });

      return services;
    }
  }
}