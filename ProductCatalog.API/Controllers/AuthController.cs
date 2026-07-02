using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProductCatalog.Application.Abstractions;
using ProductCatalog.Application.DTOs.Auth;
using System.Security.Claims;

namespace ProductCatalog.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [EnableRateLimiting("auth")]
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(
        LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username and password are required." });
        }

        var response = await _authService.LoginAsync(request, cancellationToken);

        if (response is null)
        {
            return Unauthorized(new
            {
                message = "Invalid username or password."
            });
        }

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<RefreshTokenResponseDto>> RefreshToken(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { message = "Refresh token is required." });
        }

        var response = await _authService.RefreshTokenAsync(request, cancellationToken);

        if (response is null)
        {
            return Unauthorized(new
            {
                message = "Invalid refresh token."
            });
        }

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult GetCurrentUser()
    {
        return Ok(new
        {
            id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            username = User.FindFirstValue(ClaimTypes.Name),
            email = User.FindFirstValue(ClaimTypes.Email),
            firstName = User.FindFirstValue("firstName"),
            lastName = User.FindFirstValue("lastName"),
            gender = User.FindFirstValue("gender"),
            image = User.FindFirstValue("image")
        });
    }
}