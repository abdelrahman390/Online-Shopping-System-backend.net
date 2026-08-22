using System.Net.Http.Headers;
using System.Net.Http.Json;

public class EmailService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public EmailService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(
        string to,
        string subject,
        string message)
    {
        var apiToken = _configuration["Mailtrap:ApiToken"];

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiToken);

        var email = new
        {
            from = new
            {
                email = "hello@demomailtrap.co",
                name = "Online Shopping System"
            },
            to = new[]
            {
                new
                {
                    email = to
                }
            },
            subject = subject,
            text = message
        };

        var response = await _httpClient.PostAsJsonAsync(
            "https://send.api.mailtrap.io/api/send",
            email);

        response.EnsureSuccessStatusCode();
    }
}