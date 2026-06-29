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


    public AuthController(UserManager<AppUser> userManager, IJwtService jwtService, IRefreshTokenService refreshService)
    {
      _userManager = userManager;
      _jwtService = jwtService;
      _refreshService = refreshService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
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

      return Ok(new { message = "User created. Please verify your email." });
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailDto dto)
    {
      var user = await _userManager.FindByIdAsync(dto.UserId);
      if(user == null) return BadRequest(new { message = "Invalid user." });

      var result = await _userManager.ConfirmEmailAsync(user, dto.Token);

      if(!result.Succeeded)
        return BadRequest("Invalid token or user already verified.");

      return Ok(new { message = "Email verified successfully. You can now login." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
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