namespace ProductCatalog.Infrastructure.DummyJson.Models;

internal class DummyJsonLoginRequestDto
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public int ExpiresInMins { get; set; } = 30;
}