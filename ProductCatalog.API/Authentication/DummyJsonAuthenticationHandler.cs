using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using ProductCatalog.Application.Abstractions;

namespace ProductCatalog.API.Authentication;

public class DummyJsonAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IAuthService _authService;
    private readonly IMemoryCache _cache;

    public DummyJsonAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IAuthService authService,
        IMemoryCache cache)
        : base(options, logger, encoder)
    {
        _authService = authService;
        _cache = cache;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var authorizationHeader = authorizationHeaderValues.ToString();

        if (!AuthenticationHeaderValue.TryParse(authorizationHeader, out var headerValue))
        {
            return AuthenticateResult.NoResult();
        }

        if (!headerValue.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(headerValue.Parameter))
        {
            return AuthenticateResult.NoResult();
        }

        var accessToken = headerValue.Parameter;
        var cacheKey = $"auth:dummyjson:{accessToken}";

        if (_cache.TryGetValue(cacheKey, out ClaimsPrincipal? cachedPrincipal) &&
            cachedPrincipal is not null)
        {
            var cachedTicket = new AuthenticationTicket(cachedPrincipal, Scheme.Name);
            return AuthenticateResult.Success(cachedTicket);
        }

        var user = await _authService.GetCurrentUserAsync(accessToken, Context.RequestAborted);

        if (user is null)
        {
            return AuthenticateResult.Fail("Invalid or expired access token.");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new("firstName", user.FirstName),
            new("lastName", user.LastName),
            new("gender", user.Gender),
            new("image", user.Image)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);

        _cache.Set(cacheKey, principal, TimeSpan.FromMinutes(5));

        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}