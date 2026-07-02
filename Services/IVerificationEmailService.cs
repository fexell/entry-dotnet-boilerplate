

using Entry.Auth.Models;

namespace Entry.Auth.Services
{
  public interface IVerificationEmailService
  {
    Task<bool> SendVerificationEmailAsync(AppUser user);
  }
}