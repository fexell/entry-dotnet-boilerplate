using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Entry.Auth.Services
{
  public interface IEmailService
  {
    Task SendAsync(string to, string subject, string htmlBody);
  }

  public class EmailService : IEmailService
  {
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public EmailService(IConfiguration config, HttpClient http)
    {
      _http = http;
      _apiKey = config["Resend:ApiKey"]
        ?? throw new Exception("Missing Resend API key");
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
      var payload = new
      {
        from = "onboarding@resend.dev",
        to = new[] { to },
        subject,
        html = htmlBody,
      };

      var json = JsonSerializer.Serialize(payload);
      var content = new StringContent(json, Encoding.UTF8, "application/json");

      _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

      var response = await _http.PostAsync("https://api.resend.com/emails", content);

      if (!response.IsSuccessStatusCode)
      {
        var error = await response.Content.ReadAsStringAsync();
        throw new Exception($"Failed to send email: {error}");
      }
    }
  }
}