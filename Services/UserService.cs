using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Entry.Auth.Data;
using Entry.Auth.Models;
using Entry.Auth.DTOs;

namespace Entry.Auth.Services
{
  public class UserService : IUserService
  {
    private readonly UserManager<AppUser> _userManager;
    private readonly IVerificationEmailService _verificationEmailService;
    private readonly IRefreshTokenService _refreshTokenService;

    public UserService(
      UserManager<AppUser> userManager,
      IVerificationEmailService verificationEmailService,
      IRefreshTokenService refreshTokenService
    )
    {
      _userManager = userManager;
      _verificationEmailService = verificationEmailService;
      _refreshTokenService = refreshTokenService;
    }

    // ------------------------------------------------------
    // GET USER
    // ------------------------------------------------------

    public async Task<AppUser?> GetByIdAsync(string userId)
    {
      return await _userManager.Users
        .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
      return await _userManager.Users
        .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<AppUser?> GetByUsernameAsync(string username)
    {
      return await _userManager.Users
        .FirstOrDefaultAsync(u => u.UserName == username);
    }

    // ------------------------------------------------------
    // UPDATE USER
    // ------------------------------------------------------

    public async Task<bool> UpdateUserAsync(AppUser user, UserUpdateDto dto)
    {
      var updated = false;
      var emailChanged = false;

      if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
      {
        user.Email = dto.Email;
        await _userManager.UpdateNormalizedEmailAsync(user);
        user.EmailConfirmed = false;
        updated = true;
        emailChanged = true;
      }

      if (!string.IsNullOrWhiteSpace(dto.Username) && dto.Username != user.UserName)
      {
        user.UserName = dto.Username;
        await _userManager.UpdateNormalizedUserNameAsync(user);
        updated = true;
      }

      if (!updated) return true;

      var result = await _userManager.UpdateAsync(user);

      if (result.Succeeded && emailChanged)
        await _verificationEmailService.SendVerificationEmailAsync(user);

      return result.Succeeded;
    }

    // ------------------------------------------------------
    // DELETE USER
    // ------------------------------------------------------

    public async Task<bool> DeleteUserAsync(AppUser user)
    {
      var result = await _userManager.DeleteAsync(user);
      return result.Succeeded;
    }

    // ------------------------------------------------------
    // EMAIL CONFIRMATION
    // ------------------------------------------------------

    public async Task<bool> IsEmailConfirmedAsync(AppUser user)
    {
      return await _userManager.IsEmailConfirmedAsync(user);
    }

    public async Task<string> GenerateEmailVerificationTokenAsync(AppUser user)
    {
      return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<bool> VerifyEmailAsync(AppUser user, string token)
    {
      var result = await _userManager.ConfirmEmailAsync(user, token);
      return result.Succeeded;
    }

    // ------------------------------------------------------
    // PASSWORD CHANGE
    // ------------------------------------------------------

    public async Task<bool> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword)
    {
      var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

      if (result.Succeeded)
        await _refreshTokenService.RevokeAllUserTokensAsync(user.Id);

      return result.Succeeded;
    }

    // ------------------------------------------------------
    // USER ME DTO
    // ------------------------------------------------------

    public async Task<UserMeDto> GetUserMeAsync(AppUser user)
    {
      return new UserMeDto
      {
        Id = user.Id,
        Email = user.Email,
        Username = user.UserName,
        CreatedAt = user.CreatedAt,
        EmailConfirmed = user.EmailConfirmed,
        Avatar = user.Avatar,
        DisplayName = user.DisplayName,
        Premium = user.Premium
      };
    }
  }
}
