using BookLibraryApi.DTOs;
using BookLibraryApi.Services;
using Microsoft.AspNetCore.Mvc;
namespace BookLibraryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = await _auth.RegisterAsync(dto);
        return Ok(new { user.Id, user.Username, user.Email });
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var token = await _auth.LoginAsync(dto);
        if (token is null) return Unauthorized(new { message = "Invalidcredentials" });
        return Ok(new { token });
    }
}
