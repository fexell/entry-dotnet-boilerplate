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
      _logger.LogInformation("EmailVerificationRefreshService started.");

      while (!stoppingToken.IsCancellationRequested)
      {
        try
        {
          await RefreshTokensAsync(stoppingToken);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Unexpected error while refreshing email verification tokens.");
        }

        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
      }

      _logger.LogInformation("EmailVerificationRefreshService stopped.");
    }

    private async Task RefreshTokensAsync(CancellationToken stoppingToken)
    {
      using var scope = _scopeFactory.CreateScope();

      var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
      var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

      var unverifiedUsers = userManager.Users
        .Where(u => !u.EmailConfirmed)
        .ToList();

      if (unverifiedUsers.Count == 0)
      {
        _logger.LogDebug("No unverified users found.");
        return;
      }

      _logger.LogInformation("Refreshing email verification tokens for {Count} users.", unverifiedUsers.Count);

      foreach (var user in unverifiedUsers)
      {
        if (stoppingToken.IsCancellationRequested)
          break;

        try
        {
          var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

          var link = $"http://localhost:5277/verify-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

          await emailService.SendAsync(
            user.Email!,
            "Verify your email",
            $@"<p>Klicka på länken för att verifiera din e‑post:</p>
               <a href=""{link}"">Verifiera e‑post</a>"
          );

          _logger.LogInformation(
            "Sent refreshed verification token to {Email} (UserId: {UserId}).",
            user.Email,
            user.Id
          );
        }
        catch (Exception ex)
        {
          _logger.LogError(
            ex,
            "Failed to send verification email to {Email} (UserId: {UserId}).",
            user.Email,
            user.Id
          );
        }
      }
    }
  }
}
