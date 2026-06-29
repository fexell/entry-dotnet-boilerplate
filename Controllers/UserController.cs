using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Entry.Auth.Models;
using Entry.Auth.DTOs;

namespace Entry.Auth.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class UserController : ControllerBase
  {
    private readonly UserManager<AppUser> _userManager;

    public UserController(UserManager<AppUser> userManager)
    {
      _userManager = userManager;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
      var user = await _userManager.GetUserAsync(User);
      if(user == null) return Unauthorized();

      return Ok(new
      {
        user.Id,
        user.Email,
        user.UserName,
        user.CreatedAt
      });
    }

    [Authorize]
    [HttpPut("update")]
    public async Task<IActionResult> Update(UserUpdateDto dto)
    {
      var user = await _userManager.GetUserAsync(User);
      if(user == null) return Unauthorized();

      if(!string.IsNullOrWhiteSpace(dto.Email))
        user.Email = dto.Email;

      if(!string.IsNullOrWhiteSpace(dto.Username))
        user.UserName = dto.Username;

      var result = await _userManager.UpdateAsync(user);

      if(!result.Succeeded)
        return BadRequest(result.Errors);

      return Ok(new { message = "User updated successfully." });
    }

    [Authorize]
    [HttpDelete("delete")]
    public async Task<IActionResult> Delete()
    {
      var user = await _userManager.GetUserAsync(User);
      if(user == null) return Unauthorized();

      var result = await _userManager.DeleteAsync(user);

      if(!result.Succeeded)
        return BadRequest(result.Errors);

      return Ok(new { message = "User deleted successfully." });
    }
  }
}