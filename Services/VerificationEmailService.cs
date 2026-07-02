using Microsoft.AspNetCore.Identity;

using Entry.Auth.Models;

namespace Entry.Auth.Services
{
  public class VerificationEmailService : IVerificationEmailService
  {
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public VerificationEmailService(
      UserManager<AppUser> userManager,
      IEmailService emailService,
      IConfiguration config,
      IWebHostEnvironment env
    )
    {
      _userManager = userManager;
      _emailService = emailService;
      _config = config;
      _env = env;
    }

    public async Task<bool> SendVerificationEmailAsync(AppUser user)
    {
      var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
      var baseUrl = _env.IsDevelopment() ? "http://localhost:5277" : _config["AppUrls:FrontendBaseUrl"];
      var link = $"{baseUrl}/verify-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

      await _emailService.SendAsync(
        user.Email!,
        "Verify your email",
        $"<p>Please verify your email by clicking the link below:</p><p><a href='{link}'>Verify Email</a></p>"
      );

      return true;
    }
  }
}