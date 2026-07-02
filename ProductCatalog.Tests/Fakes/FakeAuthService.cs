using ProductCatalog.Application.Abstractions;
using ProductCatalog.Application.DTOs.Auth;

namespace ProductCatalog.Tests.Fakes;

public class FakeAuthService : IAuthService
{
    public Task<AuthResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new AuthResponseDto
        {
            Id = 1,
            Username = request.Username,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Gender = "unknown",
            Image = "test.jpg",
            AccessToken = "valid-test-token",
            RefreshToken = "refresh-test-token"
        });
    }

    public Task<AuthUserDto?> GetCurrentUserAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        if (accessToken != "valid-test-token")
        {
            return Task.FromResult<AuthUserDto?>(null);
        }

        return Task.FromResult<AuthUserDto?>(new AuthUserDto
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Gender = "unknown",
            Image = "test.jpg"
        });
    }
}