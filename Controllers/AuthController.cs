using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Entry.Auth.DTOs;
using Entry.Auth.Models;
using Entry.Auth.Services;
using Entry.Auth.Utils;

namespace Entry.Auth.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class AuthController : ControllerBase
  {
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
      _authService = authService;
      _userService = userService;
    }

    // ------------------------------------------------------
    // REGISTER (NO LOGIN)
    // ------------------------------------------------------

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
      var result = await _authService.RegisterAsync(dto);

      if (!result.Success)
        return BadRequest(new { message = "Registration failed.", errors = result.Errors });

      return Ok(new { message = "User created. Please verify your email." });
    }

    // ------------------------------------------------------
    // VERIFY EMAIL
    // ------------------------------------------------------

    [AllowAnonymous]
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
    {
      var user = await _userService.GetByIdAsync(dto.UserId!);
      if (user == null)
        return NotFound(new { message = "User not found." });

      var success = await _authService.VerifyEmailAsync(user, dto.Token!);

      if (!success)
        return Conflict(new { message = "Email verification failed." });

      return Ok(new { message = "Email verified successfully." });
    }

    // ------------------------------------------------------
    // RESEND VERIFICATION
    // ------------------------------------------------------

    [AllowAnonymous]
    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto dto)
    {
      var user = await _userService.GetByEmailAsync(dto.Email!);

      if (user == null)
        return BadRequest(new { message = "User not found." });

      if (user.EmailConfirmed)
        return BadRequest(new { message = "Email already verified." });

      await _authService.SendVerificationEmailAsync(user);

      return Ok(new { message = "Verification email sent." });
    }

    // ------------------------------------------------------
    // LOGIN
    // ------------------------------------------------------

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
      var result = await _authService.LoginAsync(dto);

      if (!result.Success)
        return Unauthorized(new { message = "Invalid credentials.", errors = result.Errors });

      CookieHelper.Set(Response, "accessToken", result.AccessToken!);
      CookieHelper.Set(Response, "refreshToken", result.RefreshToken!);

      return Ok(result);
    }

    // ------------------------------------------------------
    // REFRESH TOKEN
    // ------------------------------------------------------
 
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshDto dto)
    {
      var result = await _authService.RefreshAsync(dto);

      if (!result.Success)
        return BadRequest(new { message = "Invalid refresh token.", errors = result.Errors });

      CookieHelper.Set(Response, "accessToken", result.AccessToken!);
      CookieHelper.Set(Response, "refreshToken", result.RefreshToken!);

      return Ok(result);
    }

    // ------------------------------------------------------
    // LOGOUT
    // ------------------------------------------------------

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
      var refreshToken = Request.Cookies["refreshToken"];
      if (!string.IsNullOrEmpty(refreshToken))
      {
        await _authService.RevokeRefreshTokenAsync(refreshToken);
      }

      CookieHelper.Delete(Response, "accessToken");
      CookieHelper.Delete(Response, "refreshToken");

      return Ok(new { message = "Logged out." });
    }
  }
}
