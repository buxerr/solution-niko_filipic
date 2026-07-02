using ProductCatalog.Application.Abstractions;
using ProductCatalog.Application.DTOs.Auth;

namespace ProductCatalog.Tests.Fakes;

public class FakeAuthService : IAuthService
{
    public Task<AuthResponseDto?> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Username != "test-user" || request.Password != "test-password")
        {
            return Task.FromResult<AuthResponseDto?>(null);
        }

        var response = new AuthResponseDto
        {
            AccessToken = "valid-test-token",
            RefreshToken = "valid-refresh-token",
            Username = request.Username,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        return Task.FromResult<AuthResponseDto?>(response);
    }

    public Task<RefreshTokenResponseDto?> RefreshTokenAsync(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.RefreshToken != "valid-refresh-token")
        {
            return Task.FromResult<RefreshTokenResponseDto?>(null);
        }

        var response = new RefreshTokenResponseDto
        {
            AccessToken = "valid-test-token",
            RefreshToken = "new-refresh-test-token"
        };

        return Task.FromResult<RefreshTokenResponseDto?>(response);
    }

    public Task<AuthUserDto?> GetCurrentUserAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        if (accessToken != "valid-test-token")
        {
            return Task.FromResult<AuthUserDto?>(null);
        }

        var user = new AuthUserDto
        {
            Id = 1,
            Username = "test-user",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Gender = "male",
            Image = "test-user.jpg"
        };

        return Task.FromResult<AuthUserDto?>(user);
    }
}