using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Entry.Auth.Models;

namespace Entry.Auth.Services
{
public class EmailVerificationRefreshService : BackgroundService
  {
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailVerificationRefreshService> _logger;

    public EmailVerificationRefreshService(
      IServiceScopeFactory scopeFactory,
      ILogger<EmailVerificationRefreshService> logger
    )
    {
      _scopeFactory = scopeFactory;
      _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        await RefreshTokensAsync();
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
      }
    }

    private async Task RefreshTokensAsync()
    {
      using var scope = _scopeFactory.CreateScope();

      var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
      var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

      var unverifiedUsers = userManager.Users
        .Where(u => !u.EmailConfirmed)
        .ToList();

      foreach (var user in unverifiedUsers)
      {
          var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

          var link = $"http://localhost:5277/verify-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

          await emailService.SendAsync(
            user.Email!,
            "Verify your email",
            $@"<p>Klicka på länken för att verifiera din e‑post:</p>
              <a href=""{link}"">Verifiera e‑post</a>"
          );

          _logger.LogInformation($"Refreshed email verification token for {user.Email}");
      }
    }
  }
}