using Microsoft.AspNetCore.Mvc;

namespace WasteTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    // Simple in-memory user storage for local testing
    private static readonly Dictionary<string, string> Users = new(StringComparer.OrdinalIgnoreCase)
    {
        // Pre-configured default login
        { "admin", "password123" }
    };

    [HttpPost("register")]
    public IActionResult Register([FromBody] AuthRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username and password are required.");
        }

        if (Users.ContainsKey(request.Username))
        {
            return BadRequest("Username already exists.");
        }

        // Save new user credentials
        Users[request.Username] = request.Password;

        return Ok(new { Message = "Account created successfully!" });
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] AuthRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username and password are required.");
        }

        // Validate username and password match
        if (Users.TryGetValue(request.Username, out var storedPassword) && storedPassword == request.Password)
        {
            return Ok(new { Message = "Login successful!" });
        }

        return Unauthorized("Invalid username or password.");
    }
}

public class AuthRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}