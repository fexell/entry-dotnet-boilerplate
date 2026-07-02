using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Entry.Auth.DTOs;
using Entry.Auth.Models;
using Entry.Auth.Services;

namespace Entry.Auth.Controllers
{
  [Authorize]
  [ApiController]
  [Route("api/[controller]")]
  public class UserController : ControllerBase
  {
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
      _userService = userService;
    }

    // ------------------------------------------------------
    // GET /api/user/me
    // ------------------------------------------------------

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
      var userId = User.FindFirst("sub")?.Value;
      if (string.IsNullOrEmpty(userId))
        return Unauthorized();

      var user = await _userService.GetByIdAsync(userId);
      if (user == null)
        return Unauthorized();

      var dto = await _userService.GetUserMeAsync(user);
      return Ok(dto);
    }

    // ------------------------------------------------------
    // PUT /api/user/update
    // ------------------------------------------------------

    [HttpPut("update")]
    public async Task<IActionResult> Update([FromBody] UserUpdateDto dto)
    {
      var userId = User.FindFirst("sub")?.Value;
      if (string.IsNullOrEmpty(userId))
        return Unauthorized();

      var user = await _userService.GetByIdAsync(userId);
      if (user == null)
        return Unauthorized();

      var success = await _userService.UpdateUserAsync(user, dto);

      if (!success)
        return BadRequest(new { message = "No changes were applied." });

      return Ok(new { message = "User updated successfully." });
    }

    // ------------------------------------------------------
    // DELETE /api/user/delete
    // ------------------------------------------------------

    [HttpDelete("delete")]
    public async Task<IActionResult> Delete()
    {
      var userId = User.FindFirst("sub")?.Value;
      if (string.IsNullOrEmpty(userId))
        return Unauthorized();

      var user = await _userService.GetByIdAsync(userId);
      if (user == null)
        return Unauthorized();

      var result = await _userService.DeleteUserAsync(user);

      if (!result)
        return BadRequest(new { message = "Failed to delete user." });

      return Ok(new { message = "User deleted successfully." });
    }
  }
}
