

namespace Entry.Auth.Models
{
  public class TokenPair
  {
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
  }
}