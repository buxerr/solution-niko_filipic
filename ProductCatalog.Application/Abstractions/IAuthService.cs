using ProductCatalog.Application.DTOs.Auth;

namespace ProductCatalog.Application.Abstractions;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AuthUserDto?> GetCurrentUserAsync(
        string accessToken,
        CancellationToken cancellationToken = default);
}