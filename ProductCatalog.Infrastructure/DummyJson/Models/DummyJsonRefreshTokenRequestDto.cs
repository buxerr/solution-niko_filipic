namespace ProductCatalog.Infrastructure.DummyJson.Models;

internal class DummyJsonRefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;

    public int ExpiresInMins { get; set; } = 30;
}