using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

using Entry.Auth.Models;
using Entry.Auth.DTOs;
using Entry.Auth.Services;

namespace Entry.Auth.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshService;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _env;

    public AuthController(
      UserManager<AppUser> userManager,
      IJwtService jwtService,
      IRefreshTokenService refreshService,
      IEmailService emailService,
      IWebHostEnvironment env
    )
    {
      _userManager = userManager;
      _jwtService = jwtService;
      _refreshService = refreshService;
      _emailService = emailService;
      _env = env;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
      var user = new AppUser
      {
        Email = dto.Email,
        UserName = dto.Username,
      };

      var result = await _userManager.CreateAsync(user, dto.Password);

      if(!result.Succeeded)
        return BadRequest(result.Errors);

      var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

      var confirmationLink = $"http://localhost:5277/verify-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

      await _emailService.SendAsync(user.Email, "Verify your email", $"Please click the link to verify your email: <a href=\"{confirmationLink}\">Verify Email</a>");

      if (_env.IsDevelopment())
      {
        Console.WriteLine($"[DEV] Email token: {token}");
        Console.WriteLine($"[DEV] Email confirmation link: {confirmationLink}");
      }

      return Ok(new { message = "User created. Please verify your email." });
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null) return BadRequest(new { message = "Invalid user." });

        var result = await _userManager.ConfirmEmailAsync(user, dto.Token);

        if (!result.Succeeded)
            return BadRequest(new { message = "Invalid token or user already verified." });

        return Ok(new { message = "Email verified successfully. You can now login." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
      var user = await _userManager.FindByEmailAsync(dto.Email);
      if(user == null ) return Unauthorized();

      if(!await _userManager.IsEmailConfirmedAsync(user)) return Unauthorized("Email not verified.");

      var valid = await _userManager.CheckPasswordAsync(user, dto.Password);
      if(!valid) return Unauthorized();

      var token = _jwtService.GenerateToken(user);
      var refresh = await _refreshService.CreateRefreshTokenAsync(user.Id);

      return Ok(new { token, refresh });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshDto dto)
    {
      var newToken = await _refreshService.RefreshTokenAsync(dto.RefreshToken);
      if(newToken == null) return Unauthorized();

      return Ok(new { token = newToken });
    }
  }
}