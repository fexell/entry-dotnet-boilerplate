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
        options.UseSqlServer(
          config.GetConnectionString("Default"),
          sql =>
          {
            sql.EnableRetryOnFailure(
              maxRetryCount: 5,
              maxRetryDelay: TimeSpan.FromSeconds(10),
              errorNumbersToAdd: null
            );

            sql.MigrationsAssembly(typeof(Program).Assembly.FullName);
          }
        )
      );

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
      .AddRoles<IdentityRole>()
      .AddEntityFrameworkStores<AppDbContext>()
      .AddDefaultTokenProviders()
      .AddSignInManager()
      .AddUserManager<UserManager<AppUser>>()
      .AddRoleManager<RoleManager<IdentityRole>>();

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
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
          ValidateIssuer = true,
          ValidateAudience = true,
          ValidateIssuerSigningKey= true,
          ValidateLifetime = true,
          ValidateActor = true,
          RequireExpirationTime = true,
          RequireSignedTokens = true,
          ValidateTokenReplay = true,

          ValidIssuer = config["Jwt:Issuer"],
          ValidAudience = config["Jwt:Audience"],
          IssuerSigningKey = new SymmetricSecurityKey(key),

          ClockSkew = TimeSpan.Zero,
        };

        options.Events = new JwtBearerEvents
        {
          OnMessageReceived = context =>
          {
            if(context.HttpContext.Items.TryGetValue("AccessToken", out var refreshed) && refreshed is string s)
            {
              context.Token = s;
              return Task.CompletedTask;
            }

            if(context.Request.Cookies.TryGetValue("accessToken", out var token)) context.Token = token;

            return Task.CompletedTask;
          }
        };
      });

      return services;
    }

    public static IServiceCollection AddAppServices(
      this IServiceCollection services
    )
    {
      services.AddScoped<IEmailService, EmailService>();
      services.AddScoped<IUserService, UserService>();
      services.AddScoped<IAuthService, AuthService>();
      services.AddScoped<IVerificationEmailService, VerificationEmailService>();

      services.AddScoped<IJwtService, JwtService>();
      services.AddScoped<IRefreshTokenService, RefreshTokenService>();

      services.AddHostedService<EmailVerificationRefreshService>();

      services.AddHttpClient();

      return services;
    }

    public static IServiceCollection AddAppAuthorization(
      this IServiceCollection services
    )
    {
      services.AddHttpContextAccessor();

      services.AddAuthorization(options =>
      {
        options.AddPolicy("Admin", policy =>
        {
          policy.RequireRole("Admin");
        });

        options.FallbackPolicy = new AuthorizationPolicyBuilder()
          .RequireAuthenticatedUser()
          .Build();
      });

      return services;
    }
  }
}