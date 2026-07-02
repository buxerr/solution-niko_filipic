using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProductCatalog.Application.Abstractions;
using ProductCatalog.Application.DTOs.Auth;
using ProductCatalog.Infrastructure.DummyJson.Models;

namespace ProductCatalog.Infrastructure.DummyJson;

public class DummyJsonAuthService : IAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    private readonly ILogger<DummyJsonAuthService> _logger;

    public DummyJsonAuthService(
        HttpClient httpClient,
        ILogger<DummyJsonAuthService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Attempting login for user {Username}.", request.Username);

        var dummyJsonRequest = new DummyJsonLoginRequestDto
        {
            Username = request.Username,
            Password = request.Password,
            ExpiresInMins = 30
        };

        using var response = await _httpClient.PostAsJsonAsync(
            "auth/login",
            dummyJsonRequest,
            JsonOptions,
            cancellationToken);

        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("Failed login attempt for user {Username}.", request.Username);
            throw new UnauthorizedAccessException("Invalid username or password.");
        }

        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<DummyJsonLoginResponseDto>(
            JsonOptions,
            cancellationToken);

        if (loginResponse is null)
        {
            throw new InvalidOperationException("DummyJSON login response was empty.");
        }

        _logger.LogInformation("User {Username} logged in successfully.", loginResponse.Username);

        return new AuthResponseDto
        {
            Id = loginResponse.Id,
            Username = loginResponse.Username,
            Email = loginResponse.Email,
            FirstName = loginResponse.FirstName,
            LastName = loginResponse.LastName,
            Gender = loginResponse.Gender,
            Image = loginResponse.Image,
            AccessToken = loginResponse.AccessToken,
            RefreshToken = loginResponse.RefreshToken
        };
    }

    public async Task<AuthUserDto?> GetCurrentUserAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating access token with DummyJSON.");

        using var request = new HttpRequestMessage(HttpMethod.Get, "auth/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "DummyJSON token validation failed with status code {StatusCode}.",
                response.StatusCode);

            return null;
        }

        var user = await response.Content.ReadFromJsonAsync<DummyJsonAuthUserDto>(
            JsonOptions,
            cancellationToken);

        if (user is null)
        {
            _logger.LogWarning("DummyJSON auth/me returned empty response.");
            return null;
        }

        return new AuthUserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Gender = user.Gender,
            Image = user.Image
        };
    }
}